using UnityEngine;
using UnityEditor;
using msTech.Data;
using msTech.Export;

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
                _platform = (Platform)EditorGUILayout.EnumPopup("Platform", _platform);

                if( GUILayout.Button("Export") )
                {
                    string path = EditorUtility.OpenFolderPanel("Choose folder to export project data", "", "");

                    IResourceExporter resourceExporter = new ResourceExporter(_projectData, _platform, path);
                    bool res = resourceExporter.Export();

                    if( EditorUtility.DisplayDialog("Export", res ? "Export finished" : "Export failed", "OK") ) {}
                }
            }
        }

        private ProjectData _projectData;
        private Platform _platform = Platform.iOS;
    }
}
