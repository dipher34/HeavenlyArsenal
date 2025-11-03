using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    partial class BloodCrab : BloodmoonBaseNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/ArtillerCrab";
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
				// Sets the preferred biomes of this town NPC listed in the bestiary.
				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
				new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.ArtilleryCrab1")
            ]);
        }
        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 55;
            NPC.damage = 200;
            NPC.defense = 130 / 2;
            NPC.lifeMax = 38470;
            NPC.value = 10000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3f;
            NPC.knockBackResist = 0f;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 13;
        }
    }
}
