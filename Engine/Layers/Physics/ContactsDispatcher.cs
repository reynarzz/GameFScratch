using Box2D.NET;
using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
 

    public struct Collision2D
    {
        public Collider2D Collider { get; internal set; }
        public Collider2D OtherCollider { get; internal set; }
        public int PointsCount { get; internal set; }
        public Transform Transform => Collider.Transform;
        public Actor Actor => Collider.Actor;
        internal B2ShapeId CurrentShapeId;
        internal B2ShapeId OtherShapeId;

        public void GetContacts(ref List<ContactPoint2D> contacts)
        {
            if(contacts == null)
            {
                contacts = new List<ContactPoint2D>();
            }
            else
            {
                contacts.Clear();
            }

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
        private Collision2D _collisionData;
        private struct CollisionKey : IEquatable<CollisionKey>
        {
            public B2ShapeId shapeA;
            public B2ShapeId shapeB;

            public bool WasContactEnterEventRaised;
            // TODO: use Collider2D instead of shape, so it can support colliders of multiples shapes.
            public CollisionKey(B2ShapeId a, B2ShapeId b)
            {
                shapeA = a;
                shapeB = b;
                WasContactEnterEventRaised = false;
            }

            private static bool EqualsShape(B2ShapeId a, B2ShapeId b)
            {
                return a.index1 == b.index1 &&
                       a.world0 == b.world0 &&
                       a.generation == b.generation;
            }

            private static int GetShapeHash(B2ShapeId s)
            {
                return HashCode.Combine(s.index1, s.world0, s.generation);
            }

            public bool Equals(CollisionKey other)
            {
                // Order-independent equality
                return
                (
                    EqualsShape(shapeA, other.shapeA) &&
                    EqualsShape(shapeB, other.shapeB)
                )
                ||
                (
                    EqualsShape(shapeA, other.shapeB) &&
                    EqualsShape(shapeB, other.shapeA)
                );
            }

            public override bool Equals(object obj) =>
                obj is CollisionKey other && Equals(other);

            public override int GetHashCode()
            {
                int h1 = GetShapeHash(shapeA);
                int h2 = GetShapeHash(shapeB);

                // Canonical order to make (A,B) == (B,A) produce same hash:
                if (h2 < h1)
                    (h1, h2) = (h2, h1);

                return HashCode.Combine(h1, h2);
            }
        }

        private readonly Action<ScriptBehavior, Collision2D> _onCollisionEnter = (x, y) => x.OnCollisionEnter2D(y);
        private readonly Action<ScriptBehavior, Collision2D> _onCollisionExit = (x, y) => x.OnCollisionExit2D(y);

        private HashSet<CollisionKey> _contactEnter;
        private HashSet<CollisionKey> _contactExit;

        private HashSet<CollisionKey> _sensorEnter;
        private HashSet<CollisionKey> _sensorExit;
        public ContactsDispatcher()
        {
            _collisionData = new Collision2D();
            _contactEnter = new HashSet<CollisionKey>();
            _contactExit = new HashSet<CollisionKey>();
            _contactEnter.EnsureCapacity(300);
            _contactExit.EnsureCapacity(300);

            _sensorEnter = new HashSet<CollisionKey>();
            _sensorExit = new HashSet<CollisionKey>();
            _sensorEnter.EnsureCapacity(200);
            _sensorExit.EnsureCapacity(200);
        }

        internal void Update()
        {
            // Contacts
            var contactsEvent = B2Worlds.b2World_GetContactEvents(PhysicWorld.WorldID);
            for (int i = 0; i < contactsEvent.beginCount; ++i)
            {
                var evt = contactsEvent.beginEvents[i];
                var added = _contactEnter.Add(new CollisionKey(evt.shapeIdA, evt.shapeIdB));
                if (!added)
                {

                    Debug.Error($"Not added: '{GetCollider(ref evt.shapeIdA).Name}' and {GetCollider(ref evt.shapeIdB).Name}\n" +
                        $"A:{evt.shapeIdA}, B:{evt.shapeIdB}");

                    for (int j = 0; j < _contactEnter.Count; j++)
                    {
                        var key = _contactEnter.ElementAt(j);
                        Debug.Log($"Enter Key's({j}): A:{key.shapeA}, B:{key.shapeB}");
                    }
                }
            }

            for (int i = 0; i < contactsEvent.endCount; ++i)
            {
                var evt = contactsEvent.endEvents[i];
                _contactExit.Add(new CollisionKey(evt.shapeIdA, evt.shapeIdB));
            }

            // Sensor
            var sensorEvents = B2Worlds.b2World_GetSensorEvents(PhysicWorld.WorldID);
            for (int i = 0; i < sensorEvents.beginCount; ++i)
            {
                var evt = sensorEvents.beginEvents[i];
                _sensorEnter.Add(new CollisionKey(evt.sensorShapeId, evt.visitorShapeId));
            }

            for (int i = 0; i < sensorEvents.endCount; ++i)
            {
                var evt = sensorEvents.endEvents[i];
                _sensorExit.Add(new CollisionKey(evt.sensorShapeId, evt.visitorShapeId));
            }

            RaiseEvents();
        }

        private void RaiseEvents()
        {
            for (int i = 0; i < _contactEnter.Count; i++)
            {
                var contact = _contactEnter.ElementAt(i);

                if (!contact.WasContactEnterEventRaised)
                {
                    _contactEnter.Remove(contact);
                    contact.WasContactEnterEventRaised = true;
                    _contactEnter.Add(contact);

                    OnCollision(_onCollisionEnter, ref contact.shapeA, ref contact.shapeB);
                    OnCollision(_onCollisionEnter, ref contact.shapeB, ref contact.shapeA);
                }
                else
                {
                    // Debug.Log("Contact stay");
                }
            }


            for (int i = 0; i < _contactExit.Count; i++)
            {
                var exitContactKey = _contactExit.ElementAt(i);
                var found = _contactEnter.TryGetValue(exitContactKey, out var enterContact);

                if (!found)
                {
                    Debug.Error($"Can't exit: '{GetCollider(ref exitContactKey.shapeA).Name}' and {GetCollider(ref exitContactKey.shapeB).Name}\n" +
                        $"A:{exitContactKey.shapeA}, B:{exitContactKey.shapeB}");
                }

                //var enterContact = _contactEnter.ElementAt(i);

                if (enterContact.WasContactEnterEventRaised)
                {
                    _contactEnter.Remove(enterContact);

                    //Debug.Log($"Remove contact enter: '{GetCollider(ref enterContact.shapeA).Name}' and {GetCollider(ref enterContact.shapeB).Name}\n" +
                    //    $"A:{enterContact.shapeA}, B:{enterContact.shapeB}");

                    OnCollision(_onCollisionExit, ref enterContact.shapeA, ref enterContact.shapeB);
                    OnCollision(_onCollisionExit, ref enterContact.shapeB, ref enterContact.shapeA);
                }
                else
                {
                    //Debug.Error($"Enter no raised: '{GetCollider(ref enterContact.shapeA).Name}' and {GetCollider(ref enterContact.shapeB).Name}\n" +
                    //    $"A:{enterContact.shapeA}, B:{enterContact.shapeB}");

                }
            }

            _contactExit.Clear();
        }

        private Collider2D GetCollider(ref B2ShapeId shape)
        {
            return B2Shapes.b2Shape_GetUserData(shape) as Collider2D;
        }

        private void OnCollision(Action<ScriptBehavior, Collision2D> action, ref B2ShapeId shapeIdA, ref B2ShapeId shapeIdB)
        {
            var coll1 = GetCollider(ref shapeIdA);
            var coll2 = GetCollider(ref shapeIdB);

            _collisionData.Collider = coll1;
            _collisionData.OtherCollider = coll2;
            _collisionData.CurrentShapeId = shapeIdA;
            _collisionData.OtherShapeId = shapeIdB;

            OnNotifyScripts(coll1, coll2, action, ref _collisionData);
        }

        private void OnNotifyScripts<T>(Collider2D current, Collider2D collided, Action<ScriptBehavior, T> action, ref T data)
        {
            foreach (var component in current.Actor.Components)
            {
                if (component && component.IsEnabled && component is ScriptBehavior script)
                {
                    action(script, data);
                }
            }
        }
    }
}
