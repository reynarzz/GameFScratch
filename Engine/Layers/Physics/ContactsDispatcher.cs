using Box2D.NET;
using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Engine.Layers
{
    public struct Collision2D
    {
        public Collider2D Collider { get; internal set; }
        public Collider2D OtherCollider { get; internal set; }
        public int PointsCount { get; internal set; }
        public Transform Transform => OtherCollider.Transform;
        public Actor Actor => OtherCollider.Actor;

        internal B2ShapeId CurrentShapeId; // Remove
        internal B2ShapeId OtherShapeId; // Remove

        
        public void GetContacts(ref List<ContactPoint2D> contacts)
        {
            if (contacts == null)
            {
                contacts = new List<ContactPoint2D>();
            }
            else
            {
                contacts.Clear();
            }

            // TODO: refactor to implement multiple shapes
            throw new Exception(); // Remove

            var capacity = B2Shapes.b2Shape_GetContactCapacity(CurrentShapeId);

            var contactsInternal = new B2ContactData[capacity];
            var validCount = B2Shapes.b2Shape_GetContactData(CurrentShapeId, contactsInternal, contactsInternal.Length);

            for (int i = 0; i < validCount; i++)
            {
                var contact = contactsInternal[i];

                if ((B2Ids.B2_ID_EQUALS(contact.shapeIdA, CurrentShapeId) && B2Ids.B2_ID_EQUALS(contact.shapeIdB, OtherShapeId)) ||
                    (B2Ids.B2_ID_EQUALS(contact.shapeIdA, OtherShapeId) && B2Ids.B2_ID_EQUALS(contact.shapeIdB, CurrentShapeId)))
                {
                    for (int j = 0; j < contact.manifold.pointCount; j++)
                    {
                        var point = contact.manifold.points[j];
                        var id = point.id;

                        // TODO: fix adding extra contacts
                        contacts.Add(new ContactPoint2D()
                        {
                            Position = point.point.ToVec2(),
                            NormalImpulse = point.normalImpulse,
                            TangentImpulse = point.tangentImpulse,
                            Normal = contact.manifold.normal.ToVec2(),
                            NormalVelocity = point.normalVelocity,
                        });
                    }
                }
            }
        }

        public Collision2D()
        {
        }
    }

    internal class ContactsDispatcher
    {
        private readonly Action<ScriptBehavior, Collision2D> _onCollisionEnter = (x, y) => x.OnCollisionEnter2D(y);
        private readonly Action<ScriptBehavior, Collision2D> _onCollisionExit = (x, y) => x.OnCollisionExit2D(y);
        private readonly Action<ScriptBehavior, Collision2D> _onCollisionStay = (x, y) => x.OnCollisionStay2D(y);

        private readonly Action<ScriptBehavior, Collider2D> _onTriggerEnter = (x, y) => x.OnTriggerEnter2D(y);
        private readonly Action<ScriptBehavior, Collider2D> _onTriggerExit = (x, y) => x.OnTriggerExit2D(y);
        private readonly Action<ScriptBehavior, Collider2D> _onTriggerStay = (x, y) => x.OnTriggerStay2D(y);
        private Action<Action<ScriptBehavior, Collision2D>, Collider2D, Collider2D> _collisionFuncEvent => OnCollision;
        private Action<Action<ScriptBehavior, Collider2D>, Collider2D, Collider2D> _triggerFuncEvent => OnTrigger;

        private HashSet<CollisionKey> _contactEnter;
        private HashSet<CollisionKey> _contactExit;
        private HashSet<CollisionKey> _triggerEnter;
        private HashSet<CollisionKey> _triggerExit;

        private Collision2D _collisionData;

        public ContactsDispatcher()
        {
            _collisionData = new Collision2D();
            _contactEnter = new HashSet<CollisionKey>();
            _contactExit = new HashSet<CollisionKey>();
            _triggerEnter = new HashSet<CollisionKey>();
            _triggerExit = new HashSet<CollisionKey>();

            _contactEnter.EnsureCapacity(200);
            _contactExit.EnsureCapacity(200);
            _triggerEnter.EnsureCapacity(100);
            _triggerExit.EnsureCapacity(100);
        }

        private bool GetCollisionKey(B2ShapeId shapeA, B2ShapeId shapeB, out CollisionKey key)
        {
            key = default;

            if (!(B2Worlds.b2Shape_IsValid(shapeA) && B2Worlds.b2Shape_IsValid(shapeB)))
                return false;

            var col1 = B2Shapes.b2Shape_GetUserData(shapeA) as Collider2D;
            var col2 = B2Shapes.b2Shape_GetUserData(shapeB) as Collider2D;

            if (!(col1 && col2))
                return false;

            key = new CollisionKey(col1, col2);

            return true;
        }

        internal void Update()
        {
            // Contacts
            var contactsEvent = B2Worlds.b2World_GetContactEvents(PhysicWorld.WorldID);
            for (int i = 0; i < contactsEvent.beginCount; ++i)
            {
                var evt = contactsEvent.beginEvents[i];

                if (GetCollisionKey(evt.shapeIdA, evt.shapeIdB, out var currentKey))
                {
                    var added = _contactEnter.Add(currentKey);

                    if (!added)
                    {

                        Debug.Error($"Not added: '{currentKey.colliderA.Name}' and {currentKey.colliderB.Name}\n" +
                            $"A:{evt.shapeIdA}, B:{evt.shapeIdB}");

                        for (int j = 0; j < _contactEnter.Count; j++)
                        {
                            var key = _contactEnter.ElementAt(j);
                            Debug.Log($"Enter Key's({j}): A:{key.colliderA}, B:{key.colliderB}");
                        }
                    }
                }

            }

            for (int i = 0; i < contactsEvent.endCount; ++i)
            {
                var evt = contactsEvent.endEvents[i];
                if (GetCollisionKey(evt.shapeIdA, evt.shapeIdB, out var key))
                {
                    _contactExit.Add(key);
                }
            }

            // Sensor
            var sensorEvents = B2Worlds.b2World_GetSensorEvents(PhysicWorld.WorldID);
            for (int i = 0; i < sensorEvents.beginCount; ++i)
            {
                var evt = sensorEvents.beginEvents[i];

                if (GetCollisionKey(evt.sensorShapeId, evt.visitorShapeId, out var key))
                {
                    _triggerEnter.Add(key);
                }
            }

            for (int i = 0; i < sensorEvents.endCount; ++i)
            {
                var evt = sensorEvents.endEvents[i];
                if (GetCollisionKey(evt.sensorShapeId, evt.visitorShapeId, out var key))
                {
                    _triggerExit.Add(key);
                }
            }

            RaiseEvents();
        }

        private void RaiseEvents()
        {
            OnEnter(_contactEnter, _collisionFuncEvent, _onCollisionEnter, _onCollisionStay);
            OnExit(_contactExit, _contactEnter, _collisionFuncEvent, _onCollisionExit);

            OnEnter(_triggerEnter, _triggerFuncEvent, _onTriggerEnter, _onTriggerStay);
            OnExit(_triggerExit, _triggerEnter, _triggerFuncEvent, _onTriggerExit);
        }

        private void OnEnter<T>(HashSet<CollisionKey> enterCollisions, Action<Action<ScriptBehavior, T>, Collider2D, Collider2D> eventForwarder,
                                Action<ScriptBehavior, T> onEnterFunc, Action<ScriptBehavior, T> onStayFunc)
        {
            for (int i = 0; i < enterCollisions.Count; i++)
            {
                var collision = enterCollisions.ElementAt(i);

                if (!collision.WasEnterEventRaised)
                {
                    enterCollisions.Remove(collision);
                    collision.WasEnterEventRaised = true;
                    enterCollisions.Add(collision);

                    eventForwarder(onEnterFunc, collision.colliderA, collision.colliderB);
                    eventForwarder(onEnterFunc, collision.colliderB, collision.colliderA);
                }
                else
                {
                    eventForwarder(onStayFunc, collision.colliderA, collision.colliderB);
                    eventForwarder(onStayFunc, collision.colliderB, collision.colliderA);
                }
            }
        }

        private void OnExit<T>(HashSet<CollisionKey> exitCollisions, HashSet<CollisionKey> enterCollisions,
            Action<Action<ScriptBehavior, T>, Collider2D, Collider2D> eventForwarder, Action<ScriptBehavior, T> exitEvent)
        {
            for (int i = 0; i < exitCollisions.Count; i++)
            {
                var exitCollision = exitCollisions.ElementAt(i);
                var found = enterCollisions.TryGetValue(exitCollision, out var enterCollision);

                if (!found)
                {
                    Debug.Error($"Can't exit: '{exitCollision.colliderA.Name}' and {exitCollision.colliderB.Name}\n" +
                        $"A:{exitCollision.colliderA}, B:{exitCollision.colliderB}");
                }

                //var enterContact = _contactEnter.ElementAt(i);

                if (enterCollision.WasEnterEventRaised)
                {
                    enterCollisions.Remove(enterCollision);

                    //Debug.Log($"Remove contact enter: '{GetCollider(ref enterContact.shapeA).Name}' and {GetCollider(ref enterContact.shapeB).Name}\n" +
                    //    $"A:{enterContact.shapeA}, B:{enterContact.shapeB}");

                    eventForwarder(exitEvent, enterCollision.colliderA, enterCollision.colliderB);
                    eventForwarder(exitEvent, enterCollision.colliderB, enterCollision.colliderA);
                }
                else
                {
                    //Debug.Error($"Enter no raised: '{GetCollider(ref enterContact.shapeA).Name}' and {GetCollider(ref enterContact.shapeB).Name}\n" +
                    //    $"A:{enterContact.shapeA}, B:{enterContact.shapeB}");

                }
            }

            exitCollisions.Clear();
        }


        internal void NotifyColliderToDestroy(Collider2D collider)
        {
            // TODO: Implement early OnCollisionExit/OnTriggerExit

            /* Note: OnCollisionExit/OnTriggerExit will not be called automatically after a shape is
               destroyed when an actor is destroyed because doing so will send invalid/destroyed actors
               so this function takes care of calling OnCollisionExit/OnTriggerExit 
               before the actors become invalid, and in the same frame.
            */

            if (collider.IsTrigger)
            {
                //_triggerExit.Add(collider);
            }
            else
            {
                //_contactExit
            }
        }

        private void OnCollision(Action<ScriptBehavior, Collision2D> action, Collider2D collA, Collider2D collB)
        {
            if (collA && collA.Actor && collB && collB.Actor)
            {
                _collisionData.Collider = collA;
                _collisionData.OtherCollider = collB;

                OnNotifyScripts(collA, collB, action, ref _collisionData);
            }

            _collisionData = default;
        }

        private void OnTrigger(Action<ScriptBehavior, Collider2D> action, Collider2D coll1, Collider2D coll2)
        {
            if (coll1 && coll1.IsTrigger && coll1.Actor && coll2 && coll2.Actor)
            {
                OnNotifyScripts(coll1, coll2, action, ref coll2);
            }
        }

        private void OnNotifyScripts<T>(Collider2D current, Collider2D collided, Action<ScriptBehavior, T> funcEvent, ref T data)
        {
            if (current && current.Actor && current.Actor.IsEnabled)
            {
                foreach (var component in current.Actor.Components)
                {
                    if (component && component.IsEnabled && component is ScriptBehavior script)
                    {
                        funcEvent(script, data);
                    }
                }
            }
        }
    }
}
