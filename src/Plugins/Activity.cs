namespace Oxide.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Newtonsoft.Json.Linq;

    using Oxide.Core;
    using Oxide.Core.Plugins;

    [Info("Activity", "Rust Factions", "1.2")]
    public class Activity : RustPlugin
    {
        [PluginReference("Clans")]
        private Plugin clansApi;
        private StoredData storedData;
        private string dataFile = "activity.data";

        private void Loaded()
        {
            this.storedData = Interface.GetMod().DataFileSystem.ReadObject<StoredData>(this.dataFile);

            if (this.storedData.Players == null) this.storedData.Players = new HashSet<PlayerActivity>();
        }

        private void OnServerSave()
        {
            Interface.GetMod().DataFileSystem.WriteObject(this.dataFile, this.storedData);
        }

        [ChatCommand("status")]
        private void SayMessage(BasePlayer player, string cmd, string[] args)
        {
            if (args.Length == 2)
            {
                var type = args[0];
                var playerOrClan = args[1];

                string activityData;

                if (type == "clan" || type == "c")
                {
                    activityData = this.GetClanActivity(playerOrClan);
                    this.SendReply(player, activityData);
                }
                else if (type == "player" || type == "p")
                {
                    activityData = this.GetPlayerActivity(playerOrClan);
                    this.SendReply(player, activityData);
                }
                else
                {
                    this.SendReply(player, "Unrecognised format. Valid command format: /status clan \"TAG\" or /status player \"Player Name\"");
                }
            }
            else
            {
                this.SendReply(player, "Unrecognised format. Valid command format: '/status clan \"TAG\"' or '/status player \"Player Name\"'");
            }
        }

        private string GetPlayerActivity(string name)
        {
            var playerId = this.GetUserIdByNameFromAllUsers(name);

            if (playerId != 0)
            {
                // Player
                var playerName = this.GetUserNameByIdFromAllUsers(playerId);
                var playerData = this.storedData.Players.FirstOrDefault(p => p.UserId == playerId);
                if (playerData != null)
                {
                    return $"Player <color=#ffebcd>{playerName}</color> was last active at {playerData.LastActive} UTC";
                }

                return $"No activity data found for player <color=#ffebcd>{playerName}</color>";
            }

            return $"Player <color=#ffebcd>{name}</color> not found";
        }

        private string GetClanActivity(string clanTag)
        {
            // To count active clan members in last 72 hours (3 days)
            var activityLimitTime = DateTime.UtcNow.AddDays(-3);
            var clan = this.clansApi.Call("GetClan", clanTag) as JObject;

            if (clan == null)
            {
                return $"Faction <color=#ffebcd>{clanTag}</color> not found";
            }

            var playerCount = 0;
            var playerList = new List<string>();

            foreach (var member in clan["members"])
            {
                var memberId = Convert.ToUInt64(member);

                // Check player last active time
                var playerData = this.storedData.Players.FirstOrDefault(p => p.UserId == memberId);
                if (playerData != null)
                {
                    if (playerData.LastActive > activityLimitTime)
                    {
                        playerCount++;
                        playerList.Add(this.GetUserNameByIdFromAllUsers(memberId));
                    }
                }
            }

            var formattedPlayerList = new StringBuilder();

            foreach (var player in playerList)
            {
                formattedPlayerList.Append(player);
                formattedPlayerList.Append(", ");
            }

            if (formattedPlayerList.Length >= 2)
            {
                formattedPlayerList.Remove(formattedPlayerList.Length - 2, 2);
            }

            return $"Faction <color=#ffebcd>{clan["tag"]}</color> had {playerCount} members online in the last 72 hours ({formattedPlayerList})";

            //return $"Faction <color=#ffebcd>{clan["tag"]}</color> had {playerCount} members online in the last 72 hours";

            // NOTE: Near the bottom: http://oxidemod.org/threads/rust-io-clans.7292/page-6
        }

        #region Hooks

        private void OnPlayerInit(BasePlayer player)
        {
            var playerFound = this.storedData.Players.FirstOrDefault(p => p.UserId == player.userID);
            if (playerFound != null)
            {
                playerFound.LastActive = DateTime.UtcNow;
            }
            else
            {
                // Store player login time
                this.storedData.Players.Add(new PlayerActivity(player, DateTime.UtcNow));
            }
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            var playerFound = this.storedData.Players.FirstOrDefault(p => p.UserId == player.userID);
            if (playerFound != null)
            {
                playerFound.LastActive = DateTime.UtcNow;
            }
            else
            {
                // Store player logout time
                this.storedData.Players.Add(new PlayerActivity(player, DateTime.UtcNow));
            }
        }

        #endregion

        private ulong GetUserIdByNameFromAllUsers(string userName)
        {
            var onlinePlayers = BasePlayer.activePlayerList;
            var sleepingPlayers = BasePlayer.sleepingPlayerList;
            var allPlayers = onlinePlayers.Concat(sleepingPlayers).ToList();

            var player = allPlayers.FirstOrDefault(p => p.displayName.Contains(userName));

            return player?.userID ?? 0;
        }

        private string GetUserNameByIdFromAllUsers(ulong userId)
        {
            var onlinePlayers = BasePlayer.activePlayerList;
            var sleepingPlayers = BasePlayer.sleepingPlayerList;
            var allPlayers = onlinePlayers.Concat(sleepingPlayers).ToList();

            var player = allPlayers.FirstOrDefault(p => p.userID == userId);

            return player?.displayName ?? "Unknown player";
        }

        #region DataTable

        internal class StoredData
        {
            public HashSet<PlayerActivity> Players = new HashSet<PlayerActivity>();
        }

        internal class PlayerActivity
        {
            public ulong UserId;
            public DateTime LastActive;

            public PlayerActivity()
            {
            }

            public PlayerActivity(BasePlayer player, DateTime lastActive)
            {
                this.UserId = player.userID;
                this.LastActive = lastActive;
            }
        }

        #endregion
    }
}
