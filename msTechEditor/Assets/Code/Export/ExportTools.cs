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
            string extenstion = Path.GetExtension(path);
            string shortName = path.Replace(RESOURCES_PATH, "").Replace(extenstion,"");
            return shortName;
        }

        public static string GetTextureFolder(Texture texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            string directory = Path.GetDirectoryName(path);
            string folder = directory.Replace(RESOURCES_PATH, "");
            return folder;
        }

        public static void CreateDirectoriesForPath(string folder, string path)
        {
            string[] folders = path.Split("/");
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


        private static readonly string RESOURCES_PATH = "Assets/Resources/";
    }
}
