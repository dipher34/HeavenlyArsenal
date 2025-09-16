using CalamityMod;
using CalamityMod.Particles;
using log4net.Layout;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Particle = CalamityMod.Particles.Particle;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight
{
    public class RoaringNight : ModProjectile
    {
        public Item CreatorItem;
        public PiecewiseCurve SwingCurve;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        public ref float Swinginterp => ref Projectile.localAI[0];
        public ref float t => ref Projectile.localAI[2];

        public float GlowSizeInterpolant;

        public float swingDirection = 0f;
        public float _slashScale;

        public bool canHit;
        public bool CreatingGlass;
        public bool SwingInProgress;


        public int SlashDistance = 200;
        public int SwingStage
        {
            get => (int)Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(canHit);
            writer.Write(CreatingGlass);
            writer.Write(SwingInProgress);

            
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            reader.Read();
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.coldDamage = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.Size = new Vector2(50, 50);
            const int slashLength = 24;
            _slashPositions = new Vector2[slashLength];
            _slashRotations = new float[slashLength];

            SwingInProgress = false;
            Projectile.scale = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

        }
        public override void OnSpawn(IEntitySource source)
        {
            ResetTrail();
            ResetSlash();
            UpdateSlash();
            UpdateTrail();
        }
     
        public override void AI()
        {
          

            CheckDespawnConditions();

            Projectile.timeLeft++;
            Projectile.Center = Owner.Center;
            Owner.heldProj = Projectile.whoAmI;
           

            Projectile.extraUpdates = 7;
            SlashDistance = 245;

            Time++;
            CreateBlackGlass();
            if (CreatingGlass)
                return;

            if (Owner.altFunctionUse == 2 && !CreatingGlass)
            {
                Time = 0;
                CreatingGlass = true;
            }
            if (Owner.controlUseItem && Owner.altFunctionUse != 2)
            {
                SwingInProgress = true;
            }
           
            if (SwingInProgress)
            {
                Owner.SetDummyItemTime(2);
                UpdateSlash();
                UpdateTrail();
                canHit = true;
                if (SwingStage == 0)
                {
                    if (t == 0)
                    {
                        swingDirection = (Owner.Center.AngleTo(Main.MouseWorld) + MathHelper.ToRadians(-230) * Owner.direction);
                        //Main.NewText($"{Projectile.rotation}");
                        SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.Rapture.Swing with { PitchVariance = 0.2f, Pitch = 0.4f,  }, Projectile.Center);
                      

                    }
                    if (t <= 0.2)
                    {

                        Projectile.scale = float.Lerp(Projectile.scale, 1, t);
                        Owner.direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);
                    }

                    if(t >= 0.4f && t <= 0.6f)
                    {

                        ScreenShakeSystem.StartShake(0.1f);
                    }
                    if (SwingCurve == null)
                        SwingCurve = new PiecewiseCurve()
                            .Add(EasingCurves.Sextic, EasingType.Out, 1f, 1f, 0f);

                    t = Math.Clamp(t + 0.0085f, 0, 1);
                    Swinginterp = SwingCurve.Evaluate(t);
                    Projectile.rotation = swingDirection + (MathHelper.TwoPi * Swinginterp) * Owner.direction;
                    Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

                    if (t >= 0.74)
                    {
                        Projectile.scale *= 0.88f;
                    }
                    if (t == 1)
                    {
                        SwingStage++;
                        ResetValues();
                    }
                }



                else if (SwingStage == 1)
                {
                    if (t == 0)
                    {
                        float basedir = Owner.direction == 1 ? MathHelper.ToRadians(-125) : MathHelper.ToRadians(230);
                        swingDirection = basedir * Owner.direction + Owner.Center.AngleTo(Main.MouseWorld);
                        //Main.NewText($"{Projectile.rotation}");
                        SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.Rapture.Swing2 with { PitchVariance = 0.2f}, Projectile.Center);
                        Owner.direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);

                    }
                    if (t <= 0.2f)

                        Projectile.scale = float.Lerp(Projectile.scale, 1, t);
                    if (SwingCurve == null)
                        SwingCurve = new PiecewiseCurve()
                            .Add(EasingCurves.Sextic, EasingType.Out, 1f, 1f, 0f);
                    t = Math.Clamp(t + 0.005f, 0, 1);

                    Swinginterp = SwingCurve.Evaluate(t);
                    Projectile.rotation = swingDirection + (MathHelper.TwoPi * Swinginterp) * -Owner.direction;
                    Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);


                    if (t >= 0.74)
                    {
                        Projectile.scale *= 0.88f;
                    }
                    if (t == 1)
                    {
                        SwingStage++;
                        ResetValues();
                    }
                }
            }

            
        }

        public void CreateBlackGlass()
        {
            

            if (!CreatingGlass)
            {
                if (GlowSizeInterpolant > 0)
                    GlowSizeInterpolant = float.Lerp(GlowSizeInterpolant, 0, 0.02f);

                return;
            }

            int GlassCount = 3;

            int placeholder = 45 * Projectile.MaxUpdates;
            Owner.SetDummyItemTime(2);
            Vector2 Direction = (Owner.Center.AngleTo(Main.MouseWorld)).ToRotationVector2();
            float ToMouse = Owner.Center.AngleTo(Main.MouseWorld);
            float desiredHand = float.Lerp(0, ToMouse - MathHelper.PiOver2, 1);
            swingDirection = desiredHand;
            Owner.direction = Math.Sign(Direction.X);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, desiredHand);

            GlowSizeInterpolant = float.Lerp(GlowSizeInterpolant, 1, 0.01f);

            if(Time < placeholder && Time%2 == 0)
            {
                ScreenShakeSystem.StartShake(0.1f);
                Vector2 AdjustedSpawn = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, desiredHand);
                AdjustedSpawn -= Owner.Center;
                AdjustedSpawn *= 2f;
                AdjustedSpawn += Owner.Center;
                float thing = Main.rand.NextFloat(0, 170);

                Particle d = new ManaDrainStreak(Owner, 1f, Main.rand.NextVector2CircularEdge(thing, thing), -15, TrailColorFunction(Main.rand.NextFloat(0f,1.1f)), Color.White, 10, AdjustedSpawn);
                d.Position = AdjustedSpawn;
                GeneralParticleHandler.SpawnParticle(d);
                //Time = placeholder - 30;
            }
            if (Time >= placeholder && Time % 30 == 0)
                for (int i = 0; i < GlassCount; i++)
                {
                    ScreenShakeSystem.StartShake(1.25f);
                    Vector2 Velocity = Direction.RotatedByRandom(MathHelper.ToRadians(20)) * 10;
                    int Damage = Projectile.damage/5;
                    Projectile s = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Owner.Center, Velocity, ModContent.ProjectileType<BlackGlass>(), Damage, 0);
                    s.rotation = s.velocity.ToRotation();
                    if(i == 0)
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FeatherAppear with { MaxInstances = 2, PitchVariance = 0.2f, Volume = 0.25f });
                }
            if (Time >= placeholder*2)
            {
                Owner.altFunctionUse = 0;
                CreatingGlass = false;
                
            }
        }
        public void CheckDespawnConditions()
        {
            if (Owner.HeldItem.type != ModContent.ItemType<Rapture>() || !Owner.active || Owner.dead)
            {
                Projectile.Kill();
            }
        }
        public void ResetValues()
        {
            Projectile.ResetLocalNPCHitImmunity();
            SwingInProgress = false;
            Projectile.scale = 0;
            SwingCurve = null;
            Swinginterp = 0;
            t = 0;
            Projectile.rotation = Owner.Center.AngleTo(Main.MouseWorld) + MathHelper.ToRadians(-90);
            ResetSlash();
            ResetTrail();
            canHit = false; 
            if (SwingStage > 1)
                SwingStage = 0;
        }
        #region Slash stuff
        public void ResetTrail(bool rotation = false)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Projectile.oldPos[i] = Projectile.Center;
                if (rotation)
                    Projectile.oldRot[i] = Projectile.rotation;
            }
        }
        public void UpdateTrail()
        {
            Vector2 playerPosOffset = Owner.position - Owner.oldPosition;

            if (Projectile.numUpdates == 0)
            {
                playerPosOffset = Vector2.Zero;

                for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
                {
                    Projectile.oldPos[i] = Projectile.oldPos[i - 1];
                    Projectile.oldRot[i] = Projectile.rotation.AngleLerp(Projectile.oldRot[i - 1], 0.1f);


                }

                //if (!holdTrailUpdate)
                {
                    Projectile.oldPos[0] = Projectile.Center + Projectile.velocity;
                    Projectile.oldRot[0] = Projectile.rotation;
                }

            }
        }
        public void ResetSlash()
        {
            _slashScale = 1f;
            for (int i = _slashPositions.Length - 1; i > 0; i--)
            {
                _slashPositions[i] = new Vector2(SlashDistance * Projectile.scale, 0).RotatedBy(Projectile.rotation);
                _slashRotations[i] = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
            }
        }
        public void UpdateSlash()
        {
            for (int i = _slashPositions.Length - 1; i > 0; i--)
            {
                _slashRotations[i] = _slashRotations[i - 1];
                _slashPositions[i] = _slashPositions[i - 1];
            }

            _slashRotations[0] = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
            _slashPositions[0] = new Vector2(SlashDistance * Projectile.scale * _slashScale, 0).RotatedBy(Projectile.rotation+MathHelper.ToRadians(1*Projectile.direction));
        }
        #endregion
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Inflate(50, 100);
            hitbox.Location += new Vector2(180 * Projectile.scale, 0).RotatedBy(Projectile.rotation).ToPoint();
        }
        public override bool? CanDamage()
        {
            return canHit;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (canHit)
            {
                float dist = 230f;
                

                Vector2 offset = new Vector2(dist * Projectile.scale * 1.65f, 0).RotatedBy(Projectile.rotation);
                float _ = 0;
                return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, Projectile.Center + offset, 120f, ref _);
            }

            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 AdjustedSpawn = target.Center;
            Particle d = new ImpactParticle(AdjustedSpawn, MathHelper.ToRadians(3), 0, 6, Color.White);

            GeneralParticleHandler.SpawnParticle(d);

            Vector2 thing = (Projectile.rotation + MathHelper.ToRadians(90)).ToRotationVector2() * 10;
            Dust a;
            for (int i = 0; i< 30; i++)
            {

                a = Dust.NewDustDirect(target.Center, 2, 2, DustID.AncientLight, thing.X, thing.Y);
                a.noGravity = true;
                float Variation = Main.rand.NextFloat(0.75f, 4f);
                a.velocity *= SwingStage == 1 ? Variation : -Variation;
            }

            
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.CritDamage = StatModifier.Default + 0.2f;

            modifiers.ArmorPenetration = AddableFloat.Zero + 5;
            if (target.SuperArmor)
            {
                modifiers = modifiers with { SuperArmor = false };
            }

        }

        #region Drawcode
        public override bool PreDraw(ref Color lightColor)
        {
           
            if(t >= 0.1f && t <= 0.720f)
                DrawSlash();
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            DrawShell(DrawPos, ref lightColor);
            DrawCone(DrawPos);


            return false;
        }
        public void DrawShell(Vector2 DrawPos, ref Color lightColor)
        {
            Texture2D thing = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 Origin = new Vector2(thing.Width / 2, thing.Height + 60);
            float Rot = Projectile.rotation + MathHelper.PiOver2;
            float Scale = Projectile.scale * 0.2f;

            SpriteEffects Flip = SwingStage == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;// | SpriteEffects.FlipVertically;


            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 Afterimages = DrawPos;
                Main.EntitySpriteDraw(thing, Afterimages, null, lightColor * 0.2f, Projectile.oldRot[i] + MathHelper.PiOver2, Origin, Scale, Flip);
                //Utils.DrawBorderString(Main.spriteB       atch, i.ToString(), Afterimages, Color.AntiqueWhite);
            }
            Main.EntitySpriteDraw(thing, DrawPos, null, lightColor.MultiplyRGB(Color.White), Rot, Origin, Scale, Flip);

        }

        public void DrawCone(Vector2 DrawPos)
        {
            if(GlowSizeInterpolant !< 0)
                return;
            Texture2D Cone = AssetDirectory.Textures.GlowCone.Value;

            Vector2 Adjusted = DrawPos; //+ new Vector2(12, -2.5f).RotatedBy(desiredHand);

            Vector2 Origin = new Vector2(0, Cone.Height/2);

            float Rot = swingDirection + MathHelper.PiOver2;

            Color ConeColor = TrailColorFunction((float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly))) * 0.2f;
            Vector2 Squish = new Vector2(2, 1 * GlowSizeInterpolant) * GlowSizeInterpolant;
            
            Main.EntitySpriteDraw(Cone, Adjusted, null, ConeColor, Rot, Origin, Squish, SpriteEffects.None);
            Main.EntitySpriteDraw(Cone, Adjusted, null, ConeColor *1.1f, Rot, Origin, Squish *0.75f, SpriteEffects.None);

            Texture2D placeholder = GennedAssets.Textures.GreyscaleTextures.Splatter;


        }


        private Vector2[] _slashPositions;
        private float[] _slashRotations;

        private static VertexStrip _slashStrip;

        private void DrawSlash()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            // Have a shader prepared, only special thing is that it uses a normalized matrix
            ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.RaptureSlash");
           
            trailShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 0, SamplerState.PointClamp);
            trailShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);
            trailShader.TrySetParameter("uWorldViewProjection", Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailShader.TrySetParameter("uColor", Color.AliceBlue.ToVector4() * 0.66f);
            trailShader.TrySetParameter("BetterPi", float.Pi);
            trailShader.Apply();

            // Rendering primitives involves setting vertices of each triangle to form quads
            // This does it for us
            // Have a list of positions and rotations to create vertices, width function to determine how far vertices are from the center
            // Color function determines each vertex's color, which can be used in the shader
            _slashStrip ??= new VertexStrip();
            _slashStrip.PrepareStrip(_slashPositions, _slashRotations, TrailColorFunction, TrailWidthFunction, Owner.Center - Main.screenPosition, _slashPositions.Length, true);
            _slashStrip.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        private float TrailWidthFunction(float p) => 70 * Projectile.scale * _slashScale * Projectile.direction;
        private Color TrailColorFunction(float p)
        {
            // Cycle hue over [0, 360), scaled by p
            // You can also multiply p to make the cycle repeat faster/slower
            //float hue = (p * 360f) % 360f;
            float hue = (p * 360f + Main.GlobalTimeWrappedHourly * 120f) % 360f;
           
            return HsvToColor(hue, 0.75f, 1f, 0); 
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
        #endregion
    }
}
