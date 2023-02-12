using System.Collections.Generic;
using UnityEngine;
using msTech.Export;
using System.IO;

namespace msTech.Data
{
    // Base class for collection of resources that have names. 
    public abstract class CollectionAsset<T> : ScriptableObject, ICollection where T : IResource
    {
        public string filename;
        public T[] resources;        

        public string[] GetAllStrings()
        {
            List<string> list = new List<string>();
            if ( null != resources )
                for ( int i = 0; i < resources.Length; ++i )
                    if ( null != resources[i] )
                    {
                        string[] addedStrings = resources[i].GetAllStrings();
                        if ( null != addedStrings )
                            list.AddRange(addedStrings);
                    }
            return list.ToArray();
        }

        public Texture[] GetAllTextures()
        {
            List<Texture> list = new List<Texture>();
            if ( null != resources )
                for ( int i = 0; i < resources.Length; ++i )
                    if ( null != resources[i] )
                    {
                        Texture[] addedTextures = resources[i].GetAllTextures();
                        if ( null != addedTextures )
                            list.AddRange(addedTextures);
                    }
            return list.ToArray();
        }

        public void Export(ExportContext context)
        {
            int nonEmptyCount = 0;
            if ( null != resources )
                for ( int i = 0; i < resources.Length; ++i )
                    if ( null != resources[i] )
                        ++nonEmptyCount;

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);


            bw.Write(nonEmptyCount);
            for ( int i = 0; i < resources.Length; ++i )
            {
                string resourceName = resources[i].GetName();
                int nameId = context.stringCollector.GetStringId(resourceName);
                bw.Write(nameId);
                byte[] resourceBytes = resources[i].ExportToMemory(context);
                if ( null != resourceBytes )
                {
                    bw.Write(resourceBytes.Length);
                    bw.Write(resourceBytes);
                }
                else
                    bw.Write(0);
            }
            
            ExportTools.CreateDirectoriesForPath(context.folder, COMMON_PATH);
            string path = context.folder + "/" + COMMON_PATH + "/" + filename + DATA_EXTENSION;

            bw.Flush();
            File.WriteAllBytes(path, ms.ToArray());
        }

        private static readonly string COMMON_PATH = "Common";
        private static readonly string DATA_EXTENSION = ".msd";
    }
}
