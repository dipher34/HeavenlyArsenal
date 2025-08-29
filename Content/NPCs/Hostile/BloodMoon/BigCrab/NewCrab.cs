using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Dust = Terraria.Dust;
using NoxusBoss.Core.Physics.InverseKinematics;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
  
    public class HemoCrab : BloodmoonBaseNPC
    {
        public override float buffPrio = 4;
        public override bool canBeSacrificed => false;
        public override int bloodBankMax => 10_000;

       
        public enum HemocrabAI
        {
            //me when i stride
            Traverse,


            //Ranged shenanigans
            CheckAmmo,
            Restock,
            LocateBombardPosition,

            FireAppocalypseCannon,

            //close range shit
            
            
        }

        public HemocrabAI CurrentState = HemocrabAI.Idle;
        public float BombardRange = 1000f;
        
        public ref float Time => ref NPC.ai[0];
        public int AmmoCount
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        
        
        

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 55;
            NPC.damage = 200;
            NPC.defense = 130/2;
            NPC.lifeMax = 38470;
            NPC.value = 10000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3f;
            NPC.knockBackResist = 0f;
            
        }

        public override void AI()
        {
            
        }
        
        private void ManageLegs()
        {
          
        }
        private void GetGoreInfo(out Texture2D texture, out int goreID, int Variant)
        {
            texture = null;
            goreID = 0;
            if (Main.netMode != NetmodeID.Server)
            {             
                texture = BigCrabGores[Variant].Value;
                goreID = ModContent.Find<ModGore>(Mod.Name, $"CrabGore{Variant + 1}").Type;
            }
        }
        private void createGore(int i)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            //thanks lucille
            GetGoreInfo(out _, out int goreID, i);

            Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, goreID, NPC.scale);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                return base.Predraw(spritebatch, screenPos, drawColor);
            }

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;

            int frameHeight = texture.Height / totalFrameCount;
            Vector2 origin = new Vector2(texture.Width / 2f,  frameHeight-30);
            
            SpriteEffects Direction = NPC.direction < 0 ? SpriteEffects.FlipHorizontally : 0;

            Rectangle CrabFrame = new Rectangle(0, BodyFrame * frameHeight, texture.Width, frameHeight);

            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, CrabFrame, drawColor, 0, origin, NPC.scale, Direction, 0);
            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && DownedBossSystem.downedProvidence)
                return SpawnCondition.OverworldNightMonster.Chance * 0.01f;
            return 0f;
        }
    }

   }
