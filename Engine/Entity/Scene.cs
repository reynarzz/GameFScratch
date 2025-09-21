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

            actor.Scene = null;
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
    }
}
