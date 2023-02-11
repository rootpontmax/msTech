using System;
using UnityEngine;

namespace msTech.Data
{
    [CreateAssetMenu(fileName = "ResourceLocalization", menuName = "msTech/Resources/Localization")]
    public class ResourceLocalization : ResourceBase
    {
        [Serializable]
        public class Item
        {
            public string name;
            public string en;
            public string ru;
        }

        public Item[] items;

        public override string[] GetAllStrings()
        {
            if ( null == items || items.Length == 0 )
                return null;

            int count = items.Length * 3;
            string[] allStrings = new string[count];
            int pos = 0;
            for ( int i = 0; i < items.Length; ++i )
                if ( null != items[i] )
                {
                    allStrings[pos++] = items[i].name;
                    allStrings[pos++] = items[i].en;
                    allStrings[pos++] = items[i].ru;
                }
            return allStrings;
        }

        public override Texture[] GetAllTextures() { return null; }

        public override void Export(string folder)
        {
        }
    }
}
