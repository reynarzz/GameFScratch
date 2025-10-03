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
            GetComponent<RigidBody2D>().BodyType = Body2DType.Static;
            base.OnInitialize();
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

        public void SetTilemap()
        {

        }

        private List<Box> MergeTiles(IReadOnlyList<vec2> tilePositions)
        {
            // Handle null or empty input early
            if (tilePositions == null || tilePositions.Count == 0)
            {
                return new List<Box>();
            }

            // Deduplicate tile coordinates (So we don't process the same tile twice)
            // Also track min/max bounds to know how large our occupancy grid needs to be.
            var tiles = new HashSet<(int x, int y)>();
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var pos in tilePositions)
            {
                // This ensures that floating-point positions map to integer tile coordinates.
                int tx = (int)MathF.Round(pos.x);
                int ty = (int)MathF.Round(pos.y);

                // Add to HashSet; skip if already present (duplicate tile)
                if (!tiles.Add((tx, ty)))
                {
                    continue;
                }

                // Update bounding box (min/max tile extents)
                if (tx < minX) { minX = tx; }
                if (ty < minY) { minY = ty; }
                if (tx > maxX) { maxX = tx; }
                if (ty > maxY) { maxY = ty; }
            }

            // No valid tiles after deduplication
            if (tiles.Count == 0)
            {
                return new List<Box>();
            }

            // Compute grid dimensions
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            int totalTiles = tiles.Count;

            // Build occupancy grid in a single 1D array (Better cache locality than 2D)
            // grid[y * width + x] == true means tile exists at that position
            var grid = new bool[width * height];
            foreach (var t in tiles)
            {
                int gx = t.x - minX; // shift into [0..width)
                int gy = t.y - minY; // shift into [0..height)
                grid[gy * width + gx] = true;
            }

            // Preallocate result list (Heuristic: about half the tile count will become boxes)
            var boxes = new List<Box>(Math.Max(4, totalTiles / 2));
            int consumed = 0; // counter to allow early exit when all tiles are merged

            // Main merging loop: scan row by row
            for (int y = 0; y < height; y++)
            {
                int x = 0;
                while (x < width)
                {
                    int idx = y * width + x;

                    // Skip if no tile at this position (Aleready merged or empty)
                    if (!grid[idx])
                    {
                        x++;
                        continue;
                    }

                    // Found start of a horizontal run of tiles, measure its length
                    int runStart = x;
                    while (x < width && grid[y * width + x])
                    {
                        x++;
                    }
                    int runLength = x - runStart;

                    // Try to extend this horizontal run downward to form a taller rectangle
                    int rectHeight = 1;
                    bool canExtend = true;
                    while (canExtend && (y + rectHeight) < height)
                    {
                        int rowStart = (y + rectHeight) * width + runStart;
                        for (int i = 0; i < runLength; i++)
                        {
                            if (!grid[rowStart + i])
                            {
                                // A gap in this row = can't extend further
                                canExtend = false;
                                break;
                            }
                        }
                        if (canExtend)
                        {
                            rectHeight++;
                        }
                    }

                    // Clear merged tiles so they won't be visited again
                    for (int dy = 0; dy < rectHeight; dy++)
                    {
                        int rowStart = (y + dy) * width + runStart;
                        for (int dx = 0; dx < runLength; dx++)
                        {
                            grid[rowStart + dx] = false;
                        }
                    }

                    // Compute box center in world coordinates
                    // Note: subtract 0.5 to correctly align box centers with tile centers
                    float worldX = runStart + minX + runLength * 0.5f - 0.5f;
                    float worldY = y + minY + rectHeight * 0.5f - 0.5f;

                    boxes.Add(new Box(
                        new B2Vec2(worldX, worldY),        // Center
                        new B2Vec2(runLength, rectHeight)  // Size
                    ));

                    // Track how many tiles have been consumed so far
                    consumed += runLength * rectHeight;
                    if (consumed >= totalTiles)
                    {
                        return boxes; // early exit: all tiles merged
                    }
                }
            }

            return boxes;
        }

    }
}
