using msTech.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            if ( null != _view )
                _view.Tick();

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

        private void SceneGUI(SceneView sceneView)
        {
            if ( null == _data || null == _view )
                return;
            
            Event evt = Event.current;


            sceneView.orthographic = true;
            sceneView.isRotationLocked = true;
            sceneView.rotation = Quaternion.identity;
            /*
            AllignedPos allignedWorldPos = GetAllignedWorldPos(sceneView, evt.mousePosition);
            SetSceneViewCamera(sceneView);

            if( null != _view )
                _view.RenderAux();
            DrawZonesAux();
            DrawDialogsAux();
            */

            DrawGrid();
            

            // Left click
            if( evt.type == EventType.MouseDown && evt.button == 0)
            {
                
                //Vector2 pos = evt.mousePosition;
                //Debug.LogError("Left click " + pos);
                //Debug.LogError("Left click " + allignedWorldPos.cellX + "; " + allignedWorldPos.cellY);

                /*
                if( null != _data && _currentSpriteID >= 0 && _currentSpriteID < _sprites.Count )
                {
                    if( OperationType.AddCell == _operationType )
                    {
                        _data.AddCell(_activeLayer, allignedWorldPos.cellX, allignedWorldPos.cellY, _sprites[_currentSpriteID] );
                        _view.Apply(_data, false);
                    }
                    else if( OperationType.AddBlock == _operationType )
                    {
                        _data.AddBlock(allignedWorldPos.cellX, allignedWorldPos.cellY, _blockingType);
                        _view.Apply(_data, false);
                    }                    
                    else if( OperationType.Clear == _operationType )
                    {
                        _data.RemoveCell(_activeLayer, allignedWorldPos.cellX, allignedWorldPos.cellY );
                        _view.Apply(_data, false);
                    }
                }
                */
                
            }

            sceneView.Repaint();
        }

        private void DrawGrid()
        {
            float sizeX = 1.0f;
            float sizeY = 1.0f;
            Handles.color = Color.cyan;
            if ( LayoutOrientation.Portrait == _data.orientation )
            {
                sizeX = 1.0f;
                sizeY = _data.aspect;
            }
            else if ( LayoutOrientation.Landscape == _data.orientation )
            {
                sizeX = _data.aspect;
                sizeY = 1.0f;
            }
            else
            {
                Debug.LogError("Unknown type of Layout Orientation");
            }

            Vector3 posA = new Vector3(-sizeX,  sizeY, 0.0f);
            Vector3 posB = new Vector3( sizeX,  sizeY, 0.0f);
            Vector3 posC = new Vector3( sizeX, -sizeY, 0.0f);
            Vector3 posD = new Vector3(-sizeX, -sizeY, 0.0f);
            Handles.DrawLine(posA, posB);
            Handles.DrawLine(posB, posC);
            Handles.DrawLine(posC, posD);
            Handles.DrawLine(posD, posA);
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
            // Layout data
            _data.orientation = (LayoutOrientation)EditorGUILayout.EnumPopup("Layout Orientation", _data.orientation);
            _data.aspect = EditorGUILayout.FloatField("Aspect", _data.aspect);
            

            // Items data
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
            SceneView.duringSceneGui += SceneGUI;

            // Save initial scene camera params
            SceneView view = SceneView.lastActiveSceneView;
            if( null != view )
            {
                _initSceneCameraPosition = view.pivot;
                _initSceneCameraRotation = view.rotation;
                _initSceneCameraLocked = view.isRotationLocked;
                _initSceneCameraOrtho = view.orthographic;
                view.pivot = Vector3.zero;
            }
        }

        private void ResetSceneView()
        {
            SceneView.duringSceneGui -= SceneGUI;

            // Restore initial scene camera params
            SceneView view = SceneView.lastActiveSceneView;
            if( null != view )
            {
                view.pivot = _initSceneCameraPosition;
                view.rotation = _initSceneCameraRotation;
                view.isRotationLocked = _initSceneCameraLocked;
                view.orthographic = _initSceneCameraOrtho;
            }

            if( null != _view )
            {
                _view.Dispose();
                _view = null;
            }

            if( !string.IsNullOrEmpty(_oldScenePath) )
            {
                EditorSceneManager.OpenScene(_oldScenePath, OpenSceneMode.Single);
                _oldScenePath = null;
            }
        }

        private ILayoutGO _view;
        private Layout _data;
        private Layout _assignedData;
        private bool _hasAnyUnsavedData = false;


        private SerializedObject _serializedObject;
        private SerializedProperty _prop;

        // Saved camera params
        private Vector3 _initSceneCameraPosition;
        private Quaternion _initSceneCameraRotation;
        private bool _initSceneCameraLocked;
        private bool _initSceneCameraOrtho;
        private string _oldScenePath;

        private Vector2 _commonScroll = Vector2.zero;
    }
}
