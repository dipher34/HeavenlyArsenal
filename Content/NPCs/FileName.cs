using Luminance.Assets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs
{
    public class BreadDeityBoss : ModNPC
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4; // assume 4 frames of animation
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 200;
            NPC.height = 200;
            NPC.damage = 120;
            NPC.defense = 40;
            NPC.lifeMax = 500000; // high health, like a Deity-boss tier
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = Item.buyPrice(0, 50, 0, 0);
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1; // custom AI
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            Music = MusicID.Boss5; // choose an epic track
            NPC.netAlways = true;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                if (!player.active || player.dead)
                {
                    NPC.velocity.Y -= 0.1f;
                    if (NPC.timeLeft > 10)
                        NPC.timeLeft = 10;
                    return;
                }
            }

            Vector2 direction = player.Center - NPC.Center;
            direction.Normalize();
            float speed = 10f;
            NPC.velocity = (NPC.velocity * 30f + direction * speed) / 31f;

            // phase switch example
            if (NPC.life < NPC.lifeMax * 0.5f)
            {
                DoSecondPhase();
            }
            else
            {
                DoFirstPhase();
            }
        }

        private void DoFirstPhase()
        {
            // simple projectile attack
            NPC.ai[0]++;
            if (NPC.ai[0] > 120f)
            {
                NPC.ai[0] = 0f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 shootVel = (Main.player[NPC.target].Center - NPC.Center);
                    shootVel.Normalize();
                    shootVel *= 15f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, ProjectileID.AncientDoomProjectile, 90, 10f, Main.myPlayer);
                }
            }
        }

        private void DoSecondPhase()
        {
            // more aggressive – larger damage, more projectiles, maybe teleport
            NPC.ai[1]++;
            if (NPC.ai[1] > 60f)
            {
                NPC.ai[1] = 0f;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 shootVel = (Main.player[NPC.target].Center - NPC.Center);
                        shootVel = shootVel.RotatedBy(MathHelper.ToRadians(120 * i));
                        shootVel.Normalize();
                        shootVel *= 18f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, ProjectileID.CrystalBullet, 120, 12f, Main.myPlayer);
                    }
                }
            }

            // maybe increase speed
            float speed = 14f;
            Vector2 direction = Main.player[NPC.target].Center - NPC.Center;
            direction.Normalize();
            NPC.velocity = (NPC.velocity * 40f + direction * speed) / 41f;
        }

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // drop special loot
                //Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<Items.BreadTrophy>());
            }
            base.OnKill();
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter < 10)
                NPC.frame.Y = 0 * frameHeight;
            else if (NPC.frameCounter < 20)
                NPC.frame.Y = 1 * frameHeight;
            else if (NPC.frameCounter < 30)
                NPC.frame.Y = 2 * frameHeight;
            else if (NPC.frameCounter < 40)
                NPC.frame.Y = 3 * frameHeight;
            else
                NPC.frameCounter = 0;
        }

        // Optional: override BossHeadSprite and such to show bread profile picture on map/UI
        public override string BossHeadTexture => "HeavenlyArsenal/Assets/Textures/Extra/PerlinNoise";
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.2f; // bigger health bar
            return null;
        }
    }
}
