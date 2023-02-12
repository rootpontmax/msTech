using System;
using UnityEngine;
using msTech.Export;

namespace msTech.Data
{
    [Serializable]
    public class ResourceTexture : IResource
    {
        public enum FilterMode
        {
            Point,
            Linear
        }

        public string name;
        public Texture texture;
        public FilterMode filterMode;
        public bool generateMipMaps;


        public string GetName()
        {
            return name;
        }

        public string[] GetAllStrings()
        {
            if( null == texture )
                return null;
            
            string[] textureName = new string[1];
            textureName[0] = ExportTools.GetTextureName(texture);
            return textureName;
        }

        public Texture[] GetAllTextures()
        {
            return new Texture[] { texture };
        }

        public byte[] ExportToMemory(ExportContext exportContext)
        {
            return null;
        }
    }
}
