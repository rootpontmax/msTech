using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace msTech.Export
{
    public interface ITextureCollector
    {
        void AddTextures(Texture[] textures);
        void CreateRegister();
        void Export(string folder, Platform platform);
    }

    public class TextureCollector : ITextureCollector
    {
        public TextureCollector(IStringCollector stringCollector)
        {
            _stringCollector = stringCollector;
            _textureSet = new HashSet<Texture>();
            _textureList = new List<Texture>(CAPACITY);
            _dictTextureToId = new Dictionary<Texture, int>(CAPACITY);
        }

        public void AddTextures(Texture[] textures)
        {
            if ( null != textures && textures.Length > 0 )
                for ( int i = 0; i < textures.Length; ++i )
                    if ( null != textures[i] )
                        _textureSet.Add(textures[i]);
        }
        
        public void CreateRegister()
        {
            // Collect all unique strings in list
            _textureList.Clear();
            foreach ( Texture t in _textureSet )
                _textureList.Add(t);

            // And make string-id dictionary
            for ( int i = 0; i < _textureList.Count; ++i )
                _dictTextureToId.Add(_textureList[i], i);
        }

        public void Export(string folder, Platform platform)
        {
            ExportTextures(folder, platform);
            ExportRegister(folder);
        }

        private void ExportTextures(string folder, Platform platform)
        {
            string projectFolder = Application.dataPath.Replace("Assets", "");
            string dataPath = Application.dataPath;
            string toolsPath = dataPath.Replace("Assets", "") + "ExternalTools/PVRTexToolCLI";
            string cmdLine = toolsPath;

            string format = ( Platform.iOS == platform ) ? "PVRTCI_4BPP_RGBA" : "BC3";
            


            for ( int i = 0; i < _textureList.Count; ++i )
            {
                string textureName = ExportTools.GetTextureName(_textureList[i]);
                string inputFilename = projectFolder + AssetDatabase.GetAssetPath(_textureList[i]);
                string outputFilename = folder + "/" + textureName + ".pvr";

                string textureFolder = ExportTools.GetTextureFolder(_textureList[i]);
                ExportTools.CreateDirectoriesForPath(folder, textureFolder);

                string cmdLineArg = " -i " + inputFilename + " -o " + outputFilename + " -m -f " + format;
                //UnityEngine.Debug.LogError(cmdLine + cmdLineArg);



                // 
                ProcessStartInfo info = new ProcessStartInfo(cmdLine, cmdLineArg);
                //ProcessStartInfo info = new ProcessStartInfo("/Users/Scrooge/Code/GitHub/msTech/Tools/PVRTexToolCLI", cmdLineArg);

                
                //info.WorkingDirectory = "/";
                info.CreateNoWindow = true;
                //info.UseShellExecute = false;
                
                Process process = Process.Start(info);         
                process.WaitForExit();
                process.Close();
            }
        }

        private void ExportRegister(string folder)
        {
            string binFolderPath = folder + "/Common";
            ExportTools.CreateDirectoriesForPath(folder, "/Common");

            // Save strings
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(_textureList.Count);

            // For every texture define its name, find nameID in string collector and save just id
            for ( int i = 0; i < _textureList.Count; ++i )
            {
                string textureName = ExportTools.GetTextureName(_textureList[i]);
                int id = _stringCollector.GetStringId(textureName);
                bw.Write(id);
            }

            bw.Flush();
            string filename = binFolderPath + "/textures.msd";
            File.WriteAllBytes(filename, ms.ToArray() );
        }

        

        private static readonly int CAPACITY = 100;

        private readonly IStringCollector _stringCollector;
        private readonly HashSet<Texture> _textureSet;
        private readonly List<Texture> _textureList;
        private readonly Dictionary<Texture, int> _dictTextureToId;
    }
}
