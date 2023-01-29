using msTech.Data;
using UnityEditor;
using UnityEngine;

namespace msTech.Editor
{
    public class LayoutEditor : EditorWindow
    {
        [MenuItem("msTech/Layout Editor")]
        private static void Init()
        {
            LayoutEditor window = EditorWindow.GetWindow<LayoutEditor>("LayoutEditor");
            window.Show();
        }

        private void OnGUI()
        {
            _commonScroll = EditorGUILayout.BeginScrollView(_commonScroll);

            EditorTools.DrawSeparator();
            DrawRawData();
            EditorTools.DrawSeparator();

            if( null == _data || null == _view )
            {
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawData();
            EditorGUILayout.EndScrollView();
        }

        private void OnDisable()
        {            
            ResetSceneView();
        }

        private void DrawRawData()
        {
            if( null == _data )
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Drag and drop Layout here", EditorTools.attentionStyle);
                EditorGUILayout.Space();
            }

            _assignedData = EditorGUILayout.ObjectField("Layout", _assignedData, typeof(Layout), false) as Layout;

            if( null != _assignedData && null == _data )
            {
                if( GUILayout.Button("Open") )
                    OpenLayout(_assignedData);
            }

            if( null != _data && GUILayout.Button("Close") )
                CloseLayout();

            if( null != _data && GUILayout.Button("Save") )
            {
                //_data.Save();
                EditorUtility.SetDirty(_data);
                AssetDatabase.SaveAssets();
                _hasAnyUnsavedData = false;
            }

            
            if( null != _data && GUILayout.Button("Refresh") )
                _view.Refresh(_data);
        }

        private void DrawData()
        {
            if ( null != _serializedObject && null != _prop )
            {
                EditorGUILayout.PropertyField(_prop);
                _serializedObject.ApplyModifiedProperties();
            }
        }

        private void OpenLayout(Layout data)
        {
            SetupSceneView();

            if( null != _view )
                _view.Dispose();

            _view = new LayoutGO();

            // Show new data's view in editor
            _data = data;
            if( null != _data )
            {
                _serializedObject = new SerializedObject(_data);
                _prop = _serializedObject.FindProperty("elements");
                //_data.Load();
                //_view.Apply(_data, true);
                _view.Refresh(_data);
            }

            /*
            ReinitFoldoutZones();
            ReinitFoldoutGates();
            ReinitFoldoutDialogs();
            ReinitFoldoutObjects();
            ReinitFoldoutPatches();
            ReinitFoldoutLights();
            */
            Repaint();

            _hasAnyUnsavedData = false;
        }

        private void CloseLayout()
        {
            if( null != _data && null != _view )
            {
                if( _hasAnyUnsavedData && EditorUtility.DisplayDialog("Layout Editor", "Are you sure yow want to close working Layout", "Cancel", "Close") )
                    return;
            }

            if( null != _view )
                _view.Dispose();

            _view = null;
            _data = null;
            ResetSceneView();
        }

        private void SetupSceneView()
        {
        }

        private void ResetSceneView()
        {
        }

        private ILayoutGO _view;
        private Layout _data;
        private Layout _assignedData;
        private bool _hasAnyUnsavedData = false;


        private SerializedObject _serializedObject;
        private SerializedProperty _prop;

        private Vector2 _commonScroll = Vector2.zero;
    }
}
