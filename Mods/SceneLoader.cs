using System.IO;
using UnityEngine;
using MelonLoader;

public class MyMod : MelonMod
{
    private void Update()
    {
        string filePath = "path_to_your_text_file.txt"; 
        if (Input.GetKeyDown(KeyCode.S) && Input.GetKeyDown(KeyCode.C) && Input.GetKeyDown(KeyCode.E) && Input.GetKeyDown(KeyCode.N))
        {
            string sceneName = ReadSceneNameFromFile(filePath);
            if (string.IsNullOrEmpty(sceneName))
            {
                Log("");
            }
            else{
                SceneManager.LoadScene(sceneName);
            }
        }
    }
    public static string ReadSceneNameFromFile(string filePath)
    {
        string sceneName = "";
        try{
            using (StreamReader reader = new StreamReader(filePath))
            {
                sceneName = reader.ReadLine()?.Trim();
            }
        }
        catch{
            MelonLoader.MelonLog("Scene Could not be read")
        }
        return sceneName;
    }
}