namespace Oxide.Plugins
{
    using System;

    [Info("Gather Rates", "nevada_scout", "1.0")]
    public class GatherRates : RustPlugin
    {
        /// <summary>
        /// Increase gather rates from trees, ores, animals, etc
        /// </summary>
        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            var player = entity.ToPlayer();
            if (player == null) return;

            item.amount = Convert.ToInt32(item.amount * 2);
        }

        private void OnPlantGather(PlantEntity plant, Item item, BasePlayer player)
        {
            if (player == null) return;

            item.amount = Convert.ToInt32(item.amount * 2);
        }

        private void OnCollectiblePickup(Item item, BasePlayer player)
        {
            if (player == null) return;
            
            item.amount = Convert.ToInt32(item.amount * 2);
        }

        private void OnCropGather(PlantEntity plant, Item item, BasePlayer player)
        {
            if (player == null) return;

            item.amount = Convert.ToInt32(item.amount * 2);
        }

        void OnContainerDropItems(ItemContainer container)
        {
            foreach (var item in container.itemList)
            {
                if (item.info.shortname == "scrap")
                {
                    item.amount = item.amount * 5;
                }
            }
        }
    }
}