using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AdlerBossFightMod
{
    public class Storage
    {
        public int central = 0;
    }
    public class AdlerBossFight : MelonMod
    {
        Storage storage;
        public override void OnUpdate()
        {
            if(storage == null)
                storage = new Storage();
            if(SceneManager.GetActiveScene().name == "BOS_Adler")
            {
                if (storage.central == 0)
                {
                    GameObject Somewhere = GameObject.Find("Rooms").transform.Find("Falke").gameObject.transform.Find("Somewhere").gameObject;
                    Somewhere.SetActive(true);
                    GameObject Falke = Somewhere.transform.Find("Enemy Manager").gameObject.transform.Find("FKLR").gameObject;
                    Falke.SetActive(false);
                    Somewhere.transform.Find("Chunk").gameObject.SetActive(false);
                    GameObject Adler = Somewhere.transform.Find("Enemy Manager").gameObject.transform.Find("ADLR").gameObject;
                    Adler.SetActive(true);
                    Adler.transform.position = new Vector3(265.0546f, -3580.152f, 0.0007f);
                    //Move Our Lil Gremlin
                    MelonLogger.Msg("Adler Bossfight Activated");
                    storage.central = 1;
                }
                else
                {
                }
            }
        }
    }
}
