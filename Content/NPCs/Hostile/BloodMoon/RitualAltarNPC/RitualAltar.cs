using CalamityMod;
using CalamityMod.Items.Materials;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    partial class RitualAltar : BloodmoonBaseNPC
    {
        public ref float Time => ref NPC.ai[0];
        float SpeedMulti = 1;
        public override float buffPrio => 0;
        public override bool canBeSacrificed => false;

        public override int bloodBankMax => 100;

        public bool isSacrificing = false;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            CreateLimbs();
            CultistCoordinator.CreateNewCult(NPC, Main.rand.Next(5, 8));
        }
        public enum AltarAI
        {
            LookingForSacrifice,
            Sacrificing,

            lookForBuffTargets,
            Buffing,

            WalkTowardsPlayer
        }
        public AltarAI currentAIState = AltarAI.LookingForSacrifice;



        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<PenumbralMembrane>(), 3, 10));
            npcLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<RitualAltar>()));

            npcLoot.Add(ModContent.ItemType<BloodOrb>(), 1, 10, 18);
        }


        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 80;
            NPC.lifeMax = 350_000;
            NPC.damage = 300;
            NPC.defense = 120;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            
        }
       
        public override void AI()
        {
            NPC.velocity.X = 0;

            locateSacrifices();
            StateMachine();
            UpdateList();
            UpdateLimbMotion();
            Vector2 d = Vector2.Zero;
            for (int i = 0; i < _limbs.Count(); i++)
            {

                d += _limbs[i].EndPosition;
                if (_limbs[i] == default)
                {
                    CreateLimbs();
                }
            }
            d /= 4;
            SpeedMulti = Math.Abs(MathF.Sin(Time+ NPC.whoAmI)*2);
            NPC.rotation = NPC.rotation.AngleLerp(d.AngleTo(NPC.Center), 0.2f);
            NPC.direction = Math.Sign(NPC.velocity.X) != 0 ? Math.Sign(NPC.velocity.X) : 1;
            //NPC.velocity.X = NPC.AngleTo(Main.LocalPlayer.Center).ToRotationVector2().X * 10 * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));

            updateGravity();


            Time++;
        }

    }


    
}
