using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SigiMultiplayer
{
    public class Storage
    {
        //Variables for Mod
        public float delay = 2; //Sets the Delay 
        public float timer = 0; //Records the Delay
        public bool failsafe;
        public bool active = false; //Defines Active State of Mod
        public GameObject EllieClone;
        public GameObject EllieMain;
        public Vector3 l;
        public Quaternion r;
        public List<string> StoredMessgaes = new List<string>() { };
        public List<string> MessageCollection = new List<string>() { };
        public List<string> BooleanQueue = new List<string>();
        public List<string> BooleanStorage = new List<string>();    

        //Boolean Variables
        public List<string> PENWreckRooms = new List<string>() { "Cryogenics", "Flight Deck", "Mess Hall" }; //we do not need rooms without boolean values
        public List<bool> BooleanList = new List<bool>() { false, false, false };
        public Dictionary<int, GameObject> ActiveEnemyList = new Dictionary<int, GameObject>() { }; //used by boolean handler to store active enemies 
        public Dictionary<int, GameObject> ManagedEnemies = new Dictionary<int, GameObject> { }; //used by Enemy Handler
        public Dictionary<int, string> TemporaryEnemyData = new Dictionary<int, string> { };

        //Readable Server Variables
        public bool host;
        public int ServerPort;
        public string url;
        public SigiClient client;
        public SigiServer server;
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
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.P) && (!storage.active) && (!storage.failsafe))
            {
                bool temp = EllieSetUp();
                if (!temp)
                {
                    return;
                }
                if (storage.host)
                {
                    storage.server.StartServer();
                }
                if (!storage.host)
                {
                    storage.client.StartClient(storage.url);
                }
                storage.EllieClone = GameObject.Find("Ellie_Default(Clone)").gameObject;
                storage.active = true;
            }
            if (storage.active)
            {
                storage.timer += 1;
                if (storage.host && storage.timer > storage.delay)
                {
                    MessageCentralHost();
                    storage.timer -= storage.delay;
                }
                if (!storage.host && storage.timer > storage.delay)
                {
                    MessageCentralClient();
                    storage.timer -= storage.delay;
                }
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
                                    InternalList.Add("1,1");
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
                                    InternalList.Add("2,1");
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
                                    InternalList.Add("3,1");
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
        public static void EnemyReaderLogic()
        {
            //When an enemy is in a room, the boolean checker will add the value of that rooms enemy tag 
            if (storage.ActiveEnemyList.Count == 0)
            {
                return;
            }
            foreach (int enemy in storage.ActiveEnemyList.Keys)
            {
                EnemyDetails(storage.ActiveEnemyList[enemy], enemy);
            }
        }
        public static string EnemyDetails(GameObject Enemy, int tag)
        {
            if (Enemy == null)
            {
                Console.WriteLine("Enemy Returned Null Error");
                return null;
            }
            string Details = $"E:{tag + 1}>";
            string FDet = "";
            if (storage.TemporaryEnemyData[tag] == null)
            {
                Details += "V:" + Enemy.transform.position.ToString() + ">";
                Details += "Q:" + Enemy.transform.rotation.ToString() + ">";
                Details += "B:" + true + ">"; //replace with dead logic
                FDet = Details;
            }
            else
            {
                string[] data = storage.TemporaryEnemyData[tag].Split('>');
                FDet = Details;
                if ("V:" + Enemy.transform.position.ToString() != data[1])
                {
                    Details += "V:" + Enemy.transform.position.ToString() + ">";
                }
                FDet += "V:" + Enemy.transform.position.ToString() + ">";
                if ("Q:" + Enemy.transform.rotation.ToString() + ">" != data[2])
                {
                    Details += "Q:" + Enemy.transform.rotation.ToString() + ">";
                }
                FDet += "Q:" + Enemy.transform.rotation.ToString() + ">";
                if ("B:" + true + ">" != data[3])
                {
                    Details += "B:" + true + ">"; //replace with dead logic
                }
                FDet += "B:" + true + ">"; //replace with dead logic
            }
            storage.TemporaryEnemyData[tag] = FDet;
            return Details;
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
            else if (message.StartsWith("V:"))
            {
                ApplyVector(storage.EllieClone, message);
            }
            else if (message.StartsWith("Q:"))
            {
                ApplyQuaternion(storage.EllieClone, message);
            }
            else if (message.StartsWith("B:"))
            {
                ApplyBool(message);
            }
            else if (message.StartsWith("E:"))
            {
                EnemyMessageReaderPrime(message);
            }
            else if (message == "")
            {
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Server Recieved Unhandled Logic " + message);
            }
        }
        public static void ApplyBool(string message)
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
                        ApplyBool(boolean, tag, message);
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
        public static void ApplyBool(bool boolean, int tag, string message)
        {
            switch (tag)
            {
                case 1:
                    if (!storage.BooleanList[0])
                    {
                        GameObject Chunk = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject;
                        if (Chunk == null)
                        {
                            storage.BooleanQueue.Add(message);
                            return;
                        }
                        PEN_Cryo Cryo = Chunk.transform.Find("Cryo").gameObject.transform.GetComponent<PEN_Cryo>();
                        Cryo.Open(); //Plays Cutscene Fixes Boolean Mismatch
                        storage.BooleanList[0] = boolean; //this is set true when the value is true
                    }
                    break;
                case 2:
                    if (!storage.BooleanList[1])
                    {
                        GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("Pen_CockpitEvent3D").gameObject;
                        if (Chunk == null)
                        {
                            storage.BooleanQueue.Add(message);
                            return;
                        }
                        GameObject Cockpit = Chunk.transform.Find("Interactions").gameObject;
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
                        if (Ducttape == null)
                        {
                            storage.BooleanQueue.Add(message);
                            return;
                        }
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
        public static GameObject ApplyEnemy(bool boolean, int tag)
        {
            GameObject returnvalue;
            switch (tag)
            {
                case 1:
                    returnvalue = new GameObject();
                    break;
                default:
                    returnvalue = null;
                    break;
            }
            return returnvalue;
        }
        public static void EnemyMessageReaderPrime(string message)
        {
            //All Enemy Units must have 2 digits of Boolean, First digit is active state, Second digit is primacy. If Active State is true, then opposite has primacy, if it is false then current has primacy
            string[] data = message.Split('>');
            int outtag = int.Parse(data[0]);
            if (storage.ManagedEnemies[outtag] == null)
            {
                storage.ManagedEnemies[outtag] = ApplyEnemy(true, outtag);
            }
            ApplyVector(storage.ManagedEnemies[outtag], data[1]);
            ApplyQuaternion(storage.ManagedEnemies[outtag], data[2]);
            ApplyBool(data[3]);
        }
        public static bool BooleanApply(bool boolean, int tag, string message)
        {
            switch (tag)
            {
                case 1:
                    if (!storage.BooleanList[0])
                    {
                        GameObject Chunk = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject;
                        if (Chunk == null)
                        {
                            storage.BooleanQueue.Add(message);
                            return false;
                        }
                        Chunk.active = true;
                        PEN_Cryo Cryo = Chunk.transform.Find("Cryo").gameObject.transform.GetComponent<PEN_Cryo>();
                        Cryo.Open(); //Plays Cutscene Fixes Boolean Mismatch
                        storage.BooleanList[0] = boolean; //this is set true when the value is true
                        return true;
                    }
                    return true;
                    break;
                case 2:
                    if (!storage.BooleanList[1])
                    {
                        GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("Pen_CockpitEvent3D").gameObject;
                        if (Chunk == null)
                        {
                            storage.BooleanQueue.Add(message);
                            return false;
                        }
                        GameObject Cockpit = Chunk.transform.Find("Interactions").gameObject;
                        if (Cockpit.transform.Find("PhotoPickup") != null)
                        {
                            Cockpit.transform.Find("PhotoPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Cockpit.transform.Find("PhotoPickup").gameObject.SetActive(false);
                            storage.BooleanList[1] = boolean;
                        }
                        return true;
                    }
                    return true;
                    break;
                case 3:
                    if (!storage.BooleanList[2])
                    {
                        GameObject Ducttape = GameObject.Find("Events").gameObject.transform.Find("PEN_PC").gameObject;
                        if (Ducttape == null)
                        {
                            storage.BooleanQueue.Add(message);
                            return false;
                        }
                        if (Ducttape.transform.Find("TapePickup") != null)
                        {
                            Ducttape.transform.Find("TapePickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Ducttape.transform.Find("TapePickup").gameObject.SetActive(false);
                            storage.BooleanList[2] = boolean;
                        }
                        return true;
                    }
                    return true;
                    break;
                default:
                    return false;
                    break;
            }
        }




        //Approved for Full Rewrite - 0.9
        //Set Up
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
            if (storage.host)
            {
                storage.server = new SigiServer();
            }
            if (!storage.host)
            {
                storage.client = new SigiClient();
            }
            return storage;
        }
        public static bool EllieSetUp()
        {
            try
            {
                if (GameObject.Find("__Prerequisites__").gameObject == null)
                {
                    MelonLogger.Msg("Ellie is Not Established Yet");
                    return false;
                }
                GameObject Prerequisties = GameObject.Find("__Prerequisites__").gameObject;
                GameObject CharOrigin = Prerequisties.transform.Find("Character Origin").gameObject;
                GameObject root = CharOrigin.transform.Find("Character Root").gameObject;
                storage.EllieMain = root.transform.Find("Ellie_Default").gameObject;
                storage.EllieClone = UnityEngine.Object.Instantiate(storage.EllieMain);
                storage.EllieClone.GetComponent<Anchor>().enabled = false;
                if (storage.EllieClone == null) { storage.active = false; MelonLogger.Msg("Ellie is Null"); return false; }
                try
                {
                    storage.l = storage.EllieClone.transform.position;
                    storage.r = storage.EllieClone.transform.rotation;
                }
                catch
                {
                    MelonLogger.Msg("Failure Prior to 309 due to Storage Postional Storing");
                    return false;
                }
                MelonLogger.Msg("Ellie Established");
                return true;
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Msg("Error Setting up Ellie " + e.StackTrace + " " + e.TargetSite);
                return false;
            }
        }

        //Key Runtime 
        public static void MessageCentralHost()
        {
            var itemsToRemove = new List<string>();
            if (storage.server.GetPlayerCount() == 0)
            {
                return;
            }
            else
            {
                List<string> Movement = storage.server.GetPlayerMovement();
                MessageParse(Movement[0]);
                MessageParse(Movement[1]);
                storage.MessageCollection.Clear();
                SignalisMultiplayer.AddMessages();
                if (storage.MessageCollection.Count > 0)
                {
                    string send = "";
                    foreach (string responseMessage in storage.MessageCollection)
                    {
                        send += $"~{responseMessage}";
                    }
                    if (send != "")
                    {
                        storage.server.UdpServerUpdate(send);
                    }
                }
                foreach (string s in storage.server.BList)
                {
                    MelonLogger.Msg(s);
                    if (s == null || s == "")
                    {
                        itemsToRemove.Add(s);
                    }
                    else
                    {
                        bool response = SBoolApply(s);
                        if (response)
                        {
                            MelonLogger.Msg(s, " returned as applied");
                            itemsToRemove.Add(s);
                            MelonLogger.Msg(s, " added to list to remove");
                        }
                    }
                }
                foreach (string s in itemsToRemove)
                {
                    storage.server.BList.Remove(s);
                }
            }
        }
        public static void MessageCentralClient()
        {
            var itemsToRemove = new List<string>();
            List<string> Movement = storage.client.GetPlayerMovement();
            MessageParse(Movement[0]);
            MessageParse(Movement[1]);
            storage.MessageCollection.Clear();
            SignalisMultiplayer.AddMessages();
            if (storage.MessageCollection.Count > 0)
            {
                string send = "";
                foreach (string responseMessage in storage.MessageCollection)
                {
                    send += $"~{responseMessage}";
                }
                if (send != "" || send != "~")
                {
                    storage.client.UdpClientUpdate(send);
                }
            }
            foreach (string s in storage.client.BList)
            {
                MelonLogger.Msg(s);
                if (s == null || s == "")
                {
                    itemsToRemove.Add(s);
                }
                else
                {
                    bool response = SBoolApply(s);
                    if (response)
                    {
                        itemsToRemove.Add(s);
                    }
                }
            }
            foreach (string s in itemsToRemove)
            {
                storage.client.BList.Remove(s);
            }
        }
        public static bool MessageParse(string recievedMessage)
        {
            if (recievedMessage == null || recievedMessage == "")
            {
                return false;
            }
            SignalisMultiplayer.HandleMessage(recievedMessage);
            return true;
        }

        //Read
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
                    MelonLogger.Msg(value);
                    storage.MessageCollection.Add(value);
                }
            }
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
        public static List<Vector3> CheckVector()
        {
            try
            {
                List<Vector3> VList = new List<Vector3>() { };
                Vector3 e = storage.EllieMain.transform.position;
                if (storage.l == null)
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
                Quaternion e = storage.EllieMain.transform.rotation;
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

        //Implement
        public static void ApplyVector(GameObject item, string message)
        {
            //Handles Vector Transforms Passed by Server, To Be Used if Ellie Moves
            int openParenIndex = message.IndexOf('(');
            int closeParenIndex = message.IndexOf(')');
            if (openParenIndex != -1 && closeParenIndex != -1 && closeParenIndex > openParenIndex)
            {
                string numbersPart = message.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();
                string[] numberStrings = numbersPart.Split(',');
                if (numberStrings.Length == 3)
                {
                    if (float.TryParse(numberStrings[0].Trim(), out float x) && float.TryParse(numberStrings[1].Trim(), out float y) && float.TryParse(numberStrings[2].Trim(), out float z))
                    {
                        Vector3 vector = new Vector3(x, y, z);
                        item.transform.position = vector;
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
        public static void ApplyQuaternion(GameObject item, string message)
        {
            //Handles Rotations Passed by The Server, To be Used if Ellie Rotates
            int colonIndex = message.IndexOf(':');
            int openParenIndex = message.IndexOf('(');
            int closeParenIndex = message.IndexOf(')');

            if (colonIndex != -1 && openParenIndex != -1 && closeParenIndex != -1 && colonIndex < openParenIndex && openParenIndex < closeParenIndex)
            {
                string numbersPart = message.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();
                string[] numberStrings = numbersPart.Split(',');
                if (numberStrings.Length == 4)
                {
                    if (float.TryParse(numberStrings[0].Trim(), out float x) &&
                        float.TryParse(numberStrings[1].Trim(), out float y) &&
                        float.TryParse(numberStrings[2].Trim(), out float z) &&
                        float.TryParse(numberStrings[3].Trim(), out float w))
                    {
                        Quaternion quaternion = new Quaternion(x, y, z, w);
                        item.transform.rotation = quaternion;
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Msg("Error parsing numbers.");
                    }
                }
                else
                {
                    MelonLoader.MelonLogger.Msg("Expected 4 numbers for quaternion.");
                }
            }
        }
        public static bool SBoolApply(string message)
        {
            MelonLogger.Msg(message);
            //Handles Boolean Values
            int colonIndex = message.IndexOf(':');
            if (colonIndex == -1 || colonIndex > message.Length - 1)
            {
                return false;
            }
            string dataPart = message.Substring(colonIndex + 1).Trim();
            //Splits the Two Types of Data
            string[] parts = dataPart.Split(',');
            if (parts.Length != 2)
            {
                MelonLoader.MelonLogger.Msg("Expected 2 values for the message.");
                return false;
            }
            if (int.TryParse(parts[0].Trim(), out int tag) && int.TryParse(parts[1].Trim(), out int boolValue))
            {
                bool boolean = (boolValue == 1);
                //This creates a Bool value and a int value that can be used to find the reference bool
                MelonLogger.Msg("Bool being Applied " + boolean + " " + tag + " " + message);
                bool result = BooleanApply(boolean, tag, message);
                MelonLogger.Msg("Bool Application State " + result);
                return result;
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Error parsing values.");
                return false;
            }
        }

        //Handles End State
        public override void OnApplicationQuit()
        {
        }
    }
}