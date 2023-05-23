using MelonLoader;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camera_Perspective_Mod
{
    public class PerspectiveChange : MelonMod
    {
        SignalisCodeBank Gerald = new SignalisCodeBank();
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
                        if (Gerald.GOErrorCatch("Main Camera", CharRoot))
                        {
                            GameObject MainCamera = LocalSpace.transform.Find("Main Camera").gameObject;
                            Gerald.CustomCamera(MainCamera, CharRoot, coords, position);
                        }
                        else
                        {
                            GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                            Gerald.CustomCamera(NewCamera, CharRoot, coords, position);
                        }
                        MelonLoader.MelonLogger.Msg("FPS Mode Enabled");
                    }
                    //DeadSpace Cam
                    if (Input.GetKeyDown(KeyCode.F2))
                    {
                        Vector3 coords = new Vector3(3, 7.5f, -5f);
                        Quaternion position = Quaternion.Euler(5, 355, 0);
                        if (Gerald.GOErrorCatch("Main Camera", CharRoot))
                        {
                            GameObject MainCamera = LocalSpace.transform.Find("Main Camera").gameObject;
                            Gerald.CustomCamera(MainCamera, CharRoot, coords, position);
                        }
                        else
                        {
                            GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                            Gerald.CustomCamera(NewCamera, CharRoot, coords, position);
                        }
                        MelonLoader.MelonLogger.Msg("DeadSpace Camera Mode Enabled");
                    }
                    //DMC Cam
                    if (Input.GetKeyDown(KeyCode.KeypadPlus))
                    {
                        Vector3 coords = new Vector3(0.3491f, 10.4746f, -6.7382f);
                        Quaternion position = Quaternion.Euler(13.7783f, 358.8838f, 359.6f);
                        if (Gerald.GOErrorCatch("Main Camera", CharRoot))
                        {
                            GameObject MainCamera = LocalSpace.transform.Find("Main Camera").gameObject;
                            Gerald.CustomCamera(MainCamera, CharRoot, coords, position);
                        }
                        else
                        {
                            GameObject NewCamera = CharRoot.transform.Find("Main Camera").gameObject;
                            Gerald.CustomCamera(NewCamera, CharRoot, coords, position);
                        }
                        MelonLoader.MelonLogger.Msg("DMC Camera Mode Enabled");
                    }
                }
            }
        }
    }
    public class SignalisCodeBank
    {
        public void CustomCamera(GameObject MainCamera, GameObject CharRoot, Vector3 coords, Quaternion position)
        {
            CameraToggle(MainCamera, CharRoot);
            MainCamera.transform.localPosition = coords;
            MainCamera.transform.localRotation = position;
        }
        public void CameraToggle(GameObject MainCamera, GameObject CharRoot)
        {
            MainCamera.transform.parent = CharRoot.transform;
            MelonLoader.MelonLogger.Msg("Modded Camera State Enabled");
            MainCamera.GetComponent<AngledCamControl>().enabled = false;
            UnityEngine.Camera cameraComponent = MainCamera.GetComponent<UnityEngine.Camera>();
            cameraComponent.orthographic = false;
            GameObject VHS = MainCamera.transform.Find("VHS UI").gameObject;
            UnityEngine.Camera VHSComponent = VHS.GetComponent<UnityEngine.Camera>();
            VHSComponent.orthographic = false;
        }
        public bool GOErrorCatch(string ObjectName, GameObject parent)
        {
            try
            {
                GameObject AngCamRig = parent.transform.Find(ObjectName).gameObject;
                return false;
            }
            catch
            {
                MelonLoader.MelonLogger.Msg("Error", ObjectName, "Not Found");
                return true;
            }
        }
    }
}