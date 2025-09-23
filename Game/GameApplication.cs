
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

            var camera = new Actor<Camera>("Camera").GetComponent<Camera>();
            camera.BackgroundColor = new GlmNet.vec4(0.2f, 0.2f, 0.2f, 1);
            camera.OrthographicSize = 5;
            camera.Transform.WorldPosition = new GlmNet.vec3(0, 0, -12);

            //var defChunk = sprite1.GetAtlasChunk();
            //defChunk.Pivot = new GlmNet.vec2(0.5f, 0);
            //sprite1.Texture.Atlas.UpdateChunk(0, defChunk);

            var actor = new Actor<SpriteRenderer, RotateTest>("Actor1");
            actor.GetComponent<SpriteRenderer>().Material = mat1;
            actor.GetComponent<SpriteRenderer>().Sprite = sprite1;

            //actor.GetComponent<SpriteRenderer>().Color = new Color(0, 1, 0, 1);
            actor.Transform.WorldPosition = new GlmNet.vec3(2, 0, 0);

            for (int i = 0; i < 33; i++)
            {
                var actor2 = new Actor<SpriteRenderer>("Actor2");
                actor2.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
                //actor2.GetComponent<SpriteRenderer>().SortOrder = 3;
                actor2.GetComponent<SpriteRenderer>().Sprite = sprite2;
                actor2.Transform.WorldPosition = new GlmNet.vec3(-2, 0, 0);
                actor2.Transform.Parent = actor.Transform;
                actor2.Transform.LocalScale = new GlmNet.vec3(1, 1, 0);
            }

          
            var actor3 = new Actor<SpriteRenderer>("Actor3");
            actor3.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
            //actor3.GetComponent<SpriteRenderer>().SortOrder = 1;
            actor3.GetComponent<SpriteRenderer>().Sprite = sprite3;
            actor3.Transform.WorldPosition = new GlmNet.vec3(0, 0, 0);
            actor3.Transform.LocalEulerAngles = new GlmNet.vec3(0, 0, 45);
            Log.Success("Game Layer");
        }

        public override void Close()
        {
        }
    }
}
