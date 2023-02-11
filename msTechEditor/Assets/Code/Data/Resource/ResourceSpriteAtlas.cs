using System;
using msTech.Export;
using UnityEngine;

namespace msTech.Data
{
    [CreateAssetMenu(fileName = "ResourceSpriteAtlas", menuName = "msTech/Resources/Sprite Atlas")]
    public class ResourceSpriteAtlas : ResourceBase
    {
        [Serializable]
        public class Sprite
        {
            public Texture2D originalTexture;
            public RectInt rect;
        }

        public string atlasName;
        public int size;
        public Texture2D[] textures;
        [HideInInspector] public Sprite[] sprites;
        [HideInInspector] public Texture2D atlasTexture;

        public override string[] GetAllStrings()
        {
            if ( null != atlasTexture )
            {
                string[] retArray = new string[1];
                retArray[0] = ExportTools.GetTextureName(atlasTexture);
                return retArray;
            }

            return null;
        }

        public override Texture[] GetAllTextures()
        {
            if ( null != atlasTexture )
            {
                Texture[] retArray = new Texture[1];
                retArray[0] = atlasTexture;
                return retArray;
            }

            return null;
        }

        public override void Export(string folder)
        {
        }

        

        
    }
}
