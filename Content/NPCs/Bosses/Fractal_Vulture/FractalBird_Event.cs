using CalamityMod;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    internal class FractalBird_Event : ModSystem
    {
        

        public override void PostUpdateEverything()
        {

            if (DownedBossSystem.downedYharon) 
            {
                //Main.NewText(Main.GameUpdateCount);
                if (Main.GameUpdateCount % 300 == 0 && Main.rand.NextBool(5) && voidVulture.Myself is null)
                {
                    if (FakeFlowerRender.renderSystems.Count < 1)
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Scream).WithVolumeBoost(10);
                        FakeFlowerPlacementSystem.TryPlaceFlowerAroundGenesis();

                    }
                }
            }
        }
    }
}
