using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class Component : EObject
    {
        public Actor Actor { get; internal set; }
        public Transform Transform => Actor.Transform;

        private bool _isEnabled;
        public bool IsEnabled 
        {
            get => _isEnabled;
            set 
            {
                if (_isEnabled == value)
                {
                    return;
                }

                _isEnabled = value;

                if (_isEnabled)
                {
                    OnEnabled();
                }
                else
                {
                    OnDisabled();
                }
            }
        }

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
    }
}