using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Scene : EObject
    {
        internal interface IMatcher<T, TComparer>
        {
            T Invoke(Actor actor, TComparer comparer);
        }

        private struct ComponentMatcher<T> : IMatcher<T, object> where T : Component
        {
            public T Invoke(Actor actor, object comparer) => actor.GetComponent<T>();
        }

        public struct ActorMatcher : IMatcher<Actor, string>
        {
            public Actor Invoke(Actor actor, string comparer)
            {
                if (actor.Name.Equals(comparer))
                {
                    return actor;
                }
                return null;
            }
        }

        private List<Actor> _rootActors;

        private readonly Func<Component, Component> _findComponent;
        public Scene()
        {
            Name = "Scene";
            _rootActors = new List<Actor>();
        }

        internal void RegisterRootActor(Actor actor)
        {
            _rootActors.Add(actor);
        }

        internal void UnregisterRootActor(Actor actor)
        {
            _rootActors.Remove(actor);
        }

        internal IReadOnlyList<Actor> GetRootActors()
        {
            return _rootActors;
        }

        internal void AddActor(Actor actor)
        {
            // explicitly add a new root actor
            actor.Scene = this;
            actor.Transform.Parent = null; // ensures it’s root
            RegisterRootActor(actor);
        }

        internal void RemoveActor(Actor actor)
        {
            if (actor.Transform.Parent != null)
            {
                actor.Transform.Parent.RemoveChild(actor.Transform);
            }
            else
            {
                UnregisterRootActor(actor);
            }
        }

        internal List<T> FindAll<T>() where T: Component
        {
            return FindAll<T, ComponentMatcher<T>, object>(null);
        }

        private List<T> FindAll<T, TMatcher, TComparer>(TComparer comparer) where T : EObject 
                                                                            where TMatcher : struct, IMatcher<T, TComparer>
        {
            var list = new List<T>();
            var matcher = default(TMatcher);

            void Find(Actor actor)
            {
                var result = matcher.Invoke(actor, comparer);

                if (result && result.IsAlive)
                {
                    list.Add(result);
                }

                for (int i = 0; i < actor.Transform.Children.Count; i++)
                {
                    Find(actor.Transform.Children[i].Actor);
                }
            }

            for (int i = 0; i < _rootActors.Count; i++)
            {
                Find(_rootActors[i]);
            }

            return list;
        }

        internal T FindComponent<T>() where T : Component
        {
            return Find<T, ComponentMatcher<T>, object>(null);
        }

        internal Actor FindActor(string name)
        {
            return Find<Actor, ActorMatcher, string>(name);
        }

        internal T Find<T, TMatcher, IComparer>(IComparer comparer) where TMatcher : struct, IMatcher<T, IComparer> 
                                                                    where T : EObject
        {
            var matcher = default(TMatcher);

            T Find(Actor actor)
            {
                var result = matcher.Invoke(actor, comparer);

                if (result)
                {
                    return result;
                }

                for (int i = 0; i < actor.Transform.Children.Count; i++)
                {
                    var found = Find(actor.Transform.Children[i].Actor);

                    if (found)
                    {
                        return found;
                    }
                }

                return default;
            }

            for (int i = 0; i < _rootActors.Count; i++)
            {
                var value = Find(_rootActors[i]);

                if (value)
                {
                    return value;
                }
            }

            return default;
        }

        internal void Awake()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                _rootActors[i].Awake();
            }
        }

        internal void Start()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                _rootActors[i].Start();
            }
        }

        internal void Update()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                _rootActors[i].Update();
            }
        }

        internal void LateUpdate()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                _rootActors[i].LateUpdate();
            }
        }

        internal void FixedUpdate()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                _rootActors[i].FixedUpdate();
            }
        }

        internal void DeletePending()
        {
            for (int i = _rootActors.Count - 1; i >= 0; --i)
            {
                _rootActors[i].DeletePending();
            }
        }
    }
}
