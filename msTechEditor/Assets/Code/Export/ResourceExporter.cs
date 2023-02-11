using msTech.Data;

namespace msTech.Export
{
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

            IStringCollector stringCollector = CollectAndExportStrings();
            ITextureCollector textureCollector = CollectAllExportTextures();

            //_data.Export(_folder);
                

            return true;
        }

        private IStringCollector CollectAndExportStrings()
        {
            IStringCollector stringCollector = new StringCollector();
            for ( int i = 0; i < _data.resources.Length; ++i )
                stringCollector.AddStrings(_data.resources[i].GetAllStrings());
            stringCollector.CreateRegister();
            stringCollector.Export(_folder);
            return stringCollector;
        }

        private ITextureCollector CollectAllExportTextures()
        {
            ITextureCollector textureCollector = new TextureCollector();
            for ( int i = 0; i < _data.resources.Length; ++i )
                textureCollector.AddTextures(_data.resources[i].GetAllTextures());
            textureCollector.CreateRegister();
            textureCollector.Export(_folder, _platform);
            return textureCollector;
        }

        private readonly ProjectData _data;
        private readonly Platform _platform;
        private readonly string _folder;
    }
}
