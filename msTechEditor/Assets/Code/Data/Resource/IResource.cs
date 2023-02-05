namespace msTech.Data
{
    public interface IResource
    {
        string[] GetAllStrings();
        void Export(string folder);
    }
}
