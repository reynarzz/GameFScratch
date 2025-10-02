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
        private List<Box> MergeTiles(List<vec2> tilePositions)
        {
            if (tilePositions == null || tilePositions.Count == 0)
            {
                return new List<Box>();
            }

            var tiles = new HashSet<(int x, int y)>();
            foreach (var pos in tilePositions)
            {
                tiles.Add(((int)MathF.Round(pos.x), (int)MathF.Round(pos.y)));
            }

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var t in tiles)
            {
                if (t.x < minX) minX = t.x;
                if (t.y < minY) minY = t.y;
                if (t.x > maxX) maxX = t.x;
                if (t.y > maxY) maxY = t.y;
            }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            bool[,] grid = new bool[height, width];
            foreach (var t in tiles)
            {
                grid[t.y - minY, t.x - minX] = true;
            }

            var visited = new bool[height, width];
            var boxes = new List<Box>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!grid[y, x] || visited[y, x])
                    {
                        continue;
                    }

                    int rectWidth = 1;
                    while (x + rectWidth < width && grid[y, x + rectWidth] && !visited[y, x + rectWidth])
                    {
                        rectWidth++;
                    }

                    int rectHeight = 1;
                    bool canExpand;
                    while (true)
                    {
                        if (y + rectHeight >= height)
                        {
                            break;
                        }

                        canExpand = true;
                        for (int i = 0; i < rectWidth; i++)
                        {
                            if (!grid[y + rectHeight, x + i] || visited[y + rectHeight, x + i])
                            {
                                canExpand = false;
                                break;
                            }
                        }
                        if (canExpand)
                        {
                            rectHeight++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (int dy = 0; dy < rectHeight; dy++)
                    {
                        for (int dx = 0; dx < rectWidth; dx++)
                        {
                            visited[y + dy, x + dx] = true;
                        }
                    }
                      

                    float worldX = x + minX + rectWidth / 2f - 0.5f;
                    float worldY = y + minY + rectHeight / 2f - 0.5f;

                    boxes.Add(new Box(new B2Vec2(worldX, worldY),
                                      new B2Vec2(rectWidth, rectHeight)));
                }
            }

            return boxes;
        }

    }
}
