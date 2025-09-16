using CalamityMod.Particles;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight
{
    public class Collapse : GlobalNPC
    {
        public int CollapseTimer;
        public int CollapseStage;

        public int CollapseCooldownMax = 60 * 4;
        public int CollapseCooldownTimer;
        public bool Active
        {
            get => CollapseStage > 0;
            set  => CollapseStage = value ? 1 : 0; 
        }

        public bool Collapsing;

        public RoaringNight ShellSword;
        public Player Collapser
        {
            get; 
            set;
        }
        public override bool InstancePerEntity => true;
        public override void PostAI(NPC npc)
        {
            if(CollapseCooldownTimer > 0)
            {
                CollapseCooldownTimer--;
                return;
            }
            if (!Active)
                return;

            if (CollapseStage >= 3)
            {
                if(ShellSword == null)
                {
                    if (Collapser.heldProj != -1)
                        ShellSword = Main.projectile[Collapser.heldProj].ModProjectile as RoaringNight;
                    
                }

                CollapseTimer++;

                if (CollapseTimer % 2 == 0)
                {
                    if (CollapseTimer < 50)
                    {
                        //if (CollapseTimer == 2){
                        CollapseParticle particle = CollapseParticle.pool.RequestParticle();

                        Color GlowColor = TrailColorFunction(CollapseTimer / 60f) * 0.2f;
                        float rotation = npc.rotation + MathHelper.ToRadians(Main.rand.NextFloat(0, 361));
                        float Scale = 0.5f + CollapseTimer / 17f;
                        particle.Prepare(npc.Center, Vector2.Zero, rotation, 120 - CollapseTimer, Scale, 0, (float)CollapseTimer / 50f, GlowColor, npc);
                        ParticleEngine.ShaderParticles.Add(particle);
                        //}

                    }

                    Vector2 AdjustedSpawn;
                    Dust b;
                    for (int i = 0; i < 50; i++) {
                        float ds = Main.rand.NextFloat(10, 200) * (60 * 1.75f) / (CollapseTimer+1);
                        AdjustedSpawn = Main.rand.NextVector2CircularEdge(ds, ds) + npc.Center;
                        b = Dust.NewDustDirect(AdjustedSpawn, 1, 30, DustID.AncientLight);
                        b.noGravity = true;
                        b.velocity = AdjustedSpawn.AngleTo(npc.Center).ToRotationVector2()* Vector2.Distance(AdjustedSpawn, npc.Center);
                        b.color = TrailColorFunction(Main.rand.NextFloat());
                    }
                }

                //initial sound 
                if (CollapseTimer == 1)
                {
                    SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.Rapture.Collapse with { Volume = 0.8f, PitchVariance = 0.2f});
                }
                if (CollapseTimer == 60 * 1.5f)
                {
                    SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.Rapture.CollapseImpact with { Volume = 1.2f, PitchVariance = 0.2f});
                }

                if (CollapseTimer >= 60 * 1.75f)
                {
                    CalamityMod.Particles.Particle b = new DetailedExplosion(npc.Center, Vector2.Zero, Color.AntiqueWhite *0.5f, Vector2.One, 0, 0, 4.6f, 60, true);

         
                    GeneralParticleHandler.SpawnParticle(b);

                    LightFlash particle = LightFlash.pool.RequestParticle();
                    Color GlowColor = TrailColorFunction(Main.rand.NextFloat());
                    particle.Prepare(npc.Center, Vector2.Zero, npc.rotation + MathHelper.ToRadians(Main.rand.Next(60)), 60, Main.rand.NextFloat(1.6f, 2.4f), GlowColor);
                    ParticleEngine.ShaderParticles.Add(particle);
                    
                    
                    LightFlash particle2 = LightFlash.pool.RequestParticle();
                    particle2.Prepare(npc.Center, Vector2.Zero, npc.rotation + MathHelper.ToRadians(Main.rand.Next(60)), 60, Main.rand.NextFloat(3f, 5.4f), GlowColor);
                    ParticleEngine.ShaderParticles.Add(particle2);

                    Collapsing = true;
                    if (Collapser != null)
                    {
                        ScreenShakeSystem.StartShakeAtPoint(npc.Center, 30);
                        Collapser.StrikeNPCDirect(npc, npc.CalculateHitInfo(10_000 * 3, 0));
                        float RotationOffset = Main.rand.NextFloat(0, 360);
                        //Main.NewText($"{RotationOffset}");
                        RotationOffset = MathHelper.ToRadians(RotationOffset);

                        //spawn beams
                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 SpawnPos = new Vector2(i * 4, 0).RotatedBy(RotationOffset + MathHelper.ToRadians(i * 30));
                            SpawnPos += npc.Center;


                            int Damage;
                            if (ShellSword != null && ShellSword.CreatorItem != null)
                            {
                                Damage = Collapser.GetWeaponDamage(ShellSword.CreatorItem) / 5;
                            }
                            else
                                Damage = Collapser.HeldItem.damage / 5;
                            Damage = (int)(Damage * 1.05f);
                            Damage += (int)(npc.life * 0.05f);
                            Projectile a = Projectile.NewProjectileDirect(Collapser.GetSource_FromThis(), SpawnPos,
                            Vector2.Zero, ModContent.ProjectileType<RaptureBeam>(), Damage, 30);

                            if(ShellSword != null)
                                a.CritChance = ShellSword.CreatorItem.crit;
                            
                            RaptureBeam beam = a.ModProjectile as RaptureBeam;
                            beam.Target = npc;
                            beam.BaseColor = TrailColorFunction(Main.rand.NextFloat());
                            

                        }
                        //dust vfx
                        Dust c;
                        for(int i = 0; i< 250; i++)
                        {

                            c = Dust.NewDustDirect(npc.Center, 160, 160, DustID.AncientLight, Main.rand.NextFloat(-100, 101), Main.rand.NextFloat(-100,101));
                            c.noGravity = true;
                            c.fadeIn = 1.5f;
                            c.color = TrailColorFunction(Main.rand.NextFloat());
                        }
                    }

                    CollapseStage = 0;
                    CollapseTimer = 0;
                    CollapseCooldownTimer = CollapseCooldownMax;
                    Collapsing = false;
                }
            }
        }
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if(CollapseStage >= 3 && Collapsing)
            {
                //CalamityMod.Particles.Particle a = new DetailedExplosion(npc.Center, Vector2.Zero, Color.AntiqueWhite, Vector2.One, 0, 0, 10, 60, true);


                Vector2 trailPos = npc.Center;
                float trailScale = 1;
                Color trailColor = Color.DarkRed;
                Vector2 Direction = new Vector2(56);
                CalamityMod.Particles.Particle Trail = new SparkParticle(trailPos,Direction, false, 35, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(Trail);
                //GeneralParticleHandler.SpawnParticle(a);
            }
        }
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if(CollapseCooldownTimer > 0)
            {
                return;
            }
            if (projectile.type == ModContent.ProjectileType<RoaringNight>()) { 

                CollapseStage++;
                ShellSword = projectile.ModProjectile as RoaringNight;
            }
            if (projectile.type == ModContent.ProjectileType<BlindingLight>() || projectile.type == ModContent.ProjectileType<RaptureBeam>())
            {
                if (Main.rand.NextBool(4))
                {

                    CollapseStage++;
                }
                ShellSword = projectile.ModProjectile as RoaringNight;
            }
            Collapser = Main.player[projectile.owner];
            /*
            if (CollapseStage >= 3)
            {
                Player Owner = Main.player[projectile.owner];

                if (Owner != null)
                {
                    Owner.StrikeNPCDirect(npc, npc.CalculateHitInfo(damageDone * 3, 0));
                    float RotationOffset = Main.rand.NextFloat(0, 360);
                    Main.NewText($"{RotationOffset}");
                    RotationOffset = MathHelper.ToRadians(RotationOffset);
                    for(int i = 0; i< 12; i++)
                    {
                        Vector2 SpawnPos = new Vector2(i * 4, 0).RotatedBy(RotationOffset + MathHelper.ToRadians(i*30));
                        SpawnPos += npc.Center;

                        int Damage = Owner.GetWeaponDamage(Owner.HeldItem);
                        Projectile a = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), SpawnPos, 
                        Vector2.Zero, ModContent.ProjectileType<RaptureBeam>(), Damage, 0);
                        RaptureBeam beam = a.ModProjectile as RaptureBeam;
                        beam.Target = npc;
                        beam.BaseColor = TrailColorFunction(Main.rand.NextFloat());
                        
                    }
                }

                CollapseStage = 0;
            
            }*/
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
    }
}
