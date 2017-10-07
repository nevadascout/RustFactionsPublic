namespace Oxide.Plugins
{
    using System.Linq;

    using UnityEngine;

    [Info("Radtowns", "Rust Factions", "1.0")]
    public class Radtowns : RustPlugin
    {
        private void OnServerInitialized()
        {
            //var monuments = UnityEngine.Object.FindObjectsOfType<GameObject>().Where(p => p.name.ToLower().Contains("monument")).ToArray();

            //this.Puts($"Found {monuments.Length} monuments on the map.");

            //foreach (var monument in monuments)
            //{
            //    this.Puts($"Found monument {monument.name} at {monument.transform.position}");
            //}

            // TODO
            // / Find out which radtowns need KOS zones (5 radtowns, 1 sphere, 1 airfield, 3 warehouses, 1 satellite dish ?)\
            // / Find out how to spawn walls
            // - Find out how to set rotation of walls to face center of zone
            // - Set no-build areas
            // - Set warning message in chat/GUI overlay
            // - Ensure this is only run the first time the server starts up after a wipe - otherwise the server will lag to hell
        }

        [ChatCommand("dist")]
        private void tpToCoord(BasePlayer player, string cmd, string[] args)
        {
            var position = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
            
            var distance = Vector3.Distance(player.transform.position, position);

            this.SendReply(player, distance.ToString());
        }
        
        [ChatCommand("loc")]
        private void showLoc(BasePlayer player, string cmd, string[] args)
        {
            this.SendReply(player, player.transform.position.ToString());
        }

        [ChatCommand("tp")]
        private void tpCoord(BasePlayer player, string cmd, string[] args)
        {
            this.rust.ForcePlayerPosition(player, float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
            player.Teleport(player);
        }

        [ChatCommand("wall")]
        private void spawnWall(BasePlayer player, string cmd, string[] args)
        {
            var pos = player.transform.position;
            pos.z += 2;

            var angles = new Quaternion();

            var prefab = GameManager.server.CreatePrefab("assets/prefabs/building/wall.external.high.stone/wall.external.high.stone.prefab", pos, angles, true);
            if (prefab == null)
            {
                this.SendReply(player, "Prefab is null");
                return;
            }

            var block = prefab.GetComponent<BuildingBlock>();
            if (block == null)
            {
                this.SendReply(player, "Building block is null");
                return;
            }

            block.transform.position = pos;
            block.transform.rotation = angles;
            block.gameObject.SetActive(true);
            block.blockDefinition = PrefabAttribute.server.Find<Construction>(block.prefabID);
            block.Spawn();
            block.health = block.MaxHealth();

            block.SendNetworkUpdate();
        }
    }
}
