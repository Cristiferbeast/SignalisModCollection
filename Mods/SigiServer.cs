using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

class SigiServer
{

    private static TcpListener listener;
    private static NetworkStream stream;

    private static int maxConn = 1;
    private static int conn = 0;

    private static string receivedMessage = "";

    /*
        static void Main(string[] args)
        {
            StartServer();
        }
    */


    public static string GetMessage()
    {
        return receivedMessage;
    }

    public static async Task StartServer()
    {
        string serverIP = "0.0.0.0";
        int port = 3000;
        listener = new TcpListener(IPAddress.Parse(serverIP), port);
        listener.Start();
        Console.WriteLine("server started, listening on port 3000");

        while (true)
        {
            if (maxConn > conn)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Interlocked.Increment(ref conn);
                Console.WriteLine("Client connected: " + client.Client.RemoteEndPoint);
                stream = client.GetStream();
                _ = MsgHandler();
            }
        }
    }

    public static async Task ServerUpdate(string msg)
    {
        try
        {
            byte[] buffer = Encoding.ASCII.GetBytes(msg);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception error)
        {
            Console.WriteLine("error in ServerUpdate() -> " + error);
        }
    }

    private static async Task MsgHandler()
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                // Receive data from the client
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"msg received: {receivedMessage}");
            }
        }
        catch (Exception error)
        {
            Interlocked.Decrement(ref conn);
            Console.WriteLine("error in MsgHandler -> " + error);
        }
    }
}

