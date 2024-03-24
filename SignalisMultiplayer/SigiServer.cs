using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
public class SigiServer
{
    private readonly string IpAddress = "0.0.0.0";
    private readonly int port = 3000;
    private TcpListener TcpServer;
    private UdpClient UdpServer;
    private readonly List<string> MessageQueue = new List<string>();
    private readonly List<Player> CurrentPlayers = new List<Player>();
    private byte[] buffer;
    private byte IdAssigner = 1;

    private class Player
    {
        public byte PlayerID { get; }
        public TcpClient PlayerTcpClient { get; }
        public IPEndPoint PlayerUdpClient { get; }
        public string PlayerPosition { get; set; }
        public string PlayerRotation { get; set; }
        public Player(byte Id, TcpClient tcpClient, IPEndPoint udpClient)
        {
            PlayerTcpClient = tcpClient;
            PlayerUdpClient = udpClient;
            PlayerPosition = "placeholder";
            PlayerRotation = "placeholder";
            PlayerID = Id;
        }
        public Player()
        {
            PlayerPosition = "placeholder";
            PlayerRotation = "placeholder";
        }
    }


    /*******************

    SigiMPServer!!

    *******************/

    /* public static void Main(string[] args){
        SigiMPServer server = new SigiMPServer();
        server.StartServer();
        while(true){
            Thread.Sleep(1000);
            server.UdpServerUpdate("test");
            string test = server.GetPlayerPosition() + server.GetPlayerRotation();
            Console.WriteLine(test);
        }
    }  */

    // public functions!

    // returns the first message in the message queue and removes it.
    public string GetMessage()
    {
        if (MessageQueue != null)
        {
            string msg = MessageQueue.First();
            MessageQueue.RemoveAt(0);
            return msg;
        }
        else
        {
            return "";
        }
    }

    // returns the entire message queue and clears it.
    public List<string> GetMessageQueue()
    {
        List<string> queue = MessageQueue;
        MessageQueue.Clear();
        return queue;
    }

    public string GetPlayerPosition()
    {
        return CurrentPlayers[0].PlayerPosition;
    }

    public string GetPlayerRotation()
    {
        return CurrentPlayers[0].PlayerRotation;
    }

    public List<string> GetPlayerMovement()
    {
        return new List<string> {GetPlayerPosition(),GetPlayerRotation()};
    }

    public int GetPlayerCount()
    {
        return CurrentPlayers.Count();
    }


    // starts the server. default is set to '0.0.0.0' so it listens to all possible connections on port 3000.
    public async Task StartServer()
    {
        Console.WriteLine("server listening on port 3000");
        TcpServer = new TcpListener(IPAddress.Parse(IpAddress), port);
        UdpServer = new UdpClient(port);
        TcpServer.Start();
        
        // handle UDP messages
        _ = UdpMessageHandler();

        // handle incoming tcp connections. 
        while (true)
        {
            TcpClient client = await TcpServer.AcceptTcpClientAsync();
            Console.WriteLine("client connected!");

            // add client to both the tcp and udp list
            EstablishConnection(client);
        }
    }

    // sends a string to the client. messages start with the tilde (~) key to be parsed better.
    public void TcpServerUpdate(string msg)
    {
        if(TcpServer != null)
        {
            try
            {
                buffer = Encoding.ASCII.GetBytes(msg);
                foreach (Player player in CurrentPlayers)
                {
                    player.PlayerTcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("error in ServerUpdate() -> " + error);
            }
        }
    }
    public void UdpServerUpdate(string msg)
    {
        if (UdpServer != null)
        {
            try
            {
                buffer = Encoding.ASCII.GetBytes(msg);
                foreach (Player player in CurrentPlayers)
                {
                    UdpServer.SendAsync(buffer, buffer.Length, player.PlayerUdpClient);
                }

            }
            catch (Exception error)
            {
                Console.WriteLine("error in ServerUpdate() -> " + error);
            }
        }
        else
        {
            Console.WriteLine("server not started :(");
        }
    }

    /*
    private functions!!
    */

    // handles receiving udp messages. doesnt rly care where its from.
    private async Task UdpMessageHandler()
    {
        if (UdpServer != null)
        {
            try
            {
                Boolean connected = true;
                while (connected)
                {
                    UdpReceiveResult result = await UdpServer.ReceiveAsync();
                    string ReceivedMessage = Encoding.ASCII.GetString(result.Buffer);
                    AddMessageToQueue(ReceivedMessage);
                    // Console.WriteLine($"client msg received: {ReceivedMessage}");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("error in UdpMessageHandler() for server -> " + error);
            }
        }
        else
        {
            Console.WriteLine("server not started :(");
        }

    }

    // handles receiving tcp messages. one is open for each client.
    private async Task TcpMessageHandler(TcpClient Client, byte Id)
    {
        byte[] BufferForReceive = new byte[128];

        // dispose of client after using it
        using (Client)
        {
            NetworkStream Stream = Client.GetStream();
            try
            {
                Boolean connected = true;
                while (connected)
                {
                    BufferForReceive = new byte[128];
                    int BytesRead = await Stream.ReadAsync(BufferForReceive, 0, BufferForReceive.Length);

                    if (BytesRead != 0)
                    {
                        string ReceivedMessage = Encoding.ASCII.GetString(BufferForReceive, 0, BytesRead);
                        AddMessageToQueue(ReceivedMessage);
                        /* Console.WriteLine($"client msg received: {ReceivedMessage}"); */
                    }
                    else
                    {
                        connected = false;
                    }
                }
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("lost connection to the client. they might've disconnected!");
            }
            catch (Exception error)
            {
                Console.WriteLine("error in TcpMessageHandler() for server  -> " + error);
            }
            Stream.Close();
            foreach (Player player in CurrentPlayers)
            {
                if (player.PlayerID == Id)
                {
                    CurrentPlayers.Remove(player);
                    break;
                }
            }
        }
    }

    private void EstablishConnection(TcpClient client)
    {
        byte[] BufferForReceive = new byte[128];
        Console.WriteLine("establishing connection...");

        // get the ip address of the client who connected
        IPEndPoint ClientIPAddress = client.Client.RemoteEndPoint as IPEndPoint;
        try
        {
            if (ClientIPAddress != null && ClientIPAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                IPEndPoint ParsedClientIPAddress = new IPEndPoint(ClientIPAddress.Address, 3000);
                Console.WriteLine("connected from: " + ClientIPAddress.Address.ToString());
                CurrentPlayers.Add(new Player(IdAssigner, client, ParsedClientIPAddress));
                _ = TcpMessageHandler(client, IdAssigner);
                IdAssigner++;
            }
            else
            {
                Console.WriteLine("failed to retrieve client ip");
                client.Close();
            }
        }
        catch (Exception error)
        {
            Console.WriteLine("error in EstablishConnection() -> " + error);
        }
    }

    private void AddMessageToQueue(string RawMessage)
    {
        // we might not need this
        string[] ParsedMessage = RawMessage.Split('~');
        for (int i = 1; i < ParsedMessage.Length; i++)
        {
            // we might add more to this table soon.
            string Message = ParsedMessage[i];
            switch (Message[0])
            {
                case 'V':
                    CurrentPlayers[0].PlayerPosition = Message;
                    break;
                case 'Q':
                    CurrentPlayers[0].PlayerRotation = Message;
                    break;
            }
            MessageQueue.Add(ParsedMessage[i]);
        }
    }
}