
using Engine;
using Engine.Graphics;
using Engine.Layers;
using Engine.Utils;
using GlmNet;
using ldtk;
using System.Linq;

namespace Game
{
    public class GameApplication : ApplicationLayer
    {
        // -TODO:
        // Implement physics: raycast, boxcast, circle cast.
        /* Fix collision exit being called when the shape is destroyed, which causes the function to have a invalid actor,
             This collisionsExit/TriggerExit should not be called with invalid actors/components*/
        // Fix transform interpolation not happening because of renderer.IsDirty in batcher2d
        // Fix rigidbody marked as interpolate if is made parent of another that is not, after exiting, the interpolation is disabled.
        // Simple animation system (state machine, variable(bool,int,float) and transition conditions (bool (true/false), int(equal,less, greater) float(less, greater)))
        // Implement post-proccesing system

        // For game:
        // Implement enemies
        // Five levels, small, one intro level falling from outside.
        // Colllect coins, hearts, attack enemies, go from door A to B
        // Start with nothing, then grab the hammer as a powerup (modify sprites)

        // -Stretch:
        // Implement bounds in sprites/renderers.
        // Implement event in transform to know when scale changed, and get the delta scale.
        // Add 'CheckIfValidObject()' to all properties of the engine's components and actor.
        // Investigate why colliders are not freed from memory automatically.
        // Game using both assets, and using stencil buffer to change beteen them sphere.
        // Investigate why AudioMixer frees from memory automatically

        private vec3 _playerStartPosTest;
        private void LoadTilemap(Camera cam)
        {
            var testPathNow = "Tilemap";
            var tilemapTexture = Assets.GetTexture(testPathNow + "/SunnyLand_by_Ansimuz-extended.png");

            TextureAtlasUtils.SliceTiles(tilemapTexture.Atlas, 16, 16, tilemapTexture.Width, tilemapTexture.Height);

            var tilemapSprite = new Sprite();

            tilemapSprite.Texture = tilemapTexture;
            tilemapSprite.Texture.PixelPerUnit = 16;

            //var filepath = rootPathTest + "\\Tilemap\\World.ldtk";

            var filepath = testPathNow + "/WorldTilemap.ldtk";
            // var filepath = testPathNow + "/Test.ldtk";
            string json = Assets.GetText(filepath).Text;

            var mat1 = new Material(new Shader(Assets.GetText("Shaders/SpriteVert.vert").Text, Assets.GetText("Shaders/SpriteFrag.frag").Text));

            var project = ldtk.LdtkJson.FromJson(json);
            var color = project.BgColor;

            vec3 ConvertToWorld(long[] px, Level level, LayerInstance layer)
            {
                return new vec3(level.WorldX + px[0] + layer.PxOffsetX, -level.WorldY + -px[1] + -layer.PxOffsetY, 0);
            }

            foreach (var level in project.Levels)
            {
                foreach (var layer in level.LayerInstances)
                {
                    foreach (var entity in layer.EntityInstances)
                    {
                        //Debug.Log("Entity: " + entity.Identifier);
                        if (entity.Identifier.Equals("Player"))
                        {
                            _playerStartPosTest = ConvertToWorld(entity.Px, level, layer) / tilemapTexture.PixelPerUnit;
                        }

                        foreach (var field in entity.FieldInstances)
                        {
                            //Debug.Log("Name: " + field.Identifier + ", Type: " + field.Type + ", Value: " + field.Value);
                        }
                    }
                }
            }

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
            var mainShader = new Shader(Assets.GetText("Shaders/SpriteVert.vert").Text, Assets.GetText("Shaders/SpriteFrag.frag").Text);

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



            var playerActor = new Actor<SpriteRenderer, RigidBody2D, CapsuleCollider2D, PlayerTest, SpriteAnimation2D>("Player");
            playerActor.Layer = LayerMask.NameToLayer("Player");
            playerActor.GetComponent<SpriteRenderer>().Material = mat1;
            playerActor.GetComponent<SpriteRenderer>().SortOrder = 4;
            var PlayerMatPass = mat1.Passes.ElementAt(0);
            PlayerMatPass.Stencil.Enabled = true;
            PlayerMatPass.Stencil.Func = StencilFunc.Always;
            PlayerMatPass.Stencil.Ref = 3;
            PlayerMatPass.Stencil.ZPassOp = StencilOp.Replace;

            var audioClip = Assets.GetAudioClip("Audio/music/streamloops/Stream Loops 2023-11-29.wav");
            var source = playerActor.AddComponent<AudioSource>();
            source.Clip = audioClip;
            source.Loop = true;
            source.Play();

            // var handle = System.Runtime.InteropServices.GCHandle.Alloc(source.Mixer, System.Runtime.InteropServices.GCHandleType.Normal);
            // var reverb = source.Mixer.AddAudioFX<ReverbAudioFX>();
            // reverb.RoomSize = 100;

            Debug.Log("Duration: " + audioClip.Duration);
            //source.PlayOneShot(audioClip);

            // source.Volume = 0.3f;
            //source.Time = 20;
            // playerActor.GetComponent<SpriteRenderer>().Sprite = animSprites[0];
            //sprite4.Texture.Atlas.UpdatePivot(0, new vec2(0.4f, 0.4f));

            playerActor.GetComponent<CapsuleCollider2D>().Offset = new vec2(0, 0.25f);
            playerActor.GetComponent<CapsuleCollider2D>().Size = new vec2(1.4f, 1.7f);
            var collider3 = playerActor.GetComponent<Collider2D>();
            var rigid3 = playerActor.GetComponent<RigidBody2D>();
            //rigid3.Transform.WorldEulerAngles = new GlmNet.vec3(0, 0, 42);
            rigid3.Transform.WorldPosition = _playerStartPosTest;
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

            var platform = new Actor<Platform, SpriteRenderer>("Platform");
            platform.GetComponent<SpriteRenderer>().Material = mat1;
            platform.Layer = LayerMask.NameToLayer("Platform");


            ScreenGrabTest();
            // ScreenGrabTest2();

            ScreenGrabTest3();
            ScreenGrabTest4();
            // ScreenGrabTest5();

            // WaterTest();

            Debug.Success("Game Layer");
        }

        private void ScreenGrabTest()
        {
            //var screenGrabTest = new Actor<SpriteRenderer>();
            //var renderer = screenGrabTest.GetComponent<SpriteRenderer>();
            //renderer.SortOrder = 15;

            var screenShader = new Shader(Assets.GetText("Shaders/ScreenVert.vert").Text, Assets.GetText("Shaders/CTRTv.frag").Text);
            //renderer.Material = new Material(screenShader);

            //var pass = renderer.Material.Passes.ElementAt(0);
            //pass.IsScreenGrabPass = true;
            //screenGrabTest.Transform.LocalScale = new vec3(Window.Width, Window.Height)/* / 34*/;
            //screenGrabTest.Transform.LocalPosition = new vec3(-9, -5);

            PostProcessingStack.Push(new PostProcessingSinglePass(screenShader));
        }

        private void ScreenGrabTest2()
        {
            var screenShader = new Shader(Assets.GetText("Shaders/ScreenVert.vert").Text, Assets.GetText("Shaders/GrayScale.frag").Text);
            PostProcessingStack.Push(new PostProcessingSinglePass(screenShader));
        }

        private void ScreenGrabTest3()
        {
            var vertex = Assets.GetText("Shaders/ScreenVert.vert").Text;
            var screenShader = new Shader(vertex, Assets.GetText("Shaders/Ripple.frag").Text);
            PostProcessingStack.Push(new PostProcessingSinglePass(screenShader));

            var screenShader2 = new Shader(vertex, Assets.GetText("Shaders/ChromaticAberration.frag").Text);
            PostProcessingStack.Push(new PostProcessingSinglePass(screenShader2));
        }

        private void ScreenGrabTest4()
        {
            var screenGrabTest = new Actor<SpriteRenderer, Rotate>();
            var renderer = screenGrabTest.GetComponent<SpriteRenderer>();
            renderer.SortOrder = 14;

            var screenShader = new Shader(Assets.GetText("Shaders/VertScreenGrab.vert").Text, Assets.GetText("Shaders/Wobble.frag").Text);
            var screenShader2 = new Shader(Assets.GetText("Shaders/VertScreenGrab.vert").Text, Assets.GetText("Shaders/GrayScale.frag").Text);
            renderer.Material = new Material(screenShader);

            var pass = renderer.Material.Passes.ElementAt(0);
            pass.IsScreenGrabPass = true;

            // renderer.Material.AddPass(new RenderPass() { Shader = screenShader2, Blending = Blending.Transparent });
            screenGrabTest.Transform.LocalScale = new vec3(6, 6);
            screenGrabTest.Transform.LocalPosition = new vec3(-9, -7);
        }

        private void ScreenGrabTest5()
        {
            var screenShader = new Shader(Assets.GetText("Shaders/ScreenVert.vert").Text, Assets.GetText("Shaders/FilmGrain.frag").Text);
            
            PostProcessingStack.Push(new PostProcessingSinglePass(screenShader));
        }

        private void WaterTest()
        {
            var waterActor = new Actor<SpriteRenderer>();
            var renderer = waterActor.GetComponent<SpriteRenderer>();
            renderer.SortOrder = 9;

            var mainShader = new Shader(Assets.GetText("Shaders/SpriteVert.vert").Text, Assets.GetText("Shaders/SpriteFrag.frag").Text);

            renderer.Material = new Material(mainShader);

            var pass = renderer.Material.Passes.ElementAt(0);
            pass.Stencil.Enabled = true;
            pass.Stencil.Func = StencilFunc.Equal;
            pass.Stencil.Ref = 3;
            pass.Stencil.ZFailOp = StencilOp.Keep;

            waterActor.Transform.LocalScale = new vec3(10, 5, 1);
            waterActor.Transform.LocalPosition = new vec3(2.5f, -12, 1);
        }

        public override void Close() { }
    }
}
