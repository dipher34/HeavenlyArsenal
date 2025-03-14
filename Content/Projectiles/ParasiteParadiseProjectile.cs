using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;
using rail;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using NoxusBoss.Content.Buffs;
using CalamityMod.Buffs.DamageOverTime;
using Luminance.Core.Graphics;
using static NoxusBoss.Assets.GennedAssets.Textures;
using static NoxusBoss.Assets.GennedAssets;

namespace HeavenlyArsenal.Content.Projectiles
{
    internal class ParasiteParadiseProjectile : ModProjectile, IPixelatedPrimitiveRenderer
    {

        public static float SmoothStep(float edge0, float edge1, float value)
        {
            // Clamp the input value to the range [0, 1]
            value = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);

            // Perform the smoothstep interpolation
            return value * value * (3f - 2f * value);
        }

        public float BloodWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width * 0.66f;
            float smoothTipCutoff = SmoothStep(0f, 1f, MathHelper.Lerp(0.09f, 0.3f, completionRatio));
            return smoothTipCutoff * baseWidth;
        }

        public Color BloodColorFunction(float completionRatio)
        {
            return Projectile.GetAlpha(new Color(82, 1, 23));
        }


        public override void SetStaticDefaults()
        {
            //Main.projFrames[Projectile.type] = 35;
        }

        public override void SetDefaults()
        {
            
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            //Projectile.ApplyStatsFromSource = true;

            Projectile.usesLocalNPCImmunity = true;
            //Projectile.localNPCHitCooldown = -1; // 1 hit per npc max
            Projectile.localNPCHitCooldown =1; // 20 ticks before the same npc can be hit again


            AIType = ProjectileID.Bullet;
            Projectile.idStaticNPCHitCooldown = 8;
           
        }
        public override void AI()
        {
         
           Projectile.aiStyle = 0;

            // Lighting.AddLight(Projectile.position, 0.2f, 0.2f, 0.6f);
            // Lighting.Brightness(1, 1);
            Terraria.Dust.NewDustPerfect(Projectile.Left, DustID.AncientLight, Vector2.Zero,150,Color.AntiqueWhite,1);
            //Terraria.Dust.NewDustPerfect(Projectile.position,
                //+ new Vector2(Projectile.velocity.X, 0f), 
             //   DustID.AncientLight, Projectile.velocity, 150, default, 1f);   //spawns dust behind it, this is a spectral light blue dust

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity *= 1.01f;


            //if (++Projectile.frameCounter >= 1)
            //{
             //   Projectile.frameCounter = 0;
              //  // Or more compactly Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
               // if (++Projectile.frame >= Main.projFrames[Projectile.type])
               //     Projectile.frame = 0;   
           // }



        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), // Adding Poisoned to target
                 600); // for 5 seconds (60 ticks = 1 second)
        }



        public override void OnSpawn(IEntitySource source)
        {
            //Projectile.rotation += 
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
            //for (var i = 0; i < 6; i++)
            //{
            //    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight, 0f, 0f, 100, default(Color), 1f);
            //}
        }



        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // Redraw the projectile with the color not influenced by light
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            
            return true;
        }



        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            //sets up space to do shader in 
            Rectangle viewBox = Projectile.Hitbox;
            Rectangle screenBox = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            viewBox.Inflate(540, 540);
            if (!viewBox.Intersects(screenBox))
                return; 

            float lifetimeRatio = 1/ 240f;
            float dissolveThreshold = MathHelper.Lerp(0.67f, 1f, lifetimeRatio) * 0.5f;
            ManagedShader bloodShader = ShaderManager.GetShader("NoxusBoss.BloodBlobShader");
            //initializes shader


            bloodShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 72.113f);
            bloodShader.TrySetParameter("dissolveThreshold", dissolveThreshold);
            bloodShader.TrySetParameter("accentColor", new Vector4(0.6f, 0.02f, -0.1f, 0f));
            //setting shader values
            
            Texture2D Silly = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/BubblyNoise").Value;
            bloodShader.SetTexture(Silly, 1, SamplerState.LinearWrap);
            Texture2D Silly2 = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/DendriticNoiseZoomedOut").Value;
            bloodShader.SetTexture(Silly2, 2, SamplerState.LinearWrap);
            //get textures



            //black magic
            PrimitiveSettings settings = new PrimitiveSettings(BloodWidthFunction, BloodColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.56f, Pixelate: true, Shader: bloodShader);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 9);
        }





    }
}
