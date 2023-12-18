using MelonLoader;
using Lidgren.Network;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Threading;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters;

namespace SigiMultiplayer
{
    public class Storage
    {
        //Variables for Mod
        public bool start = true;
        public bool failsafe;
        public bool active = false; //Defines Active State of Mod
        public string debugmessage = "";
        public GameObject EllieClone;
        public GameObject Prerequisties; 
        public GameObject EllieMain;
        public Vector3 l;
        public Quaternion r;
        public List<string> MessageCollection;

        //Server Variables
        public NetServer server;
        public NetClient client;
        public NetPeerConfiguration config;

        //Boolean Variables
        public List<string> PENWreckRooms = new List<string>() { "Cryogenics", "Flight Deck", "Mess Hall" }; //we do not need rooms without boolean values
        public List<bool> BooleanList = new List<bool>() { false, false, false};

        //Readable Server Variables
        public bool host;
        public int ServerPort;
        public string ServerIP;
    }
    public class SignalisMultiplayer : MelonMod
    {
        public Storage storage;
        public override void OnUpdate()
        {
            if (storage == null)
            {
                storage = new Storage();
                storage.failsafe = false;
                MelonLogger.Msg("Storage Created");
            }
            if (storage.start)
            {
                string modsFolder = MelonHandler.ModsDirectory;
                string MultiplayerModConfigPath = Path.Combine(modsFolder, "SigiMultiplayerConfig.txt");
                if (File.Exists(MultiplayerModConfigPath))
                {
                    string[] lines = File.ReadAllLines(MultiplayerModConfigPath);
                    if (lines.Length >= 3)
                    {
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
                        if (lines[2] == "your ip here" || lines[2] == "" || lines[2].Length >= 8)
                        { 
                            MelonLogger.Msg("ServerIP Value is Not Set or is incorrectly set");
                            storage.failsafe = true;
                        }
                        else
                        {
                            //ensure that ip used is IPV4
                            storage.ServerIP = lines[2];
                            MelonLogger.Msg("ServerIP Value Initalized");
                        }
                    }
                    else
                    {
                        MelonLogger.Msg("Config file does not have enough lines.");
                    }
                }
                else
                {
                    MelonLoader.MelonLogger.Msg("Config file not found at path: " + MultiplayerModConfigPath);
                }
                storage.start = false;
            }
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKeyDown(KeyCode.P) && (!storage.active) && (!storage.failsafe))
            {
                try
                {
                    NetPeerConfiguration config = new NetPeerConfiguration("Sigimulti");
                    try
                    {
                        System.Net.IPAddress iPAddress;
                        if (System.Net.IPAddress.TryParse(storage.ServerIP, out iPAddress))
                        {
                            try
                            {
                                config.LocalAddress = iPAddress;
                            }
                            catch { MelonLogger.Msg("Failure on 112 due to IP Connection"); }
                        }
                        else
                        {
                            MelonLogger.Msg("Failure on 157 due to IP Parsing");
                        }
                        config.LocalAddress = iPAddress;
                        config.Port = storage.ServerPort;
                        config.PingInterval = 2.0f;
                        config.NetworkThreadName = "Ellie";
                        config.ConnectionTimeout = 10.0f;
                        config.MaximumConnections = 2;
                        config.ReceiveBufferSize = 1024;
                        config.SendBufferSize = 1024;
                        config.UseMessageRecycling = true;
                        config.RecycledCacheMaxCount = 0;
                        config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse); //discovery stuff
                        config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest); //more discovery stuff
                    }
                    catch
                    {
                        MelonLogger.Msg("Failure on Config Prior to 128");
                    }
                    storage.server = new NetServer(config);
                    try
                    {
                        storage.server.Start();
                    }
                    catch(Exception ex)
                    {
                        MelonLogger.Msg("Failure on Server Start Prior to 137 due to " + ex.Message + " " + ex.Source + " " + ex.StackTrace);
                    }
                    NetIncomingMessage inc;
                    try
                    {
                        //discovery requests in action
                        while ((inc = storage.server.ReadMessage()) != null)
                        {
                            switch (inc.MessageType)
                            {
                                case NetIncomingMessageType.DiscoveryRequest:
                                    NetOutgoingMessage response = storage.server.CreateMessage();
                                    response.Write("wtv server name yknow yknow");
                                    storage.server.SendDiscoveryResponse(response, inc.SenderEndPoint);
                                    break;
                                    //mmnnnfhghgh ??
                            }
                        }
                    }
                    catch
                    {
                        MelonLogger.Msg("Failure on Reading Messages, Prior to 144");
                    }
                    try
                    {
                        NetPeerConfiguration clientconfig = new NetPeerConfiguration("SigiMultiplayer");
                        clientconfig.Port = storage.ServerPort;
                        System.Net.IPAddress iPAddress;
                        if (System.Net.IPAddress.TryParse(storage.ServerIP, out iPAddress))
                        {

                            clientconfig.LocalAddress = iPAddress;

                        }
                        else
                        {
                            MelonLogger.Msg("Failure on 157 due to IP Parsing");
                        }
                        clientconfig.LocalAddress = iPAddress;
                        clientconfig.NetworkThreadName = "Ellie";
                        clientconfig.EnableMessageType(NetIncomingMessageType.DiscoveryResponse); //kill me plesae god fuck
                        storage.client = new NetClient(clientconfig);
                        storage.client.Start();
                    }
                    catch
                    {
                        MelonLogger.Msg("Failure Prior to 155 due to Client Start");
                    }
                    try
                    {
                        storage.client.DiscoverLocalPeers(storage.ServerPort);//or whatever fucking port
                        Thread.Sleep(500);
                    }
                    catch
                    {
                        MelonLogger.Msg("Failure on Discovering Peers, Prior to 193");
                    }
                    NetOutgoingMessage hail;
                    try
                    {
                        hail = storage.client.CreateMessage("bla bla"); //attempt at connection, if we get a "bla bla" its working
                    }
                    catch
                    {
                        MelonLogger.Msg("Failure on Hail, Prior to 202");
                    }
                    NetIncomingMessage message;
                    MelonLogger.Msg(storage.ServerIP + "Server Port" + storage.ServerPort);
                    try
                    {
                        hail = storage.client.CreateMessage("bla bla"); //attempt at connection, if we get a "bla bla" its working
                        message = storage.client.WaitMessage(300);
                        if (message == null) //AHH
                        {
                            throw new Exception("Client Message is Null");
                        }
                        if (message != null) //AHH
                        {
                            switch (message.MessageType)
                            {
                                case NetIncomingMessageType.DiscoveryResponse:
                                    try
                                    {
                                        MelonLogger.Msg("(Client) is starting to connect to the server.."); //WE GOT A RESPONSE FROM THE SERVER !!! CONNECTION IS WORKING
                                        storage.client.Connect(message.SenderEndPoint, hail);
                                        MelonLogger.Msg("(Client) Attempting to connect to server..."); //we are securing the bag
                                    }
                                    catch
                                    {
                                        MelonLogger.Msg("Failure to Connect to Server");
                                    }
                                    break;
                                case NetIncomingMessageType.DebugMessage:
                                    MelonLogger.Msg(message.ReadString()); //auto reads stuff and prints it out
                                    break;
                                case NetIncomingMessageType.ErrorMessage:
                                    MelonLogger.Msg(message.ReadString()); //more auto read
                                    break;
                                case NetIncomingMessageType.WarningMessage:
                                    MelonLogger.Msg(message.ReadString()); //bla bla autoread
                                    break;
                                case NetIncomingMessageType.VerboseDebugMessage:
                                    MelonLogger.Msg(message.ReadString()); //auto read
                                    break;
                                case NetIncomingMessageType.Data:
                                    MelonLogger.Msg(message.ReadString()); //ya
                                    break;
                                default:
                                    //connection errors
                                    MelonLogger.Error("(Client) connection issue!!!! (" + message.MessageType + ")"); //should explain connection issue
                                    break;
                            }
                        }
                        //if its not resolved, it should shoot the message back?
                        storage.client.Recycle(message);
                    }
                    catch(Exception ex)
                    {
                        MelonLogger.Msg("Exception occured due to:" + ex.Message);
                    }
                }
                catch
                {
                    MelonLogger.Msg("Server Initalization Failed");
                    storage.failsafe = true;
                    return;
                }
                try
                {
                    //discovery requests in action
                    NetIncomingMessage inc;
                    while ((inc = storage.server.ReadMessage()) != null)
                    {
                        switch (inc.MessageType)
                        {
                            case NetIncomingMessageType.DiscoveryRequest:
                                NetOutgoingMessage response = storage.server.CreateMessage();
                                response.Write("wtv server name yknow yknow");
                                storage.server.SendDiscoveryResponse(response, inc.SenderEndPoint);
                                break;
                                //mmnnnfhghgh ?? <- Insightful
                        }
                    }
                }
                catch(Exception ex)
                {
                    MelonLogger.Msg("Server Systems Failed due to " + ex.Message + " " + ex.StackTrace);
                    storage.failsafe = true;
                    return;
                }
                try{
                    if (GameObject.Find("__Prerequisites__").gameObject != null)
                    {
                        storage.Prerequisties = GameObject.Find("__Prerequisites__").gameObject;
                        GameObject CharOrigin = storage.Prerequisties.transform.Find("Character Origin").gameObject;
                        GameObject CharRoot = CharOrigin.transform.Find("Character Root").gameObject;
                        storage.EllieMain = CharRoot.transform.Find("Ellie_Default").gameObject;
                        storage.EllieClone = UnityEngine.Object.Instantiate(storage.EllieMain);
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
                storage.active = true;
            }
            //When Mod is True Do the Following
            if (storage.active)
            {
                if (storage.client == null && !storage.host)
                {
                    MelonLoader.MelonLogger.Msg("Client Didnt Load"); //Check if Client Failed
                    storage.active = false;
                    return;
                }
                try {
                    //Sending Messages
                    if(storage.MessageCollection == null) { storage.MessageCollection = new List<string>(); }
                    AddMessages(storage); //Creation Collection of Messages

                    if (storage.server != null)
                    {
                        if(storage.MessageCollection.Count != 0)
                        {
                            foreach (string msg in storage.MessageCollection)
                            {
                                NetOutgoingMessage message = storage.server.CreateMessage();
                                message.Write(msg);
                                foreach (NetConnection clientConnection in storage.server.Connections)
                                {
                                    storage.server.SendMessage(message, clientConnection, NetDeliveryMethod.ReliableOrdered);
                                }
                            }
                        }
                    }
                    // Receiving messages
                    if (storage.server == null)
                    {
                        MelonLoader.MelonLogger.Msg("Server is Null"); //Check if Server Failed
                        storage.active = false;
                        return;
                    }
                    if (storage.server != null)
                    {
                        NetIncomingMessage incomingMessage;
                        while ((incomingMessage = storage.server.ReadMessage()) != null)
                        {
                            if (incomingMessage.MessageType == NetIncomingMessageType.Data)
                            {
                                string receivedData = incomingMessage.ReadString();
                                HandleMessage(receivedData, storage); //Read Messages 
                            }
                            storage.server.Recycle(incomingMessage); //Delete Messages after Reading Them
                        }
                    }
                }
                catch(Exception e)
                {
                    MelonLogger.Msg(e.Message + "" + e.StackTrace);
                }
            }
        }

        //Central Runtime - Sending Messages
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
                Quaternion e = storage.EllieClone.transform.rotation;
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
        public static void BooleanChecker(int index, int RoomName, List<string> InternalList, Storage storage )
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
                                if(Ducttape.transform.Find("TapePickup") == null)
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
                        storage.BooleanList[0] = true; //this is set true when the value is true
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
                            storage.BooleanList[1] = true;
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
                            storage.BooleanList[2] = true;
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
            this.storage.server?.Shutdown("Exiting");
            this.storage.client?.Shutdown("Exiting");
        }
    }
}