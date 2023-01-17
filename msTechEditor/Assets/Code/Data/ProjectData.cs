using UnityEngine;

namespace msTech.Data
{
    [CreateAssetMenu(menuName = "msTech/Project Data")]
    public class ProjectData : ScriptableObject
    {
        public ResourceBase[] resources;

        public void Export(string folder)
        {
            if ( null != resources )
                for ( int i = 0; i < resources.Length; ++i )
                    if ( null != resources[i] )
                        resources[i].Export(folder);
        }
    }
}
