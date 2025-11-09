using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    internal class JellyBloom : BloodMoonBaseNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.NeverDropsResourcePickups[Type] = true;
            this.ExcludeFromBestiary();
        }
        public int GrowthStage
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        public override void SetDefaults()
        {
            NPC.damage = 0;
            NPC.value = 0;
            
            NPC.lifeMax = 3;
            NPC.dontTakeDamage = true;
            NPC.ShowNameOnHover = false;

        }
        readonly int stage1Time = 60 * 10;
        readonly int stage2Time = 60 * 20;
        readonly int stage3Time = 60 * 30;

        public override void AI()
        {
            if(Time< stage1Time)
            {

            }

            if(Time> stage3Time)
            {

            }

            Time++;

            
        }

    }
   
}
