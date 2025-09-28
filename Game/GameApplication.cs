
using Engine;
using Engine.Layers;
using Engine.Utils;
using GlmNet;
using LDtk;
using System.Text.Json;

namespace Game
{
    public class GameApplication : ApplicationLayer
    {
        public override string GameName => "Game";

        private string SpriteVertexShader = @"
        #version 330 core
        layout(location = 0) in vec3 position;
        layout(location = 1) in vec2 uv;
        layout(location = 2) in vec3 normals;
        layout(location = 3) in uint color; 
        layout(location = 4) in int texIndex; 
        
        out vec2 fragUV;
        flat out int fragTexIndex;            // flat = no interpolation between vertices
        out vec4 vColor;
        uniform mat4 uVP;
        
        vec4 unpackColor(uint c) 
        {
            float r = float((c >> 24) & 0xFFu) / 255.0;
            float g = float((c >> 16) & 0xFFu) / 255.0;
            float b = float((c >>  8) & 0xFFu) / 255.0;
            float a = float( c        & 0xFFu) / 255.0;
            return vec4(r,g,b,a);
        }
        
        void main() 
        {
            fragUV = uv;
            fragTexIndex = texIndex; 
            vColor = unpackColor(color);
            gl_Position = uVP * vec4(position, 1.0);
        }";

        public string SpriteFragmentShader = $@"
        #version 330 core
        
        uniform sampler2D uTextures[{32}];
        in vec2 fragUV;
        in vec4 vColor;
        
        flat in int fragTexIndex;
        out vec4 fragColor;
        
        void main()
        {{
            fragColor = texture(uTextures[fragTexIndex], fragUV) * vColor;
        }}";

        // TODO:
        // Implement physics: raycast, boxcast, circle cast.
        // Implement audio
        // Implement a simple file system (compression+encryption) (texture, audio, text)
        // Tilemap (rendering, ldtk file loading, colliders)
        /* Fix collision exit being called when the shape is destroyed, which causes the function to have a invalid actor,
             This collisionsExit/TriggerExit should not be called with invalid actors/components*/
        // Investigate why colliders are not freed from memory automatically.
        // Add 'CheckIfValidObject()' to all properties of the engine's components and actor.

        // Stretch:
        // Implement bounds in sprites/renderers.
        // Implement event in transform to know when scale changed, and get the delta scale.

        private void LoadTilemap()
        {
            var rootPathTest = "D:\\Projects\\GameScratch\\Game\\Assets\\___AssetTest\\";

            var tilemapTexture = Assets.GetTexture(rootPathTest + "/Cavernas_by_Adam_Saltsman.png");

            TextureAtlasUtils.SliceTiles(tilemapTexture.Atlas, 8, 8, tilemapTexture.Width, tilemapTexture.Height);

            var tilemapSprite = new Sprite();
            tilemapSprite.Texture = tilemapTexture;
            tilemapSprite.Texture.PixelPerUnit = 8;

            string json = File.ReadAllText(rootPathTest + "\\LevelTestLTilemap.ldtk");

            // Parse it
            using JsonDocument doc = JsonDocument.Parse(json);

            // Get the root element (this is the valid JsonElement)
            JsonElement element = doc.RootElement;


            var project = LDtk.LDtkProject.LoadProject(element, rootPathTest + "\\LevelTestLTilemap.ldtk");

            var tilemapActor = new Actor<TilemapRenderer>();
            var tilemap = tilemapActor.GetComponent<TilemapRenderer>();

            var mat1 = new Material(new Shader(SpriteVertexShader, SpriteFragmentShader));
            tilemap.Material = mat1;
            tilemap.Sprite = tilemapSprite;

            //tilemap.AddTile(new Tile(220), default);
            //tilemap.AddTile(new Tile(), new vec3(1, 0, 0));
            //tilemap.AddTile(new Tile(), new vec3(-1, -1, 0));
            //tilemap.AddTile(new Tile(), new vec3(2, 0, 0));
            //tilemap.AddTile(new Tile(), new vec3(1, 1, 0));
            //tilemap.AddTile(new Tile(), new vec3(2, 1, 0));
            //tilemap.AddTile(new Tile(), new vec3(3, 2, 0));
            //tilemap.AddTile(new Tile(), new vec3(1, -1, 0));

            foreach (var level in project.Levels)
            {
                foreach (var layer in level.LayerInstances)
                {

                    switch (layer.Type)
                    {
                        case LDtk.LayerType.IntGrid:
                            var intGridLayer = layer as IntGridLayer; //It seems that the tiles are coming from here.

                            foreach (var tile in intGridLayer.AutoLayerTiles)
                            {
                                var tileId = tile.TileId;
                                var position = new vec3((tile.Coordinates.X + layer.Offset.x) / tilemapSprite.Texture.PixelPerUnit, (tile.Coordinates.Y + layer.Offset.y) / tilemapSprite.Texture.PixelPerUnit, 0);

                                tilemap.AddTile(new Engine.Tile(tileId), position);
                                //Debug.Log(new vec2(position.x, position.y));
                            }

                            break;
                        case LDtk.LayerType.Entities:
                            var entitiesLayer = layer as EntitieLayer;

                            break;
                        case LDtk.LayerType.Tiles:
                            var tilesLayer = layer as TileLayer;

                            break;
                        case LDtk.LayerType.AutoLayer:
                            var autoLayer = layer as AutoLayer;

                            break;
                    }
                }

            }


        }

        public override void Initialize()
        {
            var pTexture = Assets.GetTexture("D:\\Projects\\GameScratch\\Game\\Assets\\___AssetTest\\Idle.png");

            LoadTilemap();

            var sprite4 = new Sprite();
            sprite4.Texture = pTexture;
            sprite4.Texture.PixelPerUnit = 14;

            var sprite1 = new Sprite();
            sprite1.Texture = new Texture2D(1, 1, 4, [0xFF, 0, 0, 0xFF]);
            sprite1.Texture.PixelPerUnit = 1;

            var sprite2 = new Sprite();
            sprite2.Texture = new Texture2D(1, 1, 4, [0, 0xFF, 0, 0xFF]);
            sprite2.Texture.PixelPerUnit = 1;

            var sprite3 = new Sprite();
            sprite3.Texture = new Texture2D(1, 1, 4, [0, 0, 0xFF, 0xFF]);
            sprite3.Texture.PixelPerUnit = 1;




            var mainShader = new Shader(SpriteVertexShader, SpriteFragmentShader);

            var mat1 = new Material(mainShader);
            var mat2 = new Material(mainShader);
            var mat3 = new Material(mainShader);

            var camera = new Actor<Camera, CameraFollow>("Camera").GetComponent<Camera>();
            camera.BackgroundColor = new GlmNet.vec4(0.2f, 0.2f, 0.2f, 1);
            camera.OrthographicSize = 256.0f / 2.0f / 16.0f;

            //var defChunk = sprite1.GetAtlasChunk();
            //defChunk.Pivot = new GlmNet.vec2(0.5f, 0);
            //sprite1.Texture.Atlas.UpdateChunk(0, defChunk);

            var actor = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D, CollisionTest>("CenterRotParent");
            actor.GetComponent<RigidBody2D>().BodyType = Body2DType.Kinematic;
            actor.GetComponent<SpriteRenderer>().Material = mat1;
            actor.GetComponent<SpriteRenderer>().Sprite = sprite1;
            actor.GetComponent<SpriteRenderer>().SortOrder = 2;

            // actor.GetComponent<SpriteRenderer>().Color = new Color(0, 1, 0, 1);
            actor.Transform.WorldPosition = new GlmNet.vec3(2, 0, 0);

            actor.GetComponent<Collider2D>().IsTrigger = true;


            // (int i = 0; i < 33; i++)
            {
                var actor2 = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D, RotateTest, CollisionTest>("RotatedQuad");
                actor2.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
                actor2.GetComponent<RigidBody2D>().BodyType = Body2DType.Kinematic;

                //actor2.GetComponent<SpriteRenderer>().SortOrder = 3;
                actor2.GetComponent<SpriteRenderer>().Sprite = sprite2;
                actor2.Transform.WorldPosition = new GlmNet.vec3(-2, 0, 0);
                actor2.Transform.Parent = actor.Transform;
                actor2.Transform.LocalScale = new GlmNet.vec3(1, 1, 0);
            }

            LayerMask.AssignName(3, "Player");
            LayerMask.AssignName(1, "Floor");
            LayerMask.AssignName(5, "Platform");
            LayerMask.AssignName(4, "Enemy");
            LayerMask.TurnOff("Player", "Player");

            // LayerMask.TurnOn("Player", "Player");

            Debug.Log("Enabled: " + LayerMask.AreEnabled(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player")));


            var actor3 = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D, PlayerTest>("Player");
            actor3.Layer = LayerMask.NameToLayer("Player");
            actor3.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
            //actor3.GetComponent<SpriteRenderer>().SortOrder = 1;
            actor3.GetComponent<SpriteRenderer>().Sprite = sprite4;
            var collider3 = actor3.GetComponent<Collider2D>();
            var rigid3 = actor3.Transform.GetComponent<RigidBody2D>();
            //rigid3.Transform.WorldEulerAngles = new GlmNet.vec3(0, 0, 42);
            rigid3.Transform.WorldPosition = new GlmNet.vec3(-6.5f, 0, 0);
            camera.Transform.WorldPosition = new GlmNet.vec3(actor3.Transform.WorldPosition.x,
                                                             actor3.Transform.WorldPosition.y, -12);
            rigid3.LockZRotation = true;

            // rigid3.Actor.IsEnabled = false;
            // actor3.GetComponent<BoxCollider2D>().IsTrigger = true;

            rigid3.IsAutoMass = false;
            // Actor.Destroy(camera.Actor);
            var cam = Actor.Find("Camera");

            if (camera)
            {
                camera.GetComponent<CameraFollow>().Target = actor3.Transform;
            }

            var actor4 = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D, CollisionTest>("Floor");
            actor4.Layer = 1;

            var rigid4 = actor4.GetComponent<RigidBody2D>();
            var boxCollider = actor4.GetComponent<BoxCollider2D>();
            var polygon = actor4.AddComponent<PolygonCollider2D>();
            polygon.IsTrigger = true;

            polygon.Points =
            [
                new vec2(0, 0),  new vec2(4, 1),
                new vec2(7, 0),  new vec2(9, 2),
                new vec2(11, 1), new vec2(12, 4),
                new vec2(10, 5), new vec2(12, 7),
                new vec2(9, 9),  new vec2(7, 7),
                new vec2(6, 10), new vec2(4, 8),
                new vec2(3, 11), new vec2(0, 10),
                new vec2(1, 7),  new vec2(-2, 6),
                new vec2(0, 4),  new vec2(-3, 2),
                new vec2(-1, 1), new vec2(-2, -1)
            ];

            polygon.Generate();
            polygon.Offset = new vec2(-2, 0);
            polygon.RotationOffset = 0;
            // polygon.IsEnabled = false;

            var platform = new Actor<Platform>("Platform");
            var respawner = new Actor<Respawner>("Respawner");

            platform.Layer = LayerMask.NameToLayer("Platform");
            rigid4.BodyType = Body2DType.Kinematic;

            boxCollider.Size = new GlmNet.vec2(15, 1);

            rigid4.Transform.WorldPosition = new GlmNet.vec3(0, -4, 0);
            rigid4.Transform.WorldEulerAngles = new GlmNet.vec3(0, 0, 20);
            actor4.Transform.LocalScale = new GlmNet.vec3(15, 1, 1);

            actor4.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>()?.Material;
            //actor3.GetComponent<SpriteRenderer>().SortOrder = 1;
            actor4.GetComponent<SpriteRenderer>().Sprite = sprite2;

            Debug.Success("Game Layer");
        }

        public override void Close()
        {
        }
    }
}
