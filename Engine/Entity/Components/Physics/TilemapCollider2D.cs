using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using Box2D.NET;
using Engine.Types;
using Engine.Utils;
using System.Numerics;

namespace Engine
{
    [UniqueComponent, RequiredComponent(typeof(TilemapRenderer), typeof(RigidBody2D))]
    public class TilemapCollider2D : Collider2D
    {
        private TilemapRenderer _renderer;

        private struct Box
        {
            public B2Vec2 Position;
            public B2Vec2 Size;

            public Box(B2Vec2 pos, B2Vec2 size)
            {
                Position = pos;
                Size = size;
            }

        }

        internal override void OnInitialize()
        {
            _renderer = GetComponent<TilemapRenderer>();

            base.OnInitialize();

            RigidBody.BodyType = Body2DType.Static;
        }

        protected override B2ShapeId[] CreateShape(B2BodyId bodyId)
        {
            var polygons = GetPolygons();

            if (polygons == null || polygons.Length == 0)
                return null;

            var shapesid = new B2ShapeId[polygons.Length];

            for (int i = 0; i < polygons.Length; i++)
            {
                shapesid[i] = B2Shapes.b2CreatePolygonShape(bodyId, ref ShapeDef, ref polygons[i]);
            }

            return shapesid;
        }

        private B2Polygon[] GetPolygons()
        {
            var boxes = MergeTiles(_renderer.TilesPositions);
            var polygons = new B2Polygon[boxes.Count];
            for (int i = 0; i < boxes.Count; i++)
            {

                polygons[i] = B2Geometries.b2MakeOffsetBox(boxes[i].Size.X / 2.0f, boxes[i].Size.Y / 2.0f, boxes[i].Position + Offset.ToB2Vec2(), glm.radians(RotationOffset).ToB2Rot());
            }

            return polygons;
        }

        protected override void UpdateShape()
        {
            var polygons = GetPolygons();

            for (int i = 0; i < polygons.Length; i++)
            {
                B2Shapes.b2Shape_SetPolygon(ShapesId[i], ref polygons[i]);
            }
        }

        private List<Box> MergeTiles(IReadOnlyList<vec2> tilePositions)
        {
            if (tilePositions == null || tilePositions.Count == 0)
            {
                return new List<Box>();
            }

            // Deduplicate + bounds
            var tiles = new HashSet<(int x, int y)>();
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var pos in tilePositions)
            {
                int tx = (int)MathF.Round(pos.x);
                int ty = (int)MathF.Round(pos.y);
                if (!tiles.Add((tx, ty))) { continue; }

                if (tx < minX) { minX = tx; }
                if (ty < minY) { minY = ty; }
                if (tx > maxX) { maxX = tx; }
                if (ty > maxY) { maxY = ty; }
            }

            if (tiles.Count == 0) { return new List<Box>(); }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            int totalTiles = tiles.Count;

            // Convert grid to int array for SIMD (1 = tile present, 0 = empty)
            int[] grid = new int[width * height];
            foreach (var t in tiles)
            {
                int gx = t.x - minX;
                int gy = t.y - minY;
                grid[gy * width + gx] = 1;
            }

            var boxes = new List<Box>(Math.Max(4, totalTiles / 2));
            int consumed = 0;

            // Row-strip merging
            for (int y = 0; y < height; y++)
            {
                int x = 0;
                while (x < width)
                {
                    int idx = y * width + x;
                    if (grid[idx] == 0)
                    {
                        x++;
                        continue;
                    }

                    int runStart = x;
                    while (x < width && grid[y * width + x] == 1) { x++; }
                    int runLength = x - runStart;

                    // SIMD downward extension
                    int rectHeight = 1;
                    bool canExtend = true;
                    while (canExtend && (y + rectHeight) < height)
                    {
                        int rowStart = (y + rectHeight) * width + runStart;
                        int i = 0;
                        int simdWidth = Vector<int>.Count;

                        // Process vectorized chunks
                        for (; i <= runLength - simdWidth; i += simdWidth)
                        {
                            var block = new Vector<int>(grid, rowStart + i);
                            if (!Vector.EqualsAll(block, Vector<int>.One))
                            {
                                canExtend = false;
                                break;
                            }
                        }

                        // Process remaining elements
                        for (; i < runLength && canExtend; i++)
                        {
                            if (grid[rowStart + i] == 0)
                            {
                                canExtend = false;
                                break;
                            }
                        }

                        if (canExtend) { rectHeight++; }
                    }

                    // Clear merged tiles
                    for (int dy = 0; dy < rectHeight; dy++)
                    {
                        int rowStart = (y + dy) * width + runStart;
                        for (int dx = 0; dx < runLength; dx++)
                        {
                            grid[rowStart + dx] = 0;
                        }
                    }

                    float worldX = runStart + minX + runLength * 0.5f - 0.5f;
                    float worldY = y + minY + rectHeight * 0.5f - 0.5f;

                    boxes.Add(new Box(new B2Vec2(worldX, worldY), new B2Vec2(runLength, rectHeight)));

                    consumed += runLength * rectHeight;
                    if (consumed >= totalTiles) { return boxes; }
                }
            }

            return boxes;
        }

    }
}
