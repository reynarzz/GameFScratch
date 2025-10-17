using Engine.Graphics;
using Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Base class for all the renderers
    /// </summary>
    public abstract class Renderer : Component
    {
        public Material Material { get; set; }
        public Mesh Mesh { get; set; }
        protected internal bool IsDirty { get; protected set; } = true;
        internal event Action<Renderer> OnDestroyRenderer;
        internal override void OnInitialize()
        {
            Transform.OnChanged += Transform_OnChanged;
            IsDirty = true;
        }

        protected virtual void Transform_OnChanged(Transform obj)
        {
            IsDirty = true;
        }

        internal void MarkNotDirty()
        {
            IsDirty = false;
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            Debug.Log("Enabled");

            IsDirty = true;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            IsDirty = true;
            Debug.Log("Disabled");
            OnDestroyRenderer?.Invoke(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Transform.OnChanged -= Transform_OnChanged;

            OnDestroyRenderer?.Invoke(this);
        }
    }
}