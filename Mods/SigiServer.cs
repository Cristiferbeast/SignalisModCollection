using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class SigiServer
{
    // number of connections and maximum connections
    private static int conn = 0;
    private static int maxConn = 1;

    // buffer for data transmission
    private const int BufferSize = 1024;
    private static readonly byte[] responseBuffer = new byte[BufferSize];
    private static readonly byte[] receiveBuffer = new byte[BufferSize];

    // websocket, url, and response
    private static string url = "";
    private static string response = "";
    private static string receivedMsg = "";
    private static HttpListener listener = new HttpListener();
    private static WebSocket socket;

    /*
        Hii!! This is the server for SigiMP!

        StartServer() 
        * Used to start the client! It starts a server on localhost on port 3000! This can be adjusted by changing the url string.

        ServerUpdate(string test)
        * If you want to sent an update to all clients, use this! Just place the string you want to send to the clients as the argument.

        GetMessage()
        * If you want to get the last known message of a client, then use this!
        (i dunno if there's a better way to do this but this is all i got at the moment :sob:)

        SetMaxUsers(int newMax)
        * If you want to change the max amount of connected users, use this function!

        GetMaxUsers()
        * Alternatively, you can get the maximum allowed users with this function! Returns an int value.

        DisconnectServer()
        * Close the connection with the client using this function! It's good practice to close the server before closing the program 
        (though it's not required, the errors will be caught).
    */


    public static string GetMessage()
    {
        return receivedMsg;
    }

    public static async void StartServer()
    {
        url = "http://localhost:3000/";
        listener.Prefixes.Add(url);
        listener.Start();

        Console.WriteLine($"websocket server running on {url}");

        // run all the time
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            await HandshakeHandlerAsync(context);
        }
    }

    // Changes the max amount of players allowed to connect to server
    public static void SetMaxUsers(int newMax)
    {
        if (newMax >= maxConn && maxConn >= conn)
        {
            maxConn = newMax;
        }
        else
        {
            Console.WriteLine("you can't set the maximum amount of users below the currently connected users");
        }
    }

    public static int GetMaxUsers()
    {
        return maxConn;
    }

    public static async void ServerUpdate(string test)
    {
        response = test;
        try
        {
            if (socket != null && socket.State == WebSocketState.Open)
            {
                await SendMsgAsync(response);
            }
            else
            {
                Console.WriteLine("no established connection");
            }
        }
        catch (Exception error)
        {
            Console.WriteLine("error: " + error);
        }
    }

    public static void DisconnectServer()
    {
        try
        {
            if (socket != null && socket.State == WebSocketState.Open)
            {
                socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", CancellationToken.None);
            }
            else
            {
                Console.WriteLine("no established connection");
            }
        }
        catch (Exception error)
        {
            Console.WriteLine("error: " + error);
        }
    }

    private static async Task SendMsgAsync(string response)
    {
        byte[] responseBuffer = Encoding.UTF8.GetBytes(response);
        if (socket != null)
        {
            try
            {
                await socket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception error)
            {
                Console.Write("error:" + error);
            }
        }
    }

    // Handles the handshake function
    private static async Task HandshakeHandlerAsync(HttpListenerContext context)
    {
        // If it's a websocket request and there is lobby space, accept it.
        if (maxConn > conn && context.Request.IsWebSocketRequest)
        {
            Interlocked.Increment(ref conn);
            try
            {
                // Get the websocket info of the listener and only pass the WebSocket to the message handler
                HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                socket = webSocketContext.WebSocket;
                await MsgHandlerAsync(socket);
            }
            catch (Exception error)
            {
                Console.WriteLine("error: " + error);
            }
        }
        else if (!context.Request.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400; // 400 Bad Request (You're not a WebSocket!!! Get out!!!!)
            context.Response.Close();
        }
        else
        {
            context.Response.StatusCode = 503; // 503 Service Unavailable (Max allowed connections!!! Get out!!!!)
            context.Response.Close();
        }
    }

    // Handles message receiving
    private static async Task MsgHandlerAsync(WebSocket webSocket)
    {
        try
        {
            // Perform this while the connection is open
            ServerUpdate("omg haii!!");
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    response = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                    Console.WriteLine($"received message: {response}");
                }
            }
            // Connection lost, reduce connected count
            Interlocked.Decrement(ref conn);
            Console.WriteLine("connection closed");
        }
        catch (WebSocketException error)
        {

            // Error happened, handle it and reduce connect count
            Console.WriteLine("error:" + error);
            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Internal Server Error", CancellationToken.None);
            Interlocked.Decrement(ref conn);
        }
    }

}



