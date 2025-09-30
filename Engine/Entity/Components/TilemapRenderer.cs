using Engine.Utils;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public class TilemapRenderer : Renderer2D
    {
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

            var tileMatrix = Transform.WorldMatrix * glm.translate(mat4.identity(), position) * glm.rotate(rot, new vec3(0, 0, glm.radians(1)));

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
        }

        private void PaintTiles(ldtk.Level level, ldtk.LayerInstance layer, ldtk.TileInstance[] tiles)
        {
            foreach (var tile in tiles)
            {
                bool isFlippedX = (tile.F & 1) != 0 || tile.F == 3;
                bool isFlippedY = (tile.F & 2) != 0 || tile.F == 3;

                // Tile position in pixels relative to level top-left
                float tilePxX = tile.Px[0];
                float tilePxY = tile.Px[1];

                // Final world position in pixels
                float worldX = level.WorldX + tilePxX + layer.PxTotalOffsetX;
                float worldY = -level.WorldY + -tilePxY + -layer.PxTotalOffsetY;

                // Convert to engine units if needed
                var position = new vec3(
                    MathF.Ceiling(worldX / Sprite.Texture.PixelPerUnit),
                    MathF.Ceiling(worldY / Sprite.Texture.PixelPerUnit),
                    0
                );

                AddTile(new Tile((int)tile.T, isFlippedX, isFlippedY), position);
            }
        }

        public void SetTilemapLDtk(ldtk.LdtkJson project, LDtkOptions options)
        {
            for (int i = 0; i < project.Levels.Length; i++)
            {
                var level = project.Levels[i];

                if (level.WorldDepth != 0) // TODO: remove this, I need to check and ask the user which level depth wants to draw.
                    continue;

                for (int j = level.LayerInstances.Length - 1; j >= 0; j--)
                {
                    var layer = project.Levels[i].LayerInstances[j];
                    if (!layer.Visible)
                        continue;
               
                    var type = layer.Type;
                    var grid = layer.IntGridCsv;

                    PaintTiles(level, layer, layer.AutoLayerTiles);
                    // PaintTiles(level, layer, layer.GridTiles);
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
    }
}
