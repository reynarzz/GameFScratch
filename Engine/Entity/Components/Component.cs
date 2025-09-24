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
                if (!IsValidObject(Actor))
                {
                    return null;
                }

                return Actor.Transform;
            }
        }

        private bool _isEnabled = true;
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
        public virtual void OnDestroy() { }

        public Component AddComponent(Type type)
        {
            if (!IsValidObject(Actor) || !IsValidObject(this))
            {
                return null;
            }
            return Actor.AddComponent(type);
        }

        public T AddComponent<T>() where T : Component
        {
            if (!IsValidObject(Actor) || !IsValidObject(this))
            {
                return null;
            }
            return Actor.AddComponent<T>();
        }

        public void AddComponent<T1, T2>() where T1 : Component where T2 : Component
        {
            if (!IsValidObject(Actor) || !IsValidObject(this))
            {
                return;
            }
            Actor.AddComponent<T1, T2>();
        }

        public void AddComponent<T1, T2, T3>() where T1 : Component
                                                where T2 : Component
                                                where T3 : Component
        {
            if (!IsValidObject(Actor) || !IsValidObject(this))
            {
                return;
            }
            Actor.AddComponent<T1, T2, T3>();
        }

        public void AddComponent<T1, T2, T3, T4>() where T1 : Component
                                                    where T2 : Component
                                                    where T3 : Component
                                                    where T4 : Component
        {
            if (!IsValidObject(Actor) || !IsValidObject(this))
            {
                return;
            }
            Actor.AddComponent<T1, T2, T3, T4>();
        }

        public void AddComponent<T1, T2, T3, T4, T5>() where T1 : Component
                                                        where T2 : Component
                                                        where T3 : Component
                                                        where T4 : Component
                                                        where T5 : Component
        {
            if (!IsValidObject(Actor) || !IsValidObject(this))
            {
                return;
            }
            Actor.AddComponent<T1, T2, T3, T4, T5>();
        }

        public T GetComponent<T>() where T : Component
        {
            if (!IsValidObject(Actor) || !IsValidObject(this))
            {
                return null;
            }

            return Actor.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : Component
        {
            if (!IsValidObject(Actor) || !IsValidObject(this))
            {
                return null;
            }

            return Actor.GetComponents<T>();
        }



        internal virtual void OnInitialize()
        {
        }
    }
}