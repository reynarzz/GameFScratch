
using Engine;
using Engine.Layers;

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
        in vec2 vUV;
        in vec4 vColor;
        
        flat in int fragTexIndex;
        out vec4 fragColor;
        
        void main()
        {{
            fragColor = texture(uTextures[fragTexIndex], vUV) * vColor;
        }}";

        // TODO:
        // Implement physics: collision functions (OnCollisionEnter2D, OnTriggerEnter2D, etc...)
        // Implement layerMask
        // Implement physics: raycast, boxcast, circle cast.
        // Implement audio
        // Implement a simple file system.
        // Tilemap (rendering, ldtk file loading, colliders)
        // Use Collider2D instead of shape in CollisionKey, so it can support colliders of multiples shapes.
        // Fix collision exit being called when the shape is destroyed, which causes the function to have a invalid actor,
             // This collisionsExit/TriggerExit should not be called with invalid actors/components

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
            var mat2 = new Material(mainShader);
            var mat3 = new Material(mainShader);

            var camera = new Actor<Camera, CameraFollow>("Camera").GetComponent<Camera>();
            camera.BackgroundColor = new GlmNet.vec4(0.2f, 0.2f, 0.2f, 1);
            camera.OrthographicSize = 5;

            //var defChunk = sprite1.GetAtlasChunk();
            //defChunk.Pivot = new GlmNet.vec2(0.5f, 0);
            //sprite1.Texture.Atlas.UpdateChunk(0, defChunk);

            var actor = new Actor<SpriteRenderer, RotateTest, RigidBody2D, BoxCollider2D, CollisionTest>("Actor1");
            actor.GetComponent<RigidBody2D>().BodyType = Body2DType.Kinematic;
            actor.GetComponent<SpriteRenderer>().Material = mat1;
            actor.GetComponent<SpriteRenderer>().Sprite = sprite1;
            actor.GetComponent<SpriteRenderer>().SortOrder = 2;

            // actor.GetComponent<SpriteRenderer>().Color = new Color(0, 1, 0, 1);
            actor.Transform.WorldPosition = new GlmNet.vec3(2, 0, 0);

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

            var actor3 = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D, PlayerTest>("Player");
            actor3.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
            //actor3.GetComponent<SpriteRenderer>().SortOrder = 1;
            actor3.GetComponent<SpriteRenderer>().Sprite = sprite3;
            var collider3 = actor3.GetComponent<Collider2D>();
                     var rigid3 = actor3.Transform.GetComponent<RigidBody2D>();
            rigid3.Transform.WorldEulerAngles = new GlmNet.vec3(0, 0, 42);
            rigid3.Transform.WorldPosition = new GlmNet.vec3(.0f, 0, 0);
            camera.Transform.WorldPosition = new GlmNet.vec3(actor3.Transform.WorldPosition.x,
                                                                actor3.Transform.WorldPosition.y, -12);
            // rigid3.Actor.IsEnabled = false;
           
            rigid3.IsAutoMass = false;
            // Actor.Destroy(camera.Actor);
            var cam = Actor.Find("Camera");

            if (camera)
            {
                camera.GetComponent<CameraFollow>().Target = actor3.Transform;
            }


            var actor4 = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D, PolygonCollider2D, CollisionTest>("Floor");


            var rigid4 = actor4.GetComponent<RigidBody2D>();
            var boxCollider = actor4.GetComponent<BoxCollider2D>();

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
