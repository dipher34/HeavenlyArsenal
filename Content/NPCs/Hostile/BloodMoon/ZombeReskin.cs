using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    class ZombeReskin : ModNPC
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Zombe Reskin");
            Main.npcFrameCount[NPC.type] = 3; // Set the number of frames for the NPC
        }
        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 220;
            NPC.defense = 5;
            NPC.lifeMax = 50;
            NPC.value = 100f;
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle = 3; // This is a zombie AI
            
        }
    }
}
