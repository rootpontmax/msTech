using msTech.Data;

namespace msTech.Export
{
    public class ExportContext
    {
        public string folder;
        public IStringCollector stringCollector;
        public ITextureCollector textureCollector;
        public ISpriteCollector spriteCollector;
    }

    public enum Platform
    {
        iOS,
        OSX,
        Win
    }

    public interface IResourceExporter
    {
        bool Export();
    }

    public class ResourceExporter : IResourceExporter
    {
        public ResourceExporter(ProjectData data, Platform platform, string folder)
        {
            _data = data;
            _platform = platform;
            _folder = folder;
        }

        public bool Export()
        {
            if( string.IsNullOrEmpty(_folder) )
                return false;

            // Get all collections and collections to export
            ICollection[] allCollection = _data.GetAllCollections();            

            // Create export context and collect common data
            ExportContext context = new ExportContext();
            context.folder = _folder;
            context.stringCollector = CollectAndExportStrings(allCollection);
            context.textureCollector = CollectAllExportTextures(context.stringCollector, allCollection);
            context.spriteCollector = CollectAllSprites();

            // Export collections
            ICollection[] exportCollection = _data.GetExportCollections();
            for ( int i = 0; i < exportCollection.Length; ++i )
                exportCollection[i].Export(context);
                

            return true;
        }

        

        private IStringCollector CollectAndExportStrings(ICollection[] allCollecitons)
        {
            IStringCollector stringCollector = new StringCollector();

            for ( int i = 0; i < _data.resources.Length; ++i )
                stringCollector.AddStrings(_data.resources[i].GetAllStrings());
            
            for ( int i = 0; i < allCollecitons.Length; ++i )
                stringCollector.AddStrings(allCollecitons[i].GetAllStrings());

            stringCollector.CreateRegister();
            stringCollector.Export(_folder);
            return stringCollector;
        }

        private ITextureCollector CollectAllExportTextures(IStringCollector stringCollector, ICollection[] allCollecitons)
        {
            ITextureCollector textureCollector = new TextureCollector(stringCollector);

            for ( int i = 0; i < _data.resources.Length; ++i )
                textureCollector.AddTextures(_data.resources[i].GetAllTextures());

            for ( int i = 0; i < allCollecitons.Length; ++i )
                textureCollector.AddTextures(allCollecitons[i].GetAllTextures());

            textureCollector.CreateRegister();
            textureCollector.Export(_folder, _platform);
            return textureCollector;
        }

        private ISpriteCollector CollectAllSprites()
        {
            ISpriteCollector spriteCollector = new SpriteCollector();
            for ( int i = 0; i < _data.resources.Length; ++i )
            {
                ResourceSpriteAtlas resourceSpriteAtlas = _data.resources[i] as ResourceSpriteAtlas;
                if ( null != resourceSpriteAtlas && null != resourceSpriteAtlas.sprites )
                    for ( int j = 0; j < resourceSpriteAtlas.sprites.Length; ++j )
                        spriteCollector.AddPair(resourceSpriteAtlas.sprites[j].originalTexture, resourceSpriteAtlas.sprites[j].rect, resourceSpriteAtlas.atlasTexture);
            }
            return spriteCollector;
        }

        private readonly ProjectData _data;
        private readonly Platform _platform;
        private readonly string _folder;
    }
}
