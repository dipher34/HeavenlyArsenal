using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using HeavenlyArsenal.Content.Buffs.LifeAndCessation;

namespace HeavenlyArsenal.Content.Buffs
{
    class ThermalShock : ModBuff
    {

        public override string Texture => "HeavenlyArsenal/Content/Buffs/AntishadowAssassinBuff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.HasBuff(ModContent.BuffType<ColdBurnBuff>()) && npc.HasBuff(ModContent.BuffType<HeatBurnBuff>()))
            {
                npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<ColdBurnBuff>()));
                npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<HeatBurnBuff>()));
                
            }
            npc.takenDamageMultiplier = 1.75f;
        }
    }
}
