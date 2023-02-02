using System.Collections.Generic;
using msTech.Data;
using UnityEditor;
using UnityEngine;

namespace msTech.Editor
{
    [CustomPropertyDrawer(typeof(UIElement), true)]
    public class LayoutPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {            
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;
            SerializedProperty propName = prop.FindPropertyRelative("name");
            SerializedProperty propType = prop.FindPropertyRelative("type");
            SerializedProperty propAnchorV = prop.FindPropertyRelative("anchorV");
            SerializedProperty propAnchorH = prop.FindPropertyRelative("anchorH");
            SerializedProperty propOffsetX = prop.FindPropertyRelative("offsetX");
            SerializedProperty propOffsetY = prop.FindPropertyRelative("offsetY");
            SerializedProperty propSizeX = prop.FindPropertyRelative("sizeX");
            SerializedProperty propSizeY = prop.FindPropertyRelative("sizeY");
            SerializedProperty propNormalSprite = prop.FindPropertyRelative("normalSprite");
            SerializedProperty propPressedSprite = prop.FindPropertyRelative("pressedSprite");
            SerializedProperty propIsVisibile = prop.FindPropertyRelative("isVisible");
            SerializedProperty propIsTouchable = prop.FindPropertyRelative("isTouchable");
            



            SerializedProperty propIsFold = prop.FindPropertyRelative("_editorIsFold");


            

            EditorGUI.BeginProperty(rect, label, prop);
            propIsFold.boolValue = EditorGUI.Foldout(drawRect, propIsFold.boolValue, propName.stringValue);
            if ( propIsFold.boolValue )
            {
                // Common section
                drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                DrawPropertyWithOffset(ref drawRect, propName, "Name");
                DrawPropertyWithOffset(ref drawRect, propType, "Type");

                DrawPropertyWithOffset(ref drawRect, propAnchorV, "Anchor V");
                DrawPropertyWithOffset(ref drawRect, propAnchorH, "Anchor H");
                DrawPropertyWithOffset(ref drawRect, propOffsetX, "Offset X");
                DrawPropertyWithOffset(ref drawRect, propOffsetY, "Offset Y");
                DrawPropertyWithOffset(ref drawRect, propSizeX, "Size X");
                DrawPropertyWithOffset(ref drawRect, propSizeY, "Size Y");
                DrawPropertyWithOffset(ref drawRect, propIsVisibile, "Visible");
                DrawPropertyWithOffset(ref drawRect, propIsTouchable, "Touchable");

                // Alternative section
                UIElementType type = (UIElementType)propType.intValue;
                if ( UIElementType.Image == type )
                {
                    DrawPropertyWithOffset(ref drawRect, propNormalSprite, "Sprite");
                }
                else if ( UIElementType.Button == type )
                {
                    DrawPropertyWithOffset(ref drawRect, propNormalSprite, "Normal Sprite");
                    DrawPropertyWithOffset(ref drawRect, propPressedSprite, "Pressed Sprite");
                }
            }
            /*/

            //EditorGUI.PropertyField(rect, propName);

            //EditorGUI.LabelField(rect, propName.stringValue);


            /*
            EditorGUI.PropertyField(drawRect, propName);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, propType);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            //*/

            EditorGUI.EndProperty();
        }

        
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            SerializedProperty propType = prop.FindPropertyRelative("type");
            SerializedProperty propIsFold = prop.FindPropertyRelative("_editorIsFold");
            float oneHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            int linesCount = 11;
            UIElementType type = (UIElementType)propType.intValue;

            if ( UIElementType.Image == type )
                linesCount += 1;
            else if( UIElementType.Button == type )
                linesCount += 2;

            return propIsFold.boolValue ? oneHeight * linesCount : oneHeight;
        }

        private void DrawPropertyWithOffset(ref Rect rect, SerializedProperty prop, string name)
        {
            EditorGUI.PropertyField(rect, prop, new GUIContent(name));
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private void DrawTwoPropertiesWithOffset(ref Rect rect, SerializedProperty propA, SerializedProperty propB, string name, float offset, float widthA)
        {
            float initialX = rect.x;
            float initialWidth = rect.width;
            rect.width = widthA;
            EditorGUI.PropertyField(rect, propA, new GUIContent(name));
            rect.x += offset;
            EditorGUI.PropertyField(rect, propB, new GUIContent(name));
            rect.x = initialX;
            rect.width = initialWidth;
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
