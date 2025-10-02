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
        public Transform Transform
        {
            get
            {
                CheckIfValidObject(this);
                return Actor.Transform;
            }
        }
       
        public override string Name { get => Actor?.Name ?? GetType().Name; set => Actor.Name = value; }

        private bool _isEnabled = true;
        public virtual bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                {
                    return;
                }

                _isEnabled = value;

                if (Actor.IsActiveInHierarchy)
                {
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
        }

        internal virtual void OnInitialize() { }
        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
        public virtual void OnDestroy() { }
        public virtual void OnActorEnableChange(bool enabled) { }

        public Component AddComponent(Type type)
        {
            CheckIfValidObject(this);
            return Actor.AddComponent(type);
        }

        public T AddComponent<T>() where T : Component
        {
            CheckIfValidObject(this);
            return Actor.AddComponent<T>();
        }

        public void AddComponent<T1, T2>() where T1 : Component where T2 : Component
        {
            CheckIfValidObject(this);
            Actor.AddComponent<T1, T2>();
        }

        public void AddComponent<T1, T2, T3>() where T1 : Component
                                                where T2 : Component
                                                where T3 : Component
        {
            CheckIfValidObject(this);
            Actor.AddComponent<T1, T2, T3>();
        }

        public void AddComponent<T1, T2, T3, T4>() where T1 : Component
                                                    where T2 : Component
                                                    where T3 : Component
                                                    where T4 : Component
        {
            CheckIfValidObject(this);
            Actor.AddComponent<T1, T2, T3, T4>();
        }

        public void AddComponent<T1, T2, T3, T4, T5>() where T1 : Component
                                                        where T2 : Component
                                                        where T3 : Component
                                                        where T4 : Component
                                                        where T5 : Component
        {
            CheckIfValidObject(this);
            Actor.AddComponent<T1, T2, T3, T4, T5>();
        }

        public T GetComponent<T>() where T : Component
        {
            CheckIfValidObject(this);
            return Actor.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : Component
        {
            CheckIfValidObject(this);
            return Actor.GetComponents<T>();
        }
    }
}