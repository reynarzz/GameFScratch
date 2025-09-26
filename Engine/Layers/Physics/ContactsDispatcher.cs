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

    public struct CollisionData2D
    {
        public struct Point
        {
            public vec2 Position { get; internal set; }

        }

        public Collider2D Collider { get; internal set; }
        public Point[] Points { get; } = new Point[Collider2D.MAX_CONTACTS_PER_SHAPE];
        public int PointsCount { get; internal set; }
        public vec2 Normal { get; internal set; }
        public CollisionData2D()
        {
        }
    }

    internal class ContactsDispatcher
    {
        private CollisionData2D _collisionData;
        private struct CollisionKey : IEquatable<CollisionKey>
        {
            public B2ShapeId shapeA;
            public B2ShapeId shapeB;

            public bool WasContactEnterEventRaised;

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

        private readonly Action<ScriptBehavior, CollisionData2D> _onCollisionEnter = (x, y) => x.OnCollisionEnter2D(y);
        private readonly Action<ScriptBehavior, CollisionData2D> _onCollisionExit = (x, y) => x.OnCollisionExit2D(y);

        private HashSet<CollisionKey> _contactEnter;
        private HashSet<CollisionKey> _contactExit;

        private HashSet<CollisionKey> _sensorEnter;
        private HashSet<CollisionKey> _sensorExit;
        public ContactsDispatcher()
        {
            _collisionData = new CollisionData2D();
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
                  
                    Debug.Log($"Remove contact enter: '{GetCollider(ref enterContact.shapeA).Name}' and {GetCollider(ref enterContact.shapeB).Name}\n" +
                        $"A:{enterContact.shapeA}, B:{enterContact.shapeB}");

                    OnCollision(_onCollisionExit, ref enterContact.shapeA, ref enterContact.shapeB);
                    OnCollision(_onCollisionExit, ref enterContact.shapeB, ref enterContact.shapeA);
                    // OnCollisionEnter(ref contact.shapeA, ref contact.shapeB);
                }
                else
                {
                    Debug.Error($"Enter no raised: '{GetCollider(ref enterContact.shapeA).Name}' and {GetCollider(ref enterContact.shapeB).Name}\n" +
                        $"A:{enterContact.shapeA}, B:{enterContact.shapeB}");

                }
            }

            _contactExit.Clear();
        }

        private Collider2D GetCollider(ref B2ShapeId shape)
        {
            return B2Shapes.b2Shape_GetUserData(shape) as Collider2D;
        }

        private int GetContactData(ref B2ShapeId shape, Collider2D collider)
        {
            return B2Shapes.b2Shape_GetContactData(shape, collider.Contacts, collider.Contacts.Length);
        }

        private void GetCollisionData(B2ShapeId shapeA, B2ShapeId shapeB, ref CollisionData2D data)
        {

        }

        private void OnCollision(Action<ScriptBehavior, CollisionData2D> action, ref B2ShapeId shapeIdA, ref B2ShapeId shapeIdB)
        {
            var coll1 = GetCollider(ref shapeIdA);
            var coll2 = GetCollider(ref shapeIdB);

            var contactsCount1 = GetContactData(ref shapeIdA, coll1);

            int count = 0;
            for (int i = 0; i < contactsCount1; i++)
            {
                if (B2Ids.B2_ID_EQUALS(coll1.Contacts[i].shapeIdB, shapeIdB))
                {
                    _collisionData.Normal = coll1.Contacts[i].manifold.normal.ToVec2();
                    count++;
                    //for (int j = 0; j < coll1.Contacts[i].manifold.points.Length; j++)
                    //{
                    //    var point = coll1.Contacts[i].manifold.points[j];

                    //    _collisionData.Points[j].Position = new vec2(point.point.X, point.point.Y);
                    //}
                    // Send collisionData2D
                    // Debug.Log("Contact: " + coll1.Name + " Count: " + count);
                }
            }

            _collisionData.Collider = coll2;
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
