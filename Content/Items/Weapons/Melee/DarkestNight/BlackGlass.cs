using CalamityMod.Particles;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using Microsoft.Build.ObjectModelRemoting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight
{
    public class BlackGlass : ModProjectile
    {
        public Color GlowColor;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        public override string GlowTexture => "HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.Size = new Vector2(10, 10);
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
        }
     
       

        public override void AI()
        {

            float TrackStrength = float.Lerp(0.09f, 0, (float)Math.Clamp(Time / 60, 0, 1));
            NPC target = Projectile.FindTargetWithinRange(500);
            if (target != null && Time > 10) 
            {
                
                float loc = Projectile.Center.AngleTo(target.Center);
                Projectile.rotation = Projectile.rotation.AngleLerp(loc, TrackStrength);
            }
            
            
            Vector2 Offset = new Vector2(0, (float)Math.Cos(Time)/10).RotatedBy(Projectile.rotation)*10;
            Vector2 desiredVel = Projectile.rotation.ToRotationVector2() * 17;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel + Offset, 0.5f);
            if (Time > 60)
                ExplodeIntoLight();
            Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for(int i = 1; i< 8; i++)
            {
                BlackGlassFragment particle = BlackGlassFragment.pool.RequestParticle();

                Vector2 AdjustedPos = Projectile.Center + new Vector2(-20, 0).RotatedBy(Projectile.rotation);

                Vector2 AdjustedVelocity = Projectile.velocity + new Vector2(Main.rand.NextFloat(-1, 30), Main.rand.NextFloat(-20, 20)).RotatedBy(Projectile.rotation);
                float rotation = Projectile.rotation + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20));
                float Scale = 1;

                particle.Prepare(Projectile.Center, AdjustedVelocity, rotation, 120, GlowColor, 1, 0, i);
                ParticleEngine.ShaderParticles.Add(particle);
            }
           
        }

        public void ExplodeIntoLight()
        {
            //Vector2 Scale = new Vector2(10, 10);
            // Particle a = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.AntiqueWhite, Vector2.One, 0, 0.1f, 0, 60, true);
            //Particle a = new CrackParticle(Projectile.Center, Vector2.Zero, GlowColor, Scale, MathHelper.ToRadians(Main.rand.NextFloat(0, 360)), 1, 3, 30);
            //Particle b = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, GlowColor, Vector2.One, 0, 1, 10, 30);

            //GeneralParticleHandler.SpawnParticle(a);

            // GeneralParticleHandler.SpawnParticle(b);
            LightFlash particle = LightFlash.pool.RequestParticle();
            particle.Prepare(Projectile.Center, Vector2.Zero, Projectile.rotation + MathHelper.ToRadians(Main.rand.Next(60)), 60, Main.rand.NextFloat(0.1f,0.4f), GlowColor);
          
           
            ParticleEngine.ShaderParticles.Add(particle);
            Projectile.velocity = Vector2.Zero;
            ScreenShakeSystem.StartShake(3);
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle with { MaxInstances = 16 , PitchVariance = 0.5f});
            //var d = Projectile.NewProjectile();
            Projectile d = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlindingLight>(), Owner.HeldItem.damage / 5, 0);
            d.rotation = Projectile.rotation;
            Projectile.Kill();
        }

        private Color TrailColorFunction(float p)
        {
            // Cycle hue over [0, 360), scaled by p
            // You can also multiply p to make the cycle repeat faster/slower
            //float hue = (p * 360f) % 360f;
            float hue = (p * 360f + Main.GlobalTimeWrappedHourly * 120f) % 360f;

            return HsvToColor(hue, 0.75f, 0.5f, 0);
        }
        private Color HsvToColor(float h, float s, float v, byte alpha = 255)
        {
            int hi = (int)(h / 60f) % 6;
            float f = h / 60f - MathF.Floor(h / 60f);

            v = v * 255f;
            int vi = (int)v;
            int p = (int)(v * (1f - s));
            int q = (int)(v * (1f - f * s));
            int t = (int)(v * (1f - (1f - f) * s));

            return hi switch
            {
                0 => new Color(vi, t, p, alpha),
                1 => new Color(q, vi, p, alpha),
                2 => new Color(p, vi, t, alpha),
                3 => new Color(p, q, vi, alpha),
                4 => new Color(t, p, vi, alpha),
                _ => new Color(vi, p, q, alpha),
            };
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Base = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D Glow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow").Value;
            Texture2D Glow2 = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 Origin = Base.Size() * 0.5f;
            Vector2 Grigin = Glow.Size() * 0.5f;
            float Rot = Projectile.rotation + MathHelper.PiOver2;

            Vector2 Scale = new Vector2(0.6f);
            Vector2 GlowScale = new Vector2(0.6f, 1 )*0.75f;

            SpriteEffects flip = Projectile.direction == 1? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            DrawAfterImages(ref lightColor);

            float GlowMulti = float.Lerp(0, 1f, (float)Math.Clamp(Time / 20, 0, 1));


            GlowColor = TrailColorFunction((float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly + Projectile.whoAmI * 10)));

            Main.EntitySpriteDraw(Glow2, DrawPos, null, GlowColor with { A = 0 } * GlowMulti, Rot, Glow2.Size()*0.5f, GlowScale, flip);


            Main.EntitySpriteDraw(Base, DrawPos, null, Color.AntiqueWhite, Rot, Origin, Scale, flip);

            //Utils.DrawBorderString(Main.spriteBatch, Time.ToString(), DrawPos, Color.AntiqueWhite);
            return false;
        }

        public void DrawAfterImages(ref Color lightColor)
        {
            Texture2D Base = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 Origin = Base.Size() * 0.5f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 DrawPos = (Projectile.oldPos[i] + Projectile.Center)/2 - Main.screenPosition;

                float Rot = Projectile.oldRot[i] + MathHelper.PiOver2;

                Vector2 Scale = new Vector2(Math.Clamp(((Projectile.oldPos.Length - i) / 10f)*2, 0.1f, 2));
                Main.EntitySpriteDraw(Base, DrawPos, null, lightColor, Rot, Origin, Scale, SpriteEffects.None);
            }

            
        }
    }
}
