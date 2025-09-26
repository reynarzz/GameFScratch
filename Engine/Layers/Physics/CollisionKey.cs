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
        public Collider2D colliderA;
        public Collider2D colliderB;

        public bool WasEnterEventRaised;

        public CollisionKey(Collider2D a, Collider2D b)
        {
            colliderA = a;
            colliderB = b;
            WasEnterEventRaised = false;
        }

        private static bool EqualsShape(Collider2D a, Collider2D b)
        {
            return a.GetID() == b.GetID();
        }

        private static int CombineHash(int h1, int h2)
        {
            unchecked
            {
                return ((h1 << 5) + h1) ^ h2;
            }
        }

        private static int GetShapesHash(B2ShapeId[] shapes)
        {
            int hash = 17; // seed
            unchecked
            {
                foreach (var s in shapes)
                {
                    int sh = HashCode.Combine(s.index1, s.world0, s.generation);
                    hash = CombineHash(hash, sh);
                }
            }
            return hash;
        }

        public bool Equals(CollisionKey other)
        {
            // Order-independent equality
            return
            (
                EqualsShape(colliderA, other.colliderA) &&
                EqualsShape(colliderB, other.colliderB)
            )
            ||
            (
                EqualsShape(colliderA, other.colliderB) &&
                EqualsShape(colliderB, other.colliderA)
            );
        }

        public override bool Equals(object obj) =>
            obj is CollisionKey other && Equals(other);

        public override int GetHashCode()
        {
            int h1 = GetShapesHash(colliderA.ShapesId);
            int h2 = GetShapesHash(colliderB.ShapesId);

            // Canonical order to make (A,B) == (B,A) produce same hash:
            if (h2 < h1)
                (h1, h2) = (h2, h1);

            return HashCode.Combine(h1, h2);
        }
    }
}
