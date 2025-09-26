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
        internal IReadOnlyList<Component> Components => _components;

        public Transform Transform { get; private set; }
        public Scene Scene { get; internal set; }
        public bool IsEnabled { get; set; } = true;
        public string Tag { get; set; }
        
        private List<Component> _pendingToDeleteComponents;

        private List<Component> _onAwakeComponents;
        private List<Component> _onStartComponents;
        private static readonly Action<ScriptBehavior> _awakeAction = x => x.OnAwake();
        private static readonly Action<ScriptBehavior> _startAction = x => x.OnStart();
        private static readonly Action<ScriptBehavior> _updateAction = x => x.OnUpdate();
        private static readonly Action<ScriptBehavior> _lateUpdateAction = x => x.OnLateUpdate();
        private static readonly Action<ScriptBehavior> _fixedUpdateAction = x => x.OnFixedUpdate();


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
            _onAwakeComponents = new List<Component>();
            _onStartComponents = new List<Component>();
            _pendingToDeleteComponents = new List<Component>();



            Transform = AddComponent<Transform>();

            Scene = SceneManager.ActiveScene;
            Scene.AddActor(this);
        }

        public Component AddComponent(Type type)
        {
            if (!IsValidObject(this))
            {
                return null;
            }

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
            _onAwakeComponents.Add(component);
            _onStartComponents.Add(component);

            component.OnInitialize();
            return component;
        }

        public T AddComponent<T>() where T : Component
        {
            if (!IsValidObject(this))
            {
                return null;
            }
            return AddComponent(typeof(T)) as T;
        }

        public void AddComponent<T1, T2>() where T1 : Component where T2 : Component
        {
            if (!IsValidObject(this))
            {
                return;
            }
            AddComponent<T1>();
            AddComponent<T2>();
        }

        public void AddComponent<T1, T2, T3>() where T1 : Component
                                                where T2 : Component
                                                where T3 : Component
        {
            if (!IsValidObject(this))
            {
                return;
            }
            AddComponent<T1, T2>();
            AddComponent<T3>();
        }

        public void AddComponent<T1, T2, T3, T4>() where T1 : Component
                                                    where T2 : Component
                                                    where T3 : Component
                                                    where T4 : Component
        {
            if (!IsValidObject(this))
            {
                return;
            }
            AddComponent<T1, T2, T3>();
            AddComponent<T4>();
        }

        public void AddComponent<T1, T2, T3, T4, T5>() where T1 : Component
                                                        where T2 : Component
                                                        where T3 : Component
                                                        where T4 : Component
                                                        where T5 : Component
        {
            if (!IsValidObject(this))
            {
                return;
            }
            AddComponent<T1, T2, T3, T4>();
            AddComponent<T5>();
        }

        public Component GetComponent(Type type)
        {
            if (!IsValidObject(this))
            {
                return null;
            }
            for (int i = 0; i < _components.Count; i++)
            {
                if (type.IsAssignableFrom(_components[i].GetType()))
                {
                    return _components[i];
                }
            }

            return null;
        }

        public T GetComponent<T>() where T : Component
        {
            return GetComponent(typeof(T)) as T;
        }

        public T[] GetComponents<T>() where T : Component
        {
            if (!IsValidObject(this))
            {
                return null;
            }
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
            if (actor == null || !actor.IsAlive || actor.IsPendingToDestroy)
            {
                Console.WriteLine("Can't destroy invalid actor.");
                return;
            }

            void PendingToDestroyNotify(Actor actor)
            {
                actor.IsPendingToDestroy = true;

                // Notify children components
                for (int i = 0; i < actor.Transform.Children.Count; i++)
                {
                    PendingToDestroyNotify(actor.Transform.Children[i].Actor);
                }

                // Notify own components
                for (int i = actor._components.Count - 1; i >= 0; i--)
                {
                    actor._components[i].IsPendingToDestroy = true;
                }
            }

            PendingToDestroyNotify(actor);
        }

        public static void Destroy(Component component)
        {
            if (component && !component.IsPendingToDestroy)
            {
                component.IsPendingToDestroy = true;
                component.Actor._pendingToDeleteComponents.Add(component);
            }
            else
            {
                Debug.Info("Can't destroy and already destroyed component");
            }
        }

        public static IReadOnlyList<Actor> FindAllByTag(string tag)
        {
            return SceneManager.ActiveScene.FindActorsByTag(tag);
        }

        public static IReadOnlyList<T> FindAllByType<T>(bool findDisabled = false) where T : Component
        {
            return SceneManager.ActiveScene.FindAll<T>(findDisabled);
        }

        private static void DestroyComponentNoNotify(Component component)
        {
            if (component == null || !component.IsAlive)
            {
                Debug.Error($"Can't destroy and already destroyed component. {component.GetType().Name}");
                return;
            }

            component.Actor._components.Remove(component);
            component.Actor._onStartComponents.Remove(component);
            component.Actor._onAwakeComponents.Remove(component);

            component.Actor = null;
            component.IsAlive = false;
        }

        internal void Awake()
        {
            UpdateScriptBeginEvent(this, actor => actor._onAwakeComponents, _awakeAction);
        }

        internal void Start()
        {
            UpdateScriptBeginEvent(this, actor => actor._onStartComponents, _startAction);
        }

        public void Update()
        {
            UpdateScriptsFunction(this, _updateAction);
        }

        internal void LateUpdate()
        {
            UpdateScriptsFunction(this, _lateUpdateAction);
        }

        internal void FixedUpdate()
        {
            UpdateScriptsFunction(this, _fixedUpdateAction);
        }

        private void UpdateScriptBeginEvent(Actor actor, Func<Actor, List<Component>> getPendingComponents,
                                                         Action<ScriptBehavior> action)
        {
            if (actor && actor.IsEnabled)
            {
                var components = getPendingComponents(actor);
                var toDelete = new List<Component>();

                for (int i = 0; i < components.Count; ++i)
                {
                    if (components[i] is ScriptBehavior component && component && component.IsEnabled)
                    {
#if DEBUG
                        try
                        {
                            action(component);
                        }
                        catch (Exception e)
                        {
                            Debug.Error(e);
                        }
#else
                        action(component);
#endif
                        toDelete.Add(component);
                    }
                }

                for (int i = 0; i < toDelete.Count; i++)
                {
                    components.Remove(toDelete[i]);
                }

                for (int i = 0; i < actor.Transform.Children.Count; i++)
                {
                    UpdateScriptBeginEvent(actor.Transform.Children[i].Actor, getPendingComponents, action);
                }
            }
        }

        private void UpdateScriptsFunction(Actor actor, Action<ScriptBehavior> action)
        {
            if (actor && actor.IsEnabled)
            {
                foreach (var comp in actor._components)
                {
                    if (comp is ScriptBehavior script && script && script.IsEnabled)
                    {
#if DEBUG
                        try
                        {
                            action(script);
                        }
                        catch (Exception e)
                        {
                            Debug.Error(e);
                        }
#else
                        action(script);
#endif
                    }
                }

                for (int i = 0; i < actor.Transform.Children.Count; i++)
                {
                    UpdateScriptsFunction(actor.Transform.Children[i].Actor, action);
                }
            }
        }

        public static Actor Find(string name)
        {
            return SceneManager.ActiveScene.FindActorByName(name);
        }

        internal bool Contains(Component component)
        {
            return _components.Contains(component);
        }

        internal void DeletePending()
        {
            if (IsPendingToDestroy)
            {
                void OnDestroyEventNotify(Actor actor)
                {
                    // Notify own components
                    for (int i = 0; i < actor._components.Count; i++)
                    {
#if DEBUG
                        try
                        {
                            actor._components[i].OnDestroy();
                        }
                        catch (Exception e)
                        {
                            Debug.Error(e);
                        }
#else
                        actor._components[i].OnDestroy();
#endif
                    }

                    // Notify children components
                    for (int i = 0; i < actor.Transform.Children.Count; i++)
                    {
                        OnDestroyEventNotify(actor.Transform.Children[i].Actor);
                    }
                }

                void OnCleanUpChildren(Actor actor)
                {
                    for (int i = actor._components.Count - 1; i >= 0; i--)
                    {
                        DestroyComponentNoNotify(actor._components[i]);
                    }

                    for (int i = 0; i < actor.Transform.Children.Count; i++)
                    {
                        OnCleanUpChildren(actor.Transform.Children[i].Actor);
                    }

                    actor.IsAlive = false;
                    actor.Scene.RemoveActor(actor);
                    actor.Scene = null;
                }

                OnDestroyEventNotify(this);
                OnCleanUpChildren(this);
            }

            if (_pendingToDeleteComponents.Count > 0)
            {
                for (int i = _pendingToDeleteComponents.Count - 1; i >= 0; --i)
                {
                    var component = _pendingToDeleteComponents[i];
#if DEBUG
                    try
                    {
                        if (component != null)
                        {
                            component.OnDestroy();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Error(e);
                    }
#else
                    if (component != null)
                    {
                        component.OnDestroy();
                    }
#endif
                    DestroyComponentNoNotify(component);
                }

                _pendingToDeleteComponents.Clear();
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
