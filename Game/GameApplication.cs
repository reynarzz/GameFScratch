
using Engine;
using Engine.Layers;
using Engine.Utils;
using GlmNet;
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

        // -TODO:
        // Implement physics: raycast, boxcast, circle cast.
        // Implement audio
        // Implement a simple file system (compression+encryption) (texture, audio, text)
        /* Fix collision exit being called when the shape is destroyed, which causes the function to have a invalid actor,
             This collisionsExit/TriggerExit should not be called with invalid actors/components*/
        // Fix transform interpolation not happening because of renderer.IsDirty in batcher2d
        // Fix rigidbody marked as interpolate if is made parent of another that is not, after exiting, the interpolation is disabled.
        // Simple animation system (state machine, variable(bool,int,float) and transition conditions (bool (true/false), int(equal,less, greater) float(less, greater)))

        // For game:
        // Implement enemies
        // Five levels, small, one intro level falling from outside.
        // Colllect coins, hearts, attack enemies, go from door A to B
        // 

        // -Stretch:
        // Implement bounds in sprites/renderers.
        // Implement event in transform to know when scale changed, and get the delta scale.
        // Add 'CheckIfValidObject()' to all properties of the engine's components and actor.
        // Investigate why colliders are not freed from memory automatically.
        // Game using both assets, and using stencil buffer to change beteen them sphere.


        private void LoadTilemap(Camera cam)
        {
            var testPathNow = "D:\\Projects\\GameScratch\\Game\\Assets\\Tilemap";
            //var tilemapTexture = Assets.GetTexture(rootPathTest + "\\KingsAndPigsSprites\\14-TileSets\\Terrain (32x32).png");
            var tilemapTexture = Assets.GetTexture(testPathNow + "\\SunnyLand_by_Ansimuz-extended.png");
            //var tilemapTexture = Assets.GetTexture(testPathNow + "\\Inca_front_by_Kronbits-extended.png");

            TextureAtlasUtils.SliceTiles(tilemapTexture.Atlas, 16, 16, tilemapTexture.Width, tilemapTexture.Height);

            var tilemapSprite = new Sprite();

            tilemapSprite.Texture = tilemapTexture;
            tilemapSprite.Texture.PixelPerUnit = 16;

            //var filepath = rootPathTest + "\\Tilemap\\World.ldtk";
            var filepath = testPathNow + "\\Tilemap2.ldtk";
            //var filepath = testPathNow + "\\Tilemap3.ldtk";
            string json = File.ReadAllText(filepath);

            var mat1 = new Material(new Shader(SpriteVertexShader, SpriteFragmentShader));


            var project = ldtk.LdtkJson.FromJson(json);
            var color = project.BgColor;

            //cam.BackgroundColor = new Color32(project.BackgroundColor.R, project.BackgroundColor.G, project.BackgroundColor.B, project.BackgroundColor.A);
            //cam.BackgroundColor = new Color32(23, 28, 57, project.BackgroundColor.A);

            var tilemapActor = new Actor<TilemapRenderer>("Foreground tilemap");
            var tilemap = tilemapActor.GetComponent<TilemapRenderer>();
            tilemap.Material = mat1;
            tilemap.Sprite = tilemapSprite;

            var tilemapActor2 = new Actor<TilemapRenderer>("Background tilemap");
            var tilemap2 = tilemapActor2.GetComponent<TilemapRenderer>();
            tilemap2.Material = mat1;
            tilemap2.Sprite = tilemapSprite;

            var tilemapActor3 = new Actor<TilemapRenderer>("Grass tilemap");
            var tilemap3 = tilemapActor3.GetComponent<TilemapRenderer>();
            tilemap3.Material = mat1;
            tilemap3.Sprite = tilemapSprite;

            // tilemap.SetTilemapLDtk(project, new LDtkOptions() { RenderIntGridLayer = true, RenderTilesLayer = true, RenderAutoLayer = true });
            tilemap.SetTilemapLDtk(project, new LDtkOptions()
            {
                RenderIntGridLayer = true,
                RenderTilesLayer = true,
                RenderAutoLayer = true,
                LayersToLoadMask = 1 << 2,
                WorldDepth = 0
            });

            tilemap2.SetTilemapLDtk(project, new LDtkOptions()
            {
                RenderIntGridLayer = true,
                RenderTilesLayer = true,
                RenderAutoLayer = true,
                LayersToLoadMask = 1 << 3,
                WorldDepth = 0
            });

            //tilemap3.SetTilemapLDtk(project2, new LDtkOptions()
            //{
            //    RenderIntGridLayer = true,
            //    RenderTilesLayer = true,
            //    RenderAutoLayer = true,
            //    LayersToLoadMask = 1 << 0,
            //    WorldDepth = 0
            //});

            tilemap2.SortOrder = 0;
            tilemap.SortOrder = 3;
            tilemap3.SortOrder = 3;
            tilemap.AddComponent<TilemapCollider2D>();
            tilemap.Actor.Layer = 1;
        }

        public override void Initialize()
        {
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

            var camera = new Actor<Camera, CameraFollow>("Camera").GetComponent<Camera>();
            camera.BackgroundColor = new Engine.Color(0.2f, 0.2f, 0.2f, 1);
            camera.OrthographicSize = 512.0f / 2.0f / 16.0f;
            // camera.OrthoMatch = CameraOrthoMatch.Width;
            // camera.RenderTexture = new RenderTexture(512, 288);

            LoadTilemap(camera);

            //var defChunk = sprite1.GetAtlasChunk();
            //defChunk.Pivot = new GlmNet.vec2(0.5f, 0);
            //sprite1.Texture.Atlas.UpdateChunk(0, defChunk);

            LayerMask.AssignName(3, "Player");
            LayerMask.AssignName(1, "Floor");
            LayerMask.AssignName(5, "Platform");
            LayerMask.AssignName(4, "Enemy");
            LayerMask.TurnOff("Player", "Player");

            // LayerMask.TurnOn("Player", "Player");

            Debug.Log("Enabled: " + LayerMask.AreEnabled(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Player")));


            var actor = new Actor<SpriteRenderer, RigidBody2D, RotateTest, BoxCollider2D, CollisionTest>("CenterRotParent");
            actor.AddComponent<ParentTests>();
            actor.GetComponent<RigidBody2D>().BodyType = Body2DType.Kinematic;
            actor.GetComponent<SpriteRenderer>().Material = mat1;
            actor.GetComponent<SpriteRenderer>().Sprite = sprite1;
            actor.GetComponent<SpriteRenderer>().SortOrder = 0;
            actor.Layer = LayerMask.NameToLayer("Floor");
            // actor.GetComponent<SpriteRenderer>().Color = new Color(0, 1, 0, 1);
            actor.Transform.WorldPosition = new GlmNet.vec3(0, 0, 0);

            //actor.GetComponent<Collider2D>().IsTrigger = true;


            // (int i = 0; i < 33; i++)
            {
                var actor2 = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D, RotateTest, CollisionTest>("RotatedQuad");
                actor2.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
                actor2.GetComponent<RigidBody2D>().BodyType = Body2DType.Kinematic;

                actor2.GetComponent<SpriteRenderer>().SortOrder = 0;
                actor2.GetComponent<SpriteRenderer>().Sprite = sprite2;
                actor2.Transform.WorldPosition = new GlmNet.vec3(-3, 0, 0);
                actor2.Transform.Parent = actor.Transform;
                actor2.Transform.LocalScale = new GlmNet.vec3(1, 1, 0);
            }


            var playerActor = new Actor<SpriteRenderer, RigidBody2D, CapsuleCollider2D, PlayerTest, SpriteAnimation2D>("Player");
            playerActor.Layer = LayerMask.NameToLayer("Player");
            playerActor.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
            playerActor.GetComponent<SpriteRenderer>().SortOrder = 1;


            // playerActor.GetComponent<SpriteRenderer>().Sprite = animSprites[0];
            //sprite4.Texture.Atlas.UpdatePivot(0, new vec2(0.4f, 0.4f));

            playerActor.GetComponent<CapsuleCollider2D>().Offset = new vec2(0, 0.25f);
            playerActor.GetComponent<CapsuleCollider2D>().Size = new vec2(1, 1.7f);
            var collider3 = playerActor.GetComponent<Collider2D>();
            var rigid3 = playerActor.Transform.GetComponent<RigidBody2D>();
            //rigid3.Transform.WorldEulerAngles = new GlmNet.vec3(0, 0, 42);
            rigid3.Transform.WorldPosition = new GlmNet.vec3(0.5f, -10, 0);
            camera.Transform.WorldPosition = new GlmNet.vec3(playerActor.Transform.WorldPosition.x,
                                                             playerActor.Transform.WorldPosition.y, -12);
            rigid3.LockZRotation = true;

            // rigid3.Actor.IsEnabled = false;
            // actor3.GetComponent<BoxCollider2D>().IsTrigger = true;

            rigid3.IsAutoMass = false;
            // Actor.Destroy(camera.Actor);
            var cam = Actor.Find("Camera");

            if (camera)
            {
                camera.GetComponent<CameraFollow>().Target = playerActor.Transform;
            }

            var platform = new Actor<Platform>("Platform");

            platform.Layer = LayerMask.NameToLayer("Platform");


            Debug.Success("Game Layer");
        }

        public override void Close()
        {
        }
    }
}
