using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics.OpenGL
{
    /// <summary>
    /// Base class for all gl resources
    /// </summary>
    /// <typeparam name="T">Resource descriptor info used for creation</typeparam>
    internal abstract class GLGfxResource<T> : GfxResource where T : ResourceDescriptorBase
    {
        public uint Handle { get; protected set; }
        public bool IsValidHandle => Handle > 0;

        Action<uint> _handleBinder;
        private Action<uint> _handleDeleter;
        private Func<uint> _handleCreator;

        protected GLGfxResource(Action<uint> binder, 
                                Func<uint> creator,
                                Action<uint> deleter)
        {
            _handleBinder = binder;
            _handleDeleter = deleter;
            _handleCreator = creator;
        }

        public virtual void Bind()
        {
            _handleBinder(Handle);
        }

        public virtual void UnBind()
        {
            _handleBinder(0);
        }

        internal void Create(T descriptor)
        {
            CreateHandle();

            IsInitialized = CreateResource(descriptor);

            if (!IsInitialized)
            {
                DestroyHandle();
            }
        }

        internal void Update(T descriptor)
        {
            if (IsInitialized)
            {
                UpdateResource(descriptor);
            }
        }

        protected abstract bool CreateResource(T descriptor);
        public abstract void UpdateResource(T descriptor);

        private void CreateHandle()
        {
            Handle = _handleCreator();
        }

        private void DestroyHandle()
        {
            _handleDeleter(Handle);
            Handle = 0;
        }

        protected override void FreeResource()
        {
            if (IsInitialized)
            {
                DestroyHandle();
                IsInitialized = false;
            }
        }
    }
}
