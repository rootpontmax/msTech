using UnityEngine;
using UnityEditor;
using msTech.Data;

namespace msTech.Editor
{
    public class ResourceExporterWindow : EditorWindow
    {
        [MenuItem("msTech/Exporter")]
        private static void Init()
        {
            ResourceExporterWindow window = EditorWindow.GetWindow<ResourceExporterWindow>("Exporter");
            window.Show();
        }

        private void OnGUI()
        {
            _projectData = EditorGUILayout.ObjectField("Project Data", _projectData, typeof(ProjectData), false) as ProjectData;

            if ( null != _projectData )
            {
                if( GUILayout.Button("Export") )
                {
                    string path = EditorUtility.OpenFolderPanel("Choose folder to export project data", "", "");
                    if( !string.IsNullOrEmpty(path) )
                        _projectData.Export(path);
                }
            }
        }

        private ProjectData _projectData;
    }
}
