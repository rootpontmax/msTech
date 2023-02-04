using UnityEngine;
using UnityEditor;
using msTech.Data;

namespace msTech.Editor
{
    public class SpriteAtlasCreatorWindow : EditorWindow
    {
        [MenuItem("msTech/Sprite Atlas Creator")]
        private static void Init()
        {
            SpriteAtlasCreatorWindow window = EditorWindow.GetWindow<SpriteAtlasCreatorWindow>("Sprite Atlas Creator");
            window.Show();
        }

        private void OnGUI()
        {
            _atlasData = EditorGUILayout.ObjectField("Sprite Atlas Data", _atlasData, typeof(ResourceSpriteAtlas), false) as ResourceSpriteAtlas;

            if ( null != _atlasData )
            {
                if( GUILayout.Button("Create") )
                {
                    ISpriteAtlasCreator atlasCreator = new SpriteAtlasCreator(_atlasData);
                    atlasCreator.GenerateAtlasTexture(_atlasData.atlasName);
                }
            }
        }

        private ResourceSpriteAtlas _atlasData;
    }
}