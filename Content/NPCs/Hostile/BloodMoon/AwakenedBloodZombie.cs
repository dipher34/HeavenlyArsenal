using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    internal class AwakenedBloodZombie : BloodmoonBaseNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.ReflectStarShotsInForTheWorthy[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Fighter;
        }

        public override void AI()
        {
            
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                return base.PreDraw(spriteBatch, screenPos, drawColor);
            }
            return false;
        }


    }
}
