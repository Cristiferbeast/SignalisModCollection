using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Generic;
public class SigiClient
{
    static private int port = 3000;
    private TcpClient TcpMainClient;
    private NetworkStream Stream;

    // if youre testing sending udp between programs within your pc, change this to port+1
    private UdpClient UdpMainClient = new UdpClient(port);
    private LinkedList<string> MessageQueue = new LinkedList<string>();
    private byte[] buffer = new byte[128];

    /*******************

    SigiMPClient!!

    *******************/

    /* public static void Main(string[] args){
        SigiMPClient client = new SigiMPClient();
        client.StartClient("25.59.124.148");
        while(true){
            Thread.Sleep(1000);
            client.UdpClientUpdate("test");
            Console.WriteLine(client.GetMessage());
        }
    } */

    /*
    public functions!
    */

    // returns the first message in the message queue and removes it.
    public LinkedList<string> GetMessageQueue()
    {
        LinkedList<string> queue = MessageQueue;
        MessageQueue = new LinkedList<string>();
        return queue;
    }

    // returns the entire message queue and clears it.
    public string GetMessage()
    {
        if (MessageQueue.First != null)
        {
            string msg = MessageQueue.Last.Value;
            /* MessageQueue.RemoveFirst(); */
            MessageQueue = new LinkedList<string>();
            return msg;
        }
        else
        {
            return "";
        }
    }

    // starts the client. takes one argument, which is the IP of the host. connects to port 3000.
    public void StartClient(string url)
    {
        int port = 3000;
        TcpMainClient = new TcpClient(url, port);
        UdpMainClient.Connect(url, port);
        _ = UdpMessageHandler();
        try
        {
            _ = TcpMessageHandler(TcpMainClient);
        }
        catch (Exception error)
        {
            Console.WriteLine("error in StartClient() -> " + error);
        }
        Console.WriteLine("listening on port 3000");
    }

    // sends a string to the client. messages start with the tilde (~) key to be parsed better.
    public void TcpClientUpdate(string msg)
    {
        if (TcpMainClient != null && TcpMainClient.GetStream() != null)
        {
            try
            {
                buffer = Encoding.ASCII.GetBytes("~" + msg);
                TcpMainClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception error)
            {
                Console.WriteLine("error in TcpServerUpdate() -> " + error);
            }
        }
    }
    public void UdpClientUpdate(string msg)
    {
        try
        {
            buffer = Encoding.ASCII.GetBytes("~" + msg);
            UdpMainClient.SendAsync(buffer, buffer.Length);
        }
        catch (Exception error)
        {
            Console.WriteLine("error in UdpServerUpdate() -> " + error);
        }
    }

    /*
    private functions!!
    */

    // handles receiving messages. always running until client disconnects.
    private async Task UdpMessageHandler()
    {
        byte[] bufferRec = new byte[128];
        try
        {
            Boolean connected = true;
            while (connected)
            {
                bufferRec = new byte[128];
                // Receive data from the client
                UdpReceiveResult result = await UdpMainClient.ReceiveAsync();
                string receivedMessage = Encoding.ASCII.GetString(result.Buffer);
                /* Get the data, split it into their different messages using the tilde key,
                then add it into the message queue */
                string[] parsedMessage = receivedMessage.Split('~');
                // first msg is ignored bc it's probably an empty string
                for (int i = 1; i < parsedMessage.Length; i++)
                {
                    MessageQueue.AddLast(parsedMessage[i]);
                }
                Console.WriteLine($"server msg received: {receivedMessage}");
            }
        }
        catch (Exception error)
        {
            Console.WriteLine("error in UdpMessageHandler() for client -> " + error);
        }
    }

    private async Task TcpMessageHandler(TcpClient Client)
    {
        byte[] bufferRec = new byte[128];
        Stream = Client.GetStream();
        using (Client)
        {
            try
            {
                Boolean connected = true;
                while (connected)
                {
                    bufferRec = new byte[128];
                    // Receive data from the client
                    int bytesRead = await Stream.ReadAsync(bufferRec, 0, bufferRec.Length);
                    if (bytesRead != 0)
                    {
                        // Get the data, split it into their different messages using the tilde key,
                        // then add it into the message queue
                        string receivedMessage = Encoding.ASCII.GetString(bufferRec, 0, bytesRead);
                        string[] parsedMessage = receivedMessage.Split('~');
                        // first msg is ignored bc it's probably an empty string
                        for (int i = 1; i < parsedMessage.Length; i++)
                        {
                            MessageQueue.AddLast(parsedMessage[i]);
                        }
                        Console.WriteLine($"server msg received: {receivedMessage}");
                    }
                    else
                    {
                        connected = false;
                        Stream.Close();
                    }
                }
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("lost connection with server. they might've shut down the server!");
            }
            catch (Exception error)
            {
                Console.WriteLine("error in TcpMessageHandler() ->" + error);
            }
            Stream.Close();
        }
    }
}