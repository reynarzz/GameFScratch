using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    /// <summary>
    /// Base for all graphics resources.
    /// </summary>
    internal abstract class GfxResource : IDisposable, IResourceHandle
    {
        private bool _disposed = false;
        public bool IsInitialized { get; protected set; } = false;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            FreeResource();

            _disposed = true;
        }

        /// <summary>
        /// Unmanaged cleanup here
        /// </summary>
        protected abstract void FreeResource();
    }
}
