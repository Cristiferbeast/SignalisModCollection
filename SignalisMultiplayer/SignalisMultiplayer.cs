using MelonLoader;
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
        public Animator EllieAnimation;
        public int delayTimer4Animation = 0;

        //Boolean Variables
        public List<string> PENWreckRooms = new List<string>() { "Cryogenics", "Flight Deck", "Mess Hall", "Personell" }; //we do not need rooms without boolean values
        public List<string> LOVReEducationRooms = new List<string>() { "", "Surface Access", "OverlookOffice", "Library", "Aula", "WestCorridor", "SafeClassroom" };
        public List<string> DETDetentionRooms = new List<string>() { "", "Office", "Pantry", "Rationing", "BathroomSouth", "Showers", "EvidenceStorage", "MensaCorridor", "Kitchen", "Isolation", "SouthWestCorridor", "CellBlockCorridor", "Lockers", "Mensa", "North West Corridor", "MED Elevator"};
        public List<string> MEDMedicalsRooms = new List<string>() { "", "South Corridor", "East Corridor", "Sleeping Ward", "West Corridor", "Morgue", "HDU 2 (Dentist)", "Flooded Corridor", "ICU 1 (TV Room)", "Upper Store Room", "Nurse Station", "Protektor Bathroom", "Pump Room", "Flooded Bathroom", "Flooded Office (Save Room)", "Flooded Store Room", "Exam Room", "Sleeping Ward", "Ventilation Room", "Incinerator", "Waiting Room", "Mynah Surgery" };
        public List<string> RESResidentialRooms = new List<string>() {"", "Maintenance Office (Save Room)", "Common Room", "Service Room", "Adlers Bedroom", "Falkes Office", "Shooting Range", "Rec Room (Save Room)", "Protektor Archive", "EULR Dorm (Music Lover)", "Management Office", "Workshop", "Falkes Bedroom", "Kolibris Study", "Kolibris Bedroom", "6F ElevatorLobby", "Library", "Adlers Study", "Kolibris Bedroom", "6F S Corridor", "7F N Corridor", "STCR/KNCR Dorm", "Protektors Office", "Repair Bay", "7F E Corridor", "8F E Corridor", "8F N Corridor", "STAR Dorm South", "7F S Corridor", "STAR Dorm North", "ARAR Dorm (Boat Room)", "STCR Dorm (Music Hater)"};
        public List<string> LABLabyrinthRooms = new List<string>() {"", "Nursery", "Pattern Viewer", "Organ Music Room", "Flesh", "Falke", "Surgery", "Glass Cage Room", "Radio Room", "Shrine Room", "Torture Room", "Sacrifice", "Scale Room", "Chapel", "Tree Of Life", "Corridor", "Loop Hall", "Carcass Hall", "Execution Room", "Cage Room" };
        public List<string> MEMMemoryRooms = new List<string>() {"", "Personell", "Stern Hall", "Mess Hall", "Flight Deck", "Reactor", "Cryogenics" };
        public List<string> BIOReEducationRooms = new List<string>() {"", "SafeClassroom", "EastCorridor", "EastCorridor 3", "Aula" };
        public List<string> ROTRotfront = new List<string>() {"", "Worker Apartment", "Steampipes Corridor", "Garbage Chute (Down)", "Disinfection Room", "Butcher Store", "Computer Store (Save Room)", "Butterfly Apartment", "Dentist Office", "Blockwart Office", "Backyard", "Book Store", "Student Dorm", "Photo Store", "Packstation", "Lobby", "Hospital Room"};
        public List<string> EXCMinesRooms = new List<string>() { "", "Hatch Room", "Combat Room" };
        public List<string> BOSAdler = new List<string>() { "", "Arena" };
        public List<bool> BooleanList = Enumerable.Repeat(false, 200).ToList();
        
        //Enemy Variables
        public Dictionary<int, GameObject> ManagedEnemies = new Dictionary<int, GameObject>(200); //used by Enemy Handler
        public Dictionary<int, int> EnemyHP = new Dictionary<int, int>(200);
        public Dictionary<int, Vector3> EnemyLocation = new Dictionary<int, Vector3>();
        public Dictionary<int, Quaternion> EnemyRotation = new Dictionary<int, Quaternion>();
        public Dictionary<int, bool> EnemyStatus = new Dictionary<int, bool>(200);

        //Readable Server Variables
        public bool host;
        public int ServerPort;
        public string url;
        public bool testingenvironment = false;
        public SigiClient client;
        public SigiServer server;
        public bool neardeath;
        public bool dead;

        //Cutscene Variables
        public bool AltControllerState = false;
        public GameObject AltController;
        public GameObject AltGrounder;
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
                if (storage.testingenvironment)
                {
                    MelonLogger.Msg("Testing Environment Active, skipping network setup");
                }
                if (storage.host && !storage.testingenvironment)
                {
                    storage.server.StartServer();
                }
                if (!storage.host && !storage.testingenvironment)
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
                storage.timer += 10;
                if (storage.testingenvironment)
                {
                    MessageCentralHost(false);
                }
                if (storage.host && !storage.testingenvironment && storage.timer > storage.delay)
                {
                    MessageCentralHost();
                    storage.timer -= storage.delay;
                }
                if (!storage.host && !storage.testingenvironment && storage.timer > storage.delay)
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
                if (lines.Length > 2)
                {
                    if (bool.TryParse(lines[2], out bool testingValue))
                    {
                        storage.testingenvironment = testingValue;
                        MelonLogger.Msg("Testing Value Initalized");
                    }
                }
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Config file not found at path: " + MultiplayerModConfigPath);
            }
            if (storage.testingenvironment)
            {
                return storage;
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
                Animator aroot = root.GetComponent<Animator>();
                Rigidbody rootRb = root.GetComponent<Rigidbody>(); // Get parent's Rigidbody
                storage.EllieMain = root.transform.Find("Ellie_Default").gameObject;
                storage.dead = false;
                storage.EllieClone = UnityEngine.Object.Instantiate(storage.EllieMain);
                storage.EllieClone.GetComponent<Anchor>().enabled = false;
                storage.EllieClone.GetComponent<PlayerAim>().enabled = false;
                storage.EllieClone.transform.Find("HurtIKTarget").gameObject.SetActive(false);
                GameObject.Destroy(storage.EllieClone.transform.Find("metarig").Find("Root").Find("hips").Find("spine").Find("chest").Find("shoulder_R").Find("upper_arm_R").Find("forearm_R").Find("hand_R").Find("WeaponMount").gameObject);
                //Animator Set Up
                try
                {
                    storage.EllieAnimation = storage.EllieClone.AddComponent<Animator>();
                    storage.EllieAnimation.runtimeAnimatorController = aroot.runtimeAnimatorController;
                    storage.EllieAnimation.avatar = aroot.avatar;
                    storage.EllieAnimation.applyRootMotion = aroot.applyRootMotion;
                    storage.EllieAnimation.updateMode = aroot.updateMode;
                    storage.EllieAnimation.cullingMode = aroot.cullingMode;
                    storage.EllieAnimation.speed = aroot.speed;
                    if (rootRb != null)
                    {
                        Rigidbody cloneRb = storage.EllieClone.AddComponent<Rigidbody>();
                        cloneRb.mass = rootRb.mass;
                        cloneRb.drag = rootRb.drag;
                        cloneRb.angularDrag = rootRb.angularDrag;
                        cloneRb.useGravity = rootRb.useGravity;
                        cloneRb.isKinematic = rootRb.isKinematic;
                        cloneRb.interpolation = rootRb.interpolation;
                        cloneRb.collisionDetectionMode = rootRb.collisionDetectionMode;
                        cloneRb.constraints = rootRb.constraints;
                    }
                }
                catch
                {
                    MelonLogger.Msg("Error on Animation Handler, 226");
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Ellie cannot be found or cannot be Instantiated " + e.Message);
                GameObject.Destroy(storage.EllieClone);
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
        public static void MessageCentralHost(bool state = true)
        {
            try
            {
                var itemsToRemove = new List<string>();
                if (storage.testingenvironment)
                {
                    storage.MessageCollection.Clear();
                    SignalisMultiplayer.AddMessages();
                    return;
                }
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
                        if (send != "" && send != " ")
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
                MelonLogger.Msg("Error on 348; " + e.Message + e.StackTrace);
            }
        }
        public static void MessageCentralClient()
        {
            try
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
            catch (Exception e)
            {
                MelonLogger.Msg(e.StackTrace);
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
                ApplyVector(storage.EllieClone, message, storage);
            }
            else if (message.StartsWith("Q:"))
            {
                ApplyQuaternion(storage.EllieClone, message, storage);
            }
            else if (message.StartsWith("ED:"))
            {
                DealDamage(message);
            }
            else if (message.StartsWith("ER:"))
            {
                KillEnemyOverRide(message);
            }
            else if (message.StartsWith("B:"))
            {
                SBoolApply(message); //this may no longer be used due to the new storage method, however it is being left in case we decide to switch off the ductape storage
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
            //PeerSideEnemyHandler(storage);
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
            try
            {
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
            }catch(Exception e) { MelonLogger.Msg("Failure on Boolean Reader at 512 due to " + e.Message + e.StackTrace); }
        }
        public static List<string> BooleanReader()
        {
            List<string> InternalList = new List<string>();
            DeathHandler(InternalList, storage);
            PeerSideEnemyHandler(storage);
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
                case "MED_Medical":
                    int roomname4 = RoomChecker(storage.MEDMedicalsRooms);
                    MED_Medical(roomname4, InternalList);
                    break;
                case "RES_School":
                    if (!storage.AltControllerState)
                    {
                        storage.AltControllerState = true;
                        storage.AltController = GameObject.Find("Events").gameObject.transform.Find("ROT_School").gameObject.transform.Find("CameraOrigin").Find("CameraPivot").Find("EventCamera").gameObject;
                        storage.AltController.GetComponent<Camera>().cullingMask = -1337327360; 
                        storage.AltGrounder = GameObject.Find("Events").gameObject.transform.Find("ROT_School").gameObject.transform.Find("CameraOrigin").Find("Walker").Find("grounder").gameObject;
                    }
                    break;
                case "RES_Residential":
                    if (storage.AltControllerState)
                    {
                        storage.AltControllerState = false;
                    }
                    int roomname5 = RoomChecker(storage.RESResidentialRooms);
                    RES_Residential(roomname5, InternalList);
                    break;
                case "EXC_Mines":
                    int roomname10 = RoomChecker(storage.EXCMinesRooms);
                    EXC_Mines(roomname10, InternalList);
                    break;
                case "EXC_Gestade":
                    if (!storage.AltControllerState)
                    {
                        storage.AltControllerState = true;
                        storage.AltController = GameObject.Find("Events").gameObject.transform.Find("EXC_Gestade").gameObject.transform.Find("CameraOrigin").Find("CameraPivot").Find("EventCamera").gameObject;
                        storage.AltController.GetComponent<Camera>().cullingMask = -1337327360;
                        storage.AltGrounder = GameObject.Find("Events").gameObject.transform.Find("EXC_Gestade").gameObject.transform.Find("CameraOrigin").Find("Walker").Find("grounder").gameObject;
                    }
                    break;
                case "LAB_Labyrinth":
                    if (storage.AltControllerState)
                    {
                        storage.AltControllerState = false;
                    }
                    int roomname6 = RoomChecker(storage.LABLabyrinthRooms);
                    LAB_Nowhere(roomname6, InternalList);
                    break;
                case "MEM_Memory":
                    int roomname7 = RoomChecker(storage.MEMMemoryRooms);
                    MEM_Memory(roomname7, InternalList);
                    break;
                case "MEM_Gestade":
                    if (!storage.AltControllerState)
                    {
                        storage.AltControllerState = true;
                        storage.AltController = GameObject.Find("Events").gameObject.transform.Find("EXC_Gestade").gameObject.transform.Find("CameraOrigin").Find("CameraPivot").Find("EventCamera").gameObject;
                        storage.AltController.GetComponent<Camera>().cullingMask = -1337327360;
                        storage.AltGrounder = GameObject.Find("Events").gameObject.transform.Find("EXC_Gestade").gameObject.transform.Find("CameraOrigin").Find("Walker").Find("grounder").gameObject;
                    }
                    break;
                case "BIO_Reeducation":
                    if (storage.AltControllerState)
                    {
                        storage.AltControllerState = false;
                    }
                    int roomname8 = RoomChecker(storage.BIOReEducationRooms);
                    BIO_ReEducation(roomname8, InternalList);
                    break;
                case "ROT_Rotfront":
                    int roomname9 = RoomChecker(storage.ROTRotfront);
                    ROT_Rotfront(roomname9, InternalList);
                    break;
                case "BOS_Adler":
                    int roomname11 = RoomChecker(storage.BOSAdler);
                    BOS_Adler(roomname11, InternalList);
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
                        PEN_Cryo Cryo = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject.transform.Find("Cryo").gameObject.transform.GetComponent<PEN_Cryo>();
                        storage.BooleanList[0] = Cryo.opened; //this is set true when the value is true
                        if (storage.BooleanList[0])
                        {
                            InternalList.Add("1,1");
                        }
                    }
                    try
                    {
                        GameObject Chunk = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject;
                        if (!storage.BooleanList[5])
                        {
                            if (Chunk.transform.Find("Cryo").gameObject.transform.Find("ItemPickup_BrokenKey") == null)
                            {
                                storage.BooleanList[5] = true;
                                InternalList.Add("5,1");
                            }
                        }
                    }
                    catch { /* Unused Debugging */}
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
                    GameObject GChunk = GameObject.Find("Events").gameObject.transform.Find("LOV_GuardOffice").gameObject;
                    if (!storage.BooleanList[11])
                    {
                        try
                        {
                            if (GChunk != null && GChunk.active == true)
                            {
                                GameObject Drawer = GChunk.transform.Find("DrawerPivot (1)").gameObject.transform.Find("Desk_Drawer").gameObject;
                                if (Drawer.transform.Find("KeyPickup") == null)
                                {
                                    storage.BooleanList[11] = true;
                                    InternalList.Add("12,1");
                                }
                            }
                        }
                        catch(Exception e) { MelonLogger.Msg("Failure on Case 2, Bool 6, Drawer Pivot" + e.StackTrace + e.Message); }
                    }
                    if (!storage.BooleanList[6])
                    {
                        try
                        {
                            if (GChunk != null && GChunk.active == true)
                            {
                                GameObject Drawer = GChunk.transform.Find("DrawerPivot (1)").gameObject.transform.Find("Desk_Drawer").gameObject;
                                if (Drawer.transform.Find("PistolPickup") == null)
                                {
                                    storage.BooleanList[6] = true;
                                    InternalList.Add("7,1");
                                }
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        GameObject SecondDrawer = GChunk.transform.Find("DrawerPivot (2)").gameObject.transform.Find("Desk_Drawer").gameObject;
                        if (!SecondDrawer.transform.Find("Clutter (1)").gameObject.active) { SecondDrawer.transform.Find("Clutter (1)").gameObject.SetActive(true); }
                        if (!SecondDrawer.transform.Find("Clutter (1)_1").gameObject.active) { SecondDrawer.transform.Find("Clutter (1)_1").gameObject.SetActive(true); }
                    }
                    catch(Exception e) { MelonLogger.Msg("Issue on Drawer Pivot " + e.Message + e.StackTrace); }
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
                        //this is only true upon first loading into this room, when the cutscene plays; LMFAO THE LOGIC IS WRONG
                        storage.BooleanList[7] = true;
                        InternalList.Add("8,1");
                    }
                    break;
                case 5:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(1))
                        {
                            EnemyInstantiationCheck("WestCorridor", "Enemy 1 EULR");
                        }
                    }
                    catch(Exception e) { MelonLogger.Msg("Failure on Room Case 5, Enemy 1, Enemy Handler " + e.Message + e.StackTrace ); }
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
                        if (!storage.BooleanList[23])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_TreeSafe").gameObject;
                            if (Event.transform.Find("Card").Find("ItemPickup") == null)
                            {
                                storage.BooleanList[23] = true;
                                InternalList.Add("24,1");
                            }
                        }
                        if (!storage.BooleanList[27])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_TreeSafe").gameObject;
                            if (Event.transform.Find("Door").Find("Buttons").GetComponent<Keypad3D>().solved)
                            {
                                storage.BooleanList[27] = true;
                                InternalList.Add("27,1");
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
                    try
                    {
                        if (!storage.BooleanList[22])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("DET_MysteryBox").gameObject.transform.Find("mystery_box_scene-2").gameObject.transform.Find("MysteryBox").gameObject.transform.Find("Sign").gameObject;
                            if (Chunk.transform.Find("SignPickup") == null)
                            {
                                storage.BooleanList[22] = true;
                                InternalList.Add("23,1");
                            }
                        }
                        if (GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").gameObject.active)
                        {
                            if (!storage.AltControllerState)
                            {
                                storage.AltControllerState = true;
                                storage.AltController = GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").gameObject.transform.Find("CameraOrigin").Find("CameraPivot").Find("EventCamera").gameObject;
                                storage.AltController.GetComponent<Camera>().cullingMask = -1337327360; //now Ellie Can be Seen;
                                storage.AltGrounder = GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").gameObject.transform.Find("CameraOrigin").Find("Walker").Find("grounder").gameObject;
                                GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").Find("Mountain").Find("Base").Find("MainBuilding").Find("RadioBuildingDoor_1").gameObject.active = false; //Easter Egg 
                            }
                            if (!storage.BooleanList[19])
                            {
                                GameObject Event = GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").gameObject;
                                if (!Event.transform.Find("RadioPickup").transform.Find("Modulebox").transform.Find("Module").gameObject.active)
                                {
                                    storage.BooleanList[19] = true;
                                    InternalList.Add("20,1");
                                }
                            }
                            /*if (!storage.BooleanList[26])
                            {
                                GameObject Event = GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").gameObject;
                                if (Event.transform.Find("Mountain").Find("Base").Find("RadioBunker").Find("BunkerDoor").Find("ROT_RadioStationPuzzle").GetComponent<RadioStationTutorialPuzzle>().solved)
                                {
                                    storage.BooleanList[26] = true;
                                    InternalList.Add("27,1");
                                }
                            }*/
                        }
                        else
                        {
                            if (storage.AltControllerState)
                            {
                                storage.AltControllerState = false;
                                storage.EllieClone.transform.localScale = new Vector3(1, 1, 1); //Temporary Workaround for Ellie Scaling Issues
                                InternalList.Add("26,1"); //the puzzle is solved if this is true however watch this may cause unintended side effects
                            }
                        }
                    }
                    catch { }
                    break;
                case 7:
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(2))
                        {
                            GameObject Enemy = GameObject.Find("MensaCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 3);
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
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 8, Bool 4, Enemy Handler " + e.Message + e.StackTrace); }
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
                            GameObject Enemy = GameObject.Find("SouthWestCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 EULR").gameObject;
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
                            GameObject Enemy = GameObject.Find("SouthWestCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 2 EULR").gameObject;
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
                            GameObject Enemy = GameObject.Find("CellBlockCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 8);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 11, Bool 8, Enemy Handler " + e.Message + e.StackTrace); }
                    try
                    {
                        if (!storage.ManagedEnemies.ContainsKey(9))
                        {
                            GameObject Enemy = GameObject.Find("CellBlockCorridor").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 9);
                            }
                        }
                    }
                    catch (Exception e) { MelonLogger.Msg("Failure on Room Case 11, Bool 9, Enemy Handler " + e.Message + e.StackTrace); }
                    try
                    {
                        if (!storage.BooleanList[21])
                        {
                            GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_DoorServiceHatch").gameObject;
                            if (Event.transform.Find("KeyLogic").gameObject.transform.Find("Logic").gameObject.transform.GetComponent<PuzzleStatus>().solved == true)
                            {
                                storage.BooleanList[21] = true;
                                InternalList.Add("22,1");
                            }
                        }
                    }
                    catch { }
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
                                MelonLogger.Msg("Service Key Picked Up");
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
            try
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
                        if (!storage.BooleanList[38])
                        {
                            GameObject Chunk = GameObject.Find("Sleeping Ward").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_VCR") == null)
                            {
                                storage.BooleanList[38] = true;
                                InternalList.Add("38,1");
                            }
                        }
                        break;
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
                        //VCR Event Room and its related bools
                        if (!storage.ManagedEnemies.ContainsKey(29))
                        {
                            //Cut Content Eule
                            GameObject Enemy = GameObject.Find("ICU 1 (TV Room)").gameObject.transform.Find("Enemy Manager").gameObject.transform.Find("EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 29);
                            }
                        }
                        if (!storage.BooleanList[41])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("MED_TrainEvent").gameObject;
                            if (Chunk.transform.Find("CardPickup").Find("Interaction") == null)
                            {
                                storage.BooleanList[41] = true;
                                InternalList.Add("41,1");
                            }
                        }
                        if (GameObject.Find("Events").gameObject.transform.Find("MED_TrainEvent").gameObject.active)
                        {
                            if (!storage.AltControllerState)
                            {
                                storage.AltControllerState = true;
                                storage.AltController = GameObject.Find("Events").transform.Find("MED_TrainEvent").gameObject.transform.Find("CameraOrigin").Find("CameraPivot").Find("EventCamera").gameObject;
                                storage.AltController.GetComponent<Camera>().cullingMask = -1337327360; //now Ellie Can be Seen;
                                storage.AltGrounder = GameObject.Find("Events").transform.Find("MED_TrainEvent").gameObject.transform.Find("CameraOrigin").Find("Walker").Find("grounder").gameObject;
                            }
                        }
                        else
                        {
                            if (storage.AltControllerState)
                            {
                                storage.AltControllerState = false;
                                storage.EllieClone.transform.localScale = new Vector3(1, 1, 1); //Temporary Workaround for Ellie Scaling Issues
                            }
                        }
                        break;
                    case 9:
                        if (!storage.BooleanList[29])
                        {
                            GameObject Chunk = GameObject.Find("Upper Store Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_SocketHandle") == null)
                            {
                                storage.BooleanList[29] = true;
                                InternalList.Add("29,1");
                            }
                        }
                        break;
                    case 10:
                        if (!storage.BooleanList[30])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("DET_ScalesSafe").Find("Door").Find("Buttons").gameObject;
                            if (Chunk.GetComponent<Keypad3D>().solved)
                            {
                                storage.BooleanList[30] = true;
                                InternalList.Add("30,1");
                            }
                        }
                        if (!storage.BooleanList[31])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("DET_ScalesSafe").Find("Card").gameObject;
                            if (Chunk.transform.Find("ItemPickup_IncineratorKey") == null)
                            {
                                storage.BooleanList[31] = true;
                                InternalList.Add("31,1");
                            }
                        }
                        break;
                    case 11:
                        if (!storage.BooleanList[32])
                        {
                            GameObject Chunk = GameObject.Find("Protektor Bathroom").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_PumpKey") == null)
                            {
                                storage.BooleanList[31] = true;
                                InternalList.Add("32,1");
                            }
                        }
                        break;
                    case 12:
                        if (!storage.BooleanList[33])
                        {
                            GameObject Chunk = GameObject.Find("Pump Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("PumpLogic").GetComponent<MED_Pump>().solved)
                            {
                                storage.BooleanList[33] = true;
                                InternalList.Add("33,1");
                            }
                        }
                        break;
                    case 13:
                        if (!storage.BooleanList[34])
                        {
                            GameObject Chunk = GameObject.Find("Flooded Bathroom").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_KeyOfWater") == null)
                            {
                                storage.BooleanList[34] = true;
                                InternalList.Add("34,1");
                            }
                        }
                        break;
                    case 14:
                        if (!storage.BooleanList[35])
                        {
                            GameObject Chunk = GameObject.Find("Flooded Office (Save Room)").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_ExamKey") == null)
                            {
                                storage.BooleanList[35] = true;
                                InternalList.Add("35,1");
                            }
                        }
                        break;
                    case 15:
                        if (!storage.BooleanList[36])
                        {
                            GameObject Chunk = GameObject.Find("Flooded Store Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_KeyOfBlank") == null)
                            {
                                storage.BooleanList[36] = true;
                                InternalList.Add("36,1");
                            }
                        }
                        break;
                    case 16:
                        if (!storage.BooleanList[37])
                        {
                            GameObject Chunk = GameObject.Find("Exam Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_Socket10mm") == null)
                            {
                                storage.BooleanList[37] = true;
                                InternalList.Add("37,1");
                            }
                        }
                        break;
                    case 17:
                        if (!storage.BooleanList[38])
                        {
                            GameObject Chunk = GameObject.Find("Sleeping Ward").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_VCR") == null)
                            {
                                storage.BooleanList[38] = true;
                                InternalList.Add("38,1");
                            }
                        }
                        break;
                    case 18:
                        if (!storage.BooleanList[39])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("MED_WallVent Event").gameObject;
                            if (Chunk.transform.Find("CardPickup ").Find("Interaction") == null)
                            {
                                storage.BooleanList[39] = true;
                                InternalList.Add("39,1");
                            }
                        }
                        break;
                    case 19:
                        if (!storage.BooleanList[42])
                        {
                            GameObject Chunk = GameObject.Find("Incinerator").gameObject.transform.Find("Chunk").Find("IncineratorLogic").gameObject;
                            if (Chunk.GetComponent<MED_Incinerator>().solved)
                            {
                                storage.BooleanList[42] = true;
                                InternalList.Add("42,1");
                            }
                        }
                        if (!storage.BooleanList[43])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("MED_Incinerator").gameObject;
                            if (Chunk.transform.Find("CardPickup ").Find("Interaction") == null)
                            {
                                storage.BooleanList[43] = true;
                                InternalList.Add("43,1");
                            }
                        }
                        break;
                    case 20:
                        if (!storage.BooleanList[44])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Air)
                            {
                                storage.BooleanList[44] = true;
                                InternalList.Add("44,1");
                            }
                        }
                        if (!storage.BooleanList[45])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Gold)
                            {
                                storage.BooleanList[45] = true;
                                InternalList.Add("45,1");
                            }
                        }
                        if (!storage.BooleanList[46])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Water)
                            {
                                storage.BooleanList[46] = true;
                                InternalList.Add("46,1");
                            }
                        }
                        if (!storage.BooleanList[47])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Fire)
                            {
                                storage.BooleanList[47] = true;
                                InternalList.Add("47,1");
                            }
                        }
                        if (!storage.BooleanList[48])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Earth)
                            {
                                storage.BooleanList[48] = true;
                                InternalList.Add("48,1");
                            }
                        }
                        break;
                    case 21:
                        GameObject ChunkSurgery = GameObject.Find("Mynah Surgery").gameObject.transform.Find("Chunk").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(34))
                        {
                            GameObject Enemy = ChunkSurgery.transform.Find("Enemy 1 MNHR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 34);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(35))
                        {
                            GameObject Enemy = ChunkSurgery.transform.Find("ARAR NestTile").Find("Pivot").Find("Enemy Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 35);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(36))
                        {
                            GameObject Enemy = ChunkSurgery.transform.Find("ARAR NestTile (1)").Find("Pivot").Find("Enemy Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 36);
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("MED Medical Boolean Handler Failure: " + RoomName + e.Message + e.StackTrace);
            }
        }
        public static void RES_Residential(int RoomName, List<string> InternalList)
        {
            try
            {
                //56
                switch (RoomName)
                {
                    case 0:
                        break;
                    case 1:
                        if (!storage.BooleanList[56])
                        {
                            GameObject Chunk = GameObject.Find("Maintenance Office (Save Room)").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_FloodOverflowKey") == null || Chunk.transform.Find("ItemPickup_FloodOverflowKey").gameObject.active != true)
                            {
                                storage.BooleanList[56] = true;
                                InternalList.Add("56,1");
                            }
                        }
                        break;
                    case 2:
                        if (!storage.BooleanList[57])
                        {
                            GameObject Chunk = GameObject.Find("Common Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_ElevatorFuse") == null)
                            {
                                storage.BooleanList[57] = true;
                                InternalList.Add("57,1");
                            }
                        }
                        GameObject ChunkCommon = GameObject.Find("Common Room").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(98))
                        {
                            GameObject Enemy = ChunkCommon.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 98);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(99))
                        {
                            GameObject Enemy = ChunkCommon.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 99);
                            }
                        }
                        break;
                    case 3:
                        if (!storage.BooleanList[58])
                        {
                            RES_Power Power = GameObject.Find("Events").gameObject.transform.Find("PowerLogic").GetComponent<RES_Power>();
                            if (Power != null)
                            {
                                if (Power.solved)
                                {
                                    storage.BooleanList[58] = true;
                                    InternalList.Add("58,1");
                                }
                            }
                        }
                        break;
                    case 4:
                        if (!storage.BooleanList[59])
                        {
                            GameObject Chunk = GameObject.Find("Adlers Bedroom").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_Flashlight") == null)
                            {
                                storage.BooleanList[59] = true;
                                InternalList.Add("59,1");
                            }
                        }
                        break;
                    case 5:
                        if (!storage.BooleanList[60])
                        {
                            GameObject Chunk = GameObject.Find("Falkes Office").gameObject.transform.Find("Chunk").Find("Objects").Find("Shutter Door").gameObject;
                            if (Chunk.transform.Find("HandlePickup") == null)
                            {
                                storage.BooleanList[60] = true;
                                InternalList.Add("60,1");
                            }
                        }
                        GameObject FalkeOfficeChunk = GameObject.Find("Falkes Office").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(104))
                        {
                            GameObject Enemy = FalkeOfficeChunk.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 104);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(105))
                        {
                            GameObject Enemy = FalkeOfficeChunk.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 105);
                            }
                        }
                        break;
                    case 6:
                        if (!storage.BooleanList[61])
                        {
                            GameObject Chunk = GameObject.Find("Shooting Range").gameObject.transform.Find("Chunk").Find("Objects").gameObject;
                            if (Chunk.transform.Find("ItemPickup_Tape") == null)
                            {
                                storage.BooleanList[61] = true;
                                InternalList.Add("61,1");
                            }
                        }
                        break;
                    case 7:
                        if (!storage.BooleanList[62])
                        {
                            GameObject Chunk = GameObject.Find("Rec Room (Save Room)").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_OwlKey") == null )
                            {
                                storage.BooleanList[62] = true;
                                InternalList.Add("62,1");
                            }
                        }
                        break;
                    case 8:
                        if (!storage.BooleanList[63])
                        {
                            GameObject Chunk = GameObject.Find("Protektor Archive").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_PaintingKey") == null)
                            {
                                storage.BooleanList[63] = true;
                                InternalList.Add("63,1");
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(90))
                        {
                            GameObject Enemy = GameObject.Find("Protektor Archive").gameObject.transform.Find("Enemy Manager").Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 90);
                            }
                        }
                        break;
                    case 9:
                        if (!storage.BooleanList[64])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_MusicPlayer").Find("RES_MusicBox_Player").gameObject;
                            if (Chunk.transform.Find("Interaction") == null)
                            {
                                storage.BooleanList[64] = true;
                                InternalList.Add("64,1");
                            }
                        }
                        break;
                    case 10:
                        try
                        {
                            if (!storage.BooleanList[66])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("RES_Painting").gameObject;
                                if (Chunk.transform.Find("Pickup").Find("CardPickup ").Find("Interaction") == null)
                                {
                                    storage.BooleanList[66] = true;
                                    InternalList.Add("66,1");
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            MelonLogger.Msg("Error on Bools in Case 10 " + e.Message + e.StackTrace);
                        }
                        try
                        {
                            if (!storage.ManagedEnemies.ContainsKey(91))
                            {
                                GameObject Enemy = GameObject.Find("Management Office").gameObject.transform.Find("Enemy Manager").Find("Kolibri").gameObject;
                                if (Enemy != null && Enemy.active == true)
                                {
                                    InstantiateEnemy(Enemy, 91);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Msg("Error on Enemy in Case 10 " + e.Message + e.StackTrace);
                        }
                        break;
                    case 11:
                        if (!storage.BooleanList[67])
                        {
                            RES_MusicBox Chunk = GameObject.Find("Events").transform.Find("MusicBoxLogic").GetComponent<RES_MusicBox>();
                            if (Chunk.hasCassette)
                            {
                                storage.BooleanList[67] = true;
                                InternalList.Add("67,1");
                            }
                        }
                        break;
                    case 12:
                        if (!storage.BooleanList[68])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_MusicBox").Find("GameObject").Find("RES_MusicBox_Box").gameObject;
                            if (Chunk.transform.Find("Pickup").Find("CardPickup ") == null)
                            {
                                storage.BooleanList[68] = true;
                                InternalList.Add("68,1");
                            }
                        }
                        break;
                    case 13:
                        if (!storage.BooleanList[69])
                        {
                            GameObject Chunk = GameObject.Find("Kolibris Study").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("GameObject").Find("ItemPickup_AdlersKey") == null)
                            {
                                storage.BooleanList[69] = true;
                                InternalList.Add("69,1");
                            }
                        }
                        break;
                    case 14:
                        if (!storage.BooleanList[70])
                        {
                            GameObject Chunk = GameObject.Find("Kolibris Bedroom").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_PostboxKey") == null || Chunk.active != true)
                            {
                                storage.BooleanList[70] = true;
                                InternalList.Add("70,1");
                            }
                        }
                        // Missing Enemey - Kolibri in Bedroom
                        break;
                    case 15:
                        if (!storage.BooleanList[71])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_PostBox").gameObject;
                            if (Chunk.transform.Find("Pickup").Find("CardPickup ").Find("Interaction") == null)
                            {
                                storage.BooleanList[71] = true;
                                InternalList.Add("71,1");
                            }
                        }
                        break;
                    case 16:
                        if (!storage.BooleanList[73])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_Library").gameObject;
                            if (Chunk.transform.Find("KingInYellow").Find("CardPickup ") == null)
                            {
                                storage.BooleanList[73] = true;
                                InternalList.Add("73,1");
                            }
                        }
                        break;
                    case 17:
                        if (!storage.BooleanList[74])
                        {
                            RES_Shrine Chunk = GameObject.Find("Events").transform.Find("RES_Shrine").Find("ShrineLogic").GetComponent<RES_Shrine>();
                            if (Chunk.solved)
                            {
                                storage.BooleanList[74] = true;
                                InternalList.Add("74,1");
                            }
                        }
                        if (!storage.BooleanList[75])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_Shrine").Find("RES_Shrine").Find("RES_Shrine_Base").Find("Content").gameObject;
                            if (Chunk.transform.Find("CardPickup ") == null)
                            {
                                storage.BooleanList[75] = true;
                                InternalList.Add("75,1");
                                Chunk.transform.Find("PhotoPickup").gameObject.SetActive(true); //new easter egg found :D
                            }
                        }
                        break;
                    case 18:
                        if (!storage.ManagedEnemies.ContainsKey(85))
                        {
                            GameObject Chunk = GameObject.Find("Kolibris Bedrom").transform.Find("Enemy Manager").gameObject;
                            GameObject Enemy = Chunk.transform.Find("Kolibri").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 85);
                            }
                        }
                        break;
                    case 19:
                        GameObject Chunk6F = GameObject.Find("6F S Corridor").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(87))
                        {
                            GameObject Enemy = Chunk6F.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 87);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(88))
                        {
                            GameObject Enemy = Chunk6F.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 88);
                            }
                        }
                        break;
                    case 20:
                        GameObject Chunk7F = GameObject.Find("7F N Corridor").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(89))
                        {
                            GameObject Enemy = Chunk7F.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 89);
                            }
                        }
                        break;
                    case 21:
                        GameObject ChunkSTCR = GameObject.Find("STCR / KNCR Dorm").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(92))
                        {
                            GameObject Enemy = ChunkSTCR.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 92);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(93))
                        {
                            GameObject Enemy = ChunkSTCR.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 93);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(94))
                        {
                            GameObject Enemy = ChunkSTCR.transform.Find("Enemy 3 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 94);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(95))
                        {
                            GameObject Enemy = ChunkSTCR.transform.Find("Enemy 4 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 95);
                            }
                        }
                        break;
                    case 22:
                        GameObject Chunk23 = GameObject.Find("Protektors Office").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(96))
                        {
                            GameObject Enemy = Chunk23.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 96);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(97))
                        {
                            GameObject Enemy = Chunk23.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 97);
                            }
                        }
                        break;
                    case 23:
                        GameObject ChunkRepair = GameObject.Find("Repair Bay").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(99))
                        {
                            GameObject Enemy = ChunkRepair.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 99);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(100))
                        {
                            GameObject Enemy = ChunkRepair.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 100);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(101))
                        {
                            GameObject Enemy = ChunkRepair.transform.Find("ARAR NestTile").Find("Pivot").Find("Enemy Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 101);
                            }
                        }
                        break;
                    case 24:
                        GameObject ChunkFLKRStudy = GameObject.Find("Falke Study").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(102))
                        {
                            GameObject Enemy = ChunkFLKRStudy.transform.Find("ARAR NestTile").Find("Pivot").Find("Enemy Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 102);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(103))
                        {
                            GameObject Enemy = ChunkFLKRStudy.transform.Find("ARAR NestTile (1)").Find("Pivot").Find("Enemy Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 103);
                            }
                        }
                        break;
                    case 25:
                        GameObject Chunk7E = GameObject.Find("7F E Corridor").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(106))
                        {
                            GameObject Enemy = Chunk7E.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 106);
                            }
                        }
                        break;
                    case 26:
                        GameObject Chunk8E = GameObject.Find("8F E Corridor").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(107))
                        {
                            GameObject Enemy = Chunk8E.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 107);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(108))
                        {
                            GameObject Enemy = Chunk8E.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 108);
                            }
                        }
                        break;
                    case 27:
                        GameObject Chunk8N = GameObject.Find("8F N Corridor").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(109))
                        {
                            GameObject Enemy = Chunk8N.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 109);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(110))
                        {
                            GameObject Enemy = Chunk8N.transform.Find("Enemy 2 STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 110);
                            }
                        }
                        break;
                    case 28:
                        GameObject ChunkStar = GameObject.Find("STAR Dorm South").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(111))
                        {
                            GameObject Enemy = ChunkStar.transform.Find("STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 111);
                            }
                        }
                        break;
                    case 29:
                        GameObject Chunk7S = GameObject.Find("7F S Corridor").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(112))
                        {
                            GameObject Enemy = Chunk7S.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 112);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(113))
                        {
                            GameObject Enemy = Chunk7S.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 113);
                            }
                        }
                        break;
                    case 30:
                        GameObject ChunkStarNorth = GameObject.Find("7F S Corridor").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(114))
                        {
                            GameObject Enemy = ChunkStarNorth.transform.Find("Enemy 1 STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 114);
                            }
                        }
                        break;
                    case 32:
                        GameObject ChunkAra = GameObject.Find("ARAR Dorm (Boat Room)").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(115))
                        {
                            GameObject Enemy = ChunkAra.transform.Find("ARAR NestTile").Find("Pivot").Find("Enemy Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 115);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(116))
                        {
                            GameObject Enemy = ChunkAra.transform.Find("ARAR NestTile (1)").Find("Pivot").Find("Enemy Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 116);
                            }
                        }
                        break;
                    case 31:
                        GameObject ChunkSTCRD = GameObject.Find("STCR Dorm (Music Hater)").gameObject.transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(117))
                        {
                            GameObject Enemy = ChunkSTCRD.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 117);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(118))
                        {
                            GameObject Enemy = ChunkSTCRD.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 118);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(119))
                        {
                            GameObject Enemy = ChunkSTCRD.transform.Find("Enemy 3 STAR").gameObject;
                            if (Enemy != null && Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 119);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }catch(Exception e)
            {
                MelonLogger.Msg("RES Residential Boolean Handler Failure: " + RoomName + e.Message + e.StackTrace);
            }
        }
        public static void ROT_Rotfront(int RoomName, List<string> InternalList)
        {
            try
            {
                //Resume from 76; 
                switch (RoomName)
                {
                    case 0:
                        break;
                    case 1:
                        if (!storage.BooleanList[76])
                        {
                            GameObject Chunk = GameObject.Find("Worker Apartment").transform.Find("Chunk").gameObject;
                            if (Chunk != null || Chunk.active)
                            {
                                if (Chunk.transform.Find("ItemPickup_ValveHandwheel") == null)
                                {
                                    storage.BooleanList[76] = true;
                                    InternalList.Add("76,1");
                                }
                            }
                        }
                        break;
                    case 2:
                        if (!storage.BooleanList[77])
                        {
                            GameObject Chunk = GameObject.Find("Steampipes Corridor").transform.Find("Chunk").gameObject;
                            if (Chunk != null || Chunk.active)
                            {
                                if (Chunk.transform.Find("InstantChunk") == null)
                                {
                                    storage.BooleanList[77] = true;
                                    InternalList.Add("77,1");
                                }
                            }
                        }
                        break;
                    case 3:
                        if (!storage.BooleanList[78])
                        {
                            GameObject Chunk = GameObject.Find("Garbage Chute (Down)").transform.Find("Chunk").gameObject;
                            if (Chunk != null || Chunk.active)
                            {
                                if (Chunk.transform.Find("ItemPickup_DiskBlue") == null)
                                {
                                    storage.BooleanList[78] = true;
                                    InternalList.Add("78,1");
                                }
                            }
                        }
                        break;
                    case 4:
                        if (!storage.BooleanList[79])
                        {
                            GameObject Chunk = GameObject.Find("Disinfection Room").transform.Find("Chunk").gameObject;
                            if (Chunk != null || Chunk.active)
                            {
                                if (Chunk.transform.Find("ItemPickup_Turpentine") == null)
                                {
                                    storage.BooleanList[79] = true;
                                    InternalList.Add("79,1");
                                }
                            }
                        }
                        break;
                    case 5:
                        if (!storage.BooleanList[80])
                        {
                            if (GameObject.Find("Events").transform.Find("ROT_Tower").gameObject != null)
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Tower").Find("ROT_Tower").Find("ROT_Tower_Tray").gameObject;
                                if (Chunk.transform.Find("CardPickup ") == null)
                                {
                                    storage.BooleanList[80] = true;
                                    InternalList.Add("80,1");
                                }
                            }
                        }
                        break;
                    case 6:
                        if (!storage.BooleanList[81])
                        {
                            GameObject Chunk = GameObject.Find("Computer Store (Save Room)").transform.Find("Chunk").gameObject;
                            if (Chunk != null || Chunk.active)
                            {
                                if (Chunk.transform.Find("ItemPickup_MeatKey") == null)
                                {
                                    storage.BooleanList[81] = true;
                                    InternalList.Add("81,1");
                                }
                            }
                        }
                        if (!storage.BooleanList[82])
                        {
                            GameObject Chunk = GameObject.Find("Computer Store (Save Room)").transform.Find("Chunk").gameObject;
                            if (Chunk != null || Chunk.active)
                            {
                                if (Chunk.transform.Find("ItemPickup_MeatKey") == null)
                                {
                                    storage.BooleanList[82] = true;
                                    InternalList.Add("82,1");
                                }
                            }
                        }
                        break;
                    case 7:
                        try
                        {
                            if (!storage.BooleanList[83])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_ButterflySafe").gameObject;
                                if (Chunk != null || Chunk.active)
                                {
                                    if (Chunk.transform.Find("ROT_ButterflyBox").Find("CardPickup ").Find("Interaction") == null)
                                    {
                                        storage.BooleanList[83] = true;
                                        InternalList.Add("83,1");
                                    }
                                }
                            }
                            if (!storage.BooleanList[84])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_ButterflySafe").gameObject;
                                if (Chunk != null || Chunk.active)
                                {
                                    if (Chunk.transform.Find("PadlockLogic").GetComponent<ROT_DialLock>().solved == true)
                                    {
                                        storage.BooleanList[84] = true;
                                        InternalList.Add("84,1");
                                    }
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            MelonLogger.Msg("Failure on Boolean Handler in Butterfly Room " + e.Message + e.StackTrace);
                        }
                        break;
                    case 8:
                        if (!storage.BooleanList[85])
                        {
                            GameObject Chunk = GameObject.Find("Dentist Office").transform.Find("Chunk").gameObject.gameObject;
                            if (Chunk != null || Chunk.active)
                            {
                                if (Chunk.transform.Find("ItemPickup_DiskRed") == null)
                                {
                                    storage.BooleanList[85] = true;
                                    InternalList.Add("85,1");
                                }
                            }
                        }
                        break;
                    case 9:
                        if (!storage.BooleanList[86])
                        {
                            ROT_DiskManager Chunk = GameObject.Find("Events").transform.Find("ROT_PC").Find("Disk Logic").gameObject.GetComponent<ROT_DiskManager>();
                            if (Chunk != null)
                            {
                                if (Chunk.red)
                                {
                                    storage.BooleanList[86] = true;
                                    InternalList.Add("86,1");
                                }
                            }
                        }
                        if (!storage.BooleanList[87])
                        {
                            ROT_DiskManager Chunk = GameObject.Find("Events").transform.Find("ROT_PC").Find("Disk Logic").gameObject.GetComponent<ROT_DiskManager>();
                            if (Chunk != null)
                            {
                                if (Chunk.blue)
                                {
                                    storage.BooleanList[87] = true;
                                    InternalList.Add("87,1");
                                }
                            }
                        }
                        if (!storage.BooleanList[88])
                        {
                            //FYI in the future we can pass the logic for the p2p value manually but for now lets just handle the correct value
                            ROT_RadioAlignment Chunk = GameObject.Find("RadioAlignmentManager").GetComponent<ROT_RadioAlignment>();
                            if (Chunk != null)
                            {
                                if (Chunk.TargetRotEast == 42 && Chunk.TargetRotWest == -36)
                                {
                                    storage.BooleanList[88] = true;
                                    InternalList.Add("88,1");
                                }
                            }
                        }
                        //if (storage.BooleanList[89]) : Placeholder Bool for Transmit
                        break;
                    case 10:
                        if (!storage.BooleanList[90])
                        {
                            ROT_Keypad Chunk = GameObject.Find("Events").transform.Find("KeypadLogic").GetComponent<ROT_Keypad>();
                            if (Chunk.solved)
                            {
                                storage.BooleanList[90] = true;
                                InternalList.Add("90,1");
                            }
                        }
                        break;
                    case 11:
                        try
                        {
                            if (!storage.BooleanList[91])
                            {
                                CutsceneManager Cutscene = GameObject.Find("Cutscenes").gameObject.transform.Find("Isa Death").gameObject.GetComponent<CutsceneManager>();
                                if (Cutscene.completed)
                                {
                                    storage.BooleanList[91] = true;
                                    InternalList.Add("91,1");
                                }
                            }
                            if (storage.BooleanList[92])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Death").gameObject;
                                if (Chunk.transform.Find("CardPickup ").Find("Interaction") == null)
                                {
                                    storage.BooleanList[92] = true;
                                    InternalList.Add("92,1");
                                }
                            }
                        }
                        catch (Exception e) { MelonLogger.Msg(e.Message + "Failure on Case 12, Bool 92"); }
                        break;
                    case 12:
                        try
                        {
                            if (!storage.BooleanList[93])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Painting").Find("Pivot").Find("Painting").gameObject;
                                if (Chunk.transform.Find("Pickup").Find("CardPickup ").Find("Interaction") == null)
                                {
                                    storage.BooleanList[93] = true;
                                    InternalList.Add("93,1");
                                }
                            }
                        }catch(Exception e)
                        {
                            MelonLogger.Msg("Failure on Case 12, Bool 93 " + e.Message + e.StackTrace);
                        }
                        break;
                    case 16:
                        try
                        {

                            if (!storage.BooleanList[94])
                            {
                                ROT_Magpie Chunk = GameObject.Find("Events").transform.Find("ROT_Magpie").Find("MagpieLogic").GetComponent<ROT_Magpie>();
                                if (Chunk.opened)
                                {
                                    storage.BooleanList[94] = true;
                                    InternalList.Add("94,1");
                                }
                            }
                            if (!storage.BooleanList[95])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Magpie").Find("ROT_MagpieBox").Find("Pickups").gameObject;
                                if (Chunk != null)
                                {
                                    if (Chunk.transform.Find("Interaction") == null)
                                    {
                                        storage.BooleanList[95] = true;
                                        InternalList.Add("95,1");
                                    }
                                }//Interaction1
                            }
                            if (!storage.BooleanList[96])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Magpie").Find("ROT_MagpieBox").Find("Pickups").gameObject;
                                if (Chunk != null)
                                {
                                    if (Chunk.transform.Find("Interaction1") == null)
                                    {
                                        storage.BooleanList[96] = true;
                                        InternalList.Add("96,1");
                                    }
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            MelonLogger.Msg("Failure on Case 16, Bools 94-96 " + e.Message + e.StackTrace);
                        }
                        break;
                    case 13:
                        if (!storage.BooleanList[97])
                        {
                            GameObject Chunk = GameObject.Find("Photo Store").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_DeveloperFluid") == null)
                            {
                                storage.BooleanList[97] = true;
                                InternalList.Add("97,1");
                            }
                        }
                        break;
                    case 14:
                        try
                        {
                            if (!storage.BooleanList[98])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Packstation").Find("Logic").gameObject;
                                if (Chunk.GetComponent<DET_ServiceLock_Key>().Open)
                                {
                                    storage.BooleanList[98] = true;
                                    InternalList.Add("98,1");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Msg("Failure on Case 14, Bools 98 " + e.Message + e.StackTrace);
                        }
                        try
                        {
                            if (!storage.BooleanList[99])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Packstation").Find("Pickup").gameObject;
                                if (Chunk.transform.Find("CardPickup ").Find("Interaction") == null)
                                {
                                    storage.BooleanList[99] = true;
                                    InternalList.Add("99,1");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Msg("Failure on Case 14, Bools 99 " + e.Message + e.StackTrace);
                        }
                        break;
                    case 15:
                        try
                        {

                            if (!storage.BooleanList[100])
                            {
                                GameObject Chunk = GameObject.Find("Lobby").transform.Find("Chunk").gameObject;
                                if (Chunk.transform.Find("MeatVersion").gameObject.active)
                                {
                                    storage.BooleanList[100] = true;
                                    InternalList.Add("100,1");
                                }
                            }
                            if (storage.BooleanList[100] && !storage.BooleanList[101])
                            {
                                GameObject Chunk = GameObject.Find("Lobby").transform.Find("Chunk").gameObject;
                                if (Chunk.transform.Find("MeatVersion").Find("ItemPickup_MoonRing") == null)
                                {
                                    storage.BooleanList[101] = true;
                                    InternalList.Add("101,1");
                                }
                            }
                            if (!storage.BooleanList[102])
                            {
                                GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Mural").gameObject;
                                if (Chunk.active)
                                {
                                    if (Chunk.transform.Find("ROT_Mural").Find("Rotfront").Find("Ringactive").gameObject.active)
                                    {
                                        storage.BooleanList[102] = true;
                                        InternalList.Add("102,1");
                                    }
                                }
                            }
                            if (!storage.BooleanList[103])
                            {
                                ROT_Mural Chunk = GameObject.Find("Events").transform.Find("MuralLogic").gameObject.GetComponent<ROT_Mural>();
                                if (Chunk.finished)
                                {
                                    storage.BooleanList[103] = true;
                                    InternalList.Add("103,1");
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            MelonLogger.Msg("Failure on Case 15, Bools 98-99 " + e.Message + e.StackTrace);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Failure in ROT_Rotfront Logic " + RoomName + e.Message + e.StackTrace);
            }
        }
        public static void MEM_Memory(int RoomName, List<string> InternalList)
        {
            switch (RoomName)
            {
                case 0:
                    break;
                case 1:
                    //Disable Elster Appearing in the Cute Scene
                    break;
                case 2:
                    if (!storage.BooleanList[49])
                    {
                        MEM_ChecklistLogic Checklist = GameObject.Find("ChecklistLogic").GetComponent<MEM_ChecklistLogic>();
                        if (Checklist.CheckIfComplete() || (storage.BooleanList[50] && storage.BooleanList[51] && storage.BooleanList[52] && storage.BooleanList[53]))
                        {
                            //This is the "All Items Have Been Completed Logic; Only to be Fired Off When You Enter this Room 
                            storage.BooleanList[49] = true;
                            InternalList.Add("49,1");
                        }
                    }
                    break;
                case 3:
                    if (!storage.BooleanList[50])
                    {
                        storage.BooleanList[50] = true;
                        InternalList.Add("50,1");
                    }
                    break;
                case 4:
                    if (!storage.BooleanList[51])
                    {
                        storage.BooleanList[51] = true;
                        InternalList.Add("51,1");
                    }
                    break;
                case 5:
                    if (!storage.BooleanList[52])
                    {
                        storage.BooleanList[52] = true;
                        InternalList.Add("52,1");
                    }
                    break;
                case 6:
                    if (!storage.BooleanList[53])
                    {
                        storage.BooleanList[53] = true;
                        InternalList.Add("53,1");
                    }
                    break;
                default:
                    break;
            }
        }
        public static void BIO_ReEducation(int RoomName, List<string> InternalList) {
            switch (RoomName)
            {
                //54
                case 0:
                    break;
                case 1:
                    if (!storage.BooleanList[54])
                    {
                        GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_ClassSafe").gameObject;
                        if (Chunk != null || Chunk.active == true)
                        {
                            if (Chunk.transform.Find("Door").gameObject.transform.Find("Buttons").GetComponent<Keypad3D>().solved)
                            {
                                storage.BooleanList[54] = true;
                                InternalList.Add("54,1");
                            }
                        }
                    }
                    if (!storage.BooleanList[55])
                    {
                        GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("LOV_ClassSafe").gameObject;
                        if (Chunk != null || Chunk.active == true)
                        {
                            if (Chunk.transform.Find("Card").gameObject.transform.Find("ItemPickup") == null)
                            {
                                storage.BooleanList[55] = true;
                                InternalList.Add("55,1");
                            }
                        }
                    }
                    break;
                case 2:
                    GameObject ChunkEast1 = GameObject.Find("EastCorridor").transform.Find("Enemy Manager").gameObject;
                    if (!storage.ManagedEnemies.ContainsKey(44))
                    {
                        GameObject Enemy = ChunkEast1.transform.Find("Enemy 1 STCR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 44);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(45))
                    {
                        GameObject Enemy = ChunkEast1.transform.Find("Enemy 2 EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 45);
                        }
                    }
                    break;
                case 3:
                    GameObject ChunkEast3 = GameObject.Find("EastCorridor 3").transform.Find("Enemy Manager").gameObject;
                    if (!storage.ManagedEnemies.ContainsKey(46))
                    {
                        GameObject Enemy = ChunkEast3.transform.Find("Enemy 1 EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 46);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(47))
                    {
                        GameObject Enemy = ChunkEast3.transform.Find("Enemy 2 EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 47);
                        }
                    }
                    break;
                case 4:
                    GameObject AulaChunk = GameObject.Find("Aula").transform.Find("Enemy Manager").gameObject;
                    if (!storage.ManagedEnemies.ContainsKey(48))
                    {
                        GameObject Enemy = AulaChunk.transform.Find("Enemy 1 STCR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 48);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(49))
                    {
                        GameObject Enemy = AulaChunk.transform.Find("Enemy 2 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 49);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(50))
                    {
                        GameObject Enemy = AulaChunk.transform.Find("Enemy 3 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 50);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public static void LAB_Nowhere(int RoomName, List<string> InternalList)
        {
            try
            {
                switch (RoomName)
                {
                    case 0:
                        break;
                    case 1:
                        if (!storage.BooleanList[104])
                        {
                            GameObject Chunk = GameObject.Find("Nursery").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_Doll_B") == null)
                            {
                                storage.BooleanList[104] = true;
                                InternalList.Add("104,1");
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(80))
                        {
                            GameObject Enemy = GameObject.Find("Nursery").transform.Find("Enemy Manager").Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 164);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(81))
                        {
                            GameObject Enemy = GameObject.Find("Nursery").transform.Find("Enemy Manager").Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 165);
                            }
                        }
                        break;
                    case 2:
                        if (!storage.BooleanList[105])
                        {
                            GameObject Chunk = GameObject.Find("Pattern Viewer").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_RingBride") == null)
                            {
                                storage.BooleanList[105] = true;
                                InternalList.Add("105,1");
                            }
                        }
                        break;
                    case 3:
                        if (!storage.BooleanList[106])
                        {
                            GameObject Chunk = GameObject.Find("Organ Music Room").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_RingReagant") == null)
                            {
                                storage.BooleanList[106] = true;
                                InternalList.Add("106,1");
                            }
                        }
                        break;
                    case 4:
                        if (!storage.BooleanList[107])
                        {
                            GameObject Chunk = GameObject.Find("Flesh").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_PlateFlesh") == null)
                            {
                                storage.BooleanList[107] = true;
                                InternalList.Add("107,1");
                            }
                        }
                        if (!storage.BooleanList[108])
                        {
                            GameObject Chunk = GameObject.Find("Flesh").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_RustedKey") == null)
                            {
                                storage.BooleanList[108] = true;
                                InternalList.Add("108,1");
                            }
                        }
                        break;
                    case 5:
                        if (!storage.BooleanList[109])
                        {
                            GameObject Chunk = GameObject.Find("Falke").transform.Find("Chunk").Find("Objects").gameObject;
                            if (Chunk.transform.Find("ItemPickup_Salts") == null)
                            {
                                storage.BooleanList[109] = true;
                                InternalList.Add("109,1");
                            }
                        }
                        break;
                    case 6:
                        if (!storage.BooleanList[110])
                        {
                            GameObject Chunk = GameObject.Find("Surgery").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_Doll_S") == null)
                            {
                                storage.BooleanList[110] = true;
                                InternalList.Add("110,1");
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(56))
                        {
                            GameObject Enemy = GameObject.Find("Surgery").transform.Find("Enemy Manager").Find("Enemy 1 MNHR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 56);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(57))
                        {
                            GameObject Enemy = GameObject.Find("Surgery").transform.Find("Enemy Manager").Find("Enemy 2 STCR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 57);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(58))
                        {
                            GameObject Enemy = GameObject.Find("Surgery").transform.Find("Enemy Manager").Find("Enemy 3 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 58);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(59))
                        {
                            GameObject Enemy = GameObject.Find("Surgery").transform.Find("Enemy Manager").Find("Enemy 4 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 59);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(60))
                        {
                            GameObject Enemy = GameObject.Find("Surgery").transform.Find("Enemy Manager").Find("Enemy 5 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 60);
                            }
                        }
                        break;
                    case 7:
                        if (!storage.BooleanList[111])
                        {
                            GameObject Chunk = GameObject.Find("Glass Cage Room").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_RingSerpent") == null)
                            {
                                storage.BooleanList[111] = true;
                                InternalList.Add("111,1");
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(68))
                        {
                            GameObject Enemy = GameObject.Find("Glass Cage Room").transform.Find("Enemy Manager").Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 68);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(69))
                        {
                            GameObject Enemy = GameObject.Find("Glass Cage Room").transform.Find("Enemy Manager").Find("Enemy 2 STCR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 69);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(70))
                        {
                            GameObject Enemy = GameObject.Find("Glass Cage Room").transform.Find("Enemy Manager").Find("Enemy 3 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 70);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(71))
                        {
                            GameObject Enemy = GameObject.Find("Glass Cage Room").transform.Find("Enemy Manager").Find("Enemy 4 STCR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 71);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(72))
                        {
                            GameObject Enemy = GameObject.Find("Glass Cage Room").transform.Find("Enemy Manager").Find("Enemy 5 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 72);
                            }
                        }
                        break;
                    case 8:
                        if (!storage.BooleanList[112])
                        {
                            GameObject Chunk = GameObject.Find("Radio Room").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_Incense") == null)
                            {
                                storage.BooleanList[112] = true;
                                InternalList.Add("112,1");
                            }
                        }
                        break;
                    case 9:
                        if (!storage.BooleanList[113])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_Shrine").gameObject;
                            if (Chunk.transform.Find("RES_Painting").Find("LAB_Shrine").Find("LAB_Joss").gameObject.active)
                            {
                                storage.BooleanList[113] = true;
                                InternalList.Add("113,1");
                            }
                        }
                        if (!storage.BooleanList[114])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_Shrine").gameObject;
                            if (Chunk.transform.Find("RES_Painting").Find("LAB_Shrine").Find("Plate").Find("Pickup Plate Knowledge") == null)
                            {
                                storage.BooleanList[114] = true;
                                InternalList.Add("114,1");
                            }
                        }
                        break;
                    case 10:
                        if (!storage.BooleanList[115])
                        {
                            GameObject Chunk = GameObject.Find("Torture Room").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_Doll_M").gameObject.active)
                            {
                                storage.BooleanList[115] = true;
                                InternalList.Add("115,1");
                            }
                        }
                        break;
                    case 11:
                        if (!storage.BooleanList[116])
                        {
                            GameObject Chunk = GameObject.Find("Sacrifice").transform.Find("Chunk").gameObject;
                            if (Chunk.transform.Find("ItemPickup_PlateSacrifice") == null)
                            {
                                storage.BooleanList[116] = true;
                                InternalList.Add("116,1");
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(61))
                        {
                            GameObject Enemy = GameObject.Find("Sacrifice").transform.Find("Enemy Manager").Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 61);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(62))
                        {
                            GameObject Enemy = GameObject.Find("Sacrifice").transform.Find("Enemy Manager").Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 62);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(63))
                        {
                            GameObject Enemy = GameObject.Find("Sacrifice").transform.Find("Enemy Manager").Find("ARAR NestTile").Find("Pivot").Find("Enemy 3 Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 63);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(64))
                        {
                            GameObject Enemy = GameObject.Find("Sacrifice").transform.Find("Enemy Manager").Find("ARAR NestTile (1)").Find("Pivot").Find("Enemy 4 Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 64);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(65))
                        {
                            GameObject Enemy = GameObject.Find("Sacrifice").transform.Find("Enemy Manager").Find("ARAR NestTile (2)").Find("Pivot").Find("Enemy 5 Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 65);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(66))
                        {
                            GameObject Enemy = GameObject.Find("Sacrifice").transform.Find("Enemy Manager").Find("ARAR NestTile (3)").Find("Pivot").Find("Enemy 6 Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 66);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(67))
                        {
                            GameObject Enemy = GameObject.Find("Sacrifice").transform.Find("Enemy Manager").Find("ARAR NestTile (4)").Find("Pivot").Find("Enemy 7 Ara").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 67);
                            }
                        }
                        break;
                    case 12:
                        if (!storage.BooleanList[117])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_Waage").Find("Pivot").Find("LAB_Waage_Top").gameObject;
                            if (Chunk.transform.Find("Pickup BMS") != null)
                            {
                                storage.BooleanList[117] = true;
                                InternalList.Add("117,1");
                            }
                        }
                        break;
                    case 13:
                        if (!storage.BooleanList[118])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_Rings").gameObject;
                            if (Chunk.transform.Find("LAB_RingsLogic").GetComponent<LAB_Rings>().solved)
                            {
                                storage.BooleanList[118] = true;
                                InternalList.Add("118,1");
                            }
                        }
                        if (!storage.BooleanList[119])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_Rings").gameObject;
                            if (Chunk.transform.Find("Pivot") == null)
                            {
                                storage.BooleanList[119] = true;
                                InternalList.Add("119,1");
                            }
                        }
                        break;
                    case 14:
                        if (!storage.BooleanList[120])
                        {
                            GameObject Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").Find("LAB_TreeLockLogic").gameObject;
                            if (Chunk.GetComponent<LAB_MultiLock>().Earth == true) 
                            {
                                storage.BooleanList[120] = true;
                                InternalList.Add("120,1");
                            }
                        }
                        if (!storage.BooleanList[121])
                        {
                            GameObject Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").Find("LAB_TreeLockLogic").gameObject;
                            if (Chunk.GetComponent<LAB_MultiLock>().Air == true)
                            {
                                storage.BooleanList[121] = true;
                                InternalList.Add("121,1");
                            }
                        }
                        if (!storage.BooleanList[122])
                        {
                            GameObject Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").Find("LAB_TreeLockLogic").gameObject;
                            if (Chunk.GetComponent<LAB_MultiLock>().Fire == true)
                            {
                                storage.BooleanList[122] = true;
                                InternalList.Add("122,1");
                            }
                        }
                        if (!storage.BooleanList[123])
                        {
                            GameObject Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").Find("LAB_TreeLockLogic").gameObject;
                            if (Chunk.GetComponent<LAB_MultiLock>().Water == true)
                            {
                                storage.BooleanList[123] = true;
                                InternalList.Add("123,1");
                            }
                        }
                        if (!storage.BooleanList[124])
                        {
                            GameObject Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").Find("LAB_TreeLockLogic").gameObject;
                            if (Chunk.GetComponent<LAB_MultiLock>().Gold == true)
                            {
                                storage.BooleanList[124] = true;
                                InternalList.Add("124,1");
                            }
                        }
                        if (storage.BooleanList[125])
                        {
                            GameObject Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").Find("LAB_TreeLockLogic").gameObject;
                            if (Chunk.GetComponent<LAB_MultiLock>().Star == true)
                            {
                                storage.BooleanList[125] = true;
                                InternalList.Add("125,1");
                            }
                        }
                        break;
                    case 15:
                        GameObject HatchChunk = GameObject.Find("Corridor").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(51))
                        {
                            GameObject Enemy = HatchChunk.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 51);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(52))
                        {
                            GameObject Enemy = HatchChunk.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 52);
                            }
                        }
                        break;
                    case 16:
                        GameObject LoopChunk = GameObject.Find("Loop Hall").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(53))
                        {
                            GameObject Enemy = LoopChunk.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 53);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(54))
                        {
                            GameObject Enemy = LoopChunk.transform.Find("Enemy 2 STCR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 54);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(55))
                        {
                            GameObject Enemy = LoopChunk.transform.Find("Kolibri").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 55);
                            }
                        }
                        break;
                    case 17:
                        GameObject CarcasChunk = GameObject.Find("Carcass Hall").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(78))
                        {
                            GameObject Enemy = CarcasChunk.transform.Find("Enemy 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 78);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(79))
                        {
                            GameObject Enemy = CarcasChunk.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 79);
                            }
                        }
                        break;
                    case 18:
                        GameObject ExecutionChunk = GameObject.Find("Execution Room").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(74))
                        {
                            GameObject Enemy = ExecutionChunk.transform.Find("Execution 1 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 74);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(75))
                        {
                            GameObject Enemy = ExecutionChunk.transform.Find("Execution 2 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 75);
                            }
                        }
                        break;
                    case 19:
                        GameObject CageChunk = GameObject.Find("Cage Room").transform.Find("Enemy Manager").gameObject;
                        if (!storage.ManagedEnemies.ContainsKey(76))
                        {
                            GameObject Enemy = CageChunk.transform.Find("Enemy 1 STCR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 76);
                            }
                        }
                        if (!storage.ManagedEnemies.ContainsKey(77))
                        {
                            GameObject Enemy = CageChunk.transform.Find("Enemy 2 EULR").gameObject;
                            if (Enemy != null || Enemy.active == true)
                            {
                                InstantiateEnemy(Enemy, 77);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Failure on 4147-LAB_Nowhere " + RoomName + ex.Message + ex.StackTrace);
            }
        }
        public static void EXC_Mines(int RoomName, List<string> InternalList)
        {
            switch(RoomName)
            {
                case 0:
                    break;
                case 1:
                    //Hatch room
                    GameObject Chunk = GameObject.Find("Hatch Room").transform.Find("Enemy Manager").gameObject;
                    if (!storage.ManagedEnemies.ContainsKey(30))
                    {
                        GameObject Enemy = Chunk.transform.Find("Enemy 1 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 30);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(31))
                    {
                        GameObject Enemy = Chunk.transform.Find("EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 31);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(32))
                    {
                        GameObject Enemy = Chunk.transform.Find("Enemy 2 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 32);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(33))
                    {
                        GameObject Enemy = Chunk.transform.Find("EULR_1").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 33);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(34))
                    {
                        GameObject Enemy = Chunk.transform.Find("Enemey 3 Mynah").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 34);
                        }
                    }
                    break;
                case 2:
                    //Combat Room
                    GameObject Chunk2 = GameObject.Find("Combat Room").transform.Find("Enemy Manager").gameObject;
                    if (!storage.ManagedEnemies.ContainsKey(30))
                    {
                        GameObject Enemy = Chunk2.transform.Find("Enemy 1 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 35);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(31))
                    {
                        GameObject Enemy = Chunk2.transform.Find("Enemy 2 EULR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 36);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(32))
                    {
                        GameObject Enemy = Chunk2.transform.Find("Enemy 3 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 37);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(33))
                    {
                        GameObject Enemy = Chunk2.transform.Find("Enemy 4 STAR").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 38);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public static void BOS_Adler(int RoomName, List<string> InternalList)
        {
            switch (RoomName)
            {
                case 0:
                    break;
                case 1:
                    GameObject ChunkArena = GameObject.Find("Arena").transform.Find("Chunk").gameObject;
                    if (!storage.ManagedEnemies.ContainsKey(37))
                    {
                        GameObject Enemy = ChunkArena.transform.Find("ARAR NestTile").Find("Pivot").Find("Enemy Ara").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 43);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(38))
                    {
                        GameObject Enemy = ChunkArena.transform.Find("ARAR NestTile").Find("Pivot").Find("Enemy Ara").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 38);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(39))
                    {
                        GameObject Enemy = ChunkArena.transform.Find("ARAR NestTile_1").Find("Pivot").Find("Enemy Ara").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 39);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(40))
                    {
                        GameObject Enemy = ChunkArena.transform.Find("ARAR NestTile_2").Find("Pivot").Find("Enemy Ara").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 40);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(41))
                    {
                        GameObject Enemy = ChunkArena.transform.Find("ARAR NestTile_3").Find("Pivot").Find("Enemy Ara").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 41);
                        }
                    }
                    if (!storage.ManagedEnemies.ContainsKey(42))
                    {
                        GameObject Enemy = ChunkArena.transform.Find("ARAR NestTile_4").Find("Pivot").Find("Enemy Ara").gameObject;
                        if (Enemy != null || Enemy.active == true)
                        {
                            InstantiateEnemy(Enemy, 42);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public static int RoomChecker(List<string> secondarylist)
        {
            try
            {
                foreach (string s in secondarylist)
                {
                    if (GameObject.Find(s) != null)
                    {
                        if(GameObject.Find(s).transform.Find("Chunk") != null)
                        {
                            if(GameObject.Find(s).transform.Find("Chunk").gameObject.active == true)
                            {
                                return secondarylist.IndexOf(s); //this isnt flawless, and may interfere with other things in export requiring reformatting 
                            }
                        }
                        else if(GameObject.Find(s).transform.Find("Chunk (1)") != null)
                        {
                            if (GameObject.Find(s).transform.Find("Chunk (1)").gameObject.active == true)
                            {
                                return secondarylist.IndexOf(s); //yeah so how i said this wasnt flawless, this is a great fucking example of why its not.                        
                            }
                        }

                        else
                        {
                            MelonLogger.Msg(s + " Chunk is null");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MelonLogger.Msg("Failure on 3278-Room Checker " + ex.Message + ex.StackTrace);
            }
            return 0;
        }
        public static List<Vector3> CheckVector()
        {
            //this needs to be rewritten one of these days :C
            try
            {
                List<Vector3> VList = new List<Vector3>() { };
                Vector3 l = storage.l;
                if (storage.AltControllerState)
                {
                    Vector3 e2 = storage.AltGrounder.transform.position;
                    if (e2.x != l.x || e2.y != l.y || e2.z != l.z)
                    {
                        VList.Add(e2);
                        storage.l = e2;
                        return VList;
                    }
                    return null;
                }
                Vector3 e = storage.EllieMain.transform.position;
                if (storage.l == null)
                {
                    storage.l = e;
                    return null;
                }
                float difference = e.z - l.z; 
                if (e.x != l.x || e.y != l.y || e.z != l.z)
                {
                    VList.Add(e);
                    storage.l = e;
                    return VList;
                }
                else
                {
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
                Quaternion r = storage.r;
                if (storage.AltControllerState)
                {
                    Quaternion e2 = storage.AltController.transform.rotation;
                    if (e2.x != r.x || e2.y != r.y || e2.z != r.z || e2.w != r.w) 
                    {
                        QList.Add(e2);
                        storage.r = e2;
                        return QList;
                    }
                else
                {
                    return null;
                }
                }
                Quaternion e = storage.EllieMain.transform.rotation;
                if (storage.r == null)
                {
                    storage.r = e;
                    return null;
                }
                if (e.x != r.x || e.y != r.y || e.z != r.z || e.w != r.w) 
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
        public static void ApplyVector(GameObject item, string message, Storage storage)
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
                        if (storage.delayTimer4Animation > 100)
                        {
                            storage.EllieAnimation.Play("Elster_Run_Unarmed", -1, 0f);
                            storage.delayTimer4Animation = 0;
                        }
                        Vector3 vector = new Vector3(x, y, z);
                        item.transform.position = vector;
                    }
                    else
                    {
                        if (storage.delayTimer4Animation > 100)
                        {
                            storage.EllieAnimation.Play("Elster_Idle_Unarmed", -1, 0f);
                            storage.delayTimer4Animation = 0;
                        }
                        MelonLoader.MelonLogger.Msg("Error parsing numbers.");
                    }
                }
                else
                {
                    MelonLoader.MelonLogger.Msg("Expected 3 numbers.");
                }
                storage.delayTimer4Animation += 1;
            }
        }
        public static void ApplyQuaternion(GameObject item, string message, Storage storage)
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
                        Quaternion q2 = new Quaternion(x, y, z, w);
                        if (storage.AltControllerState)
                        {
                            //this is horribly built and really will need to be adjusted when animations are added
                            GameObject EllieHead = storage.EllieClone.transform.Find("metarig").Find("Root").Find("hips").Find("spine").Find("chest").Find("neck").Find("head").gameObject;
                            Quaternion q1 = storage.EllieClone.transform.rotation;
                            EllieHead.transform.rotation = q2;
                            if(Math.Abs(q1.y - q2.y) > 10)
                            {
                                storage.EllieClone.transform.rotation = q2;
                                q2 = new Quaternion(x, 0, z, w);
                                EllieHead.transform.rotation = q2;
                            }
                        }
                        else
                        {
                            item.transform.rotation = q2;
                        }
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
                            GameObject Chunk = GameObject.Find("BathroomSouth").gameObject.transform.Find("Chunk").gameObject.transform.Find("Objects").gameObject;
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
                            GameObject Chunk = GameObject.Find("Showers").gameObject.transform.Find("Chunk").gameObject.transform.Find("Objects").gameObject;
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
                            DET_ServiceLock serviceLock = Event.transform.Find("KeyLogic").gameObject.transform.Find("Logic").gameObject.transform.GetComponent<DET_ServiceLock>();
                            serviceLock.input[0] = serviceLock.pinning[0];
                            serviceLock.input[1] = serviceLock.pinning[1];
                            serviceLock.input[2] = serviceLock.pinning[2];
                            serviceLock.input[3] = serviceLock.pinning[3];
                            storage.BooleanList[21] = true;
                            return true;
                        }
                        return false;
                    case 23:
                        //Plate of Eternity - Complex Logic Here, Debating if Worth Firing off
                        if (!storage.BooleanList[22])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("DET_MysteryBox").gameObject.transform.Find("mystery_box_scene-2").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            GameObject.Find("Events").gameObject.transform.Find("DET_MysteryBox").gameObject.GetComponent<DET_ServiceLock_Key>().Open = true;
                            Chunk.transform.Find("Key").gameObject.SetActive(true);
                            Chunk.transform.Find("MysteryBox").gameObject.transform.Find("Sign").gameObject.transform.Find("SignPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp(); //So now the player has the plate and in theory the cutscene should begin
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
                        return false; //this was set as true??? why???
                    case 27:
                        try
                        {
                            //The numbering of boolean lists has been pissing me the fuck off; its now equal moving forward fuck whatever bullshit system was being used before it was dumb
                            if (!storage.BooleanList[27])
                            {
                                GameObject Event = GameObject.Find("Events").gameObject.transform.Find("DET_TreeSafe").gameObject;
                                if (Event == null)
                                {
                                    storage.BooleanQueue.Add(message);
                                    return false;
                                }
                                Event.transform.Find("Door").GetComponent<Keypad3D>().solved = true;
                                storage.BooleanList[27] = true;
                                return true;
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Msg("Error on 27" + e.Message);
                            return true;
                        }
                        return false;
                    case 28:
                        try
                        {
                            if (!storage.BooleanList[28])
                            {
                                GameObject Event = GameObject.Find("Events").gameObject.transform.Find("ROT_RadioStation").gameObject;
                                if (Event == null)
                                {
                                    storage.BooleanQueue.Add(message);
                                    return false;
                                }
                                Event.transform.Find("Mountain").Find("Base").Find("RadioBunker").Find("BunkerDoor").Find("ROT_RadioStationPuzzle").GetComponent<RadioStationTutorialPuzzle>().solved = true;
                                storage.BooleanList[28] = true;
                                return true;
                            }
                        }
                        catch
                        {

                        }
                        return false;
                    case 29:
                        try
                        {
                            if (!storage.BooleanList[29])
                            {
                                GameObject Chunk = GameObject.Find("Upper Store Room").gameObject.transform.Find("Chunk").gameObject;
                                if (Chunk == null)
                                {
                                    storage.BooleanQueue.Add(message);
                                    return false;
                                }
                                Chunk.transform.Find("ItemPickup_SocketHandle").GetComponent<ItemPickup>().pickUp();
                                storage.BooleanList[29] = true;
                                return true;
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Msg("Error on 29" + e.Message);
                            return true;
                        }
                        return false;
                    case 30:
                        if (!storage.BooleanList[30])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("DET_ScalesSafe").Find("Door").Find("Buttons").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.GetComponent<Keypad3D>().solved = true;
                            storage.BooleanList[30] = true;
                            return true;
                        }
                        return false;
                    case 31:
                        if (!storage.BooleanList[31])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("DET_ScalesSafe").Find("Door").Find("Buttons").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_IncineratorKey").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[31] = true;
                            return true;
                        }
                        return false;
                    case 32:
                        if (!storage.BooleanList[32])
                        {
                            GameObject Chunk = GameObject.Find("Protektor Bathroom").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_PumpKey").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[32] = true;
                            return true;
                        }
                        return false;
                    case 33:
                        if (!storage.BooleanList[33])
                        {
                            GameObject Chunk = GameObject.Find("Pump Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("PumpLogic").GetComponent<MED_Pump>().solved = true;
                            storage.BooleanList[33] = true;
                            return true;
                        }
                        return false;
                    case 34:
                        if (!storage.BooleanList[34])
                        {
                            GameObject Chunk = GameObject.Find("Flooded Bathroom").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_KeyOfWater").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[34] = true;
                            return true;
                        }
                        return false;
                    case 35:
                        if (!storage.BooleanList[35])
                        {
                            GameObject Chunk = GameObject.Find("Flooded Office (Save Room)").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_ExamKey").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[35] = true;
                            return true;
                        }
                        return false;
                    case 36:
                        if (!storage.BooleanList[36])
                        {
                            GameObject Chunk = GameObject.Find("Flooded Store Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_KeyOfBlank").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[36] = true;
                            return true;
                        }
                        return false;
                    case 37:
                        if (!storage.BooleanList[37])
                        {
                            GameObject Chunk = GameObject.Find("Exam Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_Socket10mm").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[37] = true;
                            return true;
                        }
                        return false;
                    case 38:
                        if (!storage.BooleanList[38])
                        {
                            GameObject Chunk = GameObject.Find("Sleeping Ward").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("ItemPickup_VCR").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[38] = true;
                            return true;
                        }
                        return false;
                    case 39:
                        if (!storage.BooleanList[39])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("MED_WallVent Event").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("CardPickup ").Find("Interaction").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[39] = true;
                            return true;
                        }
                        return false;
                    case 40:
                        //Bool for VCR Event
                        return false;
                    case 41:
                        if (!storage.BooleanList[41])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("MED_TrainEvent").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("CardPickup").Find("Interaction").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[41] = true;
                            return true;
                        }
                        return false;
                    case 42:
                        if (!storage.BooleanList[42])
                        {
                            GameObject Chunk = GameObject.Find("Incinerator").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Incinerator").GetComponent<MED_Incinerator>().solved = true; //this doesnt work explore why
                            storage.BooleanList[42] = true;
                            return true;
                        }
                        return false;
                    case 43:
                        if (!storage.BooleanList[43])
                        {
                            GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("MED_Incinerator").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("CardPickup").Find("Interaction").GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[43] = true;
                            return true;
                        }
                        return false;
                    case 44:
                        if (!storage.BooleanList[44])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Air = true;;
                            storage.BooleanList[44] = true;
                            return true;
                        }
                        return false;
                    case 45:
                        if (!storage.BooleanList[45])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Gold = true;
                            storage.BooleanList[45] = true;
                            return true;
                        }
                        return false;
                    case 46:
                        if (!storage.BooleanList[46])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Water = true;
                            storage.BooleanList[46] = true;
                            return true;
                        }
                        return false;
                    case 47:
                        if (!storage.BooleanList[47])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Fire = true;
                            storage.BooleanList[47] = true;
                            return true;
                        }
                        return false;
                    case 48:
                        if (!storage.BooleanList[48])
                        {
                            GameObject Chunk = GameObject.Find("Waiting Room").gameObject.transform.Find("Chunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.transform.Find("Elemental Lock Logic").GetComponent<MED_MultiLock>().Earth = true;
                            storage.BooleanList[48] = true;
                            return true;
                        }
                        return false;
                    case 49:
                        if (!storage.BooleanList[49])
                        {
                            MEM_ChecklistLogic.forceComplete = true;
                        }
                        return true;
                    case 50:
                        if (!storage.BooleanList[50])
                        {
                            storage.BooleanList[50] = true;
                        }
                        return true;
                    case 51:
                        if (!storage.BooleanList[51])
                        {
                            storage.BooleanList[51] = true;
                        }
                        return true;
                    case 52:
                        if (!storage.BooleanList[52])
                        {
                            storage.BooleanList[52] = true;
                        }
                        return true;
                    case 53:
                        if (!storage.BooleanList[53])
                        {
                            storage.BooleanList[53] = true;
                        }
                        return true;
                    case 56:
                        if (!storage.BooleanList[56])
                        {
                            GameObject Chunk = GameObject.Find("Maintenance Office (Save Room)").gameObject.transform.Find("Chunk").Find("ItemPickup_FloodOverflowKey").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[56] = true;
                        }
                        return true;
                    case 57:
                        if (!storage.BooleanList[57])
                        {
                            GameObject Chunk = GameObject.Find("Common Room").gameObject.transform.Find("Chunk").Find("ItemPickup_ElevatorFuse").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[57] = true;
                        }
                        return true;
                    case 58:
                        if (!storage.BooleanList[58])
                        {
                            RES_Power Power = GameObject.Find("Events").transform.Find("PowerLogic").GetComponent<RES_Power>();
                            if (Power == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Power.solved = true;
                            Power.powered = true;
                            storage.BooleanList[58] = true;
                        }
                        return true ;
                    case 59:
                        if (!storage.BooleanList[59])
                        {
                            GameObject Chunk = GameObject.Find("Adlers Bedroom").gameObject.transform.Find("Chunk").Find("ItemPickup_Flashlight").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[59] = true;
                        }
                        return true;
                    case 60:
                        if (!storage.BooleanList[60])
                        {
                            GameObject Chunk = GameObject.Find("Falkes Office").gameObject.transform.Find("Chunk").Find("Objects").Find("ShutterDoor").Find("HandlePickup").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[60] = true;
                        }
                        return true;
                    case 61:
                        if (!storage.BooleanList[61])
                        {
                            GameObject Chunk = GameObject.Find("Shooting Range").gameObject.transform.Find("Chunk").Find("Objects").Find("ItemPickup_Tape").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[61] = true;
                        }
                        return true;
                    case 62:
                        if (!storage.BooleanList[62])
                        {
                            GameObject Chunk = GameObject.Find("Rec Room(Save Room)").gameObject.transform.Find("Chunk").Find("ItemPickup_OwlKey").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[62] = true;
                        }
                        return true;
                    case 63:
                        if (!storage.BooleanList[63])
                        {
                            GameObject Chunk = GameObject.Find("Protektor Archive").gameObject.transform.Find("Chunk").Find("ItemPickup_PaintingKey").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[63] = true;
                        }
                        return true;
                    case 64:
                        //Needs to be Fixed
                        if (!storage.BooleanList[64])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_MusicPlayer").Find("RES_MusicBox_Player").gameObject.transform.Find("Interaction").Find("").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[64] = true;
                        }
                        return true;
                    case 65:
                    case 66:
                        if (!storage.BooleanList[66])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_Painting").Find("Pickup").Find("CardPickup ").Find("Interaction").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[66] = true;
                        }
                        return true;
                    case 67:
                        if (!storage.BooleanList[67])
                        {
                            RES_MusicBox Chunk = GameObject.Find("Events").transform.Find("MusicBoxLogic").GetComponent<RES_MusicBox>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.hasCassette = true;
                            storage.BooleanList[67] = true;
                        }
                        return true;
                    case 68:
                        if (!storage.BooleanList[68])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_MusicBox").Find("GameObject").Find("RES_MusicBox_Box").Find("Pickup").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[68] = true;
                        }
                        return true;
                    case 69:
                        if (!storage.BooleanList[69])
                        {
                            GameObject Chunk = GameObject.Find("Kolibris Study").gameObject.transform.Find("Chunk").Find("GameObject").gameObject;
                            if (Chunk.transform.Find("ItemPickup_AdlersKey") == null)
                            {
                                storage.BooleanQueue.Add(message);
                            }
                            Chunk.transform.Find("ItemPickup_AdlersKey").gameObject.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[69] = true;
                        }
                        return true;
                    case 70:
                        if (!storage.BooleanList[70])
                        {
                            GameObject Chunk = GameObject.Find("Kolibris Bedroom").gameObject.transform.Find("Chunk").Find("ItemPickup_PostboxKey").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[70] = true;
                        }
                        return true;
                    case 71:
                        if (!storage.BooleanList[71])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_PostBox").gameObject.transform.Find("Pickup").Find("CardPickup ").Find("Interaction").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[71] = true;
                        }
                        return true;
                    case 72:
                        if (!storage.BooleanList[72])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_PostBox").gameObject.transform.Find("Pickup").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[72] = true;
                        }
                        return true;
                    case 73:
                        if (!storage.BooleanList[73])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_Library").gameObject.transform.Find("KinginYellow").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[73] = true;
                        }
                        return true;
                    case 74:
                        if (!storage.BooleanList[74])
                        {
                            RES_Shrine Chunk = GameObject.Find("Events").transform.Find("RES_Shrine").Find("ShrineLogic").GetComponent<RES_Shrine>();
                            if(Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.solved = true;
                            storage.BooleanList[74] = true;
                        }
                        return true ;
                    case 75:
                        if (!storage.BooleanList[75])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("RES_Shrine").Find("RES_Shrine").Find("RES_Shrine_Base").Find("Content").Find("CardPickup").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[75] = true;
                        }
                        return true;
                    case 76:
                        if (!storage.BooleanList[76])
                        {
                            GameObject Chunk = GameObject.Find("Worker Apartment").transform.Find("Chunk").Find("ItemPickup_ValveHandwheel").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[76] = true;
                        }
                        return true;
                    case 77:
                        if (!storage.BooleanList[77])
                        {
                            GameObject Chunk = GameObject.Find("Steampipes Corridor").transform.Find("Chunk").Find("InstantChunk").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.active = false;
                            storage.BooleanList[77] = true;
                        }
                        return true;
                    case 78:
                        if (!storage.BooleanList[78])
                        {
                            GameObject Chunk = GameObject.Find("Garbage Chute (Down)").transform.Find("Chunk").Find("ItemPickup_DiskBlue").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[78] = true;
                        }
                        return true;
                    case 79:
                        if (!storage.BooleanList[79])
                        {
                            GameObject Chunk = GameObject.Find("Disinfection Room").transform.Find("Chunk").Find("ItemPickup_Turpentine").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[79] = true;
                        }
                        return true;
                    case 80:
                        if (!storage.BooleanList[80])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Tower").Find("ROT_Tower").Find("ROT_Tower_Tray").Find("CardPickup").Find("Interaction").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[80] = true;
                        }
                        return true;
                    case 81:
                        if (!storage.BooleanList[81])
                        {
                            GameObject Chunk = GameObject.Find("Computer Store (Save Room)").transform.Find("Chunk").Find("ItemPickup_MeatKey").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[81] = true;
                        }
                        return true;
                    case 82:
                        if (!storage.BooleanList[82])
                        {
                            GameObject Chunk = GameObject.Find("Computer Store (Save Room)").transform.Find("Chunk").Find("ItemPickup_MeatKey").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[82] = true;
                        }
                        return true;
                    case 83:
                        if (!storage.BooleanList[83])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_ButterflySafe").Find("ROT_ButterflyBox").Find("CardPickup").Find("Interaction").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[83] = true;
                        }
                        return true;
                    case 84:
                        if (!storage.BooleanList[84])
                        {
                            ROT_DialLock Chunk = GameObject.Find("Events").transform.Find("ROT_ButterflySafe").Find("PadlockLogic").GetComponent<ROT_DialLock>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.solved = true;
                            Chunk.Door.gameObject.SetActive(false); //this may fail miserably
                            storage.BooleanList[84] = true;
                        }
                        return true;
                    case 85:
                        if (!storage.BooleanList[85])
                        {
                            GameObject Chunk = GameObject.Find("Dentist Office").transform.Find("Chunk").Find("ItemPickup_DiskRed").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[85] = true;
                        }
                        return true;
                    case 86:
                        if (!storage.BooleanList[86])
                        {
                            ROT_DiskManager Chunk = GameObject.Find("Events").transform.Find("ROT_PC").Find("Disk Logic").gameObject.GetComponent<ROT_DiskManager>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.InsertDiskRed();
                            storage.BooleanList[86] = true;
                        }
                        return true;
                    case 87:
                        if (!storage.BooleanList[87])
                        {
                            ROT_DiskManager Chunk = GameObject.Find("Events").transform.Find("ROT_PC").Find("Disk Logic").gameObject.GetComponent<ROT_DiskManager>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.InsertDiskBlue();
                            storage.BooleanList[87] = true;
                        }
                        return true;
                    case 88:
                        if (!storage.BooleanList[88])
                        {
                            ROT_RadioAlignment Chunk = GameObject.Find("RadioAlignmentManager").GetComponent<ROT_RadioAlignment>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.TargetRotEast = 42;
                            Chunk.TargetRotWest = 324;
                            storage.BooleanList[88] = true;
                        }
                        return true;
                    case 89:
                        //if (storage.BooleanList[89]) : Placeholder Bool for Transmit
                        return true;
                    case 90:
                        if (!storage.BooleanList[90])
                        {
                            ROT_Keypad Chunk = GameObject.Find("Events").transform.Find("KeypadLogic").GetComponent<ROT_Keypad>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.solved = true;
                            Chunk.solution = "";
                        }
                        return true;
                    case 91:
                        try
                        {
                            if (!storage.BooleanList[91])
                            {
                                storage.Cheats.cheats("goto book store");
                            }
                        }
                        catch {
                            return false;
                        }
                        return true;
                    case 92:
                        if (storage.BooleanList[92])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Death").Find("CardPickup").Find("Interaction").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[92] = true;
                        }
                        return true;
                    case 93:
                        if (!storage.BooleanList[93])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Painting").Find("Pivot").Find("Painting").Find("Pickup").Find("CardPickup ").Find("Interaction").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[93] = true;
                        }
                        return true;
                    case 94:
                        if (!storage.BooleanList[94])
                        {
                            ROT_Magpie Chunk = GameObject.Find("Events").transform.Find("ROT_Magpie").GetComponent<ROT_Magpie>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.playingMagpie = true;
                            Chunk.opened = true;
                            Chunk.euleOpened = 1;
                            storage.BooleanList[94] = true;
                        }
                        return true;
                    case 95: 
                        if (!storage.BooleanList[95])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Magpie").Find("ROT_MagpieBox").Find("Pickups").Find("Interaction").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[95] = true;
                        }
                        return true;
                    case 96:
                        if (!storage.BooleanList[96])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Magpie").Find("ROT_MagpieBox").Find("Pickups").Find("Interaction1").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[96] = true;
                        }
                        return true;
                    case 97:
                        if (!storage.BooleanList[97])
                        {
                            GameObject Chunk = GameObject.Find("Photo Store").transform.Find("Chunk").Find("ItemPickup_DeveloperFluid").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[97] = true;
                        }
                        return true;
                    case 98:
                        if (!storage.BooleanList[98])
                        {
                            DET_ServiceLock_Key Chunk = GameObject.Find("Events").transform.Find("ROT_Packstation").Find("Logic").gameObject.GetComponent<DET_ServiceLock_Key>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.Open = true;
                            storage.BooleanList[98] = true;
                        }
                        return true;
                    case 99:
                        if (!storage.BooleanList[99])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Packstation").Find("Pickups").Find("CardPickup ").Find("Interaction").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[99] = true;
                        }
                        return true;
                    case 100:
                        if (!storage.BooleanList[100])
                        {
                            GameObject.Find("Lobby").transform.Find("MeatVersion").gameObject.SetActive(true);
                            storage.BooleanList[100] = true;
                        }
                        return true;
                    case 101:
                        if (!storage.BooleanList[101])
                        {
                            GameObject Chunk = GameObject.Find("Lobby").transform.Find("MeatVersion").Find("ItemPickup_MoonRing").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[101] = true;
                        }
                        return true;
                    case 102:
                        if (!storage.BooleanList[102])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("ROT_Mural").Find("ROT_Mural").Find("Rotfront").Find("Ringactive").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.SetActive(true);
                            storage.BooleanList[102] = true;
                        }
                        return true;
                    case 103:
                        if (!storage.BooleanList[103])
                        {
                            try
                            {
                                ROT_Mural Chunk = GameObject.Find("Events").transform.Find("MuralLogic").gameObject.GetComponent<ROT_Mural>();
                                Chunk.finished = true;
                                storage.Cheats.cheats("mondenkind");
                                storage.BooleanList[103] = true;
                            }
                            catch
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                        }
                        return true;
                    case 104:
                        if (!storage.BooleanList[104])
                        {
                            GameObject Chunk = GameObject.Find("Nursery").transform.Find("Chunk").Find("ItemPickup_Doll_B").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[104] = true;
                        }
                        return true;
                    case 105:
                        if (!storage.BooleanList[105])
                        {
                            GameObject Chunk = GameObject.Find("Pattern Viewer").transform.Find("Chunk").Find("ItemPickup_RingBride").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[105] = true;
                        }
                        return true;
                    case 106:
                        if (!storage.BooleanList[106])
                        {
                            GameObject Chunk = GameObject.Find("Organ Music Room").transform.Find("Chunk").Find("ItemPickup_RingReagant").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[106] = true;
                        }
                        return true;
                    case 107:
                        if (!storage.BooleanList[107])
                        {
                            GameObject Chunk = GameObject.Find("Flesh").transform.Find("Chunk").Find("ItemPickup_PlateFlesh").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[107] = true;
                        }
                        return true;
                    case 108:
                        if (!storage.BooleanList[108])
                        {
                            GameObject Chunk = GameObject.Find("Flesh").transform.Find("Chunk").Find("ItemPickup_RustedKey").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[108] = true;
                        }
                        return true;
                    case 109:
                        if (!storage.BooleanList[109])
                        {
                            GameObject Chunk = GameObject.Find("Falke").transform.Find("Chunk").Find("Objects").Find("ItemPickup_Salts").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[109] = true;
                        }
                        return true;
                    case 110:
                        if (!storage.BooleanList[110])
                        {
                            GameObject Chunk = GameObject.Find("Surgery").transform.Find("Chunk").Find("ItemPickup_Doll_S").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[110] = true;
                        }
                        return true;
                    case 111:
                        if (!storage.BooleanList[111])
                        {
                            GameObject Chunk = GameObject.Find("Glass Cage Room").transform.Find("Chunk").Find("ItemPickup_RingSerpent").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[111] = true;
                        }
                        return true;
                    case 112:
                        if (!storage.BooleanList[112])
                        {
                            GameObject Chunk = GameObject.Find("Radio Room").transform.Find("Chunk").Find("ItemPickup_Incense").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[112] = true;
                        }
                        return true;
                    case 113:
                        if (!storage.BooleanList[113])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_Shrine").Find("LAB_Joss").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.SetActive(true);
                            storage.BooleanList[113] = true;
                        }
                        return true;
                    case 114:
                        if (!storage.BooleanList[114])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_RingsLogic").Find("Pivot").Find("Pickup Plate Knowledge").Find("Plate").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[114] = true;
                        }
                        return true;
                    case 115:
                        if (!storage.BooleanList[115])
                        {
                            GameObject Chunk = GameObject.Find("Torture Room").transform.Find("Chunk").Find("ItemPickup_Doll_M").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[115] = true;
                        }
                        return true;
                    case 116:
                        if (!storage.BooleanList[116])
                        {
                            GameObject Chunk = GameObject.Find("Sacrifice").transform.Find("Chunk").Find("ItemPickup_PlateSacrifice").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[116] = true;
                        }
                        return true;
                    case 117:
                        if (!storage.BooleanList[117])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_Waage").Find("Pivot").Find("LAB_Waage_Top").Find("Pickup BMS").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[117] = true;
                        }
                        return true;
                    case 118:
                        if (!storage.BooleanList[118])
                        {
                            LAB_Rings Chunk = GameObject.Find("Events").transform.Find("LAB_RingsLogic").Find("LAB_RingsLogic").gameObject.GetComponent<LAB_Rings>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.revealFace();
                            Chunk.solved = true;
                            storage.BooleanList[118] = true;
                        }
                        return true;
                    case 119:
                        if (!storage.BooleanList[119])
                        {
                            GameObject Chunk = GameObject.Find("Events").transform.Find("LAB_RingsLogic").Find("Pivot").Find("Pickup Plate Knowledge").Find("Plate").gameObject;
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.GetComponent<ItemPickup>().pickUp();
                            storage.BooleanList[119] = true;
                        }
                        return true;
                    case 120:
                        if (!storage.BooleanList[120])
                        {
                            LAB_MultiLock Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").gameObject.GetComponent<LAB_MultiLock>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.Earth = true;
                            storage.BooleanList[120] = true;
                        }
                        return true;
                    case 121:
                        if (!storage.BooleanList[121])
                        {
                            LAB_MultiLock Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").gameObject.GetComponent<LAB_MultiLock>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.Air = true;
                            storage.BooleanList[121] = true;
                        }
                        return true;
                    case 122:
                        if (!storage.BooleanList[122])
                        {
                            LAB_MultiLock Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").gameObject.GetComponent<LAB_MultiLock>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.Fire = true;
                            storage.BooleanList[122] = true;
                        }
                        return true;
                    case 123:
                        if (!storage.BooleanList[123])
                        {
                            LAB_MultiLock Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").gameObject.GetComponent<LAB_MultiLock>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.Water = true;
                            storage.BooleanList[123] = true;
                        }
                        return true;
                    case 124:
                        if (!storage.BooleanList[124])
                        {
                            LAB_MultiLock Chunk = GameObject.Find("Tree Of Life").transform.Find("Chunk").gameObject.GetComponent<LAB_MultiLock>();
                            if (Chunk == null)
                            {
                                storage.BooleanQueue.Add(message);
                                return false;
                            }
                            Chunk.Gold = true;
                            storage.BooleanList[124] = true;
                        }
                        return true;
                    default:
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
        public static void InstantiateEnemy(GameObject enemy, int tag)
        {
            try
            {
                if( storage.ManagedEnemies.ContainsKey(tag) || storage.EnemyStatus.ContainsKey(tag))
                {
                    return; //Already Being Tracked
                }
                MelonLogger.Msg("Enemy Now Being Tracked: " + tag);
                storage.EnemyHP[tag] = 1000; // Base Value for Initation Purposes
                storage.ManagedEnemies[tag] = enemy;
                storage.EnemyStatus[tag] = true; //This prevents reactivation
                //Get Initial Positions and Rotations
                CheckEnemyVector(enemy, tag);
                CheckEnemyQuaternion(enemy, tag);
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Failure on Enemy Instantiation: " + tag + e.StackTrace);
            }
        }
        private static void EnemyInstantiationCheck(string RoomName, string EnemyLongName)
        {
            GameObject Enemy;
            string EnemyShortName = EnemyLongName.Split(' ').Last();
            try
            {
                if (GameObject.Find(RoomName).gameObject.transform.Find("Enemy Manager").gameObject.transform.Find(EnemyLongName) == null)
                {
                    if (GameObject.Find(RoomName).gameObject.transform.Find("Enemy Manager").gameObject.transform.Find(EnemyShortName) == null)
                    {
                        return;
                    }
                    Enemy = GameObject.Find(RoomName).gameObject.transform.Find("Enemy Manager").gameObject.transform.Find(EnemyShortName).gameObject;
                }
                else
                {
                    Enemy = GameObject.Find(RoomName).gameObject.transform.Find("Enemy Manager").gameObject.transform.Find(EnemyLongName).gameObject;
                }
                if (Enemy != null && Enemy.active == true)
                {
                    InstantiateEnemy(Enemy, 1);
                }
            }
            catch
            {
                MelonLogger.Msg("Failure on Instantiate Check for Enemy: " + EnemyLongName + " " + RoomName);
                return;
            }
            return;
        }
        public static void PeerSideEnemyHandler(Storage storage)
        {
            //First Handle if Enemies need to be Handled At All
            List<int> enemiesToRemove = new List<int>();
            foreach (KeyValuePair<int, GameObject> kvp in storage.ManagedEnemies)
            {
                int tag = kvp.Key;
                GameObject enemy = kvp.Value;

                //First Check if we Even Need to Handle It
                if ((enemy == null) || (!enemy.active))
                    {
                        MelonLogger.Msg(tag + " Removed due to Null");
                        enemiesToRemove.Add(tag); continue; //No Longer Active
                    }
                Hitbox hit = enemy.GetComponent<Hitbox>();
                if (hit == null)
                {
                    MelonLogger.Msg(tag + " Attempted to Remove due to Null on Hit");
                    enemiesToRemove.Add(tag); continue; //No Active Hitbox
                }
                EnemyController controller = enemy.GetComponent<EnemyController>();
                if (controller == null)
                {
                    MelonLogger.Msg(tag + " Attempted to Remove due to Null on Controller");
                    enemiesToRemove.Add(tag); continue; //No Active Controller
                }
                EnemyDamageHandlerPeer(storage, tag, hit);
                if (hit.HP <= 0) //Enemy is Dead or More Than Dead
                {
                    MelonLogger.Msg(tag + " Attempted to Remove due to Enemy Dead");
                    storage.MessageCollection.Add($"ER:{tag}");
                    enemiesToRemove.Add(tag); continue; //Enemy is dead stop tracking it
                }

                //Handle Location and Rotation
                Vector3? tempVector = CheckEnemyVector(enemy, tag);
                if (tempVector != null)
                {
                    string value = ($"EV:{tag}={tempVector}");
                    //MelonLogger.Msg(value);
                    storage.MessageCollection.Add(value);
                }
                Quaternion? tempQuater = CheckEnemyQuaternion(enemy, tag);
                if (tempQuater != null)
                {
                    string value = ($"EQ:{tag}={tempQuater}");
                    //MelonLogger.Msg(value);
                    storage.MessageCollection.Add(value);
                }

                //Handle Targeting
                if (TargetEnemy(storage, enemy))
                {
                    controller.playerPos.transform.position = storage.EllieMain.transform.position;
                }
                else
                {
                    controller.playerPos.transform.position = storage.EllieClone.transform.position;
                }
                
            }
            foreach (int tag in enemiesToRemove)
            {
                storage.ManagedEnemies.Remove(tag);
            }
        }
        private static void EnemyDamageHandlerPeer(Storage storage, int tag, Hitbox hit)
        {
            int health = hit.HP; //HP for Peer
            if (storage.EnemyHP[tag] > health) //if Peer is Less Than Stored; then Updated Stored.
            {
                storage.EnemyHP[tag] = health;
                string value = $"ED:{tag}={health}";
                MelonLogger.Msg($"Enemy {tag} HP is : " + value); 
                storage.MessageCollection.Add(value); //Need to Send Message, Damage was done by this Peer
            }
        }
        public static Vector3? CheckEnemyVector(GameObject enemy, int tag)
        {
            try
            {
                Vector3 e = enemy.transform.position;
                if (!storage.EnemyLocation.ContainsKey(tag))
                {
                    storage.EnemyLocation.Add(tag, e);
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
                    storage.EnemyRotation.Add(tag, e);
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
            catch (Exception e)
            {
                MelonLogger.Msg("Failure on 1490-Check Quaternion" + e.Message + e.StackTrace);
                return null;
            }

        }
        public static bool TargetEnemy(Storage storage, GameObject enemy)
        {
            Transform A = storage.EllieMain.transform;
            Transform B = storage.EllieClone.transform;
            Transform C = enemy.transform;
            if (A == null || B == null || C == null)
            {
                MelonLogger.Msg("Warning: One or more transforms are null in distance comparison; Error on 4437 " + enemy.name);
                return true; 
            }

            float distanceAC = Vector3.Distance(A.position, C.position);
            float distanceBc = Vector3.Distance(B.position, C.position);

            return distanceAC < distanceBc;
        }
        public static void DealDamage(string message)
        {
            Console.WriteLine("Recieved {message} for Enemy Damage Handling");
            int colonIndex = message.IndexOf(':');
            if (colonIndex != -1 && colonIndex < message.Length - 1)
            {
                message = message.Substring(colonIndex + 1);
                string[] sides = message.Split('=');
                if (int.TryParse(sides[0], out int tag) && int.TryParse(sides[1], out int value))
                {
                    if (!storage.EnemyHP.ContainsKey(tag))
                    {
                        storage.EnemyHP.Add(tag, value); // Add New HP Tracker for Set Value if Not Already Set
                    }
                    if (!storage.ManagedEnemies.ContainsKey(tag)) { return; } //Enemy Not Yet Instantiated Locally
                    if (storage.EnemyHP[tag] > value) // Tag Says Peer has Greater HP, Lower it to Match Other Peer
                    {
                        storage.ManagedEnemies[tag].GetComponent<Hitbox>().HP = storage.EnemyHP[tag]; 
                        Console.WriteLine("Damage Applied to Enemy " + tag + " for " + (storage.EnemyHP[tag] - value) + " damage.");  
                    }
                    //if false then the enemy has more damage done to it locally; 
                    return;
                }
            }
        }
        public static void KillEnemyOverRide(string message)
        {
            Console.WriteLine($"Recieved {message} for Enemy Death Handling");
            int colonIndex = message.IndexOf(':');
            if (colonIndex != -1 && colonIndex < message.Length - 1)
            {
                message = message.Substring(colonIndex + 1);
                if (int.TryParse(message, out int tag))
                {
                    if (!storage.ManagedEnemies.ContainsKey(tag)) { return; } //enemy not yet instantiated locally; this may cause unforseen bugs :D
                    storage.ManagedEnemies[tag].GetComponent<Hitbox>().HP = 0;
                    Console.WriteLine("Enemy " + tag + " Killed via Override.");
                    return;
                }
            }
        }
    }
}