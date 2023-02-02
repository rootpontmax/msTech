using System;
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
    public enum VerticalAnchor
    {
        Top,
        Center,
        Bottom
    }

    public enum HorizontalAnchor
    {
        Left,
        Center,
        Right
    }

    [Serializable]
    public class UIElement
    {
        public string name;
        public UIElementType type;
        public VerticalAnchor anchorV = VerticalAnchor.Center;
        public HorizontalAnchor anchorH = HorizontalAnchor.Center;
        public Texture2D normalSprite;
        public Texture2D pressedSprite;
        public float offsetX = 0.0f;
        public float offsetY = 0.0f;
        public float sizeX = 0.1f;
        public float sizeY = 0.1f;
        public bool isVisible;
        public bool isTouchable;
        [HideInInspector] public bool _editorIsFold;
    }

    [CreateAssetMenu(fileName = "Layout", menuName = "msTech/Layout")]
    public class Layout : ScriptableObject
    {
        public UIElement[] elements;
        public LayoutOrientation orientation;
        public float aspect = 1.6f;
    }
}
