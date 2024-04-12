using MelonLoader;
using SigiMP;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SigiMultiplayer 
{
    public class SignalisMultiplayer : MelonMod
    {
        static public SignalisLogic MultiplayerHandler = SignalisLogic.newSigiLogic();
        public override async void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.P) && (!MultiplayerHandler.Status) && (!MultiplayerHandler.failsafe))
            {
                EllieSetUp();
                if (MultiplayerHandler.host)
                {
                    MultiplayerHandler.server.StartServer();
                }
                if (!MultiplayerHandler.host)
                {
                    MultiplayerHandler.client.StartClient(MultiplayerHandler.url);
                }
                MultiplayerHandler.Elster = GameObject.Find("Ellie_Default(Clone)").gameObject;
            }
            if (MultiplayerHandler.Status)
            {
                MultiplayerHandler.timer += 1;
                if (MultiplayerHandler.host && MultiplayerHandler.timer > MultiplayerHandler.delay)
                {
                    MultiplayerHandler.MessageCentral();
                }
                if (!MultiplayerHandler.host && MultiplayerHandler.timer > MultiplayerHandler.delay)
                {
                    MultiplayerHandler.MessageCentral();
                }
            }
        }
        
        public static void EllieSetUp()
        {
            if (GameObject.Find("__Prerequisites__").gameObject != null)
            {
                MultiplayerHandler.SetUpLogic();
                MultiplayerHandler.Status = true;
            }
            else
            {
                MelonLogger.Msg("Not in a Active Scene, Please Return to a Active Scene to Enable the Mod");
                MultiplayerHandler.Status = false;
                return;
            }
        }
        //Handles End State
        public override void OnApplicationQuit()
        {
        }
    }
}