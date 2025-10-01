using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using GlmNet;

namespace Engine.Utils
{
    public class TextureAtlasUtils
    {
        public static AtlasChunk CreateTileBounds(int xPixel, int yPixel, int width, int height, float pivotX, float pivotY, int baseTextureWidth, int baseTextureHeight)
        {
            float x = xPixel / (float)baseTextureWidth;
            float y = yPixel / (float)baseTextureHeight;
            float nWidth = (float)width / (float)baseTextureWidth;
            float nHeight = (float)height / (float)baseTextureHeight;

            // adds pixel tolerance to remove bleeding pixel from adjacent tiles
            float ptX = (1.0f / (float)(baseTextureWidth * width)) / 2.0f;
            float ptY = (1.0f / (float)(baseTextureHeight * width)) / 2.0f;

            var texCoords = new QuadUV()
            {
                BottomLeftUV = new vec2(x + ptX, y + ptY),                    // Bottom left
                TopLeftUV = new vec2(x + ptX, y + nHeight - ptY),             // Top left
                TopRightUV = new vec2(x + nWidth - ptX, y + nHeight - ptY),   // Top right
                BottomRightUV = new vec2(x + nWidth - ptX, y + ptY),          // Bottom right
            };

            return new AtlasChunk()
            {
                Width = width,
                Height = height,
                Uvs = texCoords,
                Pivot = new vec2(pivotX, pivotY),
            };
        }

        public static void SliceTiles(TextureAtlasData data, int width, int height, int baseTextureWidth, int baseTextureHeight)
        {
            int tilesX = baseTextureWidth / width;
            int tilesY = baseTextureHeight / height;

            var length = tilesX * tilesY;
            var atlasChunks = new AtlasChunk[length];

            int index = 0;
            for (int y = tilesY - 1; y >= 0; --y)
            {
                for (int x = 0; x < tilesX; ++x)
                {
                    atlasChunks[index++] = CreateTileBounds(x * width, (y) * height, width, height, 0.5f, 0.5f, baseTextureWidth, baseTextureHeight);
                }
            }

            data.SetChunks(atlasChunks);
        }

        public static Sprite[] SliceSprites(Texture2D texture, int width, int height)
        {
            return SliceSprites(texture, width, height, new vec2(0.5f, 0.5f));
        }

        public static Sprite[] SliceSprites(Texture2D texture, int width, int height, vec2 pivot)
        {
            int tilesX = texture.Width / width;
            int tilesY = texture.Height / height;
            var length = tilesX * tilesY;
            var atlasChunks = new AtlasChunk[length];
            var sprites = new Sprite[length];

            int index = 0;
            for (int y = tilesY - 1; y >= 0; --y)
            {
                for (int x = 0; x < tilesX; ++x)
                {
                    var chunk = CreateTileBounds(x * width, (y) * height, width, height, 0.5f, 0.5f, texture.Width, texture.Height);

                    var sprite = new Sprite();
                    sprite.Texture = texture;
                    sprite.AtlasIndex = index;
                    chunk.Pivot = pivot;

                    sprites[index] = sprite;
                    atlasChunks[index] = chunk;

                    index++;
                }
            }

            texture.Atlas.SetChunks(atlasChunks);

            return sprites;
        }

        public QuadUV ConvertTexCoordToGraphicsApiCompatible(QuadUV coord)
        {
            // TODO: Since the engine is rendering in OpengL, the uv is always reversed,
            //       This must change when api is changed to another that texCoord do not start from the bottom 

            bool flipDueGraphicsApi = true;

            QuadUV outCoords = coord;
            if (flipDueGraphicsApi)
            {
                // Flip whole textCoord
                outCoords.BottomLeftUV.y = 1.0f - coord.BottomLeftUV.y;
                outCoords.TopLeftUV.y = 1.0f - coord.TopLeftUV.y;
                outCoords.TopRightUV.y = 1.0f - coord.TopRightUV.y;
                outCoords.BottomRightUV.y = 1.0f - coord.BottomRightUV.y;

                // Flip cell y
                float leftTempY = outCoords.TopLeftUV.y;
                float rightTempY = outCoords.TopRightUV.y;

                outCoords.BottomLeftUV.y = outCoords.TopLeftUV.y;
                outCoords.TopLeftUV.y = leftTempY;
                outCoords.TopRightUV.y = outCoords.BottomRightUV.y;
                outCoords.BottomRightUV.y = rightTempY;
            }

            return outCoords;
        }
    }
}
