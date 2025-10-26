using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    internal class WormCoordinator : ModSystem
    {
        public static Dictionary<int, List<NPC>> leechGroups = new Dictionary<int, List<NPC>>();

        static void PerformSyncing()
        {
            if(Main.netMode == NetmodeID.Server)
                return;

            foreach (var group in leechGroups)
            {
                group.Value.RemoveAll(NPC => NPC == null || !NPC.active);
                //sort by segment num so that heads are first
                group.Value.Sort((a, b) =>
                {
                    float aPrio = a.ModNPC is UmbralLeech leech ? leech.SegmentNum : 0f;    
                    float bPrio = b.ModNPC is UmbralLeech leech2 ? leech2.SegmentNum : 0f;
                    return bPrio.CompareTo(aPrio);
                });

            }
        }
        public override void PostUpdateNPCs()
        {
            if(Main.GlobalTimeWrappedHourly % 60 == 0 && leechGroups.Values.Count>0)
                PerformSyncing();
        }
    }
}
