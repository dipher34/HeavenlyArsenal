using CalamityMod.Items.Placeables.FurnitureStatigel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.ShieldGuy
{
    internal class ShieldNPC : GlobalNPC
    {
        #region setup
        public bool active;
        public int ShieldHitsRemaining;
        public int ShieldHitCooldown;


        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            if (entity.friendly)
                return false;
            else
                return true && lateInstantiation;
        }
        #endregion

        public override void PostAI(NPC npc)
        {
            base.PostAI(npc);
        }

        public void ApplyShieldToNPC(NPC npc)
        {
            if (npc == null) return;

            active = true;
            
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (active && ShieldHitsRemaining > 0)
            {

            }
        }
    }
}
