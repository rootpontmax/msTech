using System;
using msTech.Export;
using UnityEngine;

namespace msTech.Data
{
    public enum LayoutOrientation
    {
        Portrait,
        Landscape
    }

    public enum UIElementType
    {
        Base,
        Image,
        Button
    }

    public enum HorizontalAnchor
    {
        Left,
        Center,
        Right
    }

    public enum VerticalAnchor
    {
        Top,
        Center,
        Bottom
    }

    [Serializable]
    public class UIElement
    {
        public string name;
        public UIElementType type;
        public HorizontalAnchor anchorH = HorizontalAnchor.Center;
        public VerticalAnchor anchorV = VerticalAnchor.Center;        
        public Texture2D normalSprite;
        public Texture2D pressedSprite;
        public float offsetX = 0.0f;
        public float offsetY = 0.0f;
        public float sizeX = 0.1f;
        public float sizeY = 0.1f;
        public bool isVisible = true;
        public bool isTouchable = true;
        [HideInInspector] public bool _editorIsFold;
    }

    [CreateAssetMenu(fileName = "Layout", menuName = "msTech/Layout")]
    public class ResourceLayout : ResourceAsset
    {
        public UIElement[] elements;
        public LayoutOrientation orientation;
        [HideInInspector] public float aspect = 1.6f;
        [HideInInspector] public int gridsizeX;
        [HideInInspector] public int gridsizeY;
        [HideInInspector] public bool showGrid;


        public override string GetName()
        {
            return name;
        }

        public override string[] GetAllStrings()
        {
            return null;
        }

        public override Texture[] GetAllTextures()
        {
            return null;
        }

        public override byte[] ExportToMemory(ExportContext exportContext)
        {
            return null;
        }

    }
}
