namespace Oxide.Plugins
{
    [Info("Rednut", "Rust Factions", "1.0")]
    public class Rednut : RustPlugin
    {
        [ChatCommand("rednut")]
        private void SayMessage(BasePlayer player, string cmd, string[] args)
        {
            this.SendReply(player, "REDNUT STATUS: Still in the badlands");
        }
    }
}
