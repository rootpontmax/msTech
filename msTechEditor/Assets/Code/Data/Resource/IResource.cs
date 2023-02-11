using UnityEngine;

namespace msTech.Data
{
    public interface IResource
    {
        string[] GetAllStrings();
        Texture[] GetAllTextures();
        void Export(string folder);
    }
}
