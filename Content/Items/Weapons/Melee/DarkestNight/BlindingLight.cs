using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight
{
    public class BlindingLight : ModProjectile, IPixelatedPrimitiveRenderer
    {
        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeProjectiles;

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public ref float Time => ref Projectile.ai[0];
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.Size = new Vector2(16, 16);
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.damage = 1;

        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }
        public override void OnSpawn(IEntitySource source)
        {

        }
        public override void AI()
        {
            Projectile.timeLeft++;
            NPC target = Projectile.FindTargetWithinRange(2500);
            float speedMulti = Time * 2 / 5;
            //Main.NewText(speedMulti);
            if (Time > 5)
            {
                if (target == null)
                {
                    Projectile.Kill();
                }
                else
                {
                    if (target.GetGlobalNPC<Collapse>().CollapseStage >= 3)
                    {
                        float value = 2500;
                        for (int i = 0; i < 200; i++)
                        {
                            NPC nPC = Main.npc[i];
                            if (nPC.CanBeChasedBy(this) && nPC.GetGlobalNPC<Collapse>().CollapseStage < 3)
                            {
                                float num2 = Projectile.Distance(nPC.Center);
                                if (!(value <= num2))
                                {
                                    value = num2;
                                    target = nPC;
                                }
                            }
                        }
                    }

                    Projectile.velocity = (target.Center - Projectile.Center) * (0.2f + Time / 100);
                    //Main.NewText(Projectile.velocity);
                    //Projectile.Center = Vector2.Lerp(Projectile.Center, target.Center, 0.02f);

                }
            }


            Time++;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!target.SuperArmor)
            {
                modifiers = modifiers with { SuperArmor = true };

            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            BlindingLight_hitVFX particle = BlindingLight_hitVFX.pool.RequestParticle();
            Vector2 AdjustedVelocity = Projectile.velocity * 10;
            Vector2 AdjustedPos = Projectile.Center + AdjustedVelocity;

           
            float rotation = Projectile.rotation + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20));
            float Scale = 1;

            particle.Prepare(Projectile.Center, AdjustedVelocity, rotation, 120, 1, Color.AntiqueWhite);
            ParticleEngine.ShaderParticles.Add(particle);
        }



        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D placeholder = GennedAssets.Textures.GreyscaleTextures.FourPointedStar;
            Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = placeholder.Size() * 0.5f;
            Vector2 Glorigin = Glow.Size() * 0.5f;

            SpriteEffects spriteEffects = SpriteEffects.None;

            Color a = Color.AntiqueWhite with { A = 0 };
            float thing = 0.1f * (float)Math.Cos((Main.GlobalTimeWrappedHourly % 5 + Projectile.whoAmI) * 15) + 0.1f;
            float Value = thing;
            //(float)Math.Clamp(Math.Abs(Math.Sin((Main.GlobalTimeWrappedHourly + Projectile.whoAmI) * 10)), 0.5f, 1) * 0.5f;
            float Rotation = Projectile.rotation;


            Vector2 TotalScale = new Vector2(1, 1) * Value;
            Main.EntitySpriteDraw(placeholder, DrawPos, null, a, Rotation, TexOrigin, TotalScale, spriteEffects);

            Main.EntitySpriteDraw(Glow, DrawPos, null, a, Rotation, Glorigin, TotalScale * 3f, spriteEffects);
            //Utils.DrawBorderString(Main.spriteBatch, Projectile.damage.ToString(), DrawPos, Color.AntiqueWhite);
            return false;
        }
        //ugh
        public float BoltWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width;
            float tipCutFactor = InverseLerp(0.02f, 0.134f, completionRatio);
            float slownessFactor = Utils.Remap(Projectile.velocity.Length(), 1.5f, 4f, 0.18f, 1f);
            return baseWidth * tipCutFactor * slownessFactor;
        }

        public Color BoltColorFunction(float completionRatio)
        {
            float sineOffset = CalculateSinusoidalOffset(completionRatio);
            return RainbowColorGenerator.TrailColorFunction(completionRatio);//Color.Lerp(Color.White, Color.Black, sineOffset * 0.5f + 0.5f);
        }

        public float CalculateSinusoidalOffset(float completionRatio)
        {
            return (float)Math.Sin(MathHelper.TwoPi * completionRatio + Main.GlobalTimeWrappedHourly * -12f + Projectile.identity) * InverseLerp(0.01f, 0.9f, completionRatio);
        }

        public Vector4[] LightPalette =
        {
            Color.Red.ToVector4(),//Color(255, 165, 0).ToVector4(),
            Color.Green.ToVector4(),
            Color.Blue.ToVector4(),
            Color.White.ToVector4()
        };
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            Vector4[] boltPalette = LightPalette;
            ManagedShader trailShader = ShaderManager.GetShader("NoxusBoss.HomingStarBoltShader");
            trailShader.TrySetParameter("gradient", boltPalette);
            trailShader.TrySetParameter("gradientCount", boltPalette.Length);
            trailShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly * 2f + Projectile.identity * 1.8f);
            trailShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.RainbowRodTrailShape], 2, SamplerState.LinearWrap);
            trailShader.Apply();

            float perpendicularOffset = Utils.Remap(Projectile.velocity.Length(), 4f, 20f, 0.6f, 2f) * Projectile.width;
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * perpendicularOffset;
            Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
            for (int i = 0; i < trailPositions.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float sine = CalculateSinusoidalOffset(i / (float)trailPositions.Length);
                trailPositions[i] = Projectile.oldPos[i] + perpendicular * sine;
            }

            PrimitiveSettings settings = new PrimitiveSettings(BoltWidthFunction, BoltColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(trailPositions, settings, 12);
        }

    }
}
