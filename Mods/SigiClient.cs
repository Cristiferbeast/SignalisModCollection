using MelonLoader;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
public class SigiClient
{
    private NetworkStream stream;
    private string receivedMessage = "";

    public string GetMessage()
    {
        return receivedMessage;
    }

    public void StartClient(string url)
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

    private async Task MsgHandler(NetworkStream str)
    {
        byte[] buffer = new byte[1024];
        try
        {
            Boolean connected = true;
            while (connected)
            {
                int bytesRead = await str.ReadAsync(buffer, 0, buffer.Length);
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
            Console.WriteLine("error in MsgHandler() -> " + error);
        }

    }

    public async Task ClientUpdate(string msg)
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

    public async Task DisconnectClient()
    {
        if (stream != null)
        {
            try
            {
                stream.Close();
            }
            catch (Exception error)
            {
                Console.WriteLine("error in DisconnectClient() -> " + error);
            }
        }
    }
}