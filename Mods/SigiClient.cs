using MelonLoader;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

class SigiClient
{
    private static NetworkStream stream;
    private static string receivedMessage = "";
    /*
        static async Task Main(string[] args)
        {
            string hostIP = "127.0.0.1";
            StartClient(hostIP);
        }
    */

    public static string GetMessage()
    {
        return receivedMessage;
    }

    public static void StartClient(string url)
    {
        TcpClient client = new TcpClient(url, 3000);
        Console.WriteLine("server connected!!!");

        try
        {
            stream = client.GetStream();
            _ = MsgHandler(stream);

        }
        catch (Exception error)
        {
            Console.WriteLine("error in StartClient() -> " + error);
        }
    }

    private static async Task MsgHandler(NetworkStream str)
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int bytesRead = await str.ReadAsync(buffer, 0, buffer.Length);
            receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"msg received: {receivedMessage}");
        }
    }

    public static async Task ClientUpdate(string msg)
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

    public static async Task DisconnectClient()
    {
        if (stream != null)
        {
            try
            {
                stream.Close();
            }
            catch (Exception error)
            {
                Console.WriteLine("error in ServerUpdate() -> " + error);
            }
        }
    }
}