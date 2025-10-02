using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class ParentTests : ScriptBehavior
    {
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                // Actor.IsActiveSelf = false;

                //for (int i = 0; i < Transform.Children.Count; i++)
                //{
                //    Transform.Children[i].Parent = null;
                //}
            }

        }
    }
}
