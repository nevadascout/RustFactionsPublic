namespace Oxide.Plugins
{
    using System;

    using Oxide.Plugins;

    [Info("Gather Levels 2", "nevada_scout", "1.0")]
    public class GatherLevels2 : RustPlugin
    {
        private readonly float badlandsTop = 184;
        private readonly float badlandsBottom = -402;

        /// <summary>
        /// Increase gather rates from trees, ores, animals, etc
        /// </summary>
        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            var player = entity.ToPlayer();
            if (player == null) return;

            if (this.IsInBadlands(player))
            {
                var random = new Random();
                var multiplier = random.Next(2, 3);
                item.amount = Convert.ToInt32(item.amount * multiplier);
            }
        }

        /// <summary>
        /// Increase hemp gather rate -- doesn't seem to work
        /// </summary>
        private void OnPlantGather(PlantEntity plant, Item item, BasePlayer player)
        {
            if (player == null) return;


            if (this.IsInBadlands(player))
            {
                var random = new Random();
                var multiplier = random.Next(2, 3);
                item.amount = Convert.ToInt32(item.amount * multiplier);
            }
        }

        private bool IsInBadlands(UnityEngine.Component component)
        {
            return component.transform.position.z > this.badlandsBottom &&
                   component.transform.position.z < this.badlandsTop;
        }
    }
}
