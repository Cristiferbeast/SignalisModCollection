using MelonLoader;
using Lidgren.Network;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Threading;

namespace SigiMultiplayer
{
    public class Storage
    {
        //Variables for Mod
        public bool start = true;
        public bool active = false; //Defines Active State of Mod
        public string debugmessage = "";
        public GameObject EllieClone;
        public GameObject Prerequisties; 
        public GameObject EllieMain;
        public Vector3 l;
        public Quaternion r;
        public List<bool> checkers;
        public string OtherEllieScene;

        //Server Variables
        public NetServer server;
        public NetClient client;
        public NetPeerConfiguration config;

        //Readable Server Variables
        public bool host;
        public int ServerPort;
        public string ServerIP;

        //state variable
        public bool failsafe;

        public static IEnumerable DelayOneFrame()
        {
            yield return null;
        }

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
                        if (lines[2] == "your ip here" || lines[2] == "")
                        {
                            MelonLogger.Msg("ServerIP Value is Not Set ");
                            storage.failsafe = true;
                        }
                        else
                        {
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
                        CheckerInitalize(storage);
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
                    List<string> messageCollection = new List<string>();
                    AddMessages(messageCollection, storage); //Creation Collection of Messages

                    if (storage.server != null)
                    {
                        foreach (string msg in messageCollection)
                        {
                            NetOutgoingMessage message = storage.server.CreateMessage();
                            message.Write(msg);
                            foreach (NetConnection clientConnection in storage.server.Connections)
                            {
                                storage.server.SendMessage(message, clientConnection, NetDeliveryMethod.ReliableOrdered);
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

        //Handles Reading Messages from Server
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
                    Transform transform = storage.EllieClone.transform.Find("FlashLightHolder");
                    if (transform != null)
                    {
                        transform.gameObject.SetActive(boolean);
                    }
                    break;
            }
        }

        //Handles Adding Messages
        public static void AddMessages(List<string> messages, Storage storage)
        {
            string mvalue = CheckMessage(storage);
            if (mvalue != null)
            {
                string value = ("D:" + mvalue);
                messages.Add(value);
            }
            List<Vector3> vvalue = CheckVector(storage);
            if (vvalue != null)
            {
                foreach (Vector3 vecvalue in vvalue)
                {
                    string value = ("V:" + vecvalue.ToString());
                    messages.Add(value);
                }
            }
            List<Quaternion> qvalue = CheckQuaternion(storage);
            if (qvalue != null)
            {
                foreach (Quaternion quatervalue in qvalue)
                {
                    string value = ("Q:" + quatervalue.ToString());
                    messages.Add(value);
                }
            }
            List<string> bkeyvalue = CheckBool(storage);
            if (bkeyvalue != null)
            {
                foreach (string fkey in bkeyvalue)
                {
                    string value = ("B:" + fkey);
                    messages.Add(value);
                }
            }
        }

        //Handles Checking if a Message Should be Sent
        public static string CheckMessage(Storage storage)
        {
            if (storage.debugmessage == "")
            {
                return null;
            }
            else
            {
                return storage.debugmessage;
            }
        }
        public static List<Vector3> CheckVector(Storage storage)
        {
            List<Vector3> VList = new List<Vector3>();
            Vector3 e = storage.EllieMain.transform.position;
            Vector3 l = storage.l;
            if (e.x != l.x || e.y != l.y || e.z != l.z)
            {
                VList.Add(e);
                storage.l = e;
            }
            return VList;
        }

        public static List<Quaternion> CheckQuaternion(Storage storage)
        {
            List<Quaternion> QList = new List<Quaternion>();
            Quaternion e = storage.EllieClone.transform.rotation;
            Quaternion r = storage.r;
            if (e.x != r.x || e.y != r.y || e.z != r.z)
            {
                QList.Add(e);
                storage.r = e;
            }
            return QList;
        }
        public static List<string> CheckBool(Storage storage)
        {
            List<string> BList = new List<string>();
            BoolInternal(storage.EllieClone.transform.Find("FlashLightHolder").gameObject, storage, 1, BList);
            PlayerAttack playerAttack = (PlayerAttack)storage.Prerequisties.GetComponentByName("PlayerAttack");
            BoolInternal(playerAttack.aiming,storage,2, BList);
            WeaponHandler(playerAttack.weapon.name,storage, BList);
            return BList;
        }

        public static void WeaponHandler (string weaponname, Storage storage, List<string> BList)
        {
            //Weapons Occupy The 3-9 Range
            for(int i = 3; i < 10; i++)
            {
                storage.checkers[i] = false;
            }
            switch(weaponname)
            {
                case "SMGWeapon":
                    storage.checkers[3] = true; 
                    BList.Add(true + ":0" + 3);
                    break;
                case "RifleWeapon":
                    storage.checkers[4] = true; 
                    BList.Add(true + ":0" + 4);
                    break;
                case "FlareWeapon":
                    storage.checkers[5] = true; 
                    BList.Add(true + ":0" + 5);
                    break;
                case "MacheteWeapon":
                    storage.checkers[6] = true;
                    BList.Add(true + ":0" + 5);
                    break;
            }
        }

        public static void BoolInternal(GameObject item, Storage storage, int index, List<string> BList)
        {
            if(item == null) return;
            if(item.activeSelf != storage.checkers[index])
            {
                BList.Add((item.activeSelf) + ":0" + index);
                storage.checkers[index] = item.activeSelf;
            }
        }
        public static void BoolInternal(bool state, Storage storage, int index, List<string> BList)
        {
            if(state != storage.checkers[index])
            {
                BList.Add(state + ":0" + index);
                storage.checkers[index] = state;
            }
        }
        public static void BoolInternal(GameObject item, int index, List<bool> BList, List<string> BSend)
        {
            if (item != null)
            {
                bool state = item.activeSelf;
                BList.Add(state);
                BSend.Add("B:" + state + ":0" + index);
            }
        }
        public static void BoolInternal(bool state, int index, List<bool> BList, List<string> BSend)
        {
            BSend.Add(state + ":0" + index);
            BList[index] = state;
        }


        public void CheckerInitalize(Storage storage)
        {
            List<string> BSend = new List<string>();
            storage.checkers = new List<bool>();
            GameObject Flashlight = storage.EllieMain.transform.Find("FlashLightHolder").gameObject;
            BoolInternal(Flashlight, 1, storage.checkers, BSend);
            /*PlayerAttack playerAttack = (PlayerAttack)storage.Prerequisties.GetComponentByName("PlayerAttack");
            BoolInternal(playerAttack.aiming, 2, storage.checkers, BSend);*/

            //Handles The Rest of Initalization
            if (storage.host)
            {
                foreach (string msg in BSend)
                {
                    NetOutgoingMessage message = storage.server.CreateMessage();
                    message.Write(msg);
                    foreach (NetConnection clientConnection in storage.server.Connections)
                    {
                        storage.server.SendMessage(message, clientConnection, NetDeliveryMethod.ReliableOrdered);
                        Coroutine(Storage.DelayOneFrame());
                    }
                }
            }
        }
        //Handles End State
        public override void OnApplicationQuit()
        {
            // Clean up resources
            this.storage.server?.Shutdown("Exiting");
            this.storage.client?.Shutdown("Exiting");
        }

        public static GameObject Duplicate(GameObject duplicate)
        {
            MethodInfo instantiateMethod;
            UnityEngine.Object oreturnobject;
            try
            {
                instantiateMethod = typeof(UnityEngine.Object).GetMethod("Instantiate", new[] { typeof(UnityEngine.Object) });
                if (instantiateMethod == null) { MelonLogger.Msg("failed 699-1"); return null; }
            }
            catch { MelonLogger.Msg("failed 387"); return null; }
            try
            {
                oreturnobject = (UnityEngine.Object)instantiateMethod.Invoke(null, new object[] { duplicate });
                if (oreturnobject == null) { MelonLogger.Msg("failed 699-2"); return null; }
            }
            catch { MelonLogger.Msg("failed 392"); return null; }
            try
            {
                GameObject returnObject = (GameObject)oreturnobject;
                if (returnObject == null) { MelonLogger.Msg("failed 699-3"); MelonLogger.Msg("Type of oreturnobject: " + oreturnobject.GetType().ToString()); ; return null;  }
                return returnObject;
            }
            catch { MelonLogger.Msg("failed 399"); return null; }
        }

        public void Coroutine(IEnumerable coroutinetoStart)
        {
            MethodInfo startCoroutineMethod;
            UnityEngine.MonoBehaviour MBehavior = new UnityEngine.MonoBehaviour(); 
            try
            {
                startCoroutineMethod = typeof(MonoBehaviour).GetMethod("StartCoroutine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            catch { MelonLogger.Msg("failed 454"); return; }
            startCoroutineMethod.Invoke(MBehavior, new object[] { coroutinetoStart });
        }
    }
}