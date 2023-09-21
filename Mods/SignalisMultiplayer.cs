using MelonLoader;
using Lidgren.Network;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;

namespace SigiMultiplayer
{
    public class Storage
    {
        //Variables for Mod
        public bool start = true;
        public bool active = false; //Defines Active State of Mod
        public string debugmessage = "";
        public GameObject EllieClone;
        public GameObject EllieMain;
        public Vector3 l;
        public Quaternion r;
        public List<bool> checkers;

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
                        storage.ServerIP = lines[2];
                        MelonLogger.Msg("ServerIP Value Initalized");
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
                    //Activation Logic
                    storage.config = new NetPeerConfiguration("Sigimulti");
                    storage.config.Port = storage.ServerPort;
                    storage.config.PingInterval = 2.0f;
                    storage.config.ConnectionTimeout = 10.0f;
                    storage.config.MaximumConnections = 2;
                    storage.config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse); //discovery stuff
                    storage.config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest); //more discovery stuff
                    storage.server = new NetServer(storage.config);
                    storage.server.Start();
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
                catch
                {
                    MelonLogger.Msg("Server Connection Failed");
                    storage.failsafe = true;
                    return;
                }
                try
                {
                    //once discovery shit has been done, they should be friends and let the client connect. if this doesn't work, we can just implement ip parsing me thinks ? 
                    storage.client = new NetClient(storage.config);
                    storage.client.Start();
                    storage.client.Connect(storage.ServerIP, storage.ServerPort);
                }
                catch
                {
                    MelonLogger.Msg("Client Connection Failed");
                    storage.failsafe = true;
                    return;
                }
                try{
                    if (GameObject.Find("__Prerequisites__") != null)
                    {
                        GameObject PreReq = GameObject.Find("__Prerequisites__");
                        GameObject CharOrigin = PreReq.transform.Find("Character Origin").gameObject;
                        GameObject CharRoot = CharOrigin.transform.Find("Character Root").gameObject;
                        storage.EllieMain = CharRoot.transform.Find("Ellie_Default").gameObject;
                        storage.EllieClone = Duplicate(storage.EllieMain);
                        if (storage.EllieClone.name == null) { storage.active = false; return; }
                        storage.l = storage.EllieClone.transform.position;
                        storage.r = storage.EllieClone.transform.rotation;
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
                    MelonLoader.MelonLogger.Msg("Error Setting up Ellie " + e);
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
            if ((storage.EllieClone.transform.Find("FlashLightHolder").gameObject) != null)
            {
                if (storage.EllieClone.transform.Find("FlashLightHolder").gameObject.activeSelf != storage.checkers[1])
                {
                    BList.Add(storage.EllieClone.transform.Find("FlashLightHolder").gameObject.activeSelf + ":01");
                    storage.checkers[1] = storage.EllieClone.transform.Find("FlashLightHolder").gameObject.activeSelf;
                }
            }
            return BList;
        }

        public void CheckerInitalize(Storage storage)
        {
            //
            List<bool> BList = new List<bool>();
            List<string> BSend = new List<string>();
            if ((storage.EllieClone.transform.Find("FlashLightHolder").gameObject) != null)
            {
                bool state = storage.EllieClone.transform.Find("FlashLightHolder").gameObject.activeSelf;
                BList.Add(state);
                BSend.Add(state + ":01"); //not entirely sure if this works
            }
            storage.checkers = BList;

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

        public static GameObject Duplicate(UnityEngine.Object duplicate)
        {
            MethodInfo instantiateMethod;
            UnityEngine.Object oreturnobject;
            try
            {
                instantiateMethod = typeof(UnityEngine.Object).GetMethod("Instantiate", new[] { typeof(UnityEngine.Object) });
            }
            catch { MelonLogger.Msg("failed 387"); return null; }
            try
            {
                oreturnobject = (UnityEngine.Object)instantiateMethod.Invoke(null, new object[] { duplicate });
            }
            catch { MelonLogger.Msg("failed 392"); return null; }
            try
            {
                GameObject returnObject = oreturnobject as GameObject;
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