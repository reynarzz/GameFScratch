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

        public void ParseLDtk(LDtk.LDtkProject project)
        {
            foreach (var level in project.Levels)
            {
                foreach (var layer in level.LayerInstances)
                {
                    switch (layer.Type)
                    {
                        case LDtk.LayerType.IntGrid:
                            var intGridLayer = layer as LDtk.IntGridLayer;
                            PaintTiles(level, layer, intGridLayer.AutoLayerTiles);
                            break;
                        case LDtk.LayerType.Tiles:
                            var tilesLayer = layer as LDtk.TileLayer;
                            PaintTiles(level, layer, tilesLayer.GridTilesInstances);
                            break;
                        case LDtk.LayerType.AutoLayer:
                            var autoLayer = layer as LDtk.AutoLayer;
                            PaintTiles(level, layer, autoLayer.AutoLayerTiles);
                            break;
                    }
                }
            }
        }

        public void ParseLDtk(string json, LDtkParseOptions options)
        {
            try
            {
                ParseLDtk(LDtk.LDtkProject.LoadProject(JsonSerializer.Deserialize<JsonElement>(json), string.Empty));
            }
            catch (Exception e)
            {
                Debug.Error(e);
            }
        }

        private void PaintTiles(LDtk.Level level, LDtk.Layer layer, List<LDtk.Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var xPos = (level.WorldCoordinates.x + tile.Coordinates.X + layer.Offset.x);
                var yPos = (level.WorldCoordinates.y + level.Height - tile.Coordinates.Y + layer.Offset.y);

                var position = new vec3(xPos / (float)Sprite.Texture.PixelPerUnit, yPos / (float)Sprite.Texture.PixelPerUnit, 0);
                AddTile(new Tile(tile.TileId, tile.IsFlippedOnX, tile.IsFlippedOnY), position);
            }
        }

        public void RemoveTile(int x, int y)
        {

        }
    }

    public struct LDtkParseOptions
    {
        public bool RenderIntGrid { get; set; }
        public bool RenderTiles { get; set; }
        public bool RenderAutoLayer { get; set; }
    }
}
