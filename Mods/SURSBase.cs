﻿using MelonLoader;
using UnityEngine;
using System.IO;

namespace SURS
{
    public class SURSBase : MelonMod
    {
        public override void OnUpdate()
        {
            if (GameObject.Find("__Prerequisites__") != null)
            {
                if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.R))
                {
                    MelonLoader.MelonLogger.Msg("SURS has Loaded");

                    //Find the Ingame Objects We Wish to Change, this cluster is for objects related to Elsters Body or SURS Base
                    GameObject PreReq = GameObject.Find("__Prerequisites__");
                    GameObject CharOrigin = PreReq.transform.Find("Character Origin").gameObject;
                    GameObject CharRoot = CharOrigin.transform.Find("Character Root").gameObject;
                    GameObject EllieDef = CharRoot.transform.Find("Ellie_Default").gameObject;
                    GameObject metarig = EllieDef.transform.Find("metarig").gameObject;
                    GameObject Root = metarig.transform.Find("Root").gameObject;
                    GameObject hips = Root.transform.Find("hips").gameObject;


                    string modsFolder = MelonHandler.ModsDirectory;
                    if (modsFolder == null)
                    {
                        MelonLoader.MelonLogger.Msg("MelonLoader Mods Folder Not Loading");
                    }
                    string sursLibraryFolder = Path.Combine(modsFolder, "SURSLibrary");
                    if (sursLibraryFolder == null)
                    {
                        MelonLoader.MelonLogger.Msg("SURS Library Could Not Be Loaded, Check to make sure you set it up correctly");
                    }
                    if (Directory.Exists(sursLibraryFolder))
                    {
                        string elsterBodyPath = Path.Combine(sursLibraryFolder, "elster_body_texture.png");
                        if (File.Exists(elsterBodyPath))
                        {
                            GameObject normalEllie = EllieDef.transform.Find("Normal")?.gameObject;
                            if (normalEllie != null)
                            {
                                GameObject elliebody = normalEllie.transform.Find("Body").gameObject;
                                SURSTextureSet((File.Exists(elsterBodyPath)), elsterBodyPath, elliebody);
                                GameObject hair1ellie = normalEllie.transform.Find("Hair").gameObject;
                                SURSTextureSet((File.Exists(elsterBodyPath)), elsterBodyPath, hair1ellie);
                                GameObject hairEllie = normalEllie.transform.Find("HairHead").gameObject;
                                SURSTextureSet((File.Exists(elsterBodyPath)), elsterBodyPath, hairEllie);
                            }
                            else
                            {
                                MelonLoader.MelonLogger.Msg("Failed to find Body Object");
                            }
                        }

                        string armoredellieBodyPath = Path.Combine(sursLibraryFolder, "elster_armored_texture.png");
                        if (File.Exists(armoredellieBodyPath))
                        {
                            //Surs Could be Updated to a Function Method Base, However It has yet to be done so. 
                            Texture2D armoredellieBodyTexture = SURSImageCall(armoredellieBodyPath);
                            GameObject armoredEllie = EllieDef.transform.Find("Armored").gameObject;
                            if (armoredEllie == null) { }
                            GameObject body = armoredEllie.transform.Find("Body").gameObject;
                            SkinnedMeshRenderer renderer = body.GetComponent<SkinnedMeshRenderer>();
                            if (renderer == null)
                            {
                                MelonLoader.MelonLogger.Msg("Body Render Not Found");
                            }
                            renderer.material.mainTexture = armoredellieBodyTexture;
                        }

                        string isaPath = Path.Combine(sursLibraryFolder, "isa.png");
                        if (File.Exists(isaPath))
                        {
                            Texture2D evaTexture = SURSImageCall(isaPath);
                            GameObject evaEllie = EllieDef.transform.Find("Isa_Past").gameObject;
                            GameObject body = evaEllie.transform.Find("Body").gameObject;
                            SkinnedMeshRenderer renderer = body.GetComponent<SkinnedMeshRenderer>();
                            if (renderer == null)
                            {
                                MelonLoader.MelonLogger.Msg("Body Render Not Found");
                            }
                            renderer.material.mainTexture = evaTexture;
                        }

                        string evaPath = Path.Combine(sursLibraryFolder, "eva.png");
                        if (File.Exists(evaPath))
                        {
                            //New Functionality Usage 
                            GameObject evaEllie = EllieDef.transform.Find("EVA")?.gameObject;
                            if (evaEllie != null) {
                                GameObject gameObject = evaEllie.transform.Find("Body").gameObject;
                                SURSTextureSet(File.Exists(evaPath), evaPath, gameObject);
                                GameObject object2 = evaEllie.transform.Find("Visor").gameObject;
                                SURSTextureSet(File.Exists(evaPath), evaPath, gameObject);
                            }
                            else
                            {
                                MelonLoader.MelonLogger.Msg("Failed to find Eva Object");
                            }
                        }
                        string crippledPath = Path.Combine(sursLibraryFolder, "crippled.png");
                        string organsPath = Path.Combine(sursLibraryFolder, "organs.png");
                        if (File.Exists(crippledPath) || (File.Exists(organsPath)))
                        {
                            GameObject crippledEllie = EllieDef.transform.Find("Crippled")?.gameObject;
                            if (crippledEllie != null)
                            {
                                GameObject body = crippledEllie.transform.Find("Body").gameObject;
                                SURSTextureSet((File.Exists(crippledPath)), crippledPath, crippledEllie);
                                GameObject organsEllie = crippledEllie.transform.Find("Organs").gameObject;
                                SURSTextureSet((File.Exists(organsPath)), organsPath, organsEllie);
                            }
                            else
                            {
                                MelonLoader.MelonLogger.Msg("Failed to find Crippled Object");
                            }
                        }
                    }
                }
            }
        }
        public static Material TextureFind(GameObject desiredObject)
        {
            //Used in mods that swap textures without use of SURS
            SkinnedMeshRenderer renderer = desiredObject.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                MelonLoader.MelonLogger.Msg("Texture Loaded");
                Material material = renderer.material;
                return material;
            }
            else
            {
                return null;
            }
        }
        public static bool SURSTextureSet(bool state, string path, GameObject parent)
        {
            if (!state)
            {
                return false;
            }
            Texture2D evaTexture = SURSImageCall(path);
            SkinnedMeshRenderer renderer = parent.GetComponent<SkinnedMeshRenderer>();
            renderer.material.mainTexture = evaTexture;
            return true;
        }
        public static Texture2D SURSImageCall(string filename)
        {
            //Used in SURS (Signalis Universal ReSkin Mod)
            byte[] imageData = System.IO.File.ReadAllBytes(filename);
            Texture2D SURStexture = new Texture2D(2, 2);
            ImageConversion.LoadImage(SURStexture, imageData);
            return SURStexture;
        }
    }
}