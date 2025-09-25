using Box2D.NET;
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
                _contactEnter.Add(new CollisionKey(evt.shapeIdA, evt.shapeIdB));
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

        internal void RaiseEvents()
        {
            for (int i = 0; i < _contactEnter.Count; i++)
            {
                var contact = _contactEnter.ElementAt(i);

                if (!contact.WasContactEnterEventRaised)
                {
                    _contactEnter.Remove(contact);
                    contact.WasContactEnterEventRaised = true;
                    _contactEnter.Add(contact);

                    OnCollisionEnter(ref contact.shapeA, ref contact.shapeB);
                }
                else
                {
                    Debug.Log("Contact stay");
                }
            }


            for (int i = 0; i < _contactExit.Count; i++)
            {
                var contact = _contactEnter.ElementAt(i);

                if (contact.WasContactEnterEventRaised)
                {
                    _contactEnter.Remove(contact);

                    // OnCollisionEnter(ref contact.shapeA, ref contact.shapeB);
                }
            }

            _contactExit.Clear();
        }

        private void OnCollisionEnter(ref B2ShapeId shapeIdA, ref B2ShapeId shapeIdB)
        {
            var coll1 = B2Shapes.b2Shape_GetUserData(shapeIdA) as Collider2D;
            var coll2 = B2Shapes.b2Shape_GetUserData(shapeIdB) as Collider2D;

            var contactsCount1 = B2Shapes.b2Shape_GetContactData(shapeIdA, coll1.Contacts, coll1.Contacts.Length);
            var contactsCount12 = B2Shapes.b2Shape_GetContactData(shapeIdB, coll2.Contacts, coll2.Contacts.Length);

            for (int j = 0; j < contactsCount1; j++)
            {
                if (B2Ids.B2_ID_EQUALS(coll1.Contacts[j].shapeIdB, shapeIdB))
                {
                    //for (int k = 0; k < manifold.points.Length; k++)
                    //{
                    //    _collisionData.PointsCount = ;
                    //}
                }
            }

            // TODO: make sure the contact is only called once per fixed update

            // evt.manifold.points[];
            _collisionData.Collider = coll2;
            OnNotifyScripts(coll1, coll2, (x, y) => x.OnCollisionEnter2D(y), ref _collisionData);

            _collisionData.Collider = coll1;
            OnNotifyScripts(coll2, coll1, (x, y) => x.OnCollisionEnter2D(y), ref _collisionData);
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
