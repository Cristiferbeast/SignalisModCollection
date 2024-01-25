using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class SigiClient
{
    // buffer for data transmission
    private const int BufferSize = 1024;
    private static readonly byte[] buffer = new byte[BufferSize];
    private static string response = "";
    private static string receivedMsg = "";

    // websocket, url and response
    private static ClientWebSocket clientSocket = new ClientWebSocket();
    private static CancellationTokenSource token = new CancellationTokenSource();

    /*
        Hii!! This is the client for SigiMP!

        StartClient(string url)
        * Used to start the client! Place the HOST'S IP into the argument and it'll attempt to connect!

        ClientUpdate(string test) 
        * If you want to sent an update to the server, use this function! Just place the string you want to send to the server as the argument.

        GetMessage()
        * If you want to get the last known message of the server, then use this!
        (i dunno if there's a better way to do this but this is all i got at the moment :sob:)

        Close the connection with the server using the DiconnectClient() function! It's good practice disconnect the client before stopping the program
        (though it's not required, the errors will be caught).
    */

    public static string GetMessage()
    {
        return receivedMsg;
    }


    public static async void StartClient(string url)
    {
        Uri hostUrl = new Uri("ws://" + url + ":3000");
        Console.WriteLine($"attempting to establish connection to {url}");
        clientSocket = new ClientWebSocket();
        try
        {
            await clientSocket.ConnectAsync(hostUrl, CancellationToken.None);
        }
        catch (Exception error)
        {
            Console.WriteLine("error: " + error);
        }

        await MsgHandlerAsync();
    }

    public static async void ClientUpdate(string test)
    {
        response = test;
        try
        {
            if (clientSocket.State == WebSocketState.Open)
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

    public static void Disconnect()
    {
        clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", CancellationToken.None);
    }

    private static async Task SendMsgAsync(String msg)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(msg);
        await clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        Console.WriteLine($"sent message: {msg}");
    }

    private static async Task MsgHandlerAsync()
    {
        try
        {
            // Perform this while the connection is open
            ClientUpdate("omg haiii!!!!!");
            while (clientSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    receivedMsg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"received message: {receivedMsg}");
                }
            }
            // Connection lost!
            await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", CancellationToken.None);
            Console.WriteLine("connection closed");
        }
        catch (WebSocketException error)
        {
            // Error happened, handle it
            Console.WriteLine("error:" + error);
        }
    }
}
