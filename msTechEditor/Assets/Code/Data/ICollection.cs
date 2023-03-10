using UnityEngine;
using msTech.Export;

namespace msTech.Data
{
    public interface ICollection
    {
        string[] GetAllStrings();
        Texture[] GetAllTextures();
        void Export(ExportContext context);
    }
}
