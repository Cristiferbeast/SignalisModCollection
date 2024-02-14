using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

public class SigiServer
{

    private TcpListener listener;
    private NetworkStream stream;

    private int maxConn = 1;
    private int conn = 0;

    private string receivedMessage = "";

    public string GetMessage()
    {
        return receivedMessage;
    }

    public async Task StartServer()
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
                Console.WriteLine("client connected!");
                stream = client.GetStream();
                _ = MsgHandler();
            }
        }
    }

    public async Task ServerUpdate(string msg)
    {
        if (stream != null)
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
    }

    private async Task MsgHandler()
    {
        byte[] buffer = new byte[1024];
        try
        {
            Boolean connected = true;
            while (connected)
            {
                // Receive data from the client
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                // Check if stream disconnects. If not, get the message.
                if (bytesRead != 0)
                {
                    receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //Console.WriteLine($"msg received: {receivedMessage}");
                }
                else
                {
                    Console.WriteLine("Disconnected");
                    connected = false;
                }
            }
        }
        catch (Exception error)
        {
            Interlocked.Decrement(ref conn);
            Console.WriteLine("error in MsgHandler -> " + error);
        }
    }
}
