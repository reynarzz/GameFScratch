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
        public B2ShapeId shapeA;
        public B2ShapeId shapeB;

        public bool WasEnterEventRaised;

        // TODO: use Collider2D instead of shape, so it can support colliders of multiples shapes.
        public CollisionKey(B2ShapeId a, B2ShapeId b)
        {
            shapeA = a;
            shapeB = b;
            WasEnterEventRaised = false;
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
}
