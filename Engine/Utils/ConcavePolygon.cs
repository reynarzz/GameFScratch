using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Utils.Polygon
{
    public struct Vec2
    {
        public float x;
        public float y;

        public Vec2(float xVal, float yVal)
        {
            x = xVal;
            y = yVal;
        }

        public static float Length(Vec2 v)
        {
            return (float)Math.Sqrt(v.x * v.x + v.y * v.y);
        }

        public static float Dot(Vec2 a, Vec2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static float Square(Vec2 v)
        {
            return Dot(v, v);
        }

        public static float Cross(Vec2 a, Vec2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static float GetSignedArea(Vec2 v1, Vec2 v2)
        {
            return (v2.x - v1.x) * (v2.y + v1.y);
        }

        public static Vec2 Zero => new Vec2(0f, 0f);

        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
        public static Vec2 operator *(Vec2 a, float f) => new Vec2(a.x * f, a.y * f);
        public static Vec2 operator /(Vec2 a, float f) => new Vec2(a.x / f, a.y / f);
    }

    public struct PVertex
    {
        public Vec2 position;

        public PVertex(Vec2 p)
        {
            position = p;
        }

        public PVertex(float x, float y)
        {
            position.x = x;
            position.y = y;
        }

        public static float GetHandedness(PVertex v1, PVertex v2, PVertex v3)
        {
            Vec2 edge1 = v2.position - v1.position;
            Vec2 edge2 = v3.position - v2.position;
            return Vec2.Cross(edge1, edge2);
        }
    }

    public struct SliceVertex
    {
        public Vec2 position;
        public int index;
        public float distanceToSlice;

        public SliceVertex(Vec2 p)
        {
            position = p;
            index = 0;
            distanceToSlice = 0f;
        }
    }

    public struct LineSegment
    {
        public Vec2 startPos;
        public Vec2 finalPos;

        public LineSegment(Vec2 s, Vec2 f)
        {
            startPos = s;
            finalPos = f;
        }

        public Vec2 Direction() => finalPos - startPos;

        public static (bool, Vec2) Intersects(LineSegment s1, LineSegment s2)
        {
            const float TOLERANCE = 1e-2f;

            Vec2 p1 = s1.startPos;
            Vec2 p2 = s2.startPos;
            Vec2 d1 = s1.Direction();
            Vec2 d2 = s2.Direction();

            float denom = Vec2.Cross(d1, d2);
            if (Math.Abs(denom) < 1e-30f)
                return (false, Vec2.Zero);

            float t1 = Vec2.Cross(p2 - p1, d2) / denom;

            if (t1 < (0.0f - TOLERANCE) || t1 > (1.0f + TOLERANCE))
                return (false, Vec2.Zero);

            Vec2 pIntersect = p1 + d1 * t1;

            float t2 = Vec2.Dot(pIntersect - p2, s2.finalPos - p2);
            float sqLen = Vec2.Square(s2.finalPos - p2);

            if (t2 < (0.0f - TOLERANCE) || t2 / sqLen >= 1.0f - TOLERANCE)
                return (false, Vec2.Zero);

            return (true, pIntersect);
        }

        public static LineSegment operator +(LineSegment a, LineSegment b)
        {
            Vec2 newStart = (a.startPos + b.startPos) / 2.0f;
            Vec2 newFinal = (a.finalPos + b.finalPos) / 2.0f;
            return new LineSegment(newStart, newFinal);
        }
    }

    public class ConcavePolygon
    {
        public List<PVertex> vertices = new List<PVertex>();
        public List<ConcavePolygon> subPolygons = new List<ConcavePolygon>();

        private int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        private void FlipPolygon(List<PVertex> verts)
        {
            int iMax = verts.Count / 2;
            if (verts.Count % 2 != 0) iMax += 1;

            for (int i = 1; i < iMax; ++i)
            {
                int j = verts.Count - i;
                var tmp = verts[i];
                verts[i] = verts[j];
                verts[j] = tmp;
            }
        }

        private bool CheckIfRightHanded(List<PVertex> verts)
        {
            if (verts.Count < 3) return false;

            float signedArea = 0f;
            for (int i = 0; i < verts.Count; ++i)
                signedArea += Vec2.GetSignedArea(verts[i].position, verts[Mod(i + 1, verts.Count)].position);

            return signedArea < 0.0f;
        }

        private bool IsVertexInCone(LineSegment ls1, LineSegment ls2, Vec2 origin, PVertex vert)
        {
            Vec2 relativePos = vert.position - origin;
            float ls1Product = Vec2.Cross(relativePos, ls1.Direction());
            float ls2Product = Vec2.Cross(relativePos, ls2.Direction());
            return (ls1Product < 0.0f && ls2Product > 0.0f);
        }

        private List<int> FindVerticesInCone(LineSegment ls1, LineSegment ls2, Vec2 origin, List<PVertex> inputVerts)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < inputVerts.Count; ++i)
                if (IsVertexInCone(ls1, ls2, origin, inputVerts[i]))
                    result.Add(i);
            return result;
        }

        private SortedDictionary<int, PVertex> CullByDistance(SortedDictionary<int, PVertex> input, Vec2 origin, int maxVertsToKeep)
        {
            if (maxVertsToKeep >= input.Count) return new SortedDictionary<int, PVertex>(input);

            List<SliceVertex> sliceVertices = new List<SliceVertex>();
            foreach (var kv in input)
            {
                var sv = new SliceVertex(kv.Value.position);
                sv.index = kv.Key;
                sv.distanceToSlice = Vec2.Square(kv.Value.position - origin);
                sliceVertices.Add(sv);
            }

            sliceVertices.Sort((a, b) => a.distanceToSlice.CompareTo(b.distanceToSlice));
            sliceVertices = sliceVertices.Take(maxVertsToKeep).ToList();
            sliceVertices.Sort((a, b) => a.index.CompareTo(b.index));

            SortedDictionary<int, PVertex> result = new SortedDictionary<int, PVertex>();
            foreach (var sv in sliceVertices)
                result[sv.index] = new PVertex(sv.position);
            return result;
        }

        private SortedDictionary<int, PVertex> VerticesAlongLineSegment(LineSegment segment, List<PVertex> verts)
        {
            SortedDictionary<int, PVertex> result = new SortedDictionary<int, PVertex>();
            for (int i = 0; i < verts.Count; ++i)
            {
                LineSegment temp = new LineSegment(verts[i].position, verts[Mod(i + 1, verts.Count)].position);
                var intersectionResult = LineSegment.Intersects(segment, temp);
                if (intersectionResult.Item1)
                    result[i] = new PVertex(intersectionResult.Item2);
            }
            return result;
        }

        private bool CheckVisibility(Vec2 originalPosition, PVertex vert, List<PVertex> polygonVertices)
        {
            LineSegment ls = new LineSegment(originalPosition, vert.position);
            var intersectingVerts = VerticesAlongLineSegment(ls, polygonVertices);
            return intersectingVerts.Count <= 3;
        }

        private int GetBestVertexToConnect(List<int> indices, List<PVertex> polygonVertices, Vec2 origin)
        {
            if (indices.Count == 1)
            {
                if (CheckVisibility(origin, polygonVertices[indices[0]], polygonVertices))
                    return indices[0];
            }
            else if (indices.Count > 1)
            {
                foreach (int index in indices)
                {
                    int vertSize = polygonVertices.Count;
                    PVertex prevVert = polygonVertices[Mod(index - 1, vertSize)];
                    PVertex currVert = polygonVertices[index];
                    PVertex nextVert = polygonVertices[Mod(index + 1, vertSize)];

                    LineSegment ls1 = new LineSegment(prevVert.position, currVert.position);
                    LineSegment ls2 = new LineSegment(nextVert.position, currVert.position);

                    if (PVertex.GetHandedness(prevVert, currVert, nextVert) < 0.0f &&
                        IsVertexInCone(ls1, ls2, polygonVertices[index].position, new PVertex(origin)) &&
                        CheckVisibility(origin, polygonVertices[index], polygonVertices))
                        return index;
                }

                foreach (int index in indices)
                {
                    int vertSize = polygonVertices.Count;
                    PVertex prevVert = polygonVertices[Mod(index - 1, vertSize)];
                    PVertex currVert = polygonVertices[index];
                    PVertex nextVert = polygonVertices[Mod(index + 1, vertSize)];

                    if (PVertex.GetHandedness(prevVert, currVert, nextVert) < 0.0f &&
                        CheckVisibility(origin, polygonVertices[index], polygonVertices))
                        return index;
                }

                float minDistance = float.MaxValue;
                int closest = indices[0];
                foreach (int index in indices)
                {
                    float currDistance = Vec2.Square(polygonVertices[index].position - origin);
                    if (currDistance < minDistance)
                    {
                        minDistance = currDistance;
                        closest = index;
                    }
                }
                return closest;
            }
            return -1;
        }

        private int FindFirstReflexVertex(List<PVertex> verts)
        {
            for (int i = 0; i < verts.Count; ++i)
            {
                float handedness = PVertex.GetHandedness(verts[Mod(i - 1, verts.Count)],
                                                        verts[i],
                                                        verts[Mod(i + 1, verts.Count)]);
                if (handedness < 0.0f) return i;
            }
            return -1;
        }

        public ConcavePolygon() { }

        public ConcavePolygon(List<PVertex> verts)
        {
            vertices = new List<PVertex>(verts);
            if (vertices.Count > 2 && !CheckIfRightHanded())
                FlipPolygon();
        }

        public bool CheckIfRightHanded()
        {
            return CheckIfRightHanded(vertices);
        }

        public void SlicePolygon(int vertex1, int vertex2)
        {
            if (vertex1 == vertex2 ||
                vertex2 == vertex1 + 1 ||
                vertex2 == vertex1 - 1)
                return;

            if (vertex1 > vertex2)
            {
                var t = vertex1;
                vertex1 = vertex2;
                vertex2 = t;
            }

            List<PVertex> returnVerts = new List<PVertex>();
            List<PVertex> newVerts = new List<PVertex>();

            for (int i = 0; i < vertices.Count; ++i)
            {
                if (i == vertex1 || i == vertex2)
                {
                    returnVerts.Add(vertices[i]);
                    newVerts.Add(vertices[i]);
                }
                else if (i > vertex1 && i < vertex2)
                {
                    returnVerts.Add(vertices[i]);
                }
                else
                {
                    newVerts.Add(vertices[i]);
                }
            }

            subPolygons.Add(new ConcavePolygon(returnVerts));
            subPolygons.Add(new ConcavePolygon(newVerts));
        }

        public void SlicePolygon(LineSegment segment)
        {
            if (subPolygons.Count > 0)
            {
                subPolygons[0].SlicePolygon(segment);
                subPolygons[1].SlicePolygon(segment);
                return;
            }

            const float TOLERANCE = 1e-5f;
            var slicedVertices = VerticesAlongLineSegment(segment, vertices);
            slicedVertices = CullByDistance(slicedVertices, segment.startPos, 2);
            if (slicedVertices.Count < 2) return;

            List<PVertex> leftVerts = new List<PVertex>();
            List<PVertex> rightVerts = new List<PVertex>();

            var keys = slicedVertices.Keys.ToList();
            int k0 = keys[0];
            int k1 = keys[1];

            for (int i = 0; i < vertices.Count; ++i)
            {
                Vec2 relativePosition = vertices[i].position - segment.startPos;
                float perpDistance = Math.Abs(Vec2.Cross(relativePosition, segment.Direction()));

                if (perpDistance > TOLERANCE ||
                   (perpDistance <= TOLERANCE && !slicedVertices.ContainsKey(i)))
                {
                    if ((i > k0 && i <= k1) || (k0 > k1 && (i > k0 || i <= k1)))
                        leftVerts.Add(vertices[i]);
                    else
                        rightVerts.Add(vertices[i]);
                }

                if (slicedVertices.ContainsKey(i))
                {
                    rightVerts.Add(slicedVertices[i]);
                    leftVerts.Add(slicedVertices[i]);
                }
            }

            subPolygons.Add(new ConcavePolygon(leftVerts));
            subPolygons.Add(new ConcavePolygon(rightVerts));
        }

        private void ConvexDecomp(List<PVertex> verts)
        {
            if (subPolygons.Count > 0) return;

            int reflexIndex = FindFirstReflexVertex(verts);
            if (reflexIndex == -1) return;

            Vec2 prevVertPos = verts[Mod(reflexIndex - 1, verts.Count)].position;
            Vec2 currVertPos = verts[reflexIndex].position;
            Vec2 nextVertPos = verts[Mod(reflexIndex + 1, verts.Count)].position;

            LineSegment ls1 = new LineSegment(prevVertPos, currVertPos);
            LineSegment ls2 = new LineSegment(nextVertPos, currVertPos);

            var vertsInCone = FindVerticesInCone(ls1, ls2, currVertPos, verts);

            int bestVert = -1;
            if (vertsInCone.Count > 0)
            {
                bestVert = GetBestVertexToConnect(vertsInCone, verts, currVertPos);
                if (bestVert != -1)
                {
                    LineSegment newLine = new LineSegment(currVertPos, verts[bestVert].position);
                    SlicePolygon(newLine);
                }
            }

            if (vertsInCone.Count == 0 || bestVert == -1)
            {
                LineSegment newLine = new LineSegment(currVertPos, (ls1.Direction() + ls2.Direction()) * 1e10f);
                SlicePolygon(newLine);
            }

            for (int i = 0; i < subPolygons.Count; ++i)
                subPolygons[i].ConvexDecomp();
        }

        public void ConvexDecomp()
        {
            if (vertices.Count > 3)
            {
                subPolygons.Clear();
                ConvexDecomp(vertices);
            }
        }

        public void ConvexDecompBayazit()
        {
            if (!IsCCW(vertices))
            {
                EnsureCCW(vertices);
            }

            var parts = Bayazit.Decompose(vertices);

            subPolygons.Clear();
            foreach (var part in parts)
            {
                subPolygons.Add(new ConcavePolygon(part));
            }
        }

        private bool IsCCW(List<PVertex> verts)
        {
            int n = verts.Count;
            float sum = 0f;
            for (int i = 0; i < n; i++)
            {
                Vec2 current = verts[i].position;
                Vec2 next = verts[(i + 1) % n].position;
                sum += (next.x - current.x) * (next.y + current.y);
            }

            // Positive sum is CW (we are reversing here)
            return sum < 0f;
        }

        private void EnsureCCW(List<PVertex> verts)
        {
            if (!IsCCW(verts))
            {
                verts.Reverse();
            }
        }

        public List<PVertex> GetVertices() => vertices;
        public ConcavePolygon GetSubPolygon(int subPolyIndex)
        {
            if (subPolygons.Count > 0 && subPolyIndex < subPolygons.Count)
                return subPolygons[subPolyIndex];
            return this;
        }

        public int GetNumberSubPolys() => subPolygons.Count;

        public void ReturnLowestLevelPolys(List<ConcavePolygon> returnArr, int maxVertices)
        {
            if (subPolygons.Count == 0)
            {
                if (vertices.Count <= maxVertices)
                {
                    returnArr.Add(this);
                }
                else
                {
                    // Split this convex polygon into smaller convex parts
                    SplitConvexPolygonByMaxVertices(returnArr, maxVertices);
                }
            }
            else
            {
                foreach (var sub in subPolygons)
                {
                    sub.ReturnLowestLevelPolys(returnArr, maxVertices);
                }
            }
        }

        private void SplitConvexPolygonByMaxVertices(List<ConcavePolygon> returnArr, int maxVertices)
        {
            if (vertices.Count <= maxVertices)
            {
                returnArr.Add(this);
                return;
            }

            // Take the first vertex as a “fan” pivot
            PVertex pivot = vertices[0];

            // Start at 1 because pivot is 0
            for (int i = 1; i < vertices.Count - 1;)
            {
                // Determine how many vertices we can add to this sub-polygon
                int remaining = vertices.Count - i;
                int take = Math.Min(maxVertices - 1, remaining); // -1 because pivot counts once

                // Collect vertices for the subpolygon (pivot + slice of the fan)
                List<PVertex> subVerts = new List<PVertex>();
                subVerts.Add(pivot);

                for (int j = 0; j < take; j++)
                {
                    subVerts.Add(vertices[i + j]);
                }

                returnArr.Add(new ConcavePolygon(subVerts));

                // Advance by take - 1 so next subpolygon overlaps by one vertex (to stay connected)
                i += take - 1;
            }
        }

        public void Reset()
        {
            if (subPolygons.Count > 0)
            {
                subPolygons[0].Reset();
                subPolygons[1].Reset();
                subPolygons.Clear();
            }
        }

        public Vec2 GetPoint(int index)
        {
            if (index >= 0 && index < vertices.Count)
                return vertices[index].position;
            return Vec2.Zero;
        }

        public void GetPoints(ref Vec2[] points, ref int count)
        {
            if (points == null || points.Length < vertices.Count)
            {
                points = new Vec2[vertices.Count];
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                var pos = vertices[i].position;
                points[i] = new Vec2(pos.x, pos.y);
            }

            count = vertices.Count;
        }


        public int GetPointCount() => vertices.Count;

        private void FlipPolygon()
        {
            FlipPolygon(vertices);
        }
    }
}
