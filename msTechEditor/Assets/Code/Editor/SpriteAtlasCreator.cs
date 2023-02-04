using System.Collections.Generic;
using System.IO;
using msTech.Data;
using UnityEditor;
using UnityEngine;

namespace msTech.Editor
{
    public interface ISpriteAtlasCreator
    {
        void GenerateAtlasTexture(string name);
    }

    public class SpriteAtlasCreator : ISpriteAtlasCreator
    {
        public SpriteAtlasCreator(ResourceSpriteAtlas atlas)
        {
            _atlas = atlas;
        }

        public void GenerateAtlasTexture(string name)
        {
            // Add all textures to dictionary
            HashSet<Texture2D> textureSet = new HashSet<Texture2D>();
            for ( int i = 0; i < _atlas.textures.Length; ++i )
                if ( null != _atlas.textures[i] )
                {
                    Texture2D key = _atlas.textures[i];
                    if ( !textureSet.Contains(key) )
                        textureSet.Add(key);
                }

            // Create list of texture and sort it by texture dimension
            List<Texture2D> textureList = new List<Texture2D>(textureSet.Count);
            foreach( Texture2D uniqueTexture in textureSet )
                textureList.Add(uniqueTexture);
            textureList.Sort( (a,b) =>
            {
                if ( a.height > b.height )
                    return -1;
                else if ( a.height == b.height )
                {
                    if ( a.width < b.width )
                        return -1;
                    else if ( a.width > b.width )
                        return 1;
                    else
                        return 0;
                }
                else
                    return 1;
            });

            // Try to fill all textures to atlas with some size
            int size = _atlas.size;
            Node root = CanFitAllTexturesToAtlas(size, textureList);
            if ( null == root )
            {
                Debug.LogError("Can't create sprite atlas with size " + size);
                return;
            }

            _atlas.sprites = CreateAtlasTexture(name, size, root, textureList);
        }

        private Node CanFitAllTexturesToAtlas(int size, List<Texture2D> textureList)
        {
            Node root = new Node();
            root.rect = new RectInt(0, 0, size, size);

            for ( int i = 0; i < textureList.Count; ++i )
            {
                Node newNode = root.Insert(textureList[i]);
                if ( null != newNode )
                    newNode.texture = textureList[i];
                else
                    return null;
            }           
            
            return root;
        }

        private ResourceSpriteAtlas.Sprite[] CreateAtlasTexture(string name, int size, Node root, List<Texture2D> textureList)
        {
            ResourceSpriteAtlas.Sprite[] sprites = new ResourceSpriteAtlas.Sprite[textureList.Count];
            Texture2D atlasTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            for ( int i = 0; i < textureList.Count; ++i )
            {
                Color32[] colors = textureList[i].GetPixels32(0);
                Node node = GetRectForTexture(root, textureList[i]);
                Debug.Assert(null != node, "Can't find Node for texture");
                ResourceSpriteAtlas.Sprite sprite = new ResourceSpriteAtlas.Sprite();
                sprite.originalTexture = textureList[i];
                sprite.rect = node.rect;
                sprites[i] = sprite;
                
                atlasTexture.SetPixels32(node.rect.min.x, node.rect.min.y, node.rect.width, node.rect.height, colors, 0);
            }
            //atlasTexture.Apply(true, true);

            string path = "Assets/Resources/Textures/Atlases/" + name + ".png";
            byte[] textureBytes = atlasTexture.EncodeToPNG();
            File.WriteAllBytes(path, textureBytes);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Texture2D assetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Debug.Assert(null != assetTexture, "Can't load texture as asset");
            for ( int i = 0; i < sprites.Length; ++i )
                sprites[i].atlasTexture = assetTexture;

            return sprites;
        }

        private Node GetRectForTexture(Node node, Texture2D texture)
        {
            if ( null == node )
                return null;

            if ( node.texture == texture )
                return node;

            for ( int i = 0; i < 2; ++i )
            {
                Node childNode = GetRectForTexture(node.child[i], texture);
                if ( null != childNode )
                    return childNode;
            }

            return null;
        }

        private class Node
        {
            // Offses from top-left
            public RectInt rect;
            public Texture2D texture;

            public readonly Node[] child = new Node[2];

            public Node Insert(Texture2D newTexture)
            {
                // If we are not at leaf
                if ( null != child[0] && null != child[1] )
                {
                    // Try inserting info first child
                    Node newNode = child[0].Insert(newTexture);
                    if ( null != newNode )
                        return newNode;

                    // No room, insert into second child
                    return child[1].Insert(newTexture);
                }
                else
                {
                    
                    // If there is already texture return
                    if ( null != texture )
                        return null;

                    int sizeX = newTexture.width;
                    int sizeY = newTexture.height;

                    // If there is no room for new texture return
                    if ( rect.width < sizeX || rect.height < sizeY )
                        return null;

                    // If we just fits perfectly return this node
                    if ( sizeX == rect.width && sizeY == rect.height )
                        return this;

                    // We have to split this node and create some kids
                    child[0] = new Node();
                    child[1] = new Node();

                    // Define a way to split
                    int deltaX = rect.width - sizeX;
                    int deltaY = rect.height - sizeY;
                    if ( deltaX > deltaY )
                    {
                        int xA = rect.min.x;
                        int xB = rect.min.x + sizeX;
                        int widthA = sizeX;
                        int widthB = rect.width - sizeX; 
                        child[0].rect = new RectInt(xA, rect.min.y, widthA, rect.height);
                        child[1].rect = new RectInt(xB, rect.min.y, widthB, rect.height);
                    }
                    else
                    {
                        int yA = rect.min.y;
                        int yB = rect.min.y + sizeY;
                        int heightA = sizeY;
                        int heightB = rect.height - sizeY;
                        child[0].rect = new RectInt(rect.min.x, yA, rect.width, heightA);
                        child[1].rect = new RectInt(rect.min.x, yB, rect.width, heightB);
                    }

                    // Insert into first child we created
                    return child[0].Insert(newTexture);
                }
            }
        }


        private readonly ResourceSpriteAtlas _atlas;
        
    }
}
