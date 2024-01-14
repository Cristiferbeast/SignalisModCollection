using System;
using Microsoft.ClearScript.V8;
using System.Threading;

public class GunJsServer
{
    private V8ScriptEngine engine;
    private Thread serverThread;
    private string userId;
    private bool isServerRunning;

    public GunJsServer(string userId)
    {
        this.userId = userId;
        engine = new V8ScriptEngine();
    }

    public void StartServer()
    {
        isServerRunning = true;
        engine.ExecuteFile("path/to/your/bundledDependencies.js");

        // Start a new thread for the Gun.js server
        serverThread = new Thread(() =>
        {
            // Load and execute the Gun.js server setup file
            engine.ExecuteFile("path/to/your/gun_server.js");
        });

        serverThread.Start();
    }

    public string ReadMessage()
    {
        // Invoke JavaScript function to read messages for the user ID
        return engine?.Invoke<string>("ReadMessage", userId);
    }

    public void StopServer()
    {
        // Stop the server thread and clean up resources
        isServerRunning = false;
        serverThread?.Join();
        engine?.Dispose();
    }
}
