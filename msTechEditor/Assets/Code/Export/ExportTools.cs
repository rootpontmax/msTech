using System.IO;
using UnityEditor;
using UnityEngine;

namespace msTech.Export
{
    public static class ExportTools
    {
        public static string GetTextureName(Texture texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            string shortPath = GetStringThatStartedWith(path, TEXTURE_PATH_CONTAINS);
            string extenstion = Path.GetExtension(path);
            string shortName = shortPath.Replace(extenstion,"");
            return shortName;
        }

        public static string GetTextureFolder(Texture texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            string directory = Path.GetDirectoryName(path);
            string folder = GetStringThatStartedWith(directory, TEXTURE_PATH_CONTAINS);
            return folder;
        }

        public static void CreateDirectoriesForPath(string folder, string path)
        {
            string[] folders = path.Split('/');
            string parentFolder = folder;
            if ( null != folders )
                for ( int i = 0; i < folders.Length; ++i )
                    if ( !string.IsNullOrEmpty(folders[i]) )
                    {
                        string fillFolderPath = parentFolder + "/" + folders[i];
                        if ( !Directory.Exists(fillFolderPath) )
                            Directory.CreateDirectory(fillFolderPath);

                        parentFolder += "/" + folders[i];
                    }
        }

        private static string GetStringThatStartedWith(string original, string start)
        {
            int id = original.IndexOf(start);
            if ( -1 == id )
            {
                Debug.LogError("This string doesn't contain " + start);
                return null;
            }
            return original.Substring(id);
        }

        private static readonly string TEXTURE_PATH_CONTAINS = "Textures";
    }
}
