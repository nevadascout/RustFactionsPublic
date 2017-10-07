namespace Oxide.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Oxide.Core;

    [Info("GatherLevels", "Rust Factions", "1.0")]
    public class GatherLevels : RustPlugin
    {
        private StoredData storedData;
        private string dataFile = "gatherlevels.data";
        private readonly System.Random rand = new System.Random();

        // Define z coords for the badlands
        private readonly float zoneTop = 291.3f;
        private readonly float zoneBottom = -188.5f;
        private readonly string hqmName = "High Quality Metal Ore";
        private readonly string oilName = "Crude Oil";

        // If the player is less than 2x, use this rate to increase
        private readonly float quickIncreaseRate = 0.001f;

        // If the player is more than 2x, use this rate to increase
        private readonly float slowIncreaseRate = 0.0007f;

        // Quary gather rates
        private readonly int quarryMinRate = 2;
        private readonly int quarryMaxRate = 3;


        private void Loaded()
        {
            this.storedData = Interface.GetMod().DataFileSystem.ReadObject<StoredData>(this.dataFile);

            if (this.storedData.Players == null) this.storedData.Players = new HashSet<PlayerGatherLevel>();
        }


        //#region Badlands Quarry Rates

        //private void OnSurveyGather(SurveyCharge survey, Item item)
        //{
        //    if (this.IsInBadlands(survey))
        //    {
        //        var multiplier = this.rand.Next(3, 5);
        //        item.amount = item.amount * multiplier;
        //    }

        //    // // Remove HQM in the badlands + south of it
        //    // if (survey.transform.position.z < this.zoneTop)
        //    // {
        //    //     if (item.info.displayName.english == this.hqmName)
        //    //     {
        //    //         item.Remove(0f);
        //    //     }
        //    // }

        //    // // Remove Oil in the badlands + noth of it
        //    // if (survey.transform.position.z > this.zoneBottom)
        //    // {
        //    //     if (item.info.displayName.english == this.oilName)
        //    //     {
        //    //         item.Remove(0f);
        //    //     }
        //    // }
        //}

        //private void OnQuarryGather(MiningQuarry quarry, Item item)
        //{
        //    if (this.IsInBadlands(quarry))
        //    {
        //        var multiplier = this.rand.Next(this.quarryMinRate, this.quarryMaxRate);
        //        item.amount = item.amount * multiplier;
        //    }

        //    // // Remove HQM in the badlands + south of it
        //    // if (quarry.transform.position.z < this.zoneTop)
        //    // {
        //    //     if (item.info.displayName.english == this.hqmName)
        //    //     {
        //    //         item.RemoveFromContainer();
        //    //         //item.Remove(0f);
        //    //     }
        //    // }

        //    // // Remove Oil in the badlands + noth of it
        //    // if (quarry.transform.position.z > this.zoneBottom)
        //    // {
        //    //     if (item.info.displayName.english == this.oilName)
        //    //     {
        //    //         item.RemoveFromContainer();
        //    //         //item.Remove(0f);
        //    //     }
        //    // }
        //}

        //private bool IsInBadlands(UnityEngine.Component component)
        //{
        //    return component.transform.position.z > this.zoneBottom &&
        //           component.transform.position.z < this.zoneTop;
        //}

        //#endregion
        

        #region Player Levelling

        [ChatCommand("rates")]
        private void RatesCmd(BasePlayer player, string cmd, string[] args)
        {
            if (player != null)
            {
                var rates = this.GetRateMultipliers(player);

                this.SendReply(player, "Gather Rate Multipliers ---------");
                this.SendReply(player, $"Tree: {Math.Round(rates.Tree, 3)}x");
                this.SendReply(player, $"Ores: {Math.Round(rates.Ore, 3)}x");
                this.SendReply(player, $"Flesh: {Math.Round(rates.Flesh, 3)}x");
                this.SendReply(player, $"Other: {Math.Round(rates.Other, 3)}x");
            }
        }

        /// <summary>
        /// Increase gather rates from trees, ores, animals, etc
        /// </summary>
        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            var player = entity.ToPlayer();
            if (player == null) return;

            var rateMultipliers = this.GetRateMultipliers(player);
            float multiplier;

            switch (dispenser.gatherType)
            {
                case ResourceDispenser.GatherType.Tree:
                    multiplier = rateMultipliers.Tree;

                    // Increase gather rate
                    rateMultipliers.Tree = this.IncreaseRate(rateMultipliers.Tree);
                    break;

                case ResourceDispenser.GatherType.Ore:
                    multiplier = rateMultipliers.Ore;

                    // Increase gather rate
                    rateMultipliers.Ore = this.IncreaseRate(rateMultipliers.Ore);
                    break;

                case ResourceDispenser.GatherType.Flesh:
                    multiplier = rateMultipliers.Flesh;

                    // Increase gather rate
                    rateMultipliers.Flesh = this.IncreaseRate(rateMultipliers.Flesh);
                    break;

                default:
                    multiplier = rateMultipliers.Other;

                    // Increase gather rate
                    rateMultipliers.Other = this.IncreaseRate(rateMultipliers.Other);
                    break;
            }

            // Apply rate multiplier
            item.amount = Convert.ToInt32(item.amount * multiplier);

            // Save changes made to rate multipliers
            this.SaveRateMultipliers(player.userID, rateMultipliers);
        }

        /// <summary>
        /// Increase hemp gather rate -- doesn't seem to work
        /// </summary>
        private void OnPlantGather(PlantEntity plant, Item item, BasePlayer player)
        {
            if (player == null) return;

            var rateMultipliers = this.GetRateMultipliers(player);

            // Apply rate multiplier
            item.amount = Convert.ToInt32(item.amount * rateMultipliers.Plant);

            // Increase gather rate
            rateMultipliers.Plant = this.IncreaseRate(rateMultipliers.Plant);
            this.SaveRateMultipliers(player.userID, rateMultipliers);
        }

        /// <summary>
        /// Save player levelling progress to disk
        /// </summary>
        private void OnServerSave()
        {
            Interface.GetMod().DataFileSystem.WriteObject(this.dataFile, this.storedData);
        }

        /// <summary>
        /// Reset player progress on death
        /// </summary>
        private void OnPlayerRespawned(BasePlayer player)
        {
            if (player == null) return;

            var defaultRates = new RateMultipliers();
            this.SaveRateMultipliers(player.userID, defaultRates);

            this.SendReply(player, "You died! Your gather rate multiplers have been reset back to 1x");
        }


        private float IncreaseRate(float currentRate)
        {
            if (currentRate < 3f)
            {
                if (currentRate < 2f)
                {
                    // We're less than 2x, so increase quickly
                    currentRate += this.quickIncreaseRate;
                }
                else
                {
                    // We're between 2x and 3x, so increase slowly
                    currentRate += this.slowIncreaseRate;
                }
            }

            return currentRate;
        }

        private RateMultipliers GetRateMultipliers(BasePlayer player)
        {
            var data = this.storedData.Players.FirstOrDefault(p => p.UserId == player.userID);
            if (data != null)
            {
                return data.GatherMultipliers;
            }

            // Else, player is not found - so create a new entry for the player
            this.storedData.Players.Add(new PlayerGatherLevel(player, new RateMultipliers()));
            return new RateMultipliers();
        }

        private void SaveRateMultipliers(ulong userId, RateMultipliers rateMultipliers)
        {
            var data = this.storedData.Players.FirstOrDefault(p => p.UserId == userId);
            if (data != null)
            {
                data.GatherMultipliers = rateMultipliers;
            }
        }

        #endregion
        

        #region DataTable

        internal class StoredData
        {
            public HashSet<PlayerGatherLevel> Players = new HashSet<PlayerGatherLevel>();
        }

        internal class PlayerGatherLevel
        {
            public ulong UserId;
            public RateMultipliers GatherMultipliers;

            public PlayerGatherLevel()
            {
            }

            public PlayerGatherLevel(BasePlayer player, RateMultipliers gatherMultipliers)
            {
                this.UserId = player.userID;
                this.GatherMultipliers = gatherMultipliers;
            }
        }

        internal class RateMultipliers
        {
            public float Tree;
            public float Ore;
            public float Flesh;
            public float Plant;
            public float Other;

            public RateMultipliers()
            {
                this.Tree = 1f;
                this.Ore = 1f;
                this.Flesh = 1f;
                this.Plant = 1f;
                this.Other = 1f;
            }
        }

        #endregion
    }
}
