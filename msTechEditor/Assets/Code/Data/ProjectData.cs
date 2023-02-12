using System.Collections.Generic;
using UnityEngine;

namespace msTech.Data
{    
    public abstract class ProjectData : ScriptableObject
    {
        public CollectionTextures collectionTextures;
        public CollectionLayouts collectionLayouts;

        public ResourceAsset[] resources;

        protected abstract ICollection[] GetAllCustomCollections();
        protected abstract ICollection[] GetExportCustomCollections();







        public ICollection[] GetAllCollections()
        {
            ICollection[] engine = GetAllEngineCollections();
            ICollection[] custom = GetAllCustomCollections();
            return CombineTwoCollections(engine, custom);
        }

        public ICollection[] GetExportCollections()
        {
            ICollection[] engine = GetExportEngineCollections();
            ICollection[] custom = GetExportCustomCollections();
            return CombineTwoCollections(engine, custom);
        }

        private ICollection[] CombineTwoCollections(ICollection[] colA, ICollection[] colB)
        {
            List<ICollection> list = new List<ICollection>();            
            if ( null != colA )
                for ( int i = 0; i < colA.Length; ++i )
                    if ( null != colA[i] )
                        list.Add( colA[i] );

            if ( null != colB )
                for ( int i = 0; i < colB.Length; ++i )
                    if ( null != colB[i] )
                        list.Add( colB[i] );

            return list.ToArray();
        }

        private ICollection[] GetAllEngineCollections()
        {
            return new ICollection[]
            {
                collectionTextures,
                collectionLayouts
            };
        }

        private ICollection[] GetExportEngineCollections()
        {
            return new ICollection[]
            {
                collectionLayouts
            };
        }

        
    }
}
