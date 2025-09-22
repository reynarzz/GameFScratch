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
    internal abstract class GLGfxResource<T> : GfxResource where T : IResourceDescriptor
    {
        public uint Handle { get; protected set; }
        public bool IsValidHandle => Handle > 0;

        Action<uint> _handleBinder;
        private Action<uint> _handleDeleter;
        private Func<uint> _handleCreator;

        protected GLGfxResource(Func<uint> creator,
                                Action<uint> deleter,
                                Action<uint> binder)
        {
            _handleCreator = creator;
            _handleDeleter = deleter;
            _handleBinder = binder;
        }

        protected GLGfxResource(Func<uint> creator,
                                Action<uint> deleter)
        {
            _handleCreator = creator;
            _handleDeleter = deleter;
            _handleBinder = null;
        }

        /// <summary>
        /// Bind this resource for use.
        /// </summary>
        internal virtual void Bind()
        {
            if (_handleBinder == null)
            {
                Log.Error("Binder not specified in constructor, override Bind() if this was intended.");
                return;
            }
            _handleBinder(Handle);
        }

        internal virtual void Unbind()
        {
            if (_handleBinder == null)
            {
                Log.Error("Binder not specified in constructor, override UnBind() if this was intended.");
                return;
            }
            _handleBinder(0);
        }

        internal bool Create(T descriptor)
        {
            if (!IsInitialized)
            {
                CreateHandle();

                IsInitialized = CreateResource(descriptor);

                if (!IsInitialized)
                {
                    Log.Error($"Could not create resource (returns false): {GetType().Name}");
                    DestroyHandle();
                }

                return IsInitialized;
            }

            Log.Warn("Can't create an already created buffer, this is not supported.");

            return false;
        }

        internal void Update(T descriptor)
        {
            if (IsInitialized)
            {
                UpdateResource(descriptor);
            }
        }

        protected abstract bool CreateResource(T descriptor);
        internal abstract void UpdateResource(T descriptor);

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
