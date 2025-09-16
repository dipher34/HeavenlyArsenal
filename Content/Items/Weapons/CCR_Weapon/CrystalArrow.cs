using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon
{
    internal class CrystalArrow : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Timer => ref Projectile.ai[0];
        public int StuckID
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public int Charge
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.Size = new Vector2(30, 30);
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 2;
            StuckID = -1;
            Projectile.timeLeft = 1800;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if(StuckID != 0)
            {
                StickAndExhume();
            }
            Timer++;
        }

        public void StickAndExhume()
        {
            NPC victim = Main.npc[StuckID];

            if (victim.active)
            {

                Projectile.Center = victim.Center;

                int CrystalAmount = Charge > 4 ? (int)Charge / 2 : (int)Charge;
                float spawnHeight = 600f;
                float horizSpread = 200f;
                int delayPerCrystal = 3;  // ticks between each

                for (int i = 0; i < CrystalAmount; i++)
                {
                    // random X offset
                    float offsetX = Main.rand.NextFloat(-horizSpread, horizSpread);
                    Vector2 spawnPos = new Vector2(victim.Center.X + offsetX, victim.Center.Y - spawnHeight);


                    Vector2 aimDir = (victim.Center - spawnPos).SafeNormalize(Vector2.UnitY);
                    aimDir = Vector2.Lerp(Vector2.UnitY, aimDir, 0.5f);
                    Vector2 projVel = aimDir * Owner.HeldItem.shootSpeed;


                    float startDelay = i * delayPerCrystal;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        projVel,
                        ModContent.ProjectileType<EntropicCrystal>(),
                        Owner.HeldItem.damage,
                        Owner.HeldItem.knockBack,
                        Owner.whoAmI,
                        ai0: startDelay,
                        ai1: 0f
                    );

                    
                }
                
            }
            if (Timer > 360 || !victim.active)
                ShatterArrow();
        }
        public void ShatterArrow()
        {
            Projectile.Kill();

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            StuckID = target.whoAmI;
            Projectile.damage = -1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if(Projectile.isAPreviewDummy)
                return base.PreDraw(ref lightColor);
            Texture2D Arrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow").Value;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            Vector2 Origin = new Vector2(Arrow.Width, Arrow.Height / 2);

            float Rot = Projectile.rotation;
            float Scale = Projectile.scale;
            Main.EntitySpriteDraw(Arrow, DrawPos, null, lightColor, Rot, Origin, Scale, SpriteEffects.None);
            return false;
        }
    }
}
