using System.Collections.Generic;
using UnityEngine;
using Sprite = msTech.Data.Sprite;

namespace msTech.Export
{
    public interface ISpriteCollector
    {
        void AddPair(Texture spriteTexture, RectInt rect, Texture altasTexture);
    }

    public class SpriteCollector : ISpriteCollector
    {
        public SpriteCollector()
        {
            _dict = new Dictionary<Texture, Sprite>(CAPACITY);
        }

        public void AddPair(Texture spriteTexture, RectInt rect, Texture altasTexture)
        {
            Sprite sprite;
            if ( _dict.TryGetValue(spriteTexture, out sprite) )
            {
                Debug.LogError("Texture " + spriteTexture + " includes in more than one atlas");
            }
            else
            {
                sprite = new Sprite();
                sprite.texture = altasTexture;
                sprite.rect = rect;
                _dict.Add(spriteTexture, sprite);
            }
            
        }

        private static readonly int CAPACITY = 1000;

        private readonly Dictionary<Texture, Sprite> _dict;
    }
}
