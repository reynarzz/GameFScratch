using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    internal abstract class GfxResource : IDisposable
    {
        private bool _disposed = false;

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
