using UnityEngine;

namespace msTech.Data
{
    abstract public class ResourceBase : ScriptableObject, IResource
    {
        public abstract string[] GetAllStrings();
        public abstract Texture[] GetAllTextures();
        public abstract void Export(string folder);
    }
}
