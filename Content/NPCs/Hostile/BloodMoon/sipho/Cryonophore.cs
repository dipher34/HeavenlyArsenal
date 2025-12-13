using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho
{
    public enum ZooidType
    {
        basic,
        grabber,
        Ranged,
        Blizzard
    }
    public struct CryonophoreZooid
    {
        public int id;
        public ZooidType type;

        public CryonophoreZooid(int id, ZooidType type)
        {
            this.id = id;
            this.type = type;
        }
    }
    partial class Cryonophore : BloodMoonBaseNPC
    {
        #region setup
        /// <summary>
        /// dictionary used to keep track of each zooid this npc owns, as well as the npc that this zooid will represent once it is made.
        /// </summary>
        private Dictionary<int, (CryonophoreZooid, NPC)> OwnedZooids = new Dictionary<int, (CryonophoreZooid, NPC)>();
        public override float buffPrio => 0.4f;
        public override bool canBeSacrificed => false;
        public override bool canBebuffed => true;
        public override int bloodBankMax => 30;
        public override void SetDefaults()
        {
            NPC.damage = 30;
            NPC.lifeMax = 600_000;
            NPC.defense = 70;
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(30, 30);
            NPC.noGravity = true;
        }
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.MustAlwaysDraw[Type] = true;

        }
        public override void OnSpawn(IEntitySource source)
        {
            OwnedZooids = new Dictionary<int, (CryonophoreZooid, NPC)>(5);
            for (int i = 0; i < 6; i++)
            {
                ZooidType a = (ZooidType)Main.rand.Next(0, 4);
                addZoid(a);
            }
        }
        #endregion

        public override void AI()
        {
            if (NPC.ai[1] == 1)
            {
                NPC.Center = Main.MouseWorld;
                NPC.ai[1] = 0;
                return;
            }
           
            StateMachine();
        }
        #region helpers
        void addZoid(ZooidType type)
        {
            CryonophoreZooid a = new CryonophoreZooid(OwnedZooids.Count, type);
            OwnedZooids.Add(a.id, (a, null));
        }

        void SpawnZooid(int id)
        {
            if (!OwnedZooids.ContainsKey(id))
                return;

            if (OwnedZooids[id].Item2 != null)
                return;

            NPC placeholder = NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<CryonophoreLimb>());
            if (placeholder == null)
                return;

            CryonophoreLimb limb = placeholder.ModNPC as CryonophoreLimb;
            if (limb != null)
            {
                limb.self = OwnedZooids[id].Item1;
                limb.OwnerIndex = NPC.whoAmI;
            }

            // copy, modify, and reassign the value tuple to avoid CS1612
            var entry = OwnedZooids[id];
            entry.Item2 = placeholder;
            OwnedZooids[id] = entry;
        }

       
        #endregion
    }
}
