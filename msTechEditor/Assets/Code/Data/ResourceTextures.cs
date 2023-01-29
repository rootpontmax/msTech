using System;
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
                        
                        newTexture.Apply(false, false);

                        byte[] originalTextureData = newTexture.GetRawTextureData();
                        //EditorUtility.CompressTexture(newTexture, TextureFormat.ASTC_4x4, TextureCompressionQuality.Best);
                        EditorUtility.CompressTexture(newTexture, TextureFormat.PVRTC_RGBA4, TextureCompressionQuality.Best);
                        

                        
                        byte[] header = GetHeaderASTC(4, 4, oldTexture.width, oldTexture.height);
                        byte[] textureData = newTexture.GetRawTextureData();
                        byte[] fileData = CombineBuffers(header, textureData);

                        // Save data to files
                        //string newPath = folder + "/" + directory + "/" + filename + ".astc";
                        //File.WriteAllBytes(newPath, fileData);

                        string newPath = folder + "/" + directory + "/" + filename + ".pvr";
                        File.WriteAllBytes(newPath, textureData);
                    }
        }

        private byte[] GetHeaderASTC(int blockSizeX, int blockSizeY, int sizeX, int sizeY)
        {
            // ASTC header
            byte[] header = new byte[16];

            // Magic numbers to recognize ASTC format
            header[ 0] = 0x13;
            header[ 1] = 0xAB;
            header[ 2] = 0xA1;
            header[ 3] = 0x5C;

            // Block sizes
            header[ 4] = (byte)blockSizeX;
            header[ 5] = (byte)blockSizeY;
            header[ 6] = 0x00; // block_z. For 2D texture it's always zero

            // Image dimension X
            header[ 7] = (byte)(sizeX & 0x000000FF);
            header[ 8] = (byte)((sizeX & 0x0000FF00) >> 8);
            header[ 9] = (byte)((sizeX & 0x00FF0000) >> 16);

            // Image dimension Y
            header[10] = (byte)(sizeY & 0x000000FF);
            header[11] = (byte)((sizeY & 0x0000FF00) >> 8);
            header[12] = (byte)((sizeY & 0x00FF0000) >> 16);

            // Image dimension Z. For 2D texture it's always zero
            header[13] = 0x00;
            header[14] = 0x00;
            header[15] = 0x00;

            return header;
        }

        private byte[] CombineBuffers(byte[] header, byte[] data)
        {
            Debug.Assert(null != header, "Header array must be valid");
            Debug.Assert(null != data, "Data array must be valid");
            byte[] result = new byte[header.Length + data.Length];
            Buffer.BlockCopy(header, 0, result, 0, header.Length);
            Buffer.BlockCopy(data, 0, result, header.Length, data.Length);
            return result;
        }

        private static readonly int MIP_LEVEL = 0;
    }
}
