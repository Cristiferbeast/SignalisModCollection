using MelonLoader;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;

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
        public List<string> MessageCollection = new List<string>(){};
        public int connectors = 0;

        //Boolean Variables
        public List<string> PENWreckRooms = new List<string>() { "Cryogenics", "Flight Deck", "Mess Hall" }; //we do not need rooms without boolean values
        public List<bool> BooleanList = new List<bool>() { false, false, false };

        //Readable Server Variables
        public bool host;
        public int ServerPort;
        public IPAddress ServerIP;
        public IPEndPoint ServerIPAddress;
        public UdpClient server;
        public string url;

        /* Legacy Code
         * public string debugmessage = "";
         */

    }
    public class SignalisMultiplayer : MelonMod
    {
        static public Storage storage;
        public override async void OnUpdate()
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
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.P) && (!storage.active) && (!storage.failsafe))
            {
                EllieSetUp();
                storage.active = true;
                if (storage.host)
                {
                    SigiServer.StartServer();
                }
                if (!storage.host)
                {
                    SigiClient.StartClient(storage.url);
                }
            }
            if (storage.active)
            {
                if (storage.host)
                {
                    string recievedMessage = SigiServer.GetMessage();
                    SignalisMultiplayer.HandleMessage(recievedMessage);
                    SignalisMultiplayer.AddMessages();
                    if (storage.MessageCollection.Count < 0)
                    {
                        foreach (string responseMessage in storage.MessageCollection)
                        {
                            SigiServer.ServerUpdate(responseMessage);
                        }
                    }
                }
                if (!storage.host)
                {
                    string recievedMessage = SigiClient.GetMessage();
                    SignalisMultiplayer.HandleMessage(recievedMessage);
                    SignalisMultiplayer.AddMessages();
                    if (storage.MessageCollection.Count < 0)
                    {
                        foreach (string responseMessage in storage.MessageCollection)
                        {
                            SigiClient.ClientUpdate(responseMessage);
                        }
                    }
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
                if (bool.TryParse(lines[0], out bool hostValue))
                {
                    storage.host = hostValue;
                    MelonLogger.Msg("Host Value Initalized");
                }
                storage.url = lines[1];
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Config file not found at path: " + MultiplayerModConfigPath);
            }
            return storage;
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
        public static void AddMessages()
        {
            List<Vector3> vvalue = CheckVector();
            if (vvalue != null)
            {
                foreach (Vector3 vecvalue in vvalue)
                {
                    string value = ("V:" + vecvalue.ToString());
                    storage.MessageCollection.Add(value);
                }
            }
            List<Quaternion> qvalue = CheckQuaternion();
            if (qvalue != null)
            {
                foreach (Quaternion quatervalue in qvalue)
                {
                    string value = ("Q:" + quatervalue.ToString());
                    storage.MessageCollection.Add(value);
                }
            }
            List<string> bkeyvalue = BooleanChecker();
            if (bkeyvalue != null)
            {
                foreach (string fkey in bkeyvalue)
                {
                    string value = ("B:" + fkey);
                    storage.MessageCollection.Add(value);
                }
            }
        }
        public static List<Vector3> CheckVector()
        {
            try
            {
                List<Vector3> VList = new List<Vector3>() { };
                Vector3 e = storage.EllieMain.transform.position;
                if(storage.l == null)
                {
                    storage.l = e;
                    return null;
                }
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
            catch (Exception ex)
            {
                MelonLogger.Msg("Failure on 453-Check Vector " + ex.Message + ex.StackTrace);
                return null;
            }
        }
        public static List<Quaternion> CheckQuaternion()
        {
            try
            {
                List<Quaternion> QList = new List<Quaternion>() { };
                Quaternion e = storage.EllieMain.transform.rotation * Quaternion.Euler(new Vector3(0f, 0f, 270f));
                if (storage.r == null)
                {
                    storage.r = e;
                    return null;
                }
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
        public static List<string> BooleanChecker()
        {
            List<string> InternalList = new List<string>();
            string name = SceneManager.GetActiveScene().name;
            switch (name)
            {
                case "PEN_Wreck":
                    int roomname = RoomChecker(storage.PENWreckRooms);
                    BooleanChecker(1, roomname, InternalList);
                    break;
                case "PEN_Hole":
                    //no boolean cases here :D
                    break;
                default:
                    break;
            }
            return InternalList;
        }
        public static int RoomChecker(List<string> secondarylist)
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
        public static void BooleanChecker(int index, int RoomName, List<string> InternalList)
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
        public static void MoveDestination(GameObject Elster, Vector3 incomingvector)
        {
            if (Elster.transform.position.z != incomingvector.z)
            {
                Elster.transform.position = incomingvector;
                return;
            }
            if (Mathf.Abs(Elster.transform.position.x - incomingvector.x) <= 200f || Mathf.Abs(Elster.transform.position.y - incomingvector.y) <= 200f)
            {
                Elster.transform.position = incomingvector;
                return;
            }
            AlternatePlayerController playerController = Elster.GetComponent<AlternatePlayerController>();
            Vector2 vector2 = incomingvector;
            while (playerController.lastPos != vector2)
            {
                Vector2 fakeInput;
                fakeInput.x = incomingvector.x - playerController.lastPos.x;
                fakeInput.y = incomingvector.y - playerController.lastPos.y;
                playerController.input = fakeInput;
            }
        }
        public static void HandleMessage(string message)
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
                            MoveDestination(storage.EllieClone, vector);
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
            if(message == "")
            {
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Server Recieved Unhandled Logic " + message);
            }
        }
        public static void ApplyBool(bool boolean, int tag)
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
            if (!storage.host)
            {
                SigiClient.DisconnectClient();
            }
        }
    }
}