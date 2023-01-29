using System;
using UnityEditor;
using UnityEngine;

namespace msTech.Editor
{
    public static class EditorTools
    {
        public class SectionDesc
        {
            public bool foldout;
            public bool[] foldoutArray;
            public Vector2 scroll = Vector2.zero;
        }

        /*
        static EditorTools()
        {
            int count = Enum.GetNames(typeof(MapRawData.BlockingType)).Length;
            _buttonWall = new Texture2D[count];
            for (int i = 0; i < count; ++i)
                _buttonWall[i] = EditorGUIUtility.Load(ICON_BUTTON_WALL_NAMES[i]) as Texture2D;
        }
        */

        public static Texture2D GetWallButton(int id)
        {
            return _buttonWall[id];
        }

        public static GUIStyle attentionStyle
        {
            get
            {
                if( null == _attentionStyle )
                {
                    _attentionStyle = new GUIStyle(EditorStyles.textField);
                    _attentionStyle.normal.textColor = Color.yellow;
                }
                return _attentionStyle;
            }
        }

        public static GUIStyle activeButtonStyle
        {
            get
            {
                if( null == _activeButtonStyle )
                {
                    _activeButtonStyle = new GUIStyle(GUI.skin.button);
                    _activeButtonStyle.normal.textColor = Color.yellow;
                    _activeButtonStyle.active.textColor = Color.yellow;
                    _activeButtonStyle.hover.textColor = Color.yellow;
                }
                return _activeButtonStyle;
            }
        }

        public static Texture2D buttonRemove
        {
            get
            {
                if( null == _buttonRemove )
                    _buttonRemove = EditorGUIUtility.Load("Assets/Editor/Icons/icon_delete.png") as Texture2D;
                return _buttonRemove;
            }
        }

        public static Texture2D buttonCursor
        {
            get
            {
                if( null == _buttonCursor )
                    _buttonCursor = EditorGUIUtility.Load("Assets/Editor/Icons/icon_hand.png") as Texture2D;
                return _buttonCursor;
            }
        }

        public static void SwapFoldout(bool[] array, int idA, int idB)
        {
            if( null == array )
                return;

            if( idA >= 0 && idB >= 0 && idA < array.Length && idB < array.Length )
            {
                bool valA = array[idA];
                bool valB = array[idB];
                array[idB] = valA;
                array[idA] = valB;
            }
        }

        public static string GetUniqueName(string prefix)
        {
            Guid guid = Guid.NewGuid();
            string uniqueName = prefix + guid.ToString();
            return uniqueName;
        }

        public static void DrawSeparator()
        {
            EditorGUILayout.Space();
            EditorGUILayout.TextArea("",GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
        }

        public static bool DrawAddButtonFoldout(string name, bool isFolded, Action add)
        {
            EditorGUILayout.BeginHorizontal();
            bool newFoldedValue = EditorGUILayout.Foldout(isFolded, name);
            if( GUILayout.Button("+", CONTROL_BLOCK_BUTTON_WIDTH) )
            {
                add();
                newFoldedValue = true;
            }
            EditorGUILayout.EndHorizontal();

            return newFoldedValue;
        }

        public static void DrawAddButton(string name, Action add)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            if( GUILayout.Button("+", CONTROL_BLOCK_BUTTON_WIDTH) )
                add();
            EditorGUILayout.EndHorizontal();
        }

        public static bool DrawControlBlockFoldout(string name, string removeText, bool isFolded, Action moveUp, Action moveDown, Action remove)
        {
            EditorGUILayout.BeginHorizontal();
            bool newFoldedValue = EditorGUILayout.Foldout(isFolded, name);
            if( GUILayout.Button("↑", CONTROL_BLOCK_BUTTON_WIDTH ) )
                moveUp();
            if( GUILayout.Button("↓", CONTROL_BLOCK_BUTTON_WIDTH) )
                moveDown();
            if( GUILayout.Button("×", CONTROL_BLOCK_BUTTON_WIDTH) && EditorUtility.DisplayDialog(removeText, "Are you sure?", "Yes", "No") )
                remove();
            EditorGUILayout.EndHorizontal();

            return newFoldedValue;
        }

        public static void DrawControlBlock(string name, string removeText, Action moveUp, Action moveDown, Action remove)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            if( GUILayout.Button("↑", CONTROL_BLOCK_BUTTON_WIDTH) )
                moveUp();
            if( GUILayout.Button("↓", CONTROL_BLOCK_BUTTON_WIDTH) )
                moveDown();
            if( GUILayout.Button("×", CONTROL_BLOCK_BUTTON_WIDTH) && EditorUtility.DisplayDialog(removeText, "Are you sure?", "Yes", "No") )
                remove();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawListOfSomethingWithMainFold(string caption,
                                                           string removeQuestion,
                                                           Action drawCommonSection,
                                                           float minHeight,
                                                           ref bool foldout,
                                                           ref bool[] foldoutArray,
                                                           ref Vector2 scrollList,
                                                           Func<int> getCount,
                                                           Func<int, string> getDataName,
                                                           Action addData,
                                                           Action applyChanges,
                                                           Action<int, int> move,
                                                           Action<int> delete,
                                                           Action<int> drawData)
        {
            foldout = DrawAddButtonFoldout(caption, foldout,() =>
                {
                    addData();
                    applyChanges();
                    return;
                });
            
            if( !foldout )
                return;

            EditorGUI.indentLevel += INDENT_TYPE;

            if ( null != drawCommonSection )
                drawCommonSection();

            if ( minHeight <= 0.0f )
                scrollList = EditorGUILayout.BeginScrollView(scrollList);
            else
                scrollList = EditorGUILayout.BeginScrollView(scrollList, GUILayout.Height(minHeight));

            int count = getCount();
            int pos = 0;
            while( pos < count )
            {
                bool wasSomethingChanged = false;
                bool wasSomethingRemoved = false;
                int moveSide = 0;
                foldoutArray[pos] = DrawControlBlockFoldout(
                        getDataName(pos),
                        removeQuestion,
                        foldoutArray[pos],
                        () => moveSide = -1,
                        () => moveSide = 1,
                        () =>
                        {
                            delete(pos);
                            count = getCount();
                            wasSomethingChanged = true;
                            wasSomethingRemoved = true;
                        });

                if( moveSide != 0 )
                {
                    move(pos, moveSide);
                    break;
                }

                if( wasSomethingRemoved )
                {
                    applyChanges();
                    break;
                }

                if( foldoutArray[pos] )
                {
                    EditorGUI.indentLevel += INDENT_DATA;
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();

                    drawData(pos);

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel -= INDENT_DATA;

                    if( wasSomethingChanged || EditorGUI.EndChangeCheck() )
                        applyChanges();
                }

                ++pos;
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel -= INDENT_TYPE;
        }

        public static void DrawListOfSomething(string caption,
                                                           string removeQuestion,
                                                           ref bool[] foldoutArray,
                                                           ref Vector2 scroll,
                                                           Func<int> getCount,
                                                           Func<int, string> getDataName,
                                                           Action addData,
                                                           Action applyChanges,
                                                           Action<int, int> move,
                                                           Action<int> delete,
                                                           Action<int> drawData)
        {
            DrawAddButton(caption, () =>
                {
                    addData();
                    applyChanges();
                    return;
                });

            EditorGUI.indentLevel += INDENT_TYPE;
            scroll = EditorGUILayout.BeginScrollView(scroll);

            int count = getCount();
            int pos = 0;
            while( pos < count )
            {
                bool wasSomethingChanged = false;
                bool wasSomethingRemoved = false;
                int moveSide = 0;
                foldoutArray[pos] = DrawControlBlockFoldout(
                        getDataName(pos),
                        removeQuestion,
                        foldoutArray[pos],
                        () => moveSide = -1,
                        () => moveSide = 1,
                        () =>
                        {
                            delete(pos);
                            count = getCount();
                            wasSomethingChanged = true;
                            wasSomethingRemoved = true;
                        });

                if( moveSide != 0 )
                {
                    move(pos, moveSide);
                    break;
                }

                if( wasSomethingRemoved )
                {
                    applyChanges();
                    break;
                }

                if( foldoutArray[pos] )
                {
                    EditorGUI.indentLevel += INDENT_DATA;
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();

                    drawData(pos);

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel -= INDENT_DATA;

                    if( wasSomethingChanged || EditorGUI.EndChangeCheck() )
                        applyChanges();
                }

                ++pos;
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel -= INDENT_TYPE;
        }

        private static readonly string[] ICON_BUTTON_WALL_NAMES =
        {
            "Assets/Editor/Icons/icon_wall_empty.png",
            "Assets/Editor/Icons/icon_wall_full.png",
            "Assets/Editor/Icons/icon_wall_top.png",
            "Assets/Editor/Icons/icon_wall_bottom.png",
            "Assets/Editor/Icons/icon_wall_left.png",
            "Assets/Editor/Icons/icon_wall_right.png",
            "Assets/Editor/Icons/icon_wall_top_left.png",
            "Assets/Editor/Icons/icon_wall_top_right.png",
            "Assets/Editor/Icons/icon_wall_bottom_left.png",
            "Assets/Editor/Icons/icon_wall_bottom_right.png"
        };

        private static readonly int INDENT_TYPE = 1;
        private static readonly int INDENT_DATA = 2;

        private static readonly GUILayoutOption CONTROL_BLOCK_BUTTON_WIDTH = GUILayout.Width(20.0f);

        private static GUIStyle _attentionStyle;
        private static GUIStyle _activeButtonStyle;
        private static Texture2D _buttonRemove;
        private static Texture2D _buttonCursor;
        private static Texture2D _buttonWallFull;
        private static Texture2D _buttonWallPart;
        private static Texture2D[] _buttonWall;
    }
}
