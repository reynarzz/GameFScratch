using GameCooker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    internal class IOLayer : LayerBase
    {
        public override void Close()
        {
        }

        public override void Initialize()
        {
            var cooker = new AssetsCooker();

            async void Cook()
            {
                var result = await cooker.CookAll(new CookOptions() { Type = CookingType.Monolith},
                                                        "D:\\Projects\\GameScratch\\Game\\Assets", "D:\\Projects\\GameScratch\\Game\\Library\\AssetsDatabase");

                if (result)
                {
                    Debug.Success("Cook completed");
                }
            }

            Cook();
        }
    }
}
