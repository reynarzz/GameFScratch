
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
            Log.Success("Game Layer");
        }

        public override void Close()
        {
        }
    }
}
