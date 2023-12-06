using MelonLoader;
using Lidgren.Network;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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

        //Readable Server Variables
        public bool host = true;
        public int ServerPort = 5120;
        public string ServerIP;
    }
    public class SignalisMultiplayer : MelonMod
    {
        Storage storage;
        public override void OnUpdate()
        {
            if (storage.start)
            {
                string modsFolder = MelonHandler.ModsDirectory;
                string MultiplayerModConfigPath = Path.Combine(modsFolder, "SigiMultiplayerConfig");
                if (File.Exists(MultiplayerModConfigPath))
                {
                    string[] lines = File.ReadAllLines(MultiplayerModConfigPath);
                    if (lines.Length >= 3)
                    {
                        if (bool.TryParse(lines[0], out bool hostValue))
                        {
                            storage.host = hostValue;
                        }

                        if (int.TryParse(lines[1], out int portValue))
                        {
                            storage.ServerPort = portValue;
                        }
                        storage.ServerIP = lines[2];
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
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKeyDown(KeyCode.P) && (storage.active)){
                MelonLogger.Msg("Storage SetUp Still")
            }
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKeyDown(KeyCode.P) && (!storage.active))
            { //Activation Logic
              //Logic Written By Z
                NetPeerConfiguration config = new NetPeerConfiguration("Sigimultiplayer");
                config.PingInterval = 0.1f; // Interval for sending ping messages
                config.ConnectionTimeout = 2.0f; // Interval for timeout connection attempts
                config.MaximumConnections = 2; // Number of allowed connections
                config.Port = storage.ServerPort; // Port 
                if (storage.host)
                {
                    NetServer server = new NetServer(config); // Create a new NetServer instance with the above config 
                    server.Start();
                }
                else
                {
                    NetClient client = new NetClient(config); //Create a New Client Instance with the above config
                    client.Start();
                    client.Connect(storage.ServerIP, storage.ServerPort);
                }

                try
                {
                    GameObject PreReq = GameObject.Find("__Prerequisites__");
                    GameObject CharOrigin = PreReq.transform.Find("Character Origin").gameObject;
                    GameObject CharRoot = CharOrigin.transform.Find("Character Root").gameObject;
                    storage.EllieMain = CharRoot.transform.Find("Ellie_Default").gameObject;
                    storage.EllieClone = Duplicate(storage.EllieMain);
                    storage.l = storage.EllieClone.transform.position;
                    storage.r = storage.EllieClone.transform.rotation;
                }
                catch
                {
                    MelonLoader.MelonLogger.Msg("Error Setting up Ellie");
                }
                storage.active = true;
            }

            //When Mod is True Do the Following
            if (storage.active)
            {
                if (storage.client == null)
                {
                    MelonLoader.MelonLogger.Msg("Client Is Not Loaded"); //Check if Client Failed
                    storage.active = false;
                }

                //Sending Messages
                List<string> messageCollection = new List<string>();
                AddMessages(messageCollection, storage); //Creation Collection of Messages

                if (storage.client != null)
                {
                    foreach (string msg in messageCollection)
                    {
                        NetOutgoingMessage message = storage.client.CreateMessage();
                        message.Write(msg);
                        storage.client.SendMessage(message, NetDeliveryMethod.ReliableOrdered); //Iterate through the array and send each of them as a message. 
                    }
                }

                // Receiving messages
                if (storage.server == null)
                {
                    MelonLoader.MelonLogger.Msg("Server is Null"); //Check if Server Failed
                    storage.active = false;
                }
                if (storage.server != null)
                {
                    NetIncomingMessage incomingMessage;
                    while ((incomingMessage =  storage.server.ReadMessage()) != null)
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
                            ApplyBool(boolean, tag);
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

        public static void ApplyBool(bool boolean, int tag)
        {
            //do nothing
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
            List<int> bkeyvalue = CheckBool();
            if (bkeyvalue != null)
            {
                foreach (int fkey in bkeyvalue)
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
        public static List<Vector3> CheckVector(Storage storage){
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

        public static List<Quaternion> CheckQuaternion(Storage storage){
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

        public static List<int> CheckBool()
        {
            List<int> BList = new List<int>();
            /* //get bool value
            if (Boolean.isActive() != boolean[0]){
                BList.Add(000);
                boolean[0] = Boolean.isActive();
            } */
            return BList;
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
            MethodInfo instantiateMethod = typeof(GameObject).GetMethod("Instantiate", new[] { typeof(GameObject) });
            GameObject returnobject = (GameObject)instantiateMethod.Invoke(null, new object[] { duplicate });
            return returnobject;
        }
    }
}
