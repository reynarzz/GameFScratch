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

        private struct ComponentMatcher<T> : IMatcher<T, bool> where T : Component
        {
            public T Invoke(Actor actor, bool canAddDisabled)
            {
                if (!actor)
                    return null;

                var comp = actor.GetComponent<T>();

                if (canAddDisabled)
                {
                    return comp;
                }
                else if (comp && comp.IsEnabled && comp.Actor.IsActiveInHierarchy)
                {
                    return comp;
                }

                return null;
            }
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

        public struct ActorTagMatcher : IMatcher<Actor, string>
        {
            public Actor Invoke(Actor actor, string comparer)
            {
                if (actor.Tag.Equals(comparer))
                {
                    return actor;
                }
                return null;
            }
        }

        private List<Actor> _rootActors;

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
            actor.Scene = this;
            actor.Transform.Parent = null;
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

        internal IReadOnlyList<Actor> FindActorsByTag(string tag)
        {
            return FindAll<Actor, ActorTagMatcher, string>(tag);
        }

        internal T FindComponent<T>(bool findDisabled) where T : Component
        {
            return Find<T, ComponentMatcher<T>, bool>(findDisabled);
        }

        internal Actor FindActorByName(string name)
        {
            return Find<Actor, ActorMatcher, string>(name);
        }

        internal List<T> FindAll<T>(bool findDisabled) where T : Component
        {
            return FindAll<T, ComponentMatcher<T>, bool>(findDisabled);
        }

        private List<T> FindAll<T, TMatcher, TComparer>(TComparer comparer) where T : EObject
                                                                            where TMatcher : struct, IMatcher<T, TComparer>
        {
            var list = new List<T>();
            var matcher = default(TMatcher);

            void Find(Actor actor)
            {
                if (!actor) // After I put this, onDestroy is not being called.
                    return;

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
                if (_rootActors[i])
                {
                    Find(_rootActors[i]);
                }
            }

            return list;
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
                var actor = _rootActors[i];
                if (actor && actor.IsActiveInHierarchy)
                    actor.Awake();
            }
        }

        internal void Start()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                var actor = _rootActors[i];
                if (actor && actor.IsActiveInHierarchy)
                    actor.Start();
            }
        }

        internal void Update()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                var actor = _rootActors[i];
                if (actor && actor.IsActiveInHierarchy)
                    actor.Update();
            }
        }

        internal void LateUpdate()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                var actor = _rootActors[i];
                if (actor && actor.IsActiveInHierarchy)
                    actor.LateUpdate();
            }
        }

        internal void FixedUpdate()
        {
            for (int i = 0; i < _rootActors.Count; i++)
            {
                var actor = _rootActors[i];
                if (actor && actor.IsActiveInHierarchy)
                    actor.FixedUpdate();
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
