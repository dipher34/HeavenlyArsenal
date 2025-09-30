using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    
    public class BloodwhipBuff : ModBuff
    {
        public static readonly int TagDamage = 15;

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
