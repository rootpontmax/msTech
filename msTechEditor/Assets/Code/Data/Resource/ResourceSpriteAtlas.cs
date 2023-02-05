using System;
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
            public Texture2D atlasTexture;
            public RectInt rect;
        }

        public string atlasName;
        public int size;
        public Texture2D[] textures;
        [HideInInspector] public Sprite[] sprites;

        public override string[] GetAllStrings() { return null; }

        public override void Export(string folder)
        {
        }

        

        
    }
}
