using System;
using msTech.Data;


namespace msTech.Editor
{
    public interface ILayoutGO : IDisposable
    {
        void Refresh(Layout layout);
    }

    public class LayoutGO : ILayoutGO
    {
        public void Refresh(Layout layout)
        {
        }

        public void Dispose()
        {
        }
    }
}
