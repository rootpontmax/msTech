using UnityEngine;
using msTech.Export;

namespace msTech.Data
{
    public abstract class ResourceAsset : ScriptableObject, IResource
    {
        public abstract string GetName();
        public abstract string[] GetAllStrings();
        public abstract Texture[] GetAllTextures();
        public abstract byte[] ExportToMemory(ExportContext exportContext);
    }
}
