namespace Oxide.Plugins
{
    [Info("SaveMessage", "Rust Factions", "1.0")]
    public class SaveMessage : RustPlugin
    {
        void OnServerSave()
        {
            this.PrintToChat("Server saving - expect some lag...");
        }
    }
}
