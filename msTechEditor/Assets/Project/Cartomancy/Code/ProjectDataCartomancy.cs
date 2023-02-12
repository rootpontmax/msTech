using UnityEngine;

namespace msTech.Data
{
    [CreateAssetMenu(menuName = "msTech/Project Data/Cartomancy")]
    public class ProjectDataCartomancy : ProjectData
    {
        protected override ICollection[] GetAllCustomCollections()
        {
            return null;
        }

        protected override ICollection[] GetExportCustomCollections()
        {
            return null;
        }
    }
}
