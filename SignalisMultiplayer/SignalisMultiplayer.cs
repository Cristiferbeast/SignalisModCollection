using MelonLoader;
using Rotorz.Tile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string scenename; //used to track active scene
        public bool inCutscene = false; //used for Raina Checks

        //Boolean Variables
        public List<string> PENWreckRooms = new List<string>() { "Cryogenics", "Flight Deck", "Mess Hall", "Personell" }; //we do not need rooms without boolean values
        public List<string> LOVReEducationRooms = new List<string>() { "", "Surface Access", "OverlookOffice", "Library", "Aula", "WestCorridor", "SafeClassroom" };
        public List<string> DETDetentionRooms = new List<string>() { "", "Office", "Pantry", "Rationing", "BathroomSouth", "Showers", "EvidenceStorage", "MensaCorridor", "Kitchen", "Isolation", "SouthWestCorridor", "CellBlockCorridor", "Lockers", "Mensa", "North West Corridor" };
        public List<string> DETEvents = new List<string>() { "DET_Elevator", "ROT_RadioStation", "DET_DoorServiceHatch", "DET_TreeSafe" };
        public List<bool> BooleanList = Enumerable.Repeat(false, 60).ToList();
        
        //Enemy Variables
        public Dictionary<int, GameObject> ManagedEnemies = new Dictionary<int, GameObject> { }; //used by Enemy Handler
        public Dictionary<int, int> EnemyHP = new Dictionary<int, int>();
        public Dictionary<int, Vector3> EnemyLocation = new Dictionary<int, Vector3>();
        public Dictionary<int, Quaternion> EnemyRotation = new Dictionary<int, Quaternion>();

        //Readable Server Variables
        public bool host;
        public int ServerPort;
        public string url;
        public SigiClient client;
        public SigiServer server;
        public bool neardeath;
        public bool dead;
    }
    public class SignalisMultiplayer : MelonMod
    {
        static public Storage storage;
        public override async void OnUpdate()
        {
            if (storage == null) // This is the logic that runs to ensure that the storage system gets set up, without storage nothing works.
            {
                storage = StorageSetUp();
            }
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKeyDown(KeyCode.P) && (!storage.active) && (!storage.failsafe)) // Mod Activation
            {
                storage.scenename = SceneManager.GetActiveScene().name; //set active scene
                MelonLogger.Msg("Attempting to Load Mod");
                storage.EllieState = EllieSetUp(); // Load Ellie (Notorious for being the most bug ridden section of the code.)
                if (!storage.EllieState) //if storage is saying Ellie is inactive then fix that.
                {
                    storage.EllieState = EllieSetUp();
                    storage.active = storage.EllieState; //Storage is Active as is EllieState.
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
                storage.EllieClone = GameObject.Find("Ellie_Default(Clone)").gameObject; //Storage now assigns the clone to be the newly created Ellie.
                storage.active = true;
                MelonLogger.Msg("Mod Successfully Loaded");
            }
            if (!storage.EllieState) //if ellie inactive fix that.
            {
                if (GameObject.Find("__Prerequisites__") != null)
                {
                    storage.EllieState = EllieSetUp();
                    storage.timer -= storage.delay;
                    MelonLogger.Msg("Ellie Restored ", storage.EllieState);
                    storage.active = storage.EllieState;
                }
            }
            if (!SceneCheck(storage)) { return; }; //Check Active Scene, see if it has changed.
            if (storage.EllieClone == null && storage.active) //Ellie Clone is missing, Fix That
            {
                storage.EllieState = EllieSetUp();
                storage.timer -= storage.delay;
                MelonLogger.Msg("Ellie Restored at 93", storage.EllieState);
            }
            if (storage.active && storage.EllieState) //Normal Runtime.
            {
                storage.inCutscene = RainaCheck(storage); //Assign Raina Check
                if (storage.inCutscene)
                {
                    return;
                }
                if (storage.EllieClone == null)
                {
                    storage.EllieState = EllieSetUp();
                    storage.timer -= storage.delay;
                    return; //Return to allow for Retoggle.
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
            try
            {
                if (GameObject.Find("__Prerequisites__").gameObject == null)
                {
                    MelonLogger.Msg("Ellie is Not Established Yet");
                    return false;
                }
            }
            catch
            {
                MelonLogger.Msg("Preq not Established");
            }
            try
            {
                GameObject Prerequisties = GameObject.Find("__Prerequisites__").gameObject;
                GameObject CharOrigin = Prerequisties.transform.Find("Character Origin").gameObject;
                GameObject root = CharOrigin.transform.Find("Character Root").gameObject;
                storage.EllieMain = root.transform.Find("Ellie_Default").gameObject;
                storage.dead = false;
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
                storage.EllieClone.GetComponent<PlayerAim>().enabled = false;
                storage.EllieClone.transform.Find("HurtIKTarget").gameObject.SetActive(false);
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
            storage.active = true;
            storage.EllieState = true;
            return true;
        }

        //Key Runtime 
        public static void MessageCentralHost()
        {
            try
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
            catch (Exception e)
            {
                MelonLogger.Msg("Error on 284; " + e.Message + e.StackTrace);
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
            else if (message.StartsWith("ED:"))
            {
                DealDamage(message);
            }
            else if (message == "")
            {
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Server Recieved Unhandled Logic " + message);
            }
        }

        //Safety Checks
        public static bool SceneCheck(Storage storage)
        {
            try
            {
                if (storage.active && storage.EllieState)
                {
                    if (SceneManager.GetActiveScene().name != storage.scenename)
                    {
                        storage.EllieState = EllieSetUp();
                        storage.scenename = SceneManager.GetActiveScene().name;
                        storage.BooleanQueue.Clear();
                    }
                }
                return true;
            }
            catch
            {
                MelonLogger.Msg("Failure on Scene Check Code on 373");
                return false;
            }
        }
        public static bool RainaCheck(Storage storage)
        {
            if (PlayerState.cutscene)
            {
                return true;
            }
            return false;
        }

        //Read
        public static void AddMessages()
        {
            PeerSideEnemyHandler(storage);
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
        public static List<string> BooleanReader()
        {
            List<string> InternalList = new List<string>();
            DeathHandler(InternalList, storage);
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
                case "DET_Detention":
                    int roomname3 = RoomChecker(storage.DETDetentionRooms);
                    DET_Detention(roomname3, InternalList);
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
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(1))
                        {
                            GameObject Enemy = GameObject.Find("WestCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 1);
                            }
                        }
                    }
                    catch(Exception e) { MelonLogger.Msg("Failure on Room Case 5, Bool 20, Enemy Handler " + e.Message + e.StackTrace ); }
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
        public static void DET_Detention(int RoomName, List<string> InternalList) {
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
                        if (!storage.ManagedEnemies.ContainsKey(10))
                        {
                            GameObject Enemy = GameObject.Find("Pantry").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 10);
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
                            GameObject Chunk = GameObject.Find("BathroomSouth").gameObject.transform.Find("Chunk").gameObject.transform.Find("Objects").gameObject;
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
                            GameObject Chunk = GameObject.Find("Showers").gameObject.transform.Find("Chunk").gameObject.transform.Find("Objects").gameObject;
                            if (Chunk.transform.Find("ItemPickup_BoneKeyB") == null)
                            {
                                storage.BooleanList[17] = true;
                                InternalList.Add("18,1");
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(3))
                        {
                            GameObject Enemy = GameObject.Find("Showers").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 2);
                            }
                        }
                    }
                    catch { }
                    break;
                case 6:
                    /*try
                    {
                        if (!storage.BooleanList[17])
                        {
                            GameObject Chunk = GameObject.Find("EvidenceStorage").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ButterflyBoxEvent") == null)
                            {
                                storage.BooleanList[17] = true;
                                InternalList.Add("18,1");
                            }
                        }
                    }
                    catch { }
                    break;*/
                case 7:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(2))
                        {
                            GameObject Enemy = GameObject.Find("MensaCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 Star").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 2);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 7, Bool 2, Enemy Handler " + e.Message + e.StackTrace); }
                    break;
                case 8: 
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(4))
                        {
                            GameObject Enemy = GameObject.Find("Kitchen").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 4);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 8, Bool 3, Enemy Handler " + e.Message + e.StackTrace); }
                    break;
                case 9:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(5))
                        {
                            GameObject Enemy = GameObject.Find("Isolation").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 5);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 9, Bool 5, Enemy Handler " + e.Message + e.StackTrace); }
                    break;
                case 10:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(6))
                        {
                            GameObject Enemy = GameObject.Find("SouthWestCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 6);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 10, Bool 6, Enemy Handler " + e.Message + e.StackTrace); }
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(7))
                        {
                            GameObject Enemy = GameObject.Find("SouthWestCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("EULR (1)").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 7);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 10, Bool 7, Enemy Handler " + e.Message + e.StackTrace); }
                    break;
                case 11:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(8))
                        {
                            GameObject Enemy = GameObject.Find("CellBlockCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 8);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 10, Bool 8, Enemy Handler " + e.Message + e.StackTrace); }
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(9))
                        {
                            GameObject Enemy = GameObject.Find("CellBlockCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("EULR (1)").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 9);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 10, Bool 9, Enemy Handler " + e.Message + e.StackTrace); }
                    break;
                case 12:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(11))
                        {
                            GameObject Enemy = GameObject.Find("Lockers").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 11);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 10, Bool 11, Enemy Handler " + e.Message + e.StackTrace); }
                    break;
                case 13:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(12))
                        {
                            GameObject Enemy = GameObject.Find("Mensa").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 12);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 13, Bool 12, Enemy Handler " + e.Message + e.StackTrace); }
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(13))
                        {
                            GameObject Enemy = GameObject.Find("Mensa").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 2 STAR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 13);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 13, Bool 13, Enemy Handler " + e.Message + e.StackTrace); }
                    break;
                case 14:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(14))
                        {
                            GameObject Enemy = GameObject.Find("North West Corridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 14);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 14, Bool 14, Enemy Handler " + e.Message + e.StackTrace); }
                    break;
                case 15:
                    try
                    {
                        if (!storage.BooleanList[18])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("DET_Elevator").gameObject;
                            if (Chunk.transform.Find("ServiceKeyPickup") == null)
                            {
                                storage.BooleanList[18] = true;
                                InternalList.Add("19,1");
                            }
                        }
                    }
                    catch { }
                    break;
                case 16:
                    try
                    {
                        if (!storage.BooleanList[19])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").gameObject;
                            if(Event.transform.Find("RadioPickup") == null)
                            {
                                storage.BooleanList[19] = true;
                                InternalList.Add("20.1");
                            }
                        }
                    }
                    catch { }
                    break;
                case 17:
                    try
                    {
                        if (!storage.BooleanList[19])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_DoorServiceHatch").gameObject;
                            if (Event.transform.Find("KeyLogic").gameObject.transform.Find("Logic").gameObject.transform.GetComponent<PuzzleStatus>().solved == true)
                            {
                                storage.BooleanList[21] = true;
                                InternalList.Add("22.1");
                            }
                        }
                    }
                    catch { }
                    break;
                case 18:
                    try
                    {
                        if (!storage.BooleanList[19])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_TreeSafe").gameObject;
                            if (Event.transform.Find("Card") == null)
                            {
                                storage.BooleanList[23] = true;
                                InternalList.Add("24.1");
                            }
                        }
                    }
                    catch { }
                    break;
                default:
                    break;
            }
        }
        public static void MED_Medical(int RoomName, List<string> InternalList)
        {
            switch (RoomName)
            {
                case 0:
                    break;
                case 1:
                    if (!storage.ManagedEnemies.ContainsKey(15))
                    {
                        GameObject Enemy = GameObject.Find("South Corridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 15);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(16))
                    {
                        GameObject Enemy = GameObject.Find("South Corridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 2 EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 16);
                        }
                    }
                    break;
                case 2:
                    if (!storage.ManagedEnemies.ContainsKey(17))
                    {
                        GameObject Enemy = GameObject.Find("East Corridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 17);
                        }
                    }
                    break;
                case 3:
                    if (!storage.ManagedEnemies.ContainsKey(18))
                    {
                        GameObject Enemy = GameObject.Find("Sleeping Ward").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("ARAR NestTile").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 18);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(19))
                    {
                        GameObject Enemy = GameObject.Find("Sleeping Ward").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("ARAR NestTile (1)").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 19);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(20))
                    {
                        GameObject Enemy = GameObject.Find("Sleeping Ward").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("ARAR NestTile (2)").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 20);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(21))
                    {
                        GameObject Enemy = GameObject.Find("Sleeping Ward").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("ARAR NestTile (3)").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 21);
                        }
                    }
                    break;
                case 4:
                    if (!storage.ManagedEnemies.ContainsKey(22))
                    {
                        GameObject Enemy = GameObject.Find("West Corridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 22);
                        }
                    }
                    break;
                case 5:
                    if (!storage.ManagedEnemies.ContainsKey(23))
                    {
                        GameObject Enemy = GameObject.Find("Morgue").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 23);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(24))
                    {
                        GameObject Enemy = GameObject.Find("Morgue").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("ARAR NestTile").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 24);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(25))
                    {
                        GameObject Enemy = GameObject.Find("Morgue").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("ARAR NestTile (1)").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 25);
                        }
                    }
                    break;
                case 6:
                    if (!storage.ManagedEnemies.ContainsKey(26))
                    {
                        GameObject Enemy = GameObject.Find("HDU 2 (Dentist)").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 26);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(27))
                    {
                        GameObject Enemy = GameObject.Find("HDU 2 (Dentist)").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 2 EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 27);
                        }
                    }
                    break;
                case 7:
                    if (!storage.ManagedEnemies.ContainsKey(28))
                    {
                        GameObject Enemy = GameObject.Find("Flooded Corridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 28);
                        }
                    }
                    break;
                case 8:
                    if (!storage.ManagedEnemies.ContainsKey(29))
                    {
                        GameObject Enemy = GameObject.Find("ICU 1 (TV Room)").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 29);
                        }
                    }
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
        public static int RoomChecker(List<string> secondarylist, List<string> eventlist)
        {
            if(GameObject.Find("Events") != null)
            {
                foreach (string e in eventlist)
                {
                    if(GameObject.Find("Events)").transform.Find(e).gameObject.active == true)
                    {
                        return (eventlist.IndexOf(e) + 1 + secondarylist.Count);
                    }
                }
            }
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
                /*if (Math.Abs(difference) < 1) // experiment lowering value
                {
                    e.z = l.z;
                }*/
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
                            if (Chunk == null || RainaCheck(storage))
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
                            if (Chunk == null || RainaCheck(storage))
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
                            if (Ducttape == null || RainaCheck(storage))
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
                            if (Chunk == null || RainaCheck(storage))
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
                            if (Chunk == null || RainaCheck(storage))
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
                            if (Chunk == null || RainaCheck(storage))
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
                            if (Chunk == null || RainaCheck(storage))
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
                            if (Chunk == null || RainaCheck(storage))
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
                            if (Chunk == null || RainaCheck(storage))
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
                            if (Chunk == null || RainaCheck(storage))
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
                            GameObject Chunk = GameObject.Find("Showers").gameObject.transform.Find("Chunk").gameObject.transform.Find("Objects").gameObject;
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
                            GameObject Chunk = GameObject.Find(storage.DETDetentionRooms[5]).gameObject.transform.Find("Chunk").gameObject.transform.Find("Objects").gameObject;
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
                    case 19:
                        if (!storage.BooleanList[18])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_Elevator").gameObject;
                            if (Event == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Event.transform.Find("ServiceKeyPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Event.transform.Find("ServiceKeyPickup").gameObject.SetActive(false);
                            storage.BooleanList[18] = boolean;
                            return true;
                        }
                        return false;
                    case 20: 
                        if (!storage.BooleanList[19])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").gameObject;
                            if(Event == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Event.transform.Find("RadioPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Event.transform.Find("RadioPickup").gameObject.SetActive(false);
                            storage.BooleanList[19] = boolean;
                            return true;
                        }
                        return false;
                    case 21:
                        //Death Bool
                        if(storage.BooleanList[20] != boolean)
                        {
                            storage.BooleanList[20] = boolean;
                        }
                        return true;
                    case 22:
                        if (!storage.BooleanList[21])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_DoorServiceHatch").gameObject;
                            if (Event == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Event.transform.Find("KeyLogic").gameObject.transform.Find("Logic").gameObject.transform.GetComponent<PuzzleStatus>().solved = true;
                            storage.BooleanList[21] = true;
                            return true;
                        }
                        return false;
                    case 23:
                        //Plate of Eternity - Complex Logic Here, Debating if Worth Firing off
                        if (!storage.BooleanList[22])
                        {
                            GameObject Chunk = GameObject.Find("EvidenceStorage").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ButterflyBoxEvent");
                            storage.BooleanList[22] = true;
                            return true;
                        }
                        return false;
                    case 24:
                        //Identification Card
                        if (!storage.BooleanList[23])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_TreeSafe").gameObject;
                            if (Event == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Event.transform.Find("Card").gameObject.transform.Find("ItemPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[23] = true;
                            return true;
                        }
                        return false;
                    case 25:
                        //Death Sent
                        try
                        {
                            if (!storage.BooleanList[26])
                            {
                                storage.active = true;
                                storage.failsafe = true;
                                if (Cheats.buddha)
                                {
                                    Cheats.cheat("buddha");
                                }
                                PlayerState.HurtElster(100, new Vector2(0, 0));
                                storage.BooleanList[26] = true;
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Msg("Error on 26" + e.Message);
                            return true;
                        }
                        return true;
                    default:
                        return true;
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
        
        //Death Handler
        public static void DeathHandler(List<string> InternalList, Storage storage)
        {
            try
            {
                int hp = PlayerState.hp;
                if (hp <= 20)
                {
                    if (storage.BooleanList[20] && !storage.dead) //Other Player is Also Dead
                    {
                        if (Cheats.buddha)
                        {
                            Cheats.cheat("buddha");
                        }
                        PlayerState.HurtElster(100, new Vector2(0, 0));
                        InternalList.Add("26,1");
                        storage.dead = true;
                        return;
                    }
                    if (!storage.neardeath)
                    {
                        InternalList.Add("21,1");
                        storage.neardeath = true;
                    }
                    if (!Cheats.buddha)
                    {
                        //This works, if Hp <= 20 and Buddha is off it turns it on succesfully
                        Cheats.cheat("buddha");
                    }
                }
                else
                {
                    if (storage.neardeath)
                    {
                        InternalList.Add("21,0");
                    }
                    storage.neardeath = false;
                }
            }
            catch(Exception e)
            {
                MelonLogger.Msg("Error in Death Handler" + e.Message);
            }
        }

        //Enemy Handlers
        public static void PeerSideEnemyHandler(Storage storage)
        {
            //First Handle if Enemies need to be Handled At All
            List<int> enemiesToRemove = new List<int>();
            foreach (KeyValuePair<int, GameObject> kvp in storage.ManagedEnemies)
            {
                int tag = kvp.Key;
                GameObject enemy = kvp.Value;
                {
                    //First Check if we Even Need to Handle It
                    if ((enemy == null) || (!enemy.active)); {
                        enemiesToRemove.Add(tag); continue; //No Longer Active
                    }
                    Hitbox hit = enemy.GetComponent<Hitbox>();
                    if (hit == null) {
                        enemiesToRemove.Add(tag); continue; //No Active Hitbox
                    }
                    EnemyController controller = enemy.GetComponent<EnemyController>();

                    //Handling Damage; does not need to be returned, actively updated
                    int health = hit.HP;
                    if (storage.EnemyHP[tag] > hit.HP)
                    {
                        storage.EnemyHP[tag] = health;
                        string value = $"ED:{tag}={health}";
                        storage.MessageCollection.Add(value);
                        //Need to Send Message, Damage was done by this Peer
                    }
                    if (storage.EnemyHP[tag] < hit.HP)
                    {
                        hit.HP = storage.EnemyHP[tag]; //Enemy Took damage from other peer, the logic for reading this is handled by the ServerSide
                    }
                    if (hit.HP <= 0)
                    {
                        enemiesToRemove.Add(tag); continue; //Enemy is dead stop tracking it
                    }

                    //Handle Location
                    Vector3? tempVector = CheckEnemyVector(enemy, tag);
                    if(tempVector != null)
                    {
                        string value = ($"EV:{tag}={tempVector.ToString()}");
                        storage.MessageCollection.Add(value);
                    }
                    Quaternion? tempQuater = CheckEnemyQuaternion(enemy, tag);
                    if (tempQuater != null) 
                    {
                        string value = ($"EQ:{tag}={tempQuater.ToString()}");
                        storage.MessageCollection.Add(value);
                    }
                    //Handle Targeting


                    //Return Results

                }
            }
            foreach (int tag in enemiesToRemove)
            {
                storage.ManagedEnemies.Remove(tag);
            }
        }
        public static void InstantiateEnemy(GameObject enemy, int tag)
        {
            storage.ManagedEnemies.Add(tag, enemy);
            CheckEnemyVector(enemy, tag);
            CheckEnemyQuaternion(enemy, tag);
        }
        public static Vector3? CheckEnemyVector(GameObject enemy, int tag)
        {
            try
            {
                Vector3 e = enemy.transform.position;
                if (!storage.EnemyLocation.ContainsKey(tag))
                {
                    storage.EnemyLocation.Add(tag,e);
                    return null; //Instantiation Logic for CheckEnemyVector
                }
                Vector3 l = storage.EnemyLocation[tag];
                float difference = e.z - l.z;
                if (Math.Abs(difference) < 1)
                {
                    e.z = l.z;
                }
                if (e.x != l.x || e.y != l.y || e.z != l.z)
                {
                    storage.EnemyLocation[tag] = e;
                    return e;
                }
                else
                {
                    //If there is no change, send nothing, return null so no message is sent
                    return null;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Failure on 1461-Check Vector " + ex.Message + ex.StackTrace);
                return null;
            }
        }
        public static Quaternion? CheckEnemyQuaternion(GameObject enemy, int tag)
        {
            try
            {
                Quaternion e = enemy.transform.rotation;
                if (!storage.EnemyRotation.ContainsKey(tag))
                {
                    storage.EnemyRotation.Add(tag,e);
                    return null;
                }
                Quaternion r = storage.EnemyRotation[tag];
                if (e.x != r.x || e.y != r.y || e.z != r.z)
                {
                    storage.EnemyRotation[tag] = e;
                    return e;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                MelonLogger.Msg("Failure on 1490-Check Quaternion" + e.Message + e.StackTrace );
                return null;
            }

        }
        public static void MoveEnemy()
        {

        }
        public static void RotateEnemy()
        {

        }
        public static void TargetEnemy()
        {

        }
        public static void DealDamage(string message)
        {
            int colonIndex = message.IndexOf(':');
            if (colonIndex != -1 && colonIndex < message.Length - 1)
            {
                message = message.Substring(colonIndex + 1);
                string[] sides = message.Split('=');
                if (int.TryParse(sides[0], out int tag) && int.TryParse(sides[1], out int value))
                {
                    storage.EnemyHP[tag] = value;
                }
            }
        }
        public static bool EnemyStatus(GameObject Enemy, int tag, Hitbox hit)
        {
            if ((hit.HP == 0) || (!Enemy.active))
            {
                return false;
            }
            return true;
        }
    }
}