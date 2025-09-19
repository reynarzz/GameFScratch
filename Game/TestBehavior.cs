
using Engine;

namespace Game
{
    public class TestBehavior : ScriptBehavior
    {
        public override void OnAwake()
        {
            var actor = new Actor();
            actor.GetComponent<TestBehavior>();
            Actor.Destroy(default(Component));
        }

    }
}
