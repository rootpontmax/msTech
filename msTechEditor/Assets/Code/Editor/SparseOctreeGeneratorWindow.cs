using UnityEditor;
using UnityEngine;

namespace msTech.Editor
{
    public class SparseOctreeGeneratorWindow : EditorWindow
    {
        [MenuItem("msTech/Sparse Octree Generator")]
        private static void Init()
        {
            SparseOctreeGeneratorWindow window = EditorWindow.GetWindow<SparseOctreeGeneratorWindow>("Sparse Octree Generator");
            window.Show();
        }

        /*
        private void Show()
        {
            base.Show();
            SceneView.beforeSceneGui += DrawGUI;
        }

        private void Close()
        {
            base.Close();
            SceneView.beforeSceneGui -= DrawGUI;
        }
        */

        private void OnGUI()
        {
            if (null == _generator)
                return;

            GUILayout.Label("Nodes count: " + _generator.nodesCount);
            int newLevelToShow = EditorGUILayout.IntSlider("Level to show", _levelToShow, 0, MAX_LEVEL);
            _levelToGenerate = EditorGUILayout.IntSlider("Level to generate", _levelToGenerate, 0, MAX_LEVEL);
            if (newLevelToShow != _levelToShow)
            {
                _levelToShow = newLevelToShow;
                Repaint();
            }
            
            if( GUILayout.Button("Generate Octree") )
                _generator.Generate(_levelToGenerate);

            if( GUILayout.Button("Repaint") )
            {
                SceneView.beforeSceneGui -= DrawGUI;
                SceneView.beforeSceneGui += DrawGUI;
                Repaint();
            }

            if( GUILayout.Button("Export") )
            {
                string path = EditorUtility.SaveFilePanel("Choose folder to export project data", "", "", "svo");
                if (null!= path)
                    _generator.Export(path);
            }
        }

        private void DrawGUI(SceneView sceneView)
        {
            if (null != _generator)
                _generator.Draw(_levelToShow);
        }

        private static readonly int MAX_LEVEL = 17;

        private IStaticVoxelGenerator _generator = new StaticVoxelGenerator(MAX_LEVEL);
        private int _levelToGenerate;
        private int _levelToShow;
    }
}