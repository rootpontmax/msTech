using UnityEngine;

namespace msTech.Data
{
    abstract public class ResourceBase : ScriptableObject, IResource
    {
        public abstract void Export(string folder);
    }
}
