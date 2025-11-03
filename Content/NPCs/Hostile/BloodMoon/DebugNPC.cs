using Luminance.Assets;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    internal class DebugNPC : BloodMoonBaseNPC
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override float buffPrio => 5;

        public override bool canBeSacrificed => false;

        public override int bloodBankMax => 5000;

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.friendly = false;
            NPC.lifeMax = 100_000;
            NPC.dontTakeDamageFromHostiles = false;
            NPC.Size = new Microsoft.Xna.Framework.Vector2(30, 60);
        }

    }
}
