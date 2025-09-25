using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Utils.Polygon
{
    public static class Bayazit
    {
        public static List<List<PVertex>> Decompose(List<PVertex> polygon)
        {
            var result = new List<List<PVertex>>();
            if (polygon == null || polygon.Count < 3)
                return result;

            // Defensive: if already convex, return as-is
            if (IsConvex(polygon))
            {
                result.Add(new List<PVertex>(polygon));
                return result;
            }

            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                if (!IsReflex(i, polygon))
                    continue;

                // Found a reflex vertex at i — attempt to find best cut
                int prev = (i - 1 + n) % n;
                int next = (i + 1) % n;
                Vec2 vi = polygon[i].position;

                float bestScore = float.NegativeInfinity;
                int bestVertexIndex = -1;
                Vec2 bestIntersection = Vec2.Zero;
                bool bestIsIntersection = false;
                int bestEdgeIndex = -1;

                // 1) Try vertex-to-vertex visible cuts (existing vertices)
                for (int j = 0; j < n; j++)
                {
                    if (j == i || j == prev || j == next) continue;

                    if (!CanSee(i, j, polygon)) continue;

                    // compute score (prefer closer and non-reflex endpoints a bit)
                    float score = 1.0f / (Vec2.Square(vi - polygon[j].position) + 1.0f);
                    if (IsReflex(j, polygon)) score += 0.5f; // slightly prefer reflex endpoints (tweakable)
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestVertexIndex = j;
                        bestIsIntersection = false;
                    }
                }

                // 2) Try vertex-to-edge cuts (intersection points)
                // For every edge (k, k2) not adjacent to i, find if a segment from vi in the direction of the interior
                // intersects the edge, create candidate intersection point and score it.
                // We'll consider the ray from vi towards the midpoint of the cone (average of prev and next directions)
                Vec2 dirLeft = polygon[prev].position - vi;
                Vec2 dirRight = polygon[next].position - vi;
                Vec2 coneDir = dirLeft + dirRight; // rough interior direction
                if (coneDir.x == 0 && coneDir.y == 0) // degenerate, fallback to some direction
                    coneDir = polygon[next].position - polygon[prev].position;

                // Create a long segment starting at vi towards coneDir
                LineSegment cast = new LineSegment(vi, vi + Normalize(coneDir) * 1e6f);

                for (int k = 0; k < n; k++)
                {
                    int k2 = (k + 1) % n;
                    // skip edges that touch i
                    if (k == i || k2 == i) continue;

                    // If edge is adjacent to i via its endpoints, skip (already considered)
                    // Actually any non-adjacent edge is valid
                    // Test intersection between ray 'cast' and edge (k,k2)
                    var maybe = SegmentIntersectionPoint(cast.startPos, cast.finalPos, polygon[k].position, polygon[k2].position);
                    if (!maybe.Item1) continue;

                    Vec2 intersect = maybe.Item2;

                    // Ensures the intersection point is "visible" from i (segment i intersect doesn't cross polygon)
                    if (!CanSeePoint(i, intersect, polygon)) continue;

                    // Create score: prefers closer intersections
                    float score = 1.0f / (Vec2.Square(vi - intersect) + 1.0f);
                    // penalize if intersection sits extremely close to endpoints to avoid degeneracy
                    float distToK = Vec2.Square(intersect - polygon[k].position);
                    float distToK2 = Vec2.Square(intersect - polygon[k2].position);
                    if (distToK < 1e-3f || distToK2 < 1e-3f) score *= 0.5f;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIsIntersection = true;
                        bestIntersection = intersect;
                        bestEdgeIndex = k; // intersected edge is (k,k2)
                        bestVertexIndex = -1;
                    }
                }

                // If we found a good cut candidate, perform the split and recurse
                if (bestIsIntersection || bestVertexIndex >= 0)
                {
                    List<PVertex> polyA, polyB;
                    if (bestIsIntersection)
                    {
                        // We will insert intersection point into the polygon between bestEdgeIndex and bestEdgeIndex+1
                        // and then split between i and the new inserted index
                        int insertAfter = bestEdgeIndex;
                        polyA = BuildPolygonWithIntersection(polygon, i, insertAfter, bestIntersection);
                        polyB = BuildPolygonWithIntersection(polygon, insertAfter + 1, i, bestIntersection); // other side
                    }
                    else
                    {
                        // simple split between vertices i and bestVertexIndex
                        polyA = CopyPolygon(i, bestVertexIndex, polygon);
                        polyB = CopyPolygon(bestVertexIndex, i, polygon);
                    }

                    // Recurse
                    var res1 = Decompose(polyA);
                    var res2 = Decompose(polyB);
                    result.AddRange(res1);
                    result.AddRange(res2);
                    return result;
                }
            }

            // If we reach here, no valid cut was found for any reflex vertex (rare if polygon valid)
            // As a last resort, triangulate (fan from vertex 0)
            for (int j = 2; j < n; j++)
            {
                List<PVertex> tri = new List<PVertex> {
                    CopyVertex(polygon[0]),
                    CopyVertex(polygon[j - 1]),
                    CopyVertex(polygon[j])
                };
                result.Add(tri);
            }
            return result;
        }

        // Helpers
        private static PVertex CopyVertex(PVertex v)
        {
            // Assuming PVertex is a struct with a Vec2 constructor, adjust if necessary
            return new PVertex(new Vec2(v.position.x, v.position.y));
        }

        private static List<PVertex> CopyPolygon(int i, int j, List<PVertex> poly)
        {
            int n = poly.Count;
            var res = new List<PVertex>();
            int idx = i;
            while (true)
            {
                res.Add(CopyVertex(poly[idx]));
                if (idx == j) break;
                idx = (idx + 1) % n;
            }
            return res;
        }

        // Build two polygons when the cut uses an intersection point on edge (insertAfter is edge's first vertex index)
        private static List<PVertex> BuildPolygonWithIntersection(List<PVertex> original, int startIndex, int insertAfterEdgeIndex, Vec2 intersection)
        {
            int n = original.Count;
            var res = new List<PVertex>();

            int idx = startIndex;
            // Walk from startIndex forward until we've added the inserted intersection vertex (inclusive)
            while (true)
            {
                res.Add(CopyVertex(original[idx]));
                if (idx == (insertAfterEdgeIndex % n))
                {
                    // insert the intersection vertex next
                    res.Add(new PVertex(new Vec2(intersection.x, intersection.y)));
                    break;
                }
                idx = (idx + 1) % n;
            }

            // continue until we wrap back to startIndex
            idx = (insertAfterEdgeIndex + 1) % n;
            while (true)
            {
                if (idx == startIndex) break;
                res.Add(CopyVertex(original[idx]));
                idx = (idx + 1) % n;
            }

            return res;
        }
        // Primitives
        private struct LineSegment
        {
            public Vec2 startPos;
            public Vec2 finalPos;
            public LineSegment(Vec2 s, Vec2 f) { startPos = s; finalPos = f; }
        }

        private static Vec2 Normalize(Vec2 v)
        {
            float len = (float)Math.Sqrt(v.x * v.x + v.y * v.y);
            if (len <= 1e-9f) return new Vec2(0f, 0f);
            return new Vec2(v.x / len, v.y / len);
        }

        private static bool IsConvex(List<PVertex> poly)
        {
            int n = poly.Count;
            if (n < 3) return false;
            bool gotRight = false, gotLeft = false;
            for (int i = 0; i < n; i++)
            {
                int i0 = (i - 1 + n) % n;
                int i1 = i;
                int i2 = (i + 1) % n;
                Vec2 a = poly[i0].position;
                Vec2 b = poly[i1].position;
                Vec2 c = poly[i2].position;
                float cross = Vec2.Cross(b - a, c - b);
                if (cross < 0) gotRight = true;
                if (cross > 0) gotLeft = true;
                if (gotRight && gotLeft) return false;
            }
            return true;
        }

        private static bool IsReflex(int i, List<PVertex> poly)
        {
            int n = poly.Count;
            int prev = (i - 1 + n) % n;
            int next = (i + 1) % n;
            Vec2 a = poly[prev].position;
            Vec2 b = poly[i].position;
            Vec2 c = poly[next].position;
            return Vec2.Cross(b - a, c - b) < 0f;
        }

        private static bool LeftOn(Vec2 a, Vec2 b, Vec2 c) => Vec2.Cross(b - a, c - a) >= 0f;
        private static bool RightOn(Vec2 a, Vec2 b, Vec2 c) => Vec2.Cross(b - a, c - a) <= 0f;
        private static bool Left(Vec2 a, Vec2 b, Vec2 c) => Vec2.Cross(b - a, c - a) > 0f;
        private static bool Right(Vec2 a, Vec2 b, Vec2 c) => Vec2.Cross(b - a, c - a) < 0f;

        // Basic segment intersection boolean (proper intersection excluding endpoints)
        private static bool SegmentsIntersect(Vec2 a, Vec2 b, Vec2 c, Vec2 d)
        {
            float c1 = Vec2.Cross(b - a, c - a);
            float c2 = Vec2.Cross(b - a, d - a);
            float c3 = Vec2.Cross(d - c, a - c);
            float c4 = Vec2.Cross(d - c, b - c);
            return (c1 * c2 < 0f) && (c3 * c4 < 0f);
        }

        // Compute intersection point (if any) between segments ab and cd.
        // Returns (true, point) if the infinite lines intersect and the intersection lies within both segments.
        private static Tuple<bool, Vec2> SegmentIntersectionPoint(Vec2 a, Vec2 b, Vec2 c, Vec2 d)
        {
            // Solve a + t*(b-a) = c + u*(d-c)
            Vec2 r = b - a;
            Vec2 s = d - c;
            float rxs = Vec2.Cross(r, s);
            float qmpxr = Vec2.Cross((c - a), r);

            if (Math.Abs(rxs) < 1e-9f)
            {
                // Parallel
                return Tuple.Create(false, Vec2.Zero);
            }

            float t = Vec2.Cross((c - a), s) / rxs;
            float u = Vec2.Cross((c - a), r) / rxs;

            if (t >= 0f && t <= 1f && u >= 0f && u <= 1f)
            {
                Vec2 intersect = new Vec2(a.x + t * r.x, a.y + t * r.y);
                return Tuple.Create(true, intersect);
            }

            return Tuple.Create(false, Vec2.Zero);
        }

        // Can we "see" vertex j from vertex i (i.e., segment i->j is inside polygon and doesn't intersect edges)
        private static bool CanSee(int i, int j, List<PVertex> poly)
        {
            int n = poly.Count;
            Vec2 a = poly[i].position;
            Vec2 b = poly[j].position;

            int iPrev = (i - 1 + n) % n;
            int iNext = (i + 1) % n;
            Vec2 aPrev = poly[iPrev].position;
            Vec2 aNext = poly[iNext].position;

            // If b is inside the cone at a?
            if (!(LeftOn(a, aNext, b) && RightOn(a, aPrev, b)))
                return false;

            // Check for intersections against every edge (skip edges incident to a or b)
            for (int k = 0; k < n; k++)
            {
                int k2 = (k + 1) % n;
                if (k == i || k2 == i || k == j || k2 == j) continue;

                if (SegmentsIntersect(a, b, poly[k].position, poly[k2].position))
                    return false;
            }

            return true;
        }

        // Similar to CanSee but for an arbitrary point (not necessarily an existing vertex)
        private static bool CanSeePoint(int i, Vec2 point, List<PVertex> poly)
        {
            int n = poly.Count;
            Vec2 a = poly[i].position;
            Vec2 b = point;

            int iPrev = (i - 1 + n) % n;
            int iNext = (i + 1) % n;
            Vec2 aPrev = poly[iPrev].position;
            Vec2 aNext = poly[iNext].position;

            if (!(LeftOn(a, aNext, b) && RightOn(a, aPrev, b)))
                return false;

            for (int k = 0; k < n; k++)
            {
                int k2 = (k + 1) % n;
                if (k == i || k2 == i) continue;

                // Check intersection with edge (k,k2)
                var inter = SegmentIntersectionPoint(a, b, poly[k].position, poly[k2].position);
                if (inter.Item1)
                {
                    // If intersection point is exactly at 'b' (the endpoint) that's fine, otherwise it's an obstruction
                    if (Math.Abs(inter.Item2.x - b.x) > 1e-6f || Math.Abs(inter.Item2.y - b.y) > 1e-6f)
                        return false;
                }
            }
            return true;
        }
    }
}
