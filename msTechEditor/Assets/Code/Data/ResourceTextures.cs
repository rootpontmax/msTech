using System.IO;
using UnityEditor;
using UnityEngine;

namespace msTech.Data
{
    [CreateAssetMenu(fileName = "ResourceTextures", menuName = "msTech/Resources/Textures")]
    public class ResourceTextures : ResourceBase
    {
        public Texture2D[] textures;

        public override void Export(string folder)
        {
            if ( null != textures )
                for ( int i = 0; i < textures.Length; ++i )
                    if ( null != textures[i] )
                    {
                        Texture oldTexture = textures[i];

                        // Fix texture path
                        string oldPath = AssetDatabase.GetAssetPath(oldTexture);                        
                        string directory = Path.GetDirectoryName(oldPath).Replace("Assets/Resources/", "");
                        string filename = Path.GetFileNameWithoutExtension(oldPath);

                        /*
                        Debug.LogError("OldPath: " + oldPath);
                        Debug.LogError("Directory: " + directory);
                        Debug.LogError("Filename: " + filename);
                        */

                        string[] folders = directory.Split("/");
                        string parentFolder = folder;
                        if ( null != folders )
                            for ( int j = 0; j < folders.Length; ++j )
                                if ( !string.IsNullOrEmpty(folders[i]) )
                                {
                                    string fillFolderPath = parentFolder + "/" + folders[j];
                                    if ( !Directory.Exists(fillFolderPath) )
                                        Directory.CreateDirectory(fillFolderPath);

                                    string newFolderName = parentFolder + "/" + folders[j];
                                    parentFolder += "/" + folders[j];
                                }

                        // Create compressed texture
                        Texture2D newTexture = new Texture2D(oldTexture.width, oldTexture.height, TextureFormat.RGBA32, true, false);
                        Color32[] pixels = textures[i].GetPixels32(MIP_LEVEL);
                        newTexture.SetPixels32(pixels);
                        EditorUtility.CompressTexture(newTexture, TextureFormat.ASTC_8x8, TextureCompressionQuality.Best);
                        newTexture.Apply(true, true);


                        // Save data to files
                        byte[] textureData = newTexture.GetRawTextureData();
                        //byte[] fileData = ImageConversion.EncodeArrayToTGA(textureData, newTexture.graphicsFormat, (uint)newTexture.width, (uint)newTexture.height);
                        //byte[] fileData = ImageConversion.EncodeArrayToEXR(textureData, newTexture.graphicsFormat, (uint)newTexture.width, (uint)newTexture.height);
                        string newPath = folder + "/" + directory + "/" + filename + ".astc";
                        //File.WriteAllBytes(newPath, fileData);
                        File.WriteAllBytes(newPath, textureData);
                        
                        //byte[] bytes = ImageConversion.EncodeArrayToTGA(oldTexture.GetRawTextureData(), tex.graphicsFormat, (uint)width, (uint)height);

                        /*
                        

                        
                        
                        


                        // Fix texture path
                        string oldPath = AssetDatabase.GetAssetPath(oldTexture);                        
                        string directory = Path.GetDirectoryName(oldPath).Replace("Assets/Resources/", "");
                        string filename = Path.GetFileNameWithoutExtension(oldPath);

                        Debug.LogError("OldPath: " + oldPath);
                        Debug.LogError("Directory: " + directory);
                        Debug.LogError("Filename: " + filename);

                        string[] folders = directory.Split("/");
                        string parentFolder = "Assets/Output";
                        if ( null != folders )
                            for ( int j = 0; j < folders.Length; ++j )
                                if ( !string.IsNullOrEmpty(folders[i]) )
                                {
                                    string newFolderName = parentFolder + "/" + folders[j];
                                    if ( !AssetDatabase.IsValidFolder(newFolderName) )
                                    {
                                        AssetDatabase.CreateFolder(parentFolder, folders[j]);
                                        AssetDatabase.SaveAssets();
                                    }
                                    parentFolder += "/" + folders[j];
                                }
                        */

                        /*
                        string newPath = parentFolder + "/" + filename + ".png";
                        AssetDatabase.CreateAsset(newTexture, newPath);
                        */

                        /*
                        string extention = Path.GetExtension(oldPath);
                        Debug.LogError("Old file extention: " + extention);

                        string textureName = oldPath.Replace("Assets/Resources/", "");
                        textureName = textureName.Replace(extention, "");
                        Debug.LogError("Texture name: " + textureName);
                        
                        Debug.LogError("NewPath: " + newPath);

                        
                        
                        */
                    }
        }

        private static readonly int MIP_LEVEL = 0;
    }
}
