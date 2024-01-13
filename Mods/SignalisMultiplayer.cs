using MelonLoader;
using Lidgren.Network;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Net;
using System.Threading;

namespace SigiMultiplayer
{
    public class Storage
    {
        //Variables for Mod
        public bool failsafe;
        public bool active = false; //Defines Active State of Mod
        public GameObject EllieClone;
        public GameObject EllieMain;
        public Vector3 l;
        public Quaternion r;
        public List<string> MessageCollection;
        public int connectors = 0;

        //Server Variables
        public NetServer server = null;
        public NetClient client = null;
        public NetConnection connection = null;
        public NetPeerConfiguration config = null;

        //Boolean Variables
        public List<string> PENWreckRooms = new List<string>() { "Cryogenics", "Flight Deck", "Mess Hall" }; //we do not need rooms without boolean values
        public List<bool> BooleanList = new List<bool>() { false, false, false };

        //Readable Server Variables
        public bool host;
        public int ServerPort;
        public IPAddress ServerIP;
        public SynchronizationContext Synch;

        /* Legacy Code
         * public string debugmessage = "";
         */

    }
    public class SignalisMultiplayer : MelonMod
    {
        static public Storage storage;
        public override void OnUpdate()
        {
            if (storage == null)
            {
                storage = StorageSetUp();
            }
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.P) && Input.GetKey(KeyCode.Keypad0))
            {
                MelonLogger.Msg(ConsoleColor.Green, "Server Deactivated for Debugging");
                storage.active = false;
            }
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.O))
            {
                MelonLogger.Msg("Debugging Printed");
                if(storage.server != null)
                {
                    MelonLogger.Msg(storage.server.Configuration.ToString());
                    MelonLogger.Msg(storage.server.Statistics.ToString());
                    MelonLogger.Msg(storage.server.Status.ToString());
                }
                if(storage.client != null)
                {
                    MelonLogger.Msg(storage.client.Status.ToString());
                }
            }
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.P) && (!storage.active) && (!storage.failsafe))
            {
                NetPeerConfiguration config = ConfigSetUp();
                if (config == null)
                {
                    MelonLogger.Msg("Config Returned Null, Double Check Config File");
                    storage.failsafe = true;
                    return;
                }
                try
                {
                    if (storage.host)
                    {
                        HostStart(config);
                    }
                    if (!storage.host)
                    {
                        ClientStart(config);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg("Failure on Server or Client Start due to " + ex.Message + " " + ex.Source + " " + ex.StackTrace);
                    storage.failsafe = true;
                    return;
                }
                EllieSetUp(); //Sets Up Ellie
            }
            if (storage.client != null && storage.client.ConnectionStatus == NetConnectionStatus.Disconnected)
            {
                ClientConnector();
            }
            if(storage.client != null)
            {
                if (storage.client.ConnectionStatus == NetConnectionStatus.Connected)
                {
                    storage.active = true;
                }
            }
            //When Mod is True Do the Following
            if (storage.active)
            {
                if(storage.server == null && storage.host)
                {
                    //If Server Null and Host is True, Then Failed
                    storage.active = false;
                    storage.failsafe = true;
                    MelonLogger.Msg("Failure of Storage and Host");
                    return;
                }
                if (storage.host)
                {
                    HostRunTime(storage);
                    DataTradeHost(storage);
                }
                if (!storage.host && storage.client.ConnectionsCount != 0)
                {
                    ClientRunTime(storage);
                    DataTradeClient(storage);
                }
                if (storage.client == null && !storage.host)
                {
                    MelonLogger.Msg("Failure of Storage and Client");
                    storage.active = false;
                    storage.failsafe = true;
                    return;
                }
            }
        }
        //Set Up- Initalization 
        public static Storage StorageSetUp()
        {
            Storage storage = new Storage();
            storage.failsafe = false;
            MelonLogger.Msg("Storage Created");

            string modsFolder = MelonHandler.ModsDirectory;
            string MultiplayerModConfigPath = Path.Combine(modsFolder, "SigiMultiplayerConfig.txt");
            if (File.Exists(MultiplayerModConfigPath))
            {
                string[] lines = File.ReadAllLines(MultiplayerModConfigPath);
                if (lines.Length < 3)
                {
                    MelonLogger.Msg("Error on Config, File too Small");
                    storage.failsafe = true;
                }
                if (bool.TryParse(lines[0], out bool hostValue))
                {
                    storage.host = hostValue;
                    MelonLogger.Msg("Host Value Initalized");
                }
                if (int.TryParse(lines[1], out int portValue))
                {
                    storage.ServerPort = portValue;
                    MelonLogger.Msg("Port Value Initalized");
                }
                string ipv4 = lines[2];
                if (System.Net.IPAddress.TryParse(ipv4, out storage.ServerIP)) {
                    MelonLogger.Msg("ServerIP Value Initalized");
                }
                else
                {
                    MelonLogger.Msg("Failure due to IP Parsing, Check if IP Is Proper");
                    storage.failsafe = true;
                    return null;
                }
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Config file not found at path: " + MultiplayerModConfigPath);
            }
            return storage;
        }
        public static NetPeerConfiguration ConfigSetUp()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Sigimulti");
            config.AutoFlushSendQueue = true;
            config.UseMessageRecycling = true;
            config.DefaultOutgoingMessageCapacity = 0;
            //config.Port = storage.ServerPort;
            //config.BroadcastAddress = storage.ServerIP;
            return config;
        }
        //Server Start
        public static void HostStart(NetPeerConfiguration config)
        {
            config.Port = storage.ServerPort;
            config.LocalAddress = storage.ServerIP;
            storage.server = new NetServer(config);
            storage.server.Start();
            storage.active = true;
            MelonLogger.Msg("Server Started");
            MelonLogger.Msg(storage.server.Status + " " + storage.server.Configuration);
        }
        public static void ClientStart(NetPeerConfiguration config)
        {
            /*
            //Client Code
            if (SynchronizationContext.Current == null)
            {
                MelonLogger.Msg("New Thread Created");
                storage.Synch = new SynchronizationContext();
            }
            else
            {
                storage.Synch = SynchronizationContext.Current;
            }
            //storage.client.RegisterReceivedCallback(new SendOrPostCallback(RecieveMessages), storage.Synch); */
            storage.client = new NetClient(config);
            storage.client.Start();
            MelonLogger.Msg("Client Started"); //test if issue is in client creation 
            IPEndPoint IP = new IPEndPoint(storage.ServerIP, storage.ServerPort);
            storage.connection = storage.client.Connect(IP);
            MelonLogger.Msg(storage.client.ConnectionStatus);
            NetOutgoingMessage outmsg = storage.client.CreateMessage("Ping");
            storage.client.SendUnconnectedMessage(outmsg, IP);
            /* public static void RecieveMessages(object peer)
            {
                ClientRunTime(storage);
            } */
        }
        public static void ClientConnector()
        {
            IPEndPoint IP = new IPEndPoint(storage.ServerIP, storage.ServerPort);
            if (storage.client.ConnectionStatus == NetConnectionStatus.Disconnected)
            {
                storage.client.Connect(IP, null);
                NetOutgoingMessage outmsg = storage.client.CreateMessage("Ping");
                storage.client.SendUnconnectedMessage(outmsg, IP);
            }
        }

        //Server Main Runtime
        public static void HostRunTime(Storage storage)
        {
            if (storage.connectors != storage.server.ConnectionsCount)
            {
                MelonLogger.Msg("Connection Status Changed");
                //Check if New Connection and if connections are lost
                storage.connectors = storage.server.ConnectionsCount;
                MelonLogger.Msg("There are now " + storage.connectors + "connected to the server, reminder that SigiMP is built for 2 players ATM any more may cause bugs");
            }
            try
            {
                NetIncomingMessage msg = storage.server.ReadMessage();
                while (msg != null && msg.LengthBytes < 1 && msg.MessageType != NetIncomingMessageType.ErrorMessage)
                {
                    HostRunTimeFormer(storage, msg);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Failure on Host Run Time Former \n" + e.StackTrace + e.Message + e.Data);
            }
        }
        public static void HostRunTimeFormer(Storage storage, NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.UnconnectedData:
                case NetIncomingMessageType.ConnectionApproval:
                    MelonLogger.Msg("New connection...");
                    msg.SenderConnection.Approve();
                    storage.connection = storage.server.Connect(msg.SenderConnection.RemoteEndPoint);
                    MelonLogger.Msg("Attempted to Connect to Sender" + storage.connection.Status); 
                    if(storage.connection != null) { storage.server.Connections.Add(storage.connection); };
                    break;
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.ErrorMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                    MelonLogger.Msg(msg.ReadString());
                    break;
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                    if (status == NetConnectionStatus.Disconnected)
                        MelonLogger.Msg("Diconnected");
                    string reason = msg.ReadString();
                    MelonLogger.Msg(status.ToString() + ": " + reason);
                    break;
                case NetIncomingMessageType.Data:
                    string chat = msg.ReadString();
                    HandleMessage(chat, storage);
                    break;
                default:
                    if (msg.MessageType == NetIncomingMessageType.Error || msg.LengthBytes == 0)
                    {
                        break;
                    }
                    MelonLogger.Msg("Unhandled type: " + msg.MessageType + " " + msg.LengthBytes + " bytes");
                    break;
            }
            msg = null;
        }
        public static void DataTradeHost(Storage storage)
        {
            if (storage.MessageCollection == null) { storage.MessageCollection = new List<string>(); } //check if message is null
            AddMessages(storage); //Creation Collection of Messages
            if (storage.MessageCollection.Count != 0)
            {
                foreach (string omsg in storage.MessageCollection)
                {
                    NetOutgoingMessage outmsg = storage.server.CreateMessage(omsg);
                    foreach (NetConnection Connection in storage.server.Connections)
                    {
                        storage.server.SendMessage(outmsg, Connection, NetDeliveryMethod.ReliableOrdered);
                    }
                    storage.server.FlushSendQueue();
                }
            }
        }
        public static void ClientRunTime(Storage storage)
        {
            NetIncomingMessage msg;
            if ((msg = storage.client.ReadMessage()) == null) { return; }
            while ((msg != null))
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        MelonLogger.Msg(msg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        if (status == NetConnectionStatus.Disconnected)
                        {
                            MelonLogger.Msg("Diconnected" + status.ToString());
                            storage.active = false;
                            storage.connection = null;
                            storage.client?.Shutdown("Exiting");
                            return;
                        }
                        string reason = msg.ToString();
                        MelonLogger.Msg(status.ToString() + ": " + reason);
                        break;
                    case NetIncomingMessageType.Data:
                        string chat = msg.ToString();
                        HandleMessage(chat, storage);
                        break;
                    default:
                        if (msg.MessageType == NetIncomingMessageType.Error || msg.LengthBytes == 0)
                        {
                            break;
                        }
                        MelonLogger.Msg("Unhandled type: " + msg.MessageType + " " + msg.LengthBytes + " bytes");
                        break;
                }
                if (msg != null)
                {
                    storage.client.Recycle(msg);
                }
                msg = null;
                msg = storage.client.ReadMessage();
            }
        }
        public static void DataTradeClient(Storage storage)
        {
            if (storage.MessageCollection == null) { storage.MessageCollection = new List<string>(); }
            if (storage.connection == null)
            {
                storage.active = false;
                MelonLogger.Msg("Connection Failed");
                return;
            }
            if (storage.connection.Status.Equals(NetConnectionStatus.Disconnected))
            {
                storage.active = false;
                storage.client?.Shutdown("Exiting");
                storage.client = null;
                MelonLogger.Msg("Connection Ended");
                return;
            }
            else
            {
                AddMessages(storage); //Creation Collection of Messages
            }
            if (storage.MessageCollection.Count != 0)
            {
                foreach (string omsg in storage.MessageCollection)
                {
                    NetOutgoingMessage outmsg = storage.server.CreateMessage(omsg);
                    storage.client.SendMessage(outmsg, storage.connection, NetDeliveryMethod.ReliableUnordered);
                    storage.client.FlushSendQueue();
                }
            }
        }
        //Central Runtime - Sending Messages
        public static void EllieSetUp()
        {
            try
            {
                if (GameObject.Find("__Prerequisites__").gameObject != null)
                {
                    GameObject Prerequisties = GameObject.Find("__Prerequisites__").gameObject;
                    GameObject CharOrigin = Prerequisties.transform.Find("Character Origin").gameObject;
                    GameObject CharRoot = CharOrigin.transform.Find("Character Root").gameObject;
                    storage.EllieMain = CharRoot.transform.Find("Ellie_Default").gameObject;
                    storage.EllieClone = UnityEngine.Object.Instantiate(storage.EllieMain);
                    storage.EllieClone.GetComponent<Anchor>().enabled = false;
                    if (storage.EllieClone == null) { storage.active = false; MelonLogger.Msg("Ellie is Null"); return; }
                    try
                    {
                        storage.l = storage.EllieClone.transform.position;
                        storage.r = storage.EllieClone.transform.rotation;
                    }
                    catch
                    {
                        MelonLogger.Msg("Failure Prior to 309 due to Storage Postional Storing");
                    }
                    //CheckerInitalize(storage);
                    MelonLogger.Msg("Ellie Established");
                }
                else
                {
                    MelonLogger.Msg("Ellie is Not Established Yet");
                    storage.active = false;
                    return;
                }
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Msg("Error Setting up Ellie " + e.StackTrace + " " + e.TargetSite);
                storage.active = false;
                return;
            }
        }
        public static void AddMessages(Storage storage)
        {
            List<Vector3> vvalue = CheckVector(storage);
            if (vvalue != null)
            {
                foreach (Vector3 vecvalue in vvalue)
                {
                    string value = ("V:" + vecvalue.ToString());
                    storage.MessageCollection.Add(value);
                }
            }
            List<Quaternion> qvalue = CheckQuaternion(storage);
            if (qvalue != null)
            {
                foreach (Quaternion quatervalue in qvalue)
                {
                    string value = ("Q:" + quatervalue.ToString());
                    storage.MessageCollection.Add(value);
                }
            }
            List<string> bkeyvalue = BooleanChecker(storage);
            if (bkeyvalue != null)
            {
                foreach (string fkey in bkeyvalue)
                {
                    string value = ("B:" + fkey);
                    storage.MessageCollection.Add(value);
                }
            }
        }
        public static List<Vector3> CheckVector(Storage storage)
        {
            try
            {
                List<Vector3> VList = new List<Vector3>();
                Vector3 e = storage.EllieMain.transform.position;
                Vector3 l = storage.l;
                if (e.x != l.x || e.y != l.y || e.z != l.z)
                {
                    VList.Add(e);
                    storage.l = e;
                    return VList;
                }
                else
                {
                    //If there is no change, send nothing, return null so no message is sent
                    return null;
                }
            }
            catch
            {
                MelonLogger.Msg("Failure on 453-Check Vector");
                return null;
            }
        }
        public static List<Quaternion> CheckQuaternion(Storage storage)
        {
            try
            {
                List<Quaternion> QList = new List<Quaternion>();
                Quaternion e = storage.EllieClone.transform.rotation * Quaternion.Euler(new Vector3(0f, 0f, 270f));
                Quaternion r = storage.r;
                if (e.x != r.x || e.y != r.y || e.z != r.z)
                {
                    QList.Add(e);
                    storage.r = e;
                    return QList;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                MelonLogger.Msg("Failure on 487-Check Quaternion");
                return null;
            }

        }
        public static List<string> BooleanChecker(Storage storage)
        {
            List<string> InternalList = new List<string>();
            string name = SceneManager.GetActiveScene().name;
            switch (name)
            {
                case "PEN_Wreck":
                    int roomname = RoomChecker(storage.PENWreckRooms, storage);
                    BooleanChecker(1, roomname, InternalList, storage);
                    break;
                case "PEN_Hole":
                    //no boolean cases here :D
                    break;
                default:
                    break;
            }
            return InternalList;
        }
        public static int RoomChecker(List<string> secondarylist, Storage storage)
        {
            foreach (string s in secondarylist)
            {
                if (GameObject.Find(s) != null)
                {
                    //nested this just incase it bugs out as it may since we are testing for null
                    if (GameObject.Find(s).transform.Find("Chunk").gameObject.active == true)
                    {
                        //we need to export out s here, this finds s index
                        return storage.PENWreckRooms.IndexOf(s); //this isnt flawless, and may interfere with other things in export requiring reformatting 
                    }
                }
            }
            return 0;
        }
        public static void BooleanChecker(int index, int RoomName, List<string> InternalList, Storage storage)
        {
            switch (index)
            {
                case 1:
                    switch (RoomName)
                    {
                        case 0:
                            //Cryopod
                            if (!storage.BooleanList[0])
                            {
                                PEN_Cryo Cryo = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject.transform.Find("Cryo").gameObject.transform.GetComponent<PEN_Cryo>();
                                storage.BooleanList[0] = Cryo.opened; //this is set true when the value is true
                                if (storage.BooleanList[0])
                                {
                                    InternalList.Add("0,0");
                                }
                            }
                            break;
                        case 1:
                            //Cockpit
                            if (!storage.BooleanList[1])
                            {
                                GameObject Cockpit = GameObject.Find("Events").gameObject.transform.Find("Pen_CockpitEvent3D").gameObject.transform.Find("Interactions").gameObject;
                                if (Cockpit.transform.Find("PhotoPickup") == null)
                                {
                                    storage.BooleanList[1] = true;
                                    InternalList.Add("1,0");
                                };
                            }
                            break;
                        case 2:
                            //Ductape
                            if (!storage.BooleanList[2])
                            {
                                GameObject Ducttape = GameObject.Find("Events").gameObject.transform.Find("PEN_PC").gameObject;
                                if (Ducttape.transform.Find("TapePickup") == null)
                                {
                                    storage.BooleanList[2] = true;
                                    InternalList.Add("2,0");
                                }
                            }
                            break;
                        default: break;
                    }
                    break;
                default:
                    break;
            }
        }
        //Central Runtime - Handling Messages
        public static void HandleMessage(string message, Storage storage)
        {
            if (message.StartsWith("D:"))
            {
                //Handles String Messages Passed by Server
                int colonIndex = message.IndexOf(':');
                if (colonIndex != -1 && colonIndex < message.Length - 1)
                {
                    message = message.Substring(colonIndex + 1);
                    MelonLoader.MelonLogger.Msg("Message Recieved " + message);
                }
            }
            if (message.StartsWith("V:"))
            {
                //Handles Vector Transforms Passed by Server, To Be Used if Ellie Moves
                int colonIndex = message.IndexOf(':');
                if (colonIndex != -1 && colonIndex < message.Length - 1)
                {
                    string numbersPart = message.Substring(colonIndex + 1).Trim();
                    string[] numberStrings = numbersPart.Split(',');
                    if (numberStrings.Length == 3)
                    {
                        if (float.TryParse(numberStrings[0].Trim(), out float x) && float.TryParse(numberStrings[1].Trim(), out float y) && float.TryParse(numberStrings[2].Trim(), out float z))
                        {
                            Vector3 vector = new Vector3(x, y, z);
                            storage.EllieClone.transform.position = vector;
                        }
                        else
                        {
                            MelonLoader.MelonLogger.Msg("Error parsing numbers.");
                        }
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Msg("Expected 3 numbers.");
                    }
                }
            }
            if (message.StartsWith("Q:"))
            {
                //Handles Rotations Passed by The Server, To be Used if Ellie Rotates
                int colonIndex = message.IndexOf(':');
                if (colonIndex != -1 && colonIndex < message.Length - 1)
                {
                    string numbersPart = message.Substring(colonIndex + 1).Trim();
                    string[] numberStrings = numbersPart.Split(',');
                    if (numberStrings.Length == 4)
                    {
                        if (float.TryParse(numberStrings[0].Trim(), out float x) && float.TryParse(numberStrings[1].Trim(), out float y) && float.TryParse(numberStrings[2].Trim(), out float z) && float.TryParse(numberStrings[3].Trim(), out float w))
                        {
                            Quaternion quaternion = new Quaternion(x, y, z, w);
                            storage.EllieClone.transform.rotation = quaternion;
                        }
                        else
                        {
                            MelonLoader.MelonLogger.Msg("Error parsing numbers.");
                        }
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Msg("Expected 3 numbers.");
                    }
                }
            }
            if (message.StartsWith("B:"))
            {
                //Handles Boolean Values
                int colonIndex = message.IndexOf(':');
                if (colonIndex != -1 && colonIndex < message.Length - 1)
                {
                    string dataPart = message.Substring(colonIndex + 1).Trim();
                    //Splits the Two Types of Data
                    string[] parts = dataPart.Split(',');
                    if (parts.Length == 2)
                    {
                        if (int.TryParse(parts[0].Trim(), out int boolValue) && int.TryParse(parts[1].Trim(), out int tag))
                        {
                            bool boolean = (boolValue == 1);
                            //This creates a Bool value and a int value that can be used to find the reference bool
                            ApplyBool(boolean, tag, storage);
                        }
                        else
                        {
                            MelonLoader.MelonLogger.Msg("Error parsing values.");
                        }
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Msg("Expected 2 values for the message.");
                    }
                }
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Server Recieved Unhandled Logic " + message);
            }
        }
        public static void ApplyBool(bool boolean, int tag, Storage storage)
        {
            switch (tag)
            {
                case 1:
                    if (!storage.BooleanList[0])
                    {
                        PEN_Cryo Cryo = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject.transform.Find("Cryo").gameObject.transform.GetComponent<PEN_Cryo>();
                        Cryo.Open(); //Plays Cutscene Fixes Boolean Mismatch
                        storage.BooleanList[0] = boolean; //this is set true when the value is true
                    }
                    break;
                case 2:
                    if (!storage.BooleanList[1])
                    {
                        GameObject Cockpit = GameObject.Find("Events").gameObject.transform.Find("Pen_CockpitEvent3D").gameObject.transform.Find("Interactions").gameObject;
                        if (Cockpit.transform.Find("PhotoPickup") != null)
                        {
                            Cockpit.transform.Find("PhotoPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Cockpit.transform.Find("PhotoPickup").gameObject.SetActive(false);
                            storage.BooleanList[1] = boolean;
                        }
                    }
                    break;
                case 3:
                    if (!storage.BooleanList[2])
                    {
                        GameObject Ducttape = GameObject.Find("Events").gameObject.transform.Find("PEN_PC").gameObject;
                        if (Ducttape.transform.Find("TapePickup") != null)
                        {
                            Ducttape.transform.Find("TapePickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Ducttape.transform.Find("TapePickup").gameObject.SetActive(false);
                            storage.BooleanList[2] = boolean;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        //Handles End State
        public override void OnApplicationQuit()
        {
            // Clean up resources
            storage.server?.Shutdown("Exiting");
            storage.client?.Shutdown("Exiting");
            storage.Synch = null;
            storage = null;
            base.OnApplicationQuit();
        }
    }
}