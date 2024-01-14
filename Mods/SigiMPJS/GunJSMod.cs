using MelonLoader;

public class GunJsMod : MelonMod
{
    private static GunJsServer gunJsServer;

    public override void OnApplicationStart()
    {
        MelonLogger.Msg("Gun.js Mod Loaded");

        // Get the user ID from C#
        string userId = GetUserId();

        // Start Gun.js server with the user ID
        gunJsServer = new GunJsServer(userId);
        gunJsServer.StartServer();

        SendMessage("123456", "Hello from C#!");
    }

    private void SendMessage(string userId, string message)
    {
        // Invoke JavaScript function to send messages
        engine?.Invoke("SendMessage", userId, message);
    }

    private string GetUserId()
    {
        // Your logic to obtain the user ID goes here
        // For simplicity, using a hardcoded user ID for now
        return "123456";
    }

    public override void OnUpdate()
    {
        // Read and print messages for the user ID
        string message = gunJsServer?.ReadMessage();
        if (!string.IsNullOrEmpty(message))
        {
            MelonLogger.Msg($"Received message: {message}");
        }
    }

    public override void OnApplicationQuit()
    {
        // Stop the Gun.js server when the game quits
        gunJsServer?.StopServer();
    }
}
