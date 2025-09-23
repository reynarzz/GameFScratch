using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Actor : EObject
    {
        private List<Component> _components;
        public Transform Transform { get; private set; }
        public Scene Scene { get; internal set; }
        public bool IsEnabled { get; internal set; } = true;

        public Actor() : this(string.Empty, string.Empty)
        {
        }

        public Actor(string name) : this(name, string.Empty)
        {
        }

        public Actor(string name, string id) : base(name, id)
        {
            if (string.IsNullOrEmpty(name))
            {
                Name = "Actor";
            }
            else
            {
                Name = name;
            }

            _components = new List<Component>();
            Transform = AddComponent<Transform>();

            Scene = SceneManager.ActiveScene;
            Scene.AddActor(this);
        }

        public Component AddComponent(Type type)
        {
            if (!IsValidComponent(type))
            {
                return default;
            }
            else if (type.IsAssignableFrom(typeof(Transform)) && _components.Count > 0)
            {
                return Transform;
            }

            var component = Activator.CreateInstance(type) as Component;
            component.Actor = this;
            _components.Add(component);

            component.OnInitialize();
            return component;
        }

        public T AddComponent<T>() where T : Component
        {
            return AddComponent(typeof(T)) as T;
        }

        public void AddComponent<T1, T2>() where T1 : Component where T2 : Component
        {
            AddComponent<T1>();
            AddComponent<T2>();
        }

        public void AddComponent<T1, T2, T3>() where T1 : Component
                                                where T2 : Component
                                                where T3 : Component
        {
            AddComponent<T1, T2>();
            AddComponent<T3>();
        }

        public void AddComponent<T1, T2, T3, T4>() where T1 : Component
                                                    where T2 : Component
                                                    where T3 : Component
                                                    where T4 : Component
        {
            AddComponent<T1, T2, T3>();
            AddComponent<T4>();
        }

        public void AddComponent<T1, T2, T3, T4, T5>() where T1 : Component
                                                        where T2 : Component
                                                        where T3 : Component
                                                        where T4 : Component
                                                        where T5 : Component
        {
            AddComponent<T1, T2, T3, T4>();
            AddComponent<T5>();
        }

        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(_components[i].GetType()))
                {
                    return _components[i] as T;
                }
            }

            return null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            var components = new List<T>();
            for (int i = 0; i < _components.Count; i++)
            {
                if (typeof(T).IsAssignableFrom(_components[i].GetType()))
                {
                    components.Add(_components[i] as T);
                }
            }

            return components.ToArray();
        }

        private bool IsValidComponent(Type component)
        {
            return component != null && 
                   component.IsClass && 
                   typeof(Component).IsAssignableFrom(component) &&
                   typeof(Component) != component;

        }

        public static void Destroy(Actor actor)
        {
            if (actor == null || actor.IsAlive)
            {
                Console.WriteLine("Trying to destroy null or non alive actor.");
                return;
            }
            actor.IsAlive = false;
            actor.Scene.RemoveActor(actor);
        }

        public static void Destroy(Component component)
        {
            if (component == null || component.IsAlive)
            {
                Console.WriteLine("Trying to destroy null or non alive component.");
                return;
            }

            component.Actor._components.Remove(component);
            component.Actor = null;
            component.IsAlive = false;
        }

        public void Update()
        {
            foreach (var comp in _components)
            {
                if (comp as ScriptBehavior && comp.IsEnabled && comp.IsAlive)
                {
                    (comp as ScriptBehavior).OnUpdate();
                }
            }
        }

        internal void LateUpdate()
        {
            foreach (var comp in _components)
            {
                if (comp as ScriptBehavior && comp.IsEnabled && comp.IsAlive)
                {
                    (comp as ScriptBehavior).OnLateUpdate();
                }
            }
        }

        internal void FixedUpdate()
        {
            foreach (var comp in _components)
            {
                if (comp as ScriptBehavior && comp.IsEnabled && comp.IsAlive)
                {
                    (comp as ScriptBehavior).OnFixedUpdate();
                }
            }
        }
    }

    public class Actor<T1> : Actor where T1 : Component
    {
        public Actor() : this(string.Empty, string.Empty) { }
        public Actor(string name) : this(name, string.Empty) { }
        public Actor(string name, string id) : base(name, id) => AddComponent<T1>();
    }

    public class Actor<T1, T2> : Actor where T1 : Component where T2 : Component
    {
        public Actor() : this(string.Empty, string.Empty) { }
        public Actor(string name) : this(name, string.Empty) { }
        public Actor(string name, string id) : base(name, id) => AddComponent<T1, T2>();
    }

    public class Actor<T1, T2, T3> : Actor where T1 : Component where T2 : Component where T3 : Component
    {
        public Actor() : this(string.Empty, string.Empty) { }
        public Actor(string name) : this(name, string.Empty) { }
        public Actor(string name, string id) : base(name, id) => AddComponent<T1, T2, T3>();
    }

    public class Actor<T1, T2, T3, T4> : Actor where T1 : Component where T2 : Component where T3 : Component where T4 : Component
    {
        public Actor() : this(string.Empty, string.Empty) { }
        public Actor(string name) : this(name, string.Empty) { }
        public Actor(string name, string id) : base(name, id) => AddComponent<T1, T2, T3, T4>();
    }

    public class Actor<T1, T2, T3, T4, T5> : Actor where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
    {
        public Actor() : this(string.Empty, string.Empty) { }
        public Actor(string name) : this(name, string.Empty) { }
        public Actor(string name, string id) : base(name, id) => AddComponent<T1, T2, T3, T4, T5>();
    }
}
