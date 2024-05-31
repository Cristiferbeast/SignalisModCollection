using MelonLoader;
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace SigiMP
{ }
    /*public class MultiplayerLogic
    {
        //State Varibales
        public bool Status = false;
        public bool failsafe = false;

        //Server Variables
        public bool host; //Is Host?
        public string url; //IP 
        public SigiClient client; //client server
        public SigiServer server; //host server
        public int delay = 1;
        public int timer = 0;

        //Message Variables
        public List<string> StoredMessgaes = new List<string>() { };
        public List<string> OutGoingMessages = new List<string>() { };
        public List<string> BooleanQueue = new List<string>();
        public List<string> MessageCollection = new List<string>() { };


        //In Game Variables
        public GameObject MainPlayer;
        public GameObject SecondaryPlayer;
        public Vector3 l; //location
        public Quaternion r; //roation
        public GameObject BuiltLogic; //logic handler
        public Scene CurrentScene;
        public bool SwapScene = false;

        //Set Up
        public void MultiplayerSetUp()
        {
            //sets up this item to allow for a central storage point
            MelonLogger.Msg("Storage Created");
            string modsFolder = MelonHandler.ModsDirectory;
            string MultiplayerModConfigPath = Path.Combine(modsFolder, "SigiMultiplayerConfig.txt");
            if (!File.Exists(MultiplayerModConfigPath))
            {
                failsafe = true;
                MelonLogger.Msg("Config File is Missing or Not in the Proper Location, Ensure it is inside your Mods Folder");
                return;
            }
            string[] lines = File.ReadAllLines(MultiplayerModConfigPath);
            if (bool.TryParse(lines[0], out bool hostValue))
            {
                host = hostValue;
                MelonLogger.Msg("Host Value Initalized");
            }
            url = lines[1];
            if (host)
            {
                server = new SigiServer();
                return;
            }
            client = new SigiClient();
        }
        virtual public void SetUpLogic(GameObject LogicObject)
        {
            //creates the logic object   
            try
            {
                BuiltLogic = UnityEngine.Object.Instantiate(LogicObject);
            }
            catch
            {
                Console.Error.WriteLine("Unable to Instantiate Object");
            }
        }
        virtual public void SetStoredLocation(GameObject PlayerObject)
        {
            MelonLogger.Msg("Setting Stored Location");
            //stores player location
            try
            {
                this.l = PlayerObject.transform.position;
                this.r = PlayerObject.transform.rotation;
            }
            catch
            {
                MelonLogger.Msg("Failure due to Storage of Postional Storing");
            }
        }
        //Message Readers
        virtual public List<Vector3> CheckVector(GameObject gameObject, Vector3 l)
        {
            List<Vector3> VList = new List<Vector3>() { };
            Vector3 e = gameObject.transform.position;
            if (e.x != l.x || e.y != l.y || e.z != l.z)
            {
                if (e.z - 2 > l.z || e.z + 2 < l.z)
                {
                    VList.Add(e);
                    this.l = e;
                    return VList;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public List<Quaternion> CheckQuaternion(GameObject gameObject, Quaternion q)
        {
            try
            {
                List<Quaternion> QList = new List<Quaternion>() { };
                Quaternion e = gameObject.transform.rotation;
                if (q == null)
                {
                    return null;
                }
                if (e.x != q.x || e.y != q.y || e.z != q.z)
                {
                    QList.Add(e);
                    return QList;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                MelonLogger.Msg("Failure on Check Quaternion");
                return null;
            }
        }

        //Message Handlers
        virtual public void MessageCentral(bool state)
        {
            if(SceneManager.GetActiveScene() != CurrentScene)
            {
                SwapScene = true;
            }
            if (host)
            {
                if (MessageCollection.Count > 0)
                {
                    string send = "";
                    foreach (string responseMessage in MessageCollection)
                    {
                        send += $"~{responseMessage}";
                    }
                    if (send != "")
                    {
                        server.UdpServerUpdate(send);
                    }
                }
            }
            else
            {
                if (MessageCollection.Count > 0)
                {
                    string send = "";
                    foreach (string responseMessage in MessageCollection)
                    {
                        send += $"~{responseMessage}";
                    }
                    if (send != "")
                    {
                        client.UdpClientUpdate(send);
                    }
                }
            }
            timer -= delay;
        }
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
    }
}
*/