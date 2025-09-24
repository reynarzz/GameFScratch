using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Scene : EObject
    {
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

        internal List<T> FindAll<T>() where T : Component
        {
            var list = new List<T>();

            void Find(Actor actor)
            {
                var comp = actor.GetComponent<T>();

                if (comp && comp.IsAlive) 
                {
                    list.Add(comp);
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
        
        internal T Find<T>() where T : Component
        {
            T Find(Actor actor)
            {
                var comp = actor.GetComponent<T>();

                if (comp && comp.IsAlive)
                {
                    return comp;
                }

                for (int i = 0; i < actor.Transform.Children.Count; i++)
                {
                    var found = Find(actor.Transform.Children[i].Actor);

                    if (found) 
                    {
                        return found;
                    }
                }

                return null;
            }

            for (int i = 0; i < _rootActors.Count; i++)
            {
                var comp = Find(_rootActors[i]);

                if (comp)
                {
                    return comp;
                }
            }

            return null;
        }

        internal void Update()
        {
            foreach (var actor in _rootActors)
            {
                actor.Update();
            }
        }

        internal void LateUpdate()
        {
            foreach (var actor in _rootActors)
            {
                actor.LateUpdate();
            }
        }

        internal void FixedUpdate()
        {
            foreach (var actor in _rootActors)
            {
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
