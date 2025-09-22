
using Engine;
using Engine.Layers;

namespace Game
{
    public class GameApplication : ApplicationLayer
    {
        public override string GameName => "Game";

        public override void Initialize()
        {
            var sprite1 = new Sprite();
            sprite1.Texture = new Texture2D();

            var sprite2 = new Sprite();
            sprite2.Texture = new Texture2D();

            var sprite3 = new Sprite();
            sprite3.Texture = new Texture2D();


            var actor = new Actor<Camera, SpriteRenderer>("Actor1");
            actor.GetComponent<Camera>().BackgroundColor = new GlmSharp.vec4(1, 1, 0, 1);
            actor.GetComponent<SpriteRenderer>().Material = new Material();
            actor.GetComponent<SpriteRenderer>().Sprite = sprite1;
            
            var actor2 = new Actor<SpriteRenderer>("Actor2");
            actor2.GetComponent<SpriteRenderer>().Material = new Material();// actor.GetComponent<SpriteRenderer>().Material;
            actor2.GetComponent<SpriteRenderer>().SortOrder = 3;
            actor2.GetComponent<SpriteRenderer>().Sprite = sprite2;


            var actor3 = new Actor<SpriteRenderer>("Actor3");
            actor3.GetComponent<SpriteRenderer>().Material = actor2.GetComponent<SpriteRenderer>().Material;
            actor3.GetComponent<SpriteRenderer>().SortOrder = 1;
            actor3.GetComponent<SpriteRenderer>().Sprite = sprite3;

            Log.Success("Game Layer");
        }

        public override void Close()
        {
        }
    }
}
