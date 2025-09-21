
using Engine;
using Engine.Layers;

namespace Game
{
    public class GameApplication : ApplicationLayer
    {
        public override string GameName => "Game";

        public override void Initialize()
        {
            var actor = new Actor<Camera, SpriteRenderer>();
            actor.GetComponent<Camera>().BackgroundColor = new GlmSharp.vec4(1, 1, 0, 1);
            actor.GetComponent<SpriteRenderer>().Material = new Material();

            var actor2 = new Actor<SpriteRenderer>();
            actor2.GetComponent<SpriteRenderer>().Material = actor.GetComponent<SpriteRenderer>().Material;
            actor2.GetComponent<SpriteRenderer>().SortOrder = 1;
            Log.Success("Game Layer");
        }

        public override void Close()
        {
        }
    }
}
