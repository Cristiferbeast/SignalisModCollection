using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Net;
public class SigiClient
{
    private int port = 3000;
    private TcpClient TcpMainClient;
    private UdpClient UdpMainClient;
    private readonly List<string> MessageQueue = new List<string>();
    private readonly List<Player> CurrentPlayers = new List<Player>();
    private byte[] buffer = new byte[128];

    private class Player
    {
        public byte PlayerID { get; }
        public string PlayerPosition { get; set; }
        public string PlayerRotation { get; set; }
        public Player(byte Id)
        {
            PlayerPosition = "";
            PlayerRotation = "";
            PlayerID = Id;
        }
    }

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
    public List<string> GetMessageQueue()
    {
        List<string> queue = MessageQueue;
        MessageQueue.Clear();
        return queue;
    }

    // returns the entire message queue and clears it.
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
        return new List<string> { GetPlayerPosition(), GetPlayerRotation() };
    }

    // starts the client. takes one argument, which is the IP of the host. connects to port 3000.
    public void StartClient(string url)
    {
        TcpMainClient = new TcpClient(url, port);
        UdpMainClient = new UdpClient(port);
        UdpMainClient.Connect(url, port);

        //add host to players
        CurrentPlayers.Add(new Player(0));

        try
        {
            _ = UdpMessageHandler();
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
                buffer = Encoding.ASCII.GetBytes(msg);
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
        if (UdpMainClient != null)
        {
            try
            {
                buffer = Encoding.ASCII.GetBytes(msg);
                UdpMainClient.SendAsync(buffer, buffer.Length);
            }
            catch (Exception error)
            {
                Console.WriteLine("error in UdpServerUpdate() -> " + error);
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

    // handles receiving messages. always running until client disconnects.
    private async Task UdpMessageHandler()
    {
        byte[] BufferForReceive = new byte[128];
        if (UdpMainClient != null)
        {
            try
            {
                Boolean connected = true;
                while (connected)
                {
                    BufferForReceive = new byte[128];
                    UdpReceiveResult result = await UdpMainClient.ReceiveAsync();
                    string ReceivedMessage = Encoding.ASCII.GetString(result.Buffer);
                    AddMessageToQueue(ReceivedMessage);
                    // Console.WriteLine($"server msg received: {ReceivedMessage}");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("error in UdpMessageHandler() for client -> " + error);
            }
        }
    }

    // handles receiving udp messages. always running until client disconnects.
    private async Task TcpMessageHandler(TcpClient Client)
    {
        byte[] BufferForReceive = new byte[128];
        NetworkStream stream = Client.GetStream();
        using (Client)
        {
            try
            {
                Boolean connected = true;
                while (connected)
                {
                    BufferForReceive = new byte[128];
                    int BytesRead = await stream.ReadAsync(BufferForReceive, 0, BufferForReceive.Length);
                    if (BytesRead != 0)
                    {
                        string ReceivedMessage = Encoding.ASCII.GetString(BufferForReceive, 0, BytesRead);
                        AddMessageToQueue(ReceivedMessage);
                        // Console.WriteLine($"server msg received: {ReceivedMessage}");
                    }
                    else
                    {
                        connected = false;
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
            Client.Close();
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