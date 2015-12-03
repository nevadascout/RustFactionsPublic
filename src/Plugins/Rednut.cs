namespace Oxide.Plugins
{
    [Info("Rednut", "Rust Factions", "1.0")]
    public class Rednut : RustPlugin
    {
        [ChatCommand("rednut")]
        private void SayMessage(BasePlayer player, string cmd, string[] args)
        {
            this.PrintToChat($"{player.displayName} is looking for rednut...");
        }

        [ChatCommand("hello")]
        private void SayHello(BasePlayer player, string cmd, string[] args)
        {
            this.PrintToChat($"{player.displayName} says 'Hello?'");
        }
    }
}
