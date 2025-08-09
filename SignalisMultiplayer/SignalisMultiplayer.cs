using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
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
        public bool EllieState = true;
        public GameObject EllieClone;
        public GameObject EllieMain;
        public Vector3 l;
        public Quaternion r;
        public List<string> StoredMessgaes = new List<string>() { };
        public List<string> MessageCollection = new List<string>() { };
        public List<string> BooleanQueue = new List<string>();
        public List<string> BooleanStorage = new List<string>();
        public Cheats Cheats;

        //Boolean Variables
        public List<string> PENWreckRooms = new List<string>() { "Cryogenics", "Flight Deck", "Mess Hall", "Personell" }; //we do not need rooms without boolean values
        public List<string> LOVReEducationRooms = new List<string>() { "", "Surface Access", "OverlookOffice", "Library", "Aula", "WestCorridor", "SafeClassroom" };
        public List<bool> BooleanList = Enumerable.Repeat(false, 60).ToList();
        public Dictionary<int, GameObject> ActiveEnemyList = new Dictionary<int, GameObject>() { }; //used by boolean handler to store active enemies 
        public Dictionary<int, GameObject> ManagedEnemies = new Dictionary<int, GameObject> { }; //used by Enemy Handler
        public Dictionary<int, string> TemporaryEnemyData = new Dictionary<int, string> { };
        public List<string> DETDetentionRooms = new List<string>() { "", "Office", "Pantry", "Rationing", "BathroomSouth", "Showers" };
        public List<string> EnemyData = new List<string>(); //used for storing enemy data
        public List<bool> EnemyBooleans = Enumerable.Repeat(false, 100).ToList(); //used for enemy states
        public List<int> EnemyHP = Enumerable.Repeat(0, 100).ToList(); //used for Enemy HP
        public bool inCutscene = false; //used for Raina Checks

        //Readable Server Variables
        public bool host;
        public int ServerPort;
        public string url;
        public SigiClient client;
        public SigiServer server;
        public string scenename;

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
                MelonLogger.Msg("Attempting to Load Mod");
                storage.EllieState = EllieSetUp();
                storage.scenename = SceneManager.GetActiveScene().name;
                if (!storage.EllieState)
                {
                    storage.EllieState = EllieSetUp();
                    storage.active = EllieSetUp();
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
            if (!storage.EllieState)
            {
                if (GameObject.Find("__Prerequisites__") != null)
                {
                    storage.EllieState = EllieSetUp(false);
                    storage.timer -= storage.delay;
                }
            }
            if (storage.EllieClone == null && storage.active)
            {
                storage.EllieState = EllieSetUp();
                storage.timer -= storage.delay;
            }
            SceneCheck(storage);
            if (storage.active && storage.EllieState)
            {
                storage.inCutscene = RainaCheck(storage);
                if (storage.EllieClone == null)
                {
                    storage.EllieState = EllieSetUp();
                    storage.timer -= storage.delay;
                }
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
        public static bool EllieSetUp(bool log = true)
        {

            if (GameObject.Find("__Prerequisites__").gameObject == null)
            {
                MelonLogger.Msg("Ellie is Not Established Yet");
                return false;
            }
            try
            {
                GameObject Prerequisties = GameObject.Find("__Prerequisites__").gameObject;
                GameObject CharOrigin = Prerequisties.transform.Find("Character Origin").gameObject;
                GameObject root = CharOrigin.transform.Find("Character Root").gameObject;
                storage.EllieMain = root.transform.Find("Ellie_Default").gameObject;
            }
            catch
            {
                MelonLogger.Msg("Ellie cannot be found");
                return false;
            }
            try
            {
                storage.EllieClone = UnityEngine.Object.Instantiate(storage.EllieMain);
                storage.EllieClone.GetComponent<Anchor>().enabled = false;
            }
            catch
            {
                MelonLogger.Msg("Ellie is unable to be Instantiated");
                return false;
            }
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
            try
            {
                storage.Cheats = GameObject.FindObjectOfType<Cheats>();
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Failure to Collect Cheat Device due to " + e.StackTrace);
            }
            MelonLogger.Msg("Ellie Established");
            return true;
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
                        int response = SBoolApply(s);
                        if (response == 1)
                        {
                            MelonLogger.Msg(s, " returned as applied");
                            itemsToRemove.Add(s);
                            MelonLogger.Msg(s, " added to list to remove");
                        }
                        if (response == 3)
                        {
                            itemsToRemove.Add(s);
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
                    int response = SBoolApply(s);
                    if (response == 1)
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
                SBoolApply(message); //this may no longer be used due to the new storage method, however it is being left in case we decide to switch off the ductape storage
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

        public static void SceneCheck(Storage storage)
        {
            if (storage.active && storage.EllieState)
            {
                if (SceneManager.GetActiveScene().name != storage.scenename)
                {
                    storage.EllieState = EllieSetUp();
                    if (!storage.EllieState)
                    {
                        storage.active = false;
                        storage.EllieState = false;
                    }
                    storage.scenename = SceneManager.GetActiveScene().name;
                }
            }
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
            List<string> bkeyvalue = BooleanReader();
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
        /*public static bool IsBoolSafe(int input)
        {
            return true;
            string name = SceneManager.GetActiveScene().name;
            switch (name)
            {
                case "PEN_Wreck":
                    if(input <= 6)
                    {
                        return false;
                    }
                    return true;
                case "PEN_Hole":
                    return false;
                case "LOV_Reeducation":
                    if (input > 6)
                    {
                        return false;
                    }
                    return true;
                default:
                    return false;
            }
        }*/
        public static List<string> BooleanReader()
        {
            List<string> InternalList = new List<string>();
            string name = SceneManager.GetActiveScene().name;
            switch (name)
            {
                case "PEN_Wreck":
                    int roomname = RoomChecker(storage.PENWreckRooms);
                    PEN_Wreck(roomname, InternalList);
                    break;
                case "PEN_Hole":
                    //no boolean cases here :D
                    break;
                case "LOV_Reeducation":
                    int roomname2 = RoomChecker(storage.LOVReEducationRooms);
                    LOV_ReEducation(roomname2, InternalList);
                    break;
                default:
                    break;
            }
            return InternalList;
        }
        public static void PEN_Wreck(int RoomName, List<string> InternalList)
        {
            switch (RoomName)
            {
                case 0:
                    //Cryopod
                    if (!storage.BooleanList[0])
                    {
                        try
                        {
                            PEN_Cryo Cryo = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject.transform.Find("Cryo").gameObject.transform.GetComponent<PEN_Cryo>();
                            storage.BooleanList[0] = Cryo.opened; //this is set true when the value is true
                            if (storage.BooleanList[0])
                            {
                                InternalList.Add("1,1");
                            }
                        }
                        catch {  /*unused debugging due to it working*/   }
                    }
                    if (!storage.BooleanList[5])
                    {
                        try
                        {
                            GameObject Chunk = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject;
                            if (storage.BooleanList[0] && !storage.BooleanList[4])
                            {
                                //we need to check that this whole song and dance has started
                                if (Chunk.transform.Find("Cryo").gameObject.transform.Find("ItemPickup_BrokenKey").gameObject.active)
                                {
                                    storage.BooleanList[5] = true;
                                    InternalList.Add("5,1");
                                }
                            }
                        }
                        catch { /* Unused Debugging */}
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
                case 3:
                    //Tayto Update cause Hes a Real One
                    if (!storage.BooleanList[3])
                    {
                        GameObject.Find("Personell").gameObject.transform.Find("Chunk").gameObject.transform.Find("Static Objects").gameObject.transform.Find("Bathroom").gameObject.SetActive(false);
                        GameObject.Find("Personell").gameObject.transform.Find("Chunk").gameObject.transform.Find("Lighting Low").gameObject.SetActive(true);
                        storage.BooleanList[3] = true;
                    }
                    break;
                default: break;
            }
        }
        public static void LOV_ReEducation(int RoomName, List<string> InternalList)
        {
            switch (RoomName)
            {
                case 1:
                    if (!storage.BooleanList[5])
                    {
                        GameObject Chunk = GameObject.Find("Surface Access").gameObject.transform.Find("Chunk").gameObject;
                        if (Chunk.transform.Find("ItemPickup_SurfaceKey") == null)
                        {
                            storage.BooleanList[5] = true;
                            InternalList.Add("6,1"); //remember it will be #case + 1 due to 0 indexing 
                        }
                    }
                    break;
                case 2:
                    if (!storage.BooleanList[6])
                    {
                        try
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_GuardOffice").gameObject;
                            if (Chunk != null && Chunk.active == true)
                            {
                                GameObject Drawer = Chunk.transform.Find("DrawerPivot (1)").gameObject.transform.Find("Desk_Drawer").gameObject;
                                if (Drawer.transform.Find("KeyPickup") == null)
                                {
                                    storage.BooleanList[6] = true;
                                    InternalList.Add("7,1");
                                }
                                GameObject SecondDrawer = Chunk.transform.Find("DrawerPivot (2)").gameObject.transform.Find("Desk_Drawer").gameObject;
                                SecondDrawer.transform.Find("Cluter (1)").gameObject.SetActive(true);
                                SecondDrawer.transform.Find("Cluter (1)_1").gameObject.SetActive(true);
                            }
                        }
                        catch { }
                    }
                    if (!storage.BooleanList[11])
                    {
                        try
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_GuardOffice").gameObject;
                            if (Chunk != null && Chunk.active == true)
                            {
                                GameObject Drawer = Chunk.transform.Find("DrawerPivot (1)").gameObject.transform.Find("Desk_Drawer").gameObject;
                                if (Drawer.transform.Find("PistolPickup") == null)
                                {
                                    storage.BooleanList[11] = true;
                                    InternalList.Add("12,1");
                                }
                            }
                        }
                        catch { }
                    }
                    break;
                case 3:
                    try
                    {
                        if (!storage.BooleanList[9])
                        {
                            GameObject Cutscene = GameObject.Find("Cutscenes").gameObject.transform.Find("Isa Cutscene Holder (OFF)").gameObject;
                            //only returns true after cutscene is played
                            if (Cutscene != null || Cutscene.active == true)
                            {
                                storage.BooleanList[9] = true;
                                InternalList.Add("10,1");
                            }
                        }
                    }
                    catch { }
                    break;
                case 4:
                    try
                    {
                        if (!storage.BooleanList[8])
                        {
                            GameObject Chunk = GameObject.Find("Aula").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_ObservationKey") == null)
                            {
                                storage.BooleanList[8] = true;
                                InternalList.Add("9,1");
                            }
                        }
                    }
                    catch { }
                    if (!storage.BooleanList[7])
                    {
                        //this is only true upon first loading into this room, when the cutscene plays
                        storage.BooleanList[7] = true;
                        InternalList.Add("8,1");
                    }
                    break;
                case 5:
                    break;
                case 6:
                    if (!storage.BooleanList[10])
                    {
                        GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_ClassSafe").gameObject;
                        if (Chunk != null || Chunk.active == true)
                        {
                            if (Chunk.transform.Find("Door").gameObject.transform.Find("Buttons").GetComponent<Keypad3D>().solved)
                            {
                                storage.BooleanList[10] = true;
                                InternalList.Add("11,1");
                            }
                        }
                    }
                    if (!storage.BooleanList[12])
                    {
                        GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_ClassSafe").gameObject;
                        if (Chunk != null || Chunk.active == true)
                        {
                            if (Chunk.transform.Find("Card").gameObject.transform.Find("ItemPickup") == null)
                            {
                                storage.BooleanList[12] = true;
                                InternalList.Add("13,1");
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public static void DET_Detention(int RoomName, List<string> InternalList)
        {
            switch (RoomName)
            {
                case 0:
                    break;
                case 1:
                    try
                    {
                        if (!storage.BooleanList[13])
                        {
                            GameObject Chunk = GameObject.Find("Office").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_EastKey") == null)
                            {
                                storage.BooleanList[13] = true;
                                InternalList.Add("14,1");
                            }
                        }
                    }
                    catch { }
                    break;
                case 2:
                    try
                    {
                        if (!storage.BooleanList[14])
                        {
                            GameObject Chunk = GameObject.Find("Pantry").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_MensaKey") == null)
                            {
                                storage.BooleanList[14] = true;
                                InternalList.Add("15,1");
                            }
                        }
                    }
                    catch { }
                    break;
                case 3:
                    try
                    {
                        if (!storage.BooleanList[15])
                        {
                            GameObject Chunk = GameObject.Find("Rationing").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_ShowerKey") == null)
                            {
                                storage.BooleanList[15] = true;
                                InternalList.Add("16,1");
                            }
                        }
                    }
                    catch { }
                    break;
                case 4:
                    try
                    {
                        if (!storage.BooleanList[16])
                        {
                            GameObject Chunk = GameObject.Find("BathroomSouth").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_BoneKeyA") == null)
                            {
                                storage.BooleanList[16] = true;
                                InternalList.Add("17,1");
                            }
                        }
                    }
                    catch { }
                    break;
                case 5:
                    try
                    {
                        if (!storage.BooleanList[17])
                        {
                            GameObject Chunk = GameObject.Find("Showers").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_BoneKeyB") == null)
                            {
                                storage.BooleanList[17] = true;
                                InternalList.Add("18,1");
                            }
                        }
                    }
                    catch { }
                    break;
                default:
                    break;
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
                        return secondarylist.IndexOf(s); //this isnt flawless, and may interfere with other things in export requiring reformatting 
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
                float difference = e.z - l.z; //if l > - if e > +
                if (Math.Abs(difference) < 1)
                {
                    //if e.z - l.z or new - old < 10 that means slight variation so set e(new) to l(old)
                    e.z = l.z;
                }
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
        public static int SBoolApply(string message)
        {
            MelonLogger.Msg(message);
            //Handles Boolean Values
            int colonIndex = message.IndexOf(':');
            if (colonIndex == -1 || colonIndex > message.Length - 1)
            {
                return 3;
            }
            string dataPart = message.Substring(colonIndex + 1).Trim();
            //Splits the Two Types of Data
            string[] parts = dataPart.Split(',');
            if (parts.Length != 2)
            {
                MelonLoader.MelonLogger.Msg("Expected 2 values for the message.");
                return 3;
            }
            if (int.TryParse(parts[0].Trim(), out int tag) && int.TryParse(parts[1].Trim(), out int boolValue))
            {
                bool boolean = (boolValue == 1);
                //This creates a Bool value and a int value that can be used to find the reference bool
                MelonLogger.Msg("Bool being Applied " + boolean + " " + tag + " " + message);
                //if (!IsBoolSafe(tag)) { MelonLogger.Msg("Players in Different Scenes to Avoid Crash, Message Denied"); return 3; }
                bool result = BooleanApply(boolean, tag, message);
                MelonLogger.Msg("Bool Application State " + result);
                if (result == true) { return 1; }
                else return 2;
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Error parsing values.");
                return 3;
            }
        }
        public static bool BooleanApply(bool boolean, int tag, string message)
        {
            try
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
                    case 4:
                        MelonLogger.Msg("That wasn't supposed to happen - Case 4 Failure");
                        return true;
                    case 5:
                        if (!storage.BooleanList[4])
                        {
                            GameObject Chunk = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Cryo").gameObject.transform.Find("ItemPickup_BrokenKey").gameObject.SetActive(true);
                            Chunk.transform.Find("Cryo").gameObject.transform.Find("ItemPickup_BrokenKey").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("Cryo").gameObject.transform.Find("ItemPickup_BrokenKey").gameObject.SetActive(false);
                            storage.BooleanList[5] = boolean;
                        }
                        return true;
                    case 6:
                        if (!storage.BooleanList[5])
                        {
                            GameObject Chunk = GameObject.Find("Surface Access").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_SurfaceKey").gameObject.SetActive(true);
                            Chunk.transform.Find("ItemPickup_SurfaceKey").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("ItemPickup_SurfaceKey").gameObject.SetActive(false);
                            storage.BooleanList[5] = boolean;
                            return true;
                        }
                        return false;
                    case 7:
                        if (!storage.BooleanList[6])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_GuardOffice").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            GameObject Drawer = Chunk.transform.Find("DrawerPivot (1)").gameObject.transform.Find("Desk_Drawer").gameObject;
                            if (Drawer == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Drawer.transform.Find("PistolPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Drawer.transform.Find("PistolPickup").gameObject.SetActive(false);
                            storage.BooleanList[6] = boolean;
                            GameObject SecondDrawer = Chunk.transform.Find("DrawerPivot (2)").gameObject.transform.Find("Desk_Drawer").gameObject;
                            SecondDrawer.transform.Find("Cluter (1)").gameObject.SetActive(true);
                            SecondDrawer.transform.Find("Cluter (1)_1").gameObject.SetActive(true);
                            return true;
                        }
                        return false;
                    case 8:
                        if (!storage.BooleanList[7])
                        {
                            try
                            {
                                storage.Cheats.cheats("goto aula");
                                storage.BooleanList[7] = true;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        return true;
                    case 9:
                        if (!storage.BooleanList[8])
                        {
                            GameObject Chunk = GameObject.Find(storage.LOVReEducationRooms[4]).gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_ObservationKey").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("ItemPickup_ObservationKey").gameObject.SetActive(false);
                            storage.BooleanList[8] = boolean;
                            return true;
                        }
                        return false;
                    case 10:
                        if (!storage.BooleanList[9])
                        {
                            storage.Cheats.cheats("goto Library");
                            GameObject Cutscene = GameObject.Find("Cutscenes").gameObject.transform.Find("Isa Cutscene Holder (OFF)").gameObject;
                            Cutscene.transform.Find("Isa Intro").gameObject.GetComponent<CutsceneManager>().StartCutscene();
                            storage.BooleanList[9] = true;
                            return true;
                        }
                        return false;
                    case 11:
                        if (!storage.BooleanList[10])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_ClassSafe").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Door").gameObject.transform.Find("Buttons").GetComponent<Keypad3D>().solution = "";
                            storage.BooleanList[10] = boolean;
                            return true;
                        }
                        return false;
                    case 12:
                        if (!storage.BooleanList[11])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_GuardOffice").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            GameObject Drawer = Chunk.transform.Find("DrawerPivot (1)").gameObject.transform.Find("Desk_Drawer").gameObject;
                            if (Drawer == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Drawer.transform.Find("KeyPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Drawer.transform.Find("KeyPickup").gameObject.SetActive(false);
                            storage.BooleanList[11] = boolean;
                            return true;
                        }
                        return false;
                    case 13:
                        if (!storage.BooleanList[12])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_ClassSafe").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Card").gameObject.transform.Find("ItemPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("Card").gameObject.transform.Find("ItemPickup").gameObject.SetActive(false);
                            storage.BooleanList[12] = boolean;
                            return true;
                        }
                        return false;
                    default:
                        return true;
                    case 14:
                        //Office - East Key
                        if (!storage.BooleanList[13])
                        {
                            GameObject Chunk = GameObject.Find(storage.DETDetentionRooms[1]).gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_EastKey").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("ItemPickup_EastKey").gameObject.SetActive(false);
                            storage.BooleanList[13] = boolean;
                            return true;
                        }
                        return false;
                    case 15:
                        //Pantry - Mensa Key
                        if (!storage.BooleanList[14])
                        {
                            GameObject Chunk = GameObject.Find(storage.DETDetentionRooms[2]).gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_MensaKey").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("ItemPickup_MensaKey").gameObject.SetActive(false);
                            storage.BooleanList[14] = boolean;
                            return true;
                        }
                        return false;
                    case 16:
                        //Rations Office - "Shower" Key (West Key)
                        if (!storage.BooleanList[15])
                        {
                            GameObject Chunk = GameObject.Find(storage.DETDetentionRooms[3]).gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_ShowerKey").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("ItemPickup_ShowerKey").gameObject.SetActive(false);
                            storage.BooleanList[15] = boolean;
                            return true;
                        }
                        return false;
                    case 17:
                        /*
                        Southern Bathroom - Butterfly Key A
                        Showers - Butterfly Key B
                        */
                        if (!storage.BooleanList[16])
                        {
                            GameObject Chunk = GameObject.Find(storage.DETDetentionRooms[4]).gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_BoneKeyA").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("ItemPickup_BoneKeyA").gameObject.SetActive(false);
                            storage.BooleanList[16] = boolean;
                            return true;
                        }
                        return false;
                    case 18:
                        if (!storage.BooleanList[17])
                        {
                            GameObject Chunk = GameObject.Find(storage.DETDetentionRooms[5]).gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_BoneKeyB").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Chunk.transform.Find("ItemPickup_BoneKeyB").gameObject.SetActive(false);
                            storage.BooleanList[17] = boolean;
                            return true;
                        }
                        return false;
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error(e.StackTrace + e.Message + e.Source);
                MelonLogger.Msg(tag + message);
                return false;
            }
        }

        //Handles End State
        public override void OnApplicationQuit()
        {
        }


        //Planned for 0.10.0 
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
            SBoolApply(data[3]);
        }
        public static bool RainaCheck(Storage storage)
        {
            if (PlayerState.cutscene)
            {
                return false;
            }
            return true;
        }
    }
}