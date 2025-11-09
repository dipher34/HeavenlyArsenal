using CalamityMod;
using CalamityMod.Items.Materials;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    partial class RitualAltar : BloodMoonBaseNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarConcept";

        float SpeedMulti = 1;
        public override float buffPrio => 0;
        public override bool canBeSacrificed => false;


        public int SacrificeCooldown;
        public override int bloodBankMax => 100;

        public bool isSacrificing = false;
        private int Variant;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;

            ContentSamples.NpcBestiaryRarityStars[Type] = 0;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.rotation = -MathHelper.PiOver2;
            CreateLimbs();
            CultistCoordinator.CreateNewCult(NPC, Main.rand.Next(5, 8));
            for(int i = 0; i< Main.rand.Next(2,5); i++)
            {
                float thing = 1;
                if (i % 2 == 0)
                    thing = -1;
                Vector2 offset = new Vector2(10*thing*i, 0);
                NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<FleshlingCultist.FleshlingCultist>());
            }
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

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
            bestiaryEntry.Info.AddRange([
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Visuals.EclipseSun,
                
				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement($"Mods.{Mod.Name}.Bestiary.RitualAltar1"),
                
				// By default the last added IBestiaryBackgroundImagePathAndColorProvider will be used to show the background image.
				// The ExampleSurfaceBiome ModBiomeBestiaryInfoElement is automatically populated into bestiaryEntry.Info prior to this method being called
				// so we use this line to tell the game to prioritize a specific InfoElement for sourcing the background image.
				//new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ExampleSurfaceBiome>().ModBiomeBestiaryInfoElement),
            ]);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<PenumbralMembrane>(), 3, 10));
            npcLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<RitualAltar>()));

            npcLoot.Add(ModContent.ItemType<BloodOrb>(), 1, 10, 18);
        }


        public override void SetDefaults()
        {
            Variant = Main.rand.Next(1, 5);
            NPC.width = 80;
            NPC.height = 80;
            NPC.lifeMax = 350_000;
            NPC.damage = 300;
            NPC.defense = 120;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            SpawnModBiomes =
            [
               ModContent.GetInstance<RiftEclipseBloodMoon>().Type
            ];
        }

        public override void AI()
        {
            
           // NPC.Center = Main.MouseWorld;
            
            NPC.velocity.X = 0;
            locateSacrifices();

            StateMachine();

            SpeedMulti = Math.Abs(MathF.Sin(Time + NPC.whoAmI) * 2);
            NPC.direction = Math.Sign(NPC.velocity.X.NonZeroSign()) != 0 ? Math.Sign(NPC.velocity.X) : 1;
            if (isSacrificing && (NPCTarget == null || !NPCTarget.active))
                isSacrificing = false;
            if (NPCTarget == null || !NPCTarget.active)
                NPCTarget = default;


            //NPC.Center = Main.LocalPlayer.Calamity().mouseWorld;
            Time++;
        }


        void balanceHead(float interp = 0.2f)
        {
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

            NPC.rotation = NPC.rotation.AngleLerp(d.AngleTo(NPC.Center), interp);
        }
        public override void PostAI()
        {
            balanceHead(0.01f);
            UpdateList();
            UpdateLimbMotion();

            UpdateGravity();


            if (NPCTarget != null)
            {
                currentTarget = NPCTarget;
            }
            else
            if (playerTarget != null)
                currentTarget = playerTarget;

        }
    }



}
