using UnityEngine;
using msTech.Export;

namespace msTech.Data
{
    public interface IResource
    {
        string GetName();
        string[] GetAllStrings();
        Texture[] GetAllTextures();
        byte[] ExportToMemory(ExportContext exportContext);
    }
}
