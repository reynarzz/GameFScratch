using Box2D.NET;
using Engine.Types;
using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Engine
{
    public struct Tile
    {
        public int Index { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        public Tile(int index, bool flipX, bool flipY)
        {
            Index = index;
            FlipX = flipX;
            FlipY = flipY;
        }
    }

    [UniqueComponent]
    public class TilemapRenderer : Renderer2D
    {
        public IReadOnlyList<vec2> TilesPositions => _tilesPositions;
        private List<vec2> _tilesPositions = new();

        internal override void OnInitialize()
        {
            base.OnInitialize();

            Mesh = new Mesh();
            Mesh.IndicesToDrawCount = 0;
        }

        public void AddTile(Tile tile, vec3 position, float rot = 0)
        {
            QuadVertices vertices = default;

            var texture = Sprite.Texture;
            var chunk = texture.Atlas.GetChunk(tile.Index);

            float ppu = texture.PixelPerUnit;
            var width = (float)chunk.Width / ppu;
            var height = (float)chunk.Height / ppu;

            var tileMatrix = Transform.WorldMatrix * glm.translate(mat4.identity(), position) * glm.rotate(glm.radians(rot), new vec3(0, 0, 1));

            chunk.Uvs = QuadUV.FlipUV(chunk.Uvs, tile.FlipX, tile.FlipY);

            GraphicsHelper.CreateQuad(ref vertices, chunk.Uvs, width, height, chunk.Pivot, Color, tileMatrix);

            Mesh.Vertices.Add(vertices.v0);
            Mesh.Vertices.Add(vertices.v1);
            Mesh.Vertices.Add(vertices.v2);
            Mesh.Vertices.Add(vertices.v3);

            Mesh.IndicesToDrawCount += 6;

            IsDirty = true;

            //var index = (uint)Mesh.Vertices.Count - 4;
            //Mesh.Indices.Add(index + 0);
            //Mesh.Indices.Add(index + 1);
            //Mesh.Indices.Add(index + 2);
            //Mesh.Indices.Add(index + 2);
            //Mesh.Indices.Add(index + 3);
            //Mesh.Indices.Add(index + 0);

           // Debug.Log("Quads count: " + Mesh.Vertices.Count / 4);
        }

        private void PaintTiles(ldtk.Level level, ldtk.LayerInstance layer, ldtk.TileInstance[] tiles)
        {
            foreach (var tile in tiles)
            {
                bool isFlippedX = (tile.F & 1) != 0 || tile.F == 3;
                bool isFlippedY = (tile.F & 2) != 0 || tile.F == 3;

                float tilePxX = tile.Px[0];
                float tilePxY = tile.Px[1];

                float worldX = level.WorldX + tilePxX + layer.PxTotalOffsetX;
                float worldY = -level.WorldY + -tilePxY + -layer.PxTotalOffsetY;

                var position = new vec3(
                    MathF.Floor(worldX / Sprite.Texture.PixelPerUnit),
                    MathF.Floor(worldY / Sprite.Texture.PixelPerUnit),
                    0
                );

                _tilesPositions.Add(new vec2(position.x, position.y));

                AddTile(new Tile((int)tile.T, isFlippedX, isFlippedY), position);
            }
        }

        public void SetTilemapLDtk(ldtk.LdtkJson project, LDtkOptions options)
        {
            _tilesPositions.Clear();

            for (int i = 0; i < project.Levels.Length; i++)
            {
                var level = project.Levels[i];

                if (level.WorldDepth != options.WorldDepth)
                    continue;

                for (int j = level.LayerInstances.Length - 1; j >= 0; j--)
                {
                    if (((options.LayersToLoadMask & (1UL << j)) == 0) && options.LayersToLoadMask != 0)
                        continue;

                    var layer = level.LayerInstances[j];

                    if (!layer.Visible)
                        continue;

                    var type = layer.Type;

                    switch (type)
                    {
                        case "AutoLayer":
                        case "IntGrid":
                            PaintTiles(level, layer, layer.AutoLayerTiles);
                            break;
                        case "Entities":
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void SetTilemapLDtk(string json, LDtkOptions options)
        {
            if (!string.IsNullOrEmpty(json))
            {
                SetTilemapLDtk(ldtk.LdtkJson.FromJson(json), options);
            }
        }

        public void RemoveTile(int x, int y)
        {

        }
    }

    public struct LDtkOptions
    {
        public bool RenderIntGridLayer { get; set; }
        public bool RenderTilesLayer { get; set; }
        public bool RenderAutoLayer { get; set; }
        public int[] LevelsToLoad { get; set; }
        public ulong LayersToLoadMask { get; set; }
        public int WorldDepth { get; set; }
    }
}
