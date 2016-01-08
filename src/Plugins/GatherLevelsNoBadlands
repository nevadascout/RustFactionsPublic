namespace Oxide.Plugins
{
    using System;

    using Oxide.Plugins;

    [Info("Gather Levels No Badlands", "nevada_scout", "1.0")]
    public class GatherLevelsNoBadlands : RustPlugin
    {
        /// <summary>
        /// Increase gather rates from trees, ores, animals, etc
        /// </summary>
        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (entity.ToPlayer() == null) return;

            item.amount = Convert.ToInt32(item.amount * 2);
        }

        /// <summary>
        /// Increase hemp gather rate -- doesn't seem to work
        /// </summary>
        private void OnPlantGather(PlantEntity plant, Item item, BasePlayer player)
        {
            if (player == null) return;

            item.amount = Convert.ToInt32(item.amount * 2);
        }
    }
}
