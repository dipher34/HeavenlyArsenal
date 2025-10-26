using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    public class RitualSystem : ModSystem
    {
        public static HashSet<NPC> BuffedNPCs = new();

        public override void OnWorldUnload()
        {
            BuffedNPCs.Clear();
        }
    }

}
