using Il2CppSystem;
using MelonLoader;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camera_Perspective_Mod
{
    public class PerspectiveChange : MelonMod
    {
        public override void OnUpdate()
        {
            if (SceneManager.GetActiveScene().name != "PEN_Hole")
            {
                if (GameObject.Find("__Prerequisites__") != null)
                {
                    ///Basics Variables
                    GameObject PreReq = GameObject.Find("__Prerequisites__");
                    GameObject AngCamRig = PreReq.transform.Find("Angled Camera Rig").gameObject;
                    GameObject LocalSpace = AngCamRig.transform.Find("LocalSpace").gameObject;

                    //Char Variables
                    GameObject CharOrigin = PreReq.transform.Find("Character Origin").gameObject;
                    GameObject CharRoot = CharOrigin.transform.Find("Character Root").gameObject;

                    //FPS Cam
                    if (Input.GetKeyDown(KeyCode.F1))
                    {
                        Vector3 coords = new Vector3(0.3f, 8.4f, 2.1f);
                        Quaternion position = Quaternion.Euler(0, 0, 0);
                        if (!SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot))
                        {
                            GameObject MainCamera = LocalSpace.transform.Find("Main Camera").gameObject;
                            SignalisCodeBank.CustomCamera(MainCamera, CharRoot, coords, position);
                        }
                        else
                        {
                            GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                            SignalisCodeBank.CustomCamera(NewCamera, CharRoot, coords, position);
                        }
                        MelonLoader.MelonLogger.Msg("FPS Mode Enabled");
                    }
                    //DeadSpace Cam
                    if (Input.GetKeyDown(KeyCode.F2))
                    {
                        Vector3 coords = new Vector3(3, 7.5f, -5f);
                        Quaternion position = Quaternion.Euler(5, 355, 0);
                        if (!SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot))
                        {
                            GameObject MainCamera = LocalSpace.transform.Find("Main Camera").gameObject;
                            SignalisCodeBank.CustomCamera(MainCamera, CharRoot, coords, position);
                        }
                        else
                        {
                            GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                            SignalisCodeBank.CustomCamera(NewCamera, CharRoot, coords, position);
                        }
                        MelonLoader.MelonLogger.Msg("DeadSpace Camera Mode Enabled");
                    }
                    //DMC Cam
                    if (Input.GetKeyDown(KeyCode.KeypadPlus))
                    {
                        Vector3 coords = new Vector3(0.3491f, 10.4746f, -6.7382f);
                        Quaternion position = Quaternion.Euler(13.7783f, 358.8838f, 359.6f);
                        if (!SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot))
                        {
                            GameObject MainCamera = LocalSpace.transform.Find("Main Camera").gameObject;
                            SignalisCodeBank.CustomCamera(MainCamera, CharRoot, coords, position);
                        }
                        else
                        {
                            GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                            SignalisCodeBank.CustomCamera(NewCamera, CharRoot, coords, position);
                        }
                        MelonLoader.MelonLogger.Msg("DMC Camera Mode Enabled");
                    }

                    //Default Cam
                    if (Input.GetKeyDown(KeyCode.KeypadMinus) && (SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot, false)))
                    {
                        SignalisCodeBank.CameraRestore(CharRoot, LocalSpace);
                    }

                    //Add Camera Control Logic 
                    if (Input.GetKey(KeyCode.UpArrow) && (SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot, false)))
                    {
                        GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                        SignalisCodeBank.CameraControl(NewCamera, false, true);
                    }
                    if (Input.GetKey(KeyCode.DownArrow) && (SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot, false)))
                    {
                        GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                        SignalisCodeBank.CameraControl(NewCamera, true, true);
                    }
                    if (Input.GetKey(KeyCode.LeftArrow) && (SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot, false)))
                    {
                        GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                        SignalisCodeBank.CameraControl(NewCamera, false, false); ;
                    }
                    if (Input.GetKey(KeyCode.RightArrow) && (SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot, false)))
                    {
                        GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                        SignalisCodeBank.CameraControl(NewCamera, true, false);
                    }
                    if (Input.GetKeyDown(KeyCode.Space) && (SignalisCodeBank.GOErrorCatch("Main Camera", CharRoot, false))){
                        GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                        if (NewCamera.transform.localPosition ==  new Vector3(0.3f, 8.4f, 2.1f)) 
                        {
                            //DMC
                            NewCamera.transform.localRotation = Quaternion.Euler(13.7783f, 358.8838f, 359.6f);
                        }
                        if (NewCamera.transform.localPosition == new Vector3(3, 7.5f, -5f))
                        {
                            //FPS
                            NewCamera.transform.localRotation = Quaternion.Euler(0,0,0);
                        }
                        if (NewCamera.transform.localPosition == new Vector3(0.3491f, 10.4746f, -6.7382f))
                        {
                            //DeadSpace
                            NewCamera.transform.localRotation = Quaternion.Euler(5, 355, 0);
                        }
                    }
                }
            }
        }
    }
    public class SignalisCodeBank
    {
        public static void CustomCamera(GameObject MainCamera, GameObject CharRoot, Vector3 coords, Quaternion position, bool initialize = false)
        {
            CameraToggle(MainCamera, CharRoot, initialize);
            MainCamera.transform.localPosition = coords;
            MainCamera.transform.localRotation = position;
        }
        public static void CameraToggle(GameObject MainCamera, GameObject CharRoot, bool initialize = false)
        {
            MainCamera.transform.parent = CharRoot.transform;
            MelonLoader.MelonLogger.Msg("Modded Camera State Enabled");
            MainCamera.GetComponent<AngledCamControl>().enabled = initialize;
            UnityEngine.Camera cameraComponent = MainCamera.GetComponent<UnityEngine.Camera>();
            cameraComponent.orthographic = initialize;
            GameObject VHS = MainCamera.transform.Find("VHS UI").gameObject;
            UnityEngine.Camera VHSComponent = VHS.GetComponent<UnityEngine.Camera>();
            VHSComponent.orthographic = initialize;
        }

        public static void CameraControl(GameObject MainCamera, bool vChange, bool dChange, float degreeChange = 0.01f)
        {
            Quaternion customRotate = MainCamera.transform.localRotation;
            if ((vChange == dChange) && dChange)
            {
                customRotate.x += degreeChange;
                MainCamera.transform.localRotation = customRotate;
            }
            if ((vChange == dChange) && !dChange)
            {
                customRotate.y -= degreeChange;
                MainCamera.transform.localRotation=customRotate;
            }
            if ((vChange != dChange) && dChange)
            {
                customRotate.x -= degreeChange;
                MainCamera.transform.localRotation = customRotate;
            }
            if ((vChange != dChange) && !dChange)
            {
                customRotate.y += degreeChange;
                MainCamera.transform.localRotation = customRotate;
            }

        }
        public static void CameraRestore(GameObject CharRoot, GameObject LocalSpace)
        {
            GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
            Vector3 coords = new Vector3(-1073, 0.8987f, -172.2069f);
            Quaternion position = Quaternion.Euler(0, 0, 0);
            SignalisCodeBank.CustomCamera(NewCamera, LocalSpace, coords, position, true);
            MelonLoader.MelonLogger.Msg("Default Camera Restored");
        }
        public static bool GOErrorCatch(string ObjectName, GameObject parent, bool logger = true)
        {
            try
            {
                GameObject AngCamRig = parent.transform.Find(ObjectName).gameObject;
                return true;
            }
            catch
            {
                if (logger)
                {
                    MelonLoader.MelonLogger.Msg("Error", ObjectName, "Not Found");
                }
                return false;
            }
        }
    }
}