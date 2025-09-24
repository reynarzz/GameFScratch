using System;
using System.Collections.Generic;

namespace Engine.Utils.Polygon
{
    public static class Bayazit
    {
        // Polygons should be in in CCW order
        public static List<List<PVertex>> Decompose(List<PVertex> polygon)
        {
            List<List<PVertex>> result = new List<List<PVertex>>();
            if (polygon == null || polygon.Count < 3)
            {
                // degenerate
                return result;
            }

            if (IsConvex(polygon))
            {
                result.Add(new List<PVertex>(polygon));
                return result;
            }

            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                if (IsReflex(i, polygon))
                {
                    // For this reflex vertex, try to find “visible” vertices to cut to
                    Vec2 vi = polygon[i].position;
                    int prev = (i - 1 + n) % n;
                    int next = (i + 1) % n;

                    Vec2 vPrev = polygon[prev].position;
                    Vec2 vNext = polygon[next].position;

                    // Extend edges around i to find valid cut region
                    // We’ll iterate through all other vertices j and pick best
                    float bestScore = float.NegativeInfinity;
                    int bestJ = -1;
                    Vec2 bestIntersect = Vec2.Zero;

                    for (int j = 0; j < n; j++)
                    {
                        if (j == i || j == prev || j == next) continue;

                        if (CanSee(i, j, polygon))
                        {
                            Vec2 pj = polygon[j].position;
                            float score = 1.0f / (Vec2.Square(vi - pj) + 1.0f);

                            if (IsReflex(j, polygon))
                            {
                                score += 3.0f;
                            }
                            else
                            {
                                score += 1.0f;
                            }

                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestJ = j;
                            }
                        }
                    }

                    if (bestJ >= 0)
                    {
                        // Split polygon at (i, bestJ)
                        List<PVertex> poly1 = CopyPolygon(i, bestJ, polygon);
                        List<PVertex> poly2 = CopyPolygon(bestJ, i, polygon);

                        var rec1 = Decompose(poly1);
                        var rec2 = Decompose(poly2);
                        result.AddRange(rec1);
                        result.AddRange(rec2);
                        return result;
                    }
                }
            }

            // Fallback: if no good cut found, just triangulate (or do naive cut)
            // We cut from vertex 0 to all others (makes triangles)
            for (int j = 2; j < n; j++)
            {
                List<PVertex> tri = new List<PVertex> {
                    polygon[0],
                    polygon[j - 1],
                    polygon[j]
                };
                result.Add(tri);
            }
            return result;
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
                if (gotRight && gotLeft)
                    return false;
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
            // reflex if turned “right” (negative cross) in CCW polygon
            return Vec2.Cross(b - a, c - b) < 0f;
        }

        private static bool CanSee(int i, int j, List<PVertex> poly)
        {
            int n = poly.Count;

            Vec2 a = poly[i].position;
            Vec2 b = poly[j].position;

            // Must be inside the “cone” at i
            int iPrev = (i - 1 + n) % n;
            int iNext = (i + 1) % n;
            Vec2 aPrev = poly[iPrev].position;
            Vec2 aNext = poly[iNext].position;

            if (LeftOn(a, aNext, b) && RightOn(a, aPrev, b))
                return false;

            // Check if the segment (i,j) intersects any polygon edge
            for (int k = 0; k < n; k++)
            {
                int k2 = (k + 1) % n;
                // skip edges incident to i or j
                if (k == i || k2 == i || k == j || k2 == j)
                    continue;

                Vec2 c = poly[k].position;
                Vec2 d = poly[k2].position;

                if (SegmentsIntersect(a, b, c, d))
                    return false;
            }

            return true;
        }

        private static List<PVertex> CopyPolygon(int i, int j, List<PVertex> poly)
        {
            int n = poly.Count;
            List<PVertex> res = new List<PVertex>();
            int idx = i;
            while (true)
            {
                res.Add(poly[idx]);
                if (idx == j) break;
                idx = (idx + 1) % n;
            }
            return res;
        }

        // Helper geometry tests
        private static bool LeftOn(Vec2 a, Vec2 b, Vec2 c)
        {
            return Vec2.Cross(b - a, c - a) >= 0f;
        }
        private static bool RightOn(Vec2 a, Vec2 b, Vec2 c)
        {
            return Vec2.Cross(b - a, c - a) <= 0f;
        }

        private static bool SegmentsIntersect(Vec2 a, Vec2 b, Vec2 c, Vec2 d)
        {
            // implement standard segment intersection test (excluding collinear / endpoint)
            // Using orientation tests
            if (LinesIntersect(a, b, c, d) == false)
                return false;
            // Optional: ensure proper intersection not just collinear overlap
            return true;
        }

        private static bool LinesIntersect(Vec2 a, Vec2 b, Vec2 c, Vec2 d)
        {
            // from geometry: (a,b) intersects (c,d)
            float c1 = Vec2.Cross(b - a, c - a);
            float c2 = Vec2.Cross(b - a, d - a);
            float c3 = Vec2.Cross(d - c, a - c);
            float c4 = Vec2.Cross(d - c, b - c);
            return (c1 * c2 < 0f) && (c3 * c4 < 0f);
        }
    }
}
