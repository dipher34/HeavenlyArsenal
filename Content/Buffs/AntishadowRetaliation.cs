using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Buffs
{
    public class AntishadowRetaliation : ModBuff
    {
        public override void SetStaticDefaults()
        {
            
            
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage<GenericDamageClass>() += 0.5f;
        }
    }
}
