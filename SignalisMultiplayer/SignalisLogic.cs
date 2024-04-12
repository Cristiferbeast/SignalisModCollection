using MelonLoader;
using SigiMultiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SigiMP
{
    public class SignalisLogic : MultiplayerLogic
    {
        //interior storage
        public BooleanHandler Handler = new BooleanHandler();
        public GameObject ElsterOriginal;
        public GameObject Elster;
        
        //Set Up
        static public SignalisLogic newSigiLogic()
        {
            //creates base storage
            return new MultiplayerLogic() as SignalisLogic;
        }
        public void SetUpLogic()
        {
            //sets up ellie storage
            GameObject Prerequisties = GameObject.Find("__Prerequisites__").gameObject;
            GameObject CharOrigin = Prerequisties.transform.Find("Character Origin").gameObject;
            ElsterOriginal = CharOrigin.transform.Find("Character Root").gameObject;
            try
            {
                this.SetUpLogic(ElsterOriginal); //stores the root under Built Logic 
                Elster = this.BuiltLogic.transform.Find("Ellie_Default").gameObject;
                Elster.GetComponent<Anchor>().enabled = false;
                if (Elster == null) { this.Status = false; MelonLogger.Msg("Ellie is Null"); return; }
            }
            catch
            {
                Console.Error.WriteLine("Set Up Of Logic Failed");
            }
            this.SetStoredLocation(Elster);
        }

        //Core Runtime
        public void MessageCentral()
        {
            if (BooleanQueue.Count() != 0)
            {
                foreach (string s in BooleanQueue)
                {
                    HandleMessage(s);
                }
            }
            if (host)
            {
                if (server.GetPlayerCount() != 0)
                {
                    List<string> Movement = server.GetPlayerMovement();
                    HandleMessage(Movement[0]);
                    HandleMessage(Movement[1]);
                    Movement = server.GetMessageQueue();
                    foreach (string s in Movement)
                    {
                        HandleMessage(s);
                    }
                }
                MessageCollection.Clear();
                AddMessages();
                this.MessageCentral(true);
            }
            else
            {
                List<string> Movement = client.GetPlayerMovement();
                HandleMessage(Movement[0]);
                HandleMessage(Movement[1]);
                Movement = client.GetMessageQueue();
                foreach (string s in Movement)
                {
                    HandleMessage(s);
                }
                MessageCollection.Clear();
                AddMessages();
                this.MessageCentral(true);
            }
        }

        //Message Handling 
        public void HandleMessage(string message)
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
                ApplyVector(Elster, message);
            }
            else if (message.StartsWith("Q:"))
            {
                ApplyQuaternion(Elster, message);
            }
            else if (message.StartsWith("B:"))
            {
                Handler.ApplyBool(message);
            }
            else if (message.StartsWith("E:"))
            {
                Handler.EnemyMessageReaderPrime(message);
            }
            else if (message == "" || message == "placeholder")
            {
            }
            else
            {
                MelonLoader.MelonLogger.Msg("Server Recieved Unhandled Logic " + message);
            }
        }

        //Message Reading
        public void AddMessages()
        {
            List<Vector3> vvalue = CheckVector();
            if (vvalue != null)
            {
                foreach (Vector3 vecvalue in vvalue)
                {
                    string value = ("V:" + vecvalue.ToString());
                    MessageCollection.Add(value);
                }
            }
            List<Quaternion> qvalue = CheckQuaternion();
            if (qvalue != null)
            {
                foreach (Quaternion quatervalue in qvalue)
                {
                    string value = ("Q:" + quatervalue.ToString());
                    MessageCollection.Add(value);
                }
            }
            List<string> bkeyvalue = Handler.BooleanChecker();
            if (bkeyvalue != null)
            {
                foreach (string fkey in bkeyvalue)
                {
                    string value = ("B:" + fkey);
                    MessageCollection.Add(value);
                }
            }
        }
        public List<Vector3> CheckVector()
        {
            try
            {
                if (this.l == null)
                {
                    this.l = Elster.transform.position;
                    return null;
                }
                return CheckVector(ElsterOriginal, this.l);
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Failure on Check Vector " + ex.Message + ex.StackTrace);
                return null;
            }
        }
        public List<Quaternion> CheckQuaternion()
        {
            try
            {
                r = ElsterOriginal.transform.rotation;
            }
            catch
            {

            }
            return CheckQuaternion(ElsterOriginal, r);

        }

    }
    public class BooleanHandler
    {
        public List<string> Result;
        public List<bool> BooleanList;
        public Dictionary<int,GameObject> ActiveEnemyList;
        public List<string> TemporaryEnemyData;
        public List<GameObject> ManagedEnemies;
        public List<string> BooleanQueue;

        //Core Boolean Handler
        public BooleanHandler()
        {
            Result = new List<string>() {""};
            BooleanList = new List<bool>() {false, false, false};
            ActiveEnemyList = new Dictionary<int, GameObject>();
            TemporaryEnemyData = new List<string>();
        }
        public List<string> BooleanChecker() {
            Result.Clear();
            switch (SceneManager.GetActiveScene().name)
            {
                case "PEN_Wreck":
                    PenHole();
                    break;
                default:
                    break;
            }
            return Result;
        }
        
        //Scenes
        public void PenHole()
        {
            try
            {
                if (!BooleanList[0])
                {
                    //if the Cryopod is Active we can search it
                    if (GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject.activeSelf == true)
                    {
                        PEN_Cryo Cryo = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject.transform.Find("Cryo").gameObject.transform.GetComponent<PEN_Cryo>();
                        if (Cryo != null)
                        {
                            BooleanList[0] = Cryo.opened; //this is set true when the value is true
                            if (BooleanList[0])
                            {
                                Result.Add("0,0");
                            }
                        }
                        return;
                    }
                }
                if (!BooleanList[1])
                {
                    if(GameObject.Find("Events").gameObject.transform.Find("Pen_CockpitEvent3D").gameObject.activeSelf == true)
                    {
                        GameObject Cockpit = GameObject.Find("Events").gameObject.transform.Find("Pen_CockpitEvent3D").gameObject.transform.Find("Interactions").gameObject;
                        if (Cockpit.transform.Find("PhotoPickup") == null)
                        {
                            BooleanList[1] = true;
                            Result.Add("1,0");
                        };
                    }
                    return; //in the future invert the ifs
                }
                if (!BooleanList[2])
                {
                    if(GameObject.Find("Events").gameObject.transform.Find("PEN_PC").gameObject.active == true)
                    {
                        GameObject Ducttape = GameObject.Find("Events").gameObject.transform.Find("PEN_PC").gameObject;
                        if (Ducttape.transform.Find("TapePickup") == null)
                        {
                            BooleanList[2] = true;
                            Result.Add("2,0");
                        }
                    }
                    return;
                }
                
            }
            catch(Exception e)
            {
                MelonLogger.Msg(e.Message + " " + e.StackTrace);
            }
        }

        //Enemy Logic 
        public void EnemyReaderLogic()
        {
            //When an enemy is in a room, the boolean checker will add the value of that rooms enemy tag 
            if (ActiveEnemyList.Count == 0)
            {
                return;
            }
            foreach (int enemy in ActiveEnemyList.Keys)
            {
                EnemyDetails(ActiveEnemyList[enemy], enemy);
            }
        }
        public string EnemyDetails(GameObject Enemy, int tag)
        {
            if (Enemy == null)
            {
                Console.WriteLine("Enemy Returned Null Error");
                return null;
            }
            string Details = $"E:{tag + 1}>";
            string FDet = "";
            if (TemporaryEnemyData[tag] == null)
            {
                Details += "V:" + Enemy.transform.position.ToString() + ">";
                Details += "Q:" + Enemy.transform.rotation.ToString() + ">";
                Details += "B:" + true + ">"; //replace with dead logic
                FDet = Details;
            }
            else
            {
                string[] data = TemporaryEnemyData[tag].Split('>');
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
            TemporaryEnemyData[tag] = FDet;
            return Details;
        }
        public static GameObject ApplyEnemy(int tag)
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
        public void EnemyMessageReaderPrime(string message)
        {
            //All Enemy Units must have 2 digits of Boolean, First digit is active state, Second digit is primacy. If Active State is true, then opposite has primacy, if it is false then current has primacy
            string[] data = message.Split('>');
            int outtag = int.Parse(data[0]);
            if (ManagedEnemies[outtag] == null)
            {
                ManagedEnemies[outtag] = ApplyEnemy(outtag);
            }
            SignalisLogic.ApplyVector(ManagedEnemies[outtag], data[1]);
            SignalisLogic.ApplyQuaternion(ManagedEnemies[outtag], data[2]);
            ApplyBool(data[3]);
        }

        //Applying Bools
        public  void ApplyBool(string message)
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
        public void ApplyBool(bool boolean, int tag, string message)
        {
            switch (tag)
            {
                case 1:
                    if (!BooleanList[0])
                    {
                        GameObject Chunk = GameObject.Find("Cryogenics").gameObject.transform.Find("Chunk").gameObject;
                        if (Chunk == null)
                        {
                            BooleanQueue.Add(message);
                            MelonLogger.Msg("Bool Qued");
                            return;
                        }
                        MelonLogger.Msg("Bool Used");
                        PEN_Cryo Cryo = Chunk.transform.Find("Cryo").gameObject.transform.GetComponent<PEN_Cryo>();
                        Cryo.Open(); //Plays Cutscene Fixes Boolean Mismatch
                        BooleanList[0] = boolean; //this is set true when the value is true
                    }
                    break;
                case 2:
                    if (!BooleanList[1])
                    {
                        GameObject Chunk = GameObject.Find("Events").gameObject.transform.Find("Pen_CockpitEvent3D").gameObject;
                        if (Chunk == null)
                        {
                            BooleanQueue.Add(message);
                            MelonLogger.Msg("Bool Qued");
                            return;
                        }
                        MelonLogger.Msg("Bool Used");
                        GameObject Cockpit = Chunk.transform.Find("Interactions").gameObject;
                        if (Cockpit.transform.Find("PhotoPickup") != null)
                        {
                            Cockpit.transform.Find("PhotoPickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Cockpit.transform.Find("PhotoPickup").gameObject.SetActive(false);
                            BooleanList[1] = boolean;
                        }
                    }
                    break;
                case 3:
                    if (!BooleanList[2])
                    {
                        GameObject Ducttape = GameObject.Find("Events").gameObject.transform.Find("PEN_PC").gameObject;
                        if (Ducttape == null)
                        {
                            BooleanQueue.Add(message);
                            MelonLogger.Msg("Bool Qued");
                            return;
                        }
                        if (Ducttape.transform.Find("TapePickup") != null)
                        {
                            Ducttape.transform.Find("TapePickup").gameObject.transform.GetComponent<ItemPickup>().pickUp();
                            Ducttape.transform.Find("TapePickup").gameObject.SetActive(false);
                            BooleanList[2] = boolean;
                        }
                        else
                        {
                            BooleanList[2] = boolean;
                            MelonLogger.Msg("Already Obtained Tape?");
                        }
                    }
                    break;
                default:
                    break;
            }
        }

    }
}
