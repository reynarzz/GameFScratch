
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
            camera.Transform.WorldPosition = new GlmNet.vec3(0, 0, -12);

            //var defChunk = sprite1.GetAtlasChunk();
            //defChunk.Pivot = new GlmNet.vec2(0.5f, 0);
            //sprite1.Texture.Atlas.UpdateChunk(0, defChunk);

            var actor = new Actor<SpriteRenderer, RotateTest>("Actor1");
            actor.GetComponent<SpriteRenderer>().Material = mat1;
            actor.GetComponent<SpriteRenderer>().Sprite = sprite1;
            actor.GetComponent<SpriteRenderer>().SortOrder = 2;

            // actor.GetComponent<SpriteRenderer>().Color = new Color(0, 1, 0, 1);
            actor.Transform.WorldPosition = new GlmNet.vec3(2, 0, 0);

            // (int i = 0; i < 33; i++)
            {
                var actor2 = new Actor<SpriteRenderer>("Actor2");
                actor2.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
                //actor2.GetComponent<SpriteRenderer>().SortOrder = 3;
                actor2.GetComponent<SpriteRenderer>().Sprite = sprite2;
                actor2.Transform.WorldPosition = new GlmNet.vec3(-2, 0, 0);
                //actor2.Transform.Parent = actor.Transform;
                actor2.Transform.LocalScale = new GlmNet.vec3(1, 1, 0);
            }


            var actor3 = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D, PlayerTest>("Player");
            actor3.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
            //actor3.GetComponent<SpriteRenderer>().SortOrder = 1;
            actor3.GetComponent<SpriteRenderer>().Sprite = sprite3;
            var collider3 = actor3.GetComponent<Collider2D>();
            var rigid3 = actor3.Transform.GetComponent<RigidBody2D>();
            collider3.Friction = 0.1f;
            rigid3.WorldEulerAngles = new GlmNet.vec3(0, 0, 42);
            // rigid3.WorldPosition = new GlmNet.vec3(-1.0f, 0, 0);
            rigid3.IsAutoMass = false;
            rigid3.Mass = 1;


            camera.GetComponent<CameraFollow>().Target = actor3.Transform;
            var actor4 = new Actor<SpriteRenderer, RigidBody2D, BoxCollider2D>("Actor3");
            var boxCollider = actor4.GetComponent<BoxCollider2D>();
            boxCollider.Size = new GlmNet.vec2(3, 1);
           
            var rigid4 = actor4.GetComponent<RigidBody2D>();
            rigid4.BodyType = Body2DType.Kinematic;
            rigid4.WorldPosition = new GlmNet.vec3(0, -4, 0);
           // rigid4.WorldEulerAngles = new GlmNet.vec3(0, 0, 45);
            actor4.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
            //actor3.GetComponent<SpriteRenderer>().SortOrder = 1;
            actor4.GetComponent<SpriteRenderer>().Sprite = sprite2;
            actor4.Transform.LocalScale = new GlmNet.vec3(3, 1, 1);

            Log.Success("Game Layer");
        }

        public override void Close()
        {
        }
    }
}
