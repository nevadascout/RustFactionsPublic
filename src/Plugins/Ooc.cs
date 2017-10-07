namespace Oxide.Plugins
{
    [Info("Ooc", "Rust Factions", "1.0")]
    public class Ooc : RustPlugin
    {
        [ChatCommand("o")]
        private void OCmd(BasePlayer player, string cmd, string[] args)
        {
            this.DisplayMessage(player, args);
        }
        [ChatCommand("ooc")]
        private void OocCmd(BasePlayer player, string cmd, string[] args)
        {
            this.DisplayMessage(player, args);
        }

        private void DisplayMessage(BasePlayer player, string[] args)
        {
            var message = "[OOC] " + string.Join(" ", args);

            this.rust.BroadcastChat(player.displayName, message);

            //this.SendReply(player, "Rust RP", message);
            //this.PrintToChat($"<color=#ffcd80>{message}</color>");
            //ConsoleSystem.Run.Server.Normal($"echo [CHAT] {player.displayName}: {message}");

            ConsoleSystem.Run(ConsoleSystem.Option.Server, $"echo [CHAT] {player.displayName}: {message}");
        }
    }
}
