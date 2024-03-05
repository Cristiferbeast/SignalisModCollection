using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
public class SigiServer
{
    private string IpAddress = "0.0.0.0";
    static private int port = 3000;
    private TcpListener TcpServer;
    private List<TcpClient> TcpClients = new List<TcpClient>();
    private UdpClient UdpServer = new UdpClient(port);
    private List<IPEndPoint> UdpClients = new List<IPEndPoint>();
    private int conn = 0;
    private LinkedList<string> MessageQueue = new LinkedList<string>();
    private byte[] buffer;


    /*******************

    SigiMPServer!!

    *******************/

    /* public static void Main(string[] args){
        SigiMPServer server = new SigiMPServer();
        server.StartServer();
        while(true){
            Thread.Sleep(1000);
            server.UdpServerUpdate("test");
            Console.WriteLine(server.GetMessage());
        }
    } */

    // public functions!

    // returns the first message in the message queue and removes it.
    public string GetMessage()
    {
        if (MessageQueue.First != null)
        {
            string msg = MessageQueue.First.Value;
            MessageQueue.RemoveFirst();
            return msg;
        }
        else
        {
            return "";
        }
    }

    // returns the entire message queue and clears it.
    public LinkedList<string> GetMessageQueue()
    {
        LinkedList<string> queue = MessageQueue;
        MessageQueue = new LinkedList<string>();
        return queue;
    }

    // starts the server. default is set to '0.0.0.0' so it listens to all possible connections on port 3000.
    public async Task StartServer()
    {
        Console.WriteLine("server listening on port 3000");
        TcpServer = new TcpListener(IPAddress.Parse(IpAddress), port);
        TcpServer.Start();

        // handle UDP messages
        _ = UdpMessageHandler();

        // handle incoming tcp connections. 
        while (true)
        {
            // finds a connection
            TcpClient client = await TcpServer.AcceptTcpClientAsync();
            Interlocked.Increment(ref conn);
            Console.WriteLine("client connected!");

            // add client to both the tcp and udp list
            EstablishConnection(client);
            _ = TcpMessageHandler(client);

        }
    }

    // sends a string to the client. messages start with the tilde (~) key to be parsed better.
    public void TcpServerUpdate(string msg)
    {
        try
        {
            buffer = Encoding.ASCII.GetBytes("~" + msg);
            foreach (TcpClient client in TcpClients)
            {
                client.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
        }
        catch (Exception error)
        {
            Console.WriteLine("error in ServerUpdate() -> " + error);
        }
    }
    public void UdpServerUpdate(string msg)
    {
        try
        {
            buffer = Encoding.ASCII.GetBytes("~" + msg);
            foreach (IPEndPoint client in UdpClients)
            {
                UdpServer.SendAsync(buffer, buffer.Length, client);
            }
        }
        catch (Exception error)
        {
            Console.WriteLine("error in ServerUpdate() -> " + error);
        }
    }

    /*
    private functions!!
    */

    // handles receiving udp messages. doesnt rly care where its from.
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
                UdpReceiveResult result = await UdpServer.ReceiveAsync();
                string receivedMessage = Encoding.ASCII.GetString(result.Buffer);
                /* Get the data, split it into their different messages using the tilde key,
                then add it into the message queue */
                string[] parsedMessage = receivedMessage.Split('~');
                // first msg is ignored bc it's probably an empty string
                for (int i = 1; i < parsedMessage.Length; i++)
                {
                    MessageQueue.AddLast(parsedMessage[i]);
                }
                Console.WriteLine($"client msg received: {receivedMessage}");
            }
        }
        catch (Exception error)
        {
            Interlocked.Decrement(ref conn);
            Console.WriteLine("error in UdpMessageHandler() for server -> " + error);
        }
    }

    // handles receiving tcp messages. one is open for each client.
    private async Task TcpMessageHandler(TcpClient Client)
    {
        byte[] bufferRec = new byte[128];

        // dispose of client after using it
        using (Client)
        {
            NetworkStream Stream = Client.GetStream();
            try
            {
                Boolean connected = true;
                while (connected)
                {
                    bufferRec = new byte[128];
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
                        /* Console.WriteLine($"client msg received: {receivedMessage}"); */
                    }
                    else
                    {
                        connected = false;
                        Stream.Close();
                    }
                }
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("lost connection to the client. they might've disconnected!");
            }
            catch (Exception error)
            {
                Interlocked.Decrement(ref conn);
                Console.WriteLine("error in TcpMessageHandler() for server  -> " + error);
            }
            Stream.Close();
        }
    }

    private void EstablishConnection(TcpClient client)
    {
        byte[] bufferRec = new byte[128];
        Console.WriteLine("establishing connection...");

        // get the ip address of the client who connected
        IPEndPoint remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        try
        {
            // if the ip address isn't null and it's a viable ip address, add to it the udp list.
            if (remoteEndPoint != null && remoteEndPoint.AddressFamily == AddressFamily.InterNetwork)
            {
                // if you're testing sending udp to programs within your pc, set this to 3001
                IPEndPoint endpoint = new IPEndPoint(remoteEndPoint.Address, 3000);
                Console.WriteLine("connected from: " + remoteEndPoint.Address.ToString());
                TcpClients.Add(client);
                UdpClients.Add(endpoint);
            }
            else
            {
                Console.WriteLine("failed to retrieve client ip");
            }
        }
        catch (Exception error)
        {
            Console.WriteLine("error in EstablishConnection() -> " + error);
        }
    }
}