namespace Oxide.Plugins
{
    using System;
    
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
                item.amount = Convert.ToInt32(item.amount * 3);
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
                item.amount = Convert.ToInt32(item.amount * 3);
            }
        }

        private bool IsInBadlands(UnityEngine.Component component)
        {
            return component.transform.position.z > this.badlandsBottom &&
                   component.transform.position.z < this.badlandsTop;
        }
    }
}
