using Box2D.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    internal struct CollisionKey : IEquatable<CollisionKey>
    {
        public readonly Guid ColliderAId;
        public readonly Guid ColliderBId;

        public readonly Collider2D colliderA;
        public readonly Collider2D colliderB;

        public bool WasEnterEventRaised;
        public int CollisionsCount;

        public CollisionKey(Collider2D a, Collider2D b)
        {
            colliderA = a;
            colliderB = b;

            var idA = a.GetID();
            var idB = b.GetID();

            // canonical order so (A,B) == (B,A)
            if (idB.CompareTo(idA) < 0)
            {
                ColliderAId = idB;
                ColliderBId = idA;
            }
            else
            {
                ColliderAId = idA;
                ColliderBId = idB;
            }

            CollisionsCount = 1;
        }

        public bool Equals(CollisionKey other) =>
            ColliderAId == other.ColliderAId &&
            ColliderBId == other.ColliderBId;

        public override bool Equals(object obj) =>
            obj is CollisionKey other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(ColliderAId, ColliderBId);
    }

}
