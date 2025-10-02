using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight
{
    internal class BlackGlassFragment_Projectile : ModProjectile
    {
        public static List<int> ActiveFragments = new List<int>();

        public int fragIndex;
        private int Mass = 1; // mass contributed by this fragment

        public ref float Time => ref Projectile.ai[0];
        public ref float Claimed => ref Projectile.localAI[1]; // 0 = unclaimed, 1 = claimed/locking

        public override string Texture => "HeavenlyArsenal/Assets/Textures/Particles/BlackGlass_Fragments";

        public override void OnSpawn(IEntitySource source)
        {
            if (!ActiveFragments.Contains(Projectile.whoAmI))
                ActiveFragments.Add(Projectile.whoAmI);
            fragIndex = Main.rand.Next(0, 7);
            Projectile.timeLeft = 60 * 10;
            Time = 0;
            Claimed = 0;
        }

        public override void Kill(int timeLeft)
        {
            ActiveFragments.RemoveAll(i => i == Projectile.whoAmI);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
        public override void AI()
        {
            // movement damping
            Projectile.velocity *= 0.9f;

            // cheap early out
            Time++;

            Player ownerPlayer = Main.player[Projectile.owner];

            if (Time > 30)
            {
                // 1) Prefer existing masses: search only ActiveMasses (cheap)
                int nearestMassId = -1;
                float nearestMassDistSq = float.MaxValue;
                float massSearchRadius = 400f; // tune this
                float massSearchRadiusSq = massSearchRadius * massSearchRadius;

                for (int i = 0; i < BlackGlassMass.ActiveMasses.Count; i++)
                {
                    int id = BlackGlassMass.ActiveMasses[i];
                    if (id < 0 || id >= Main.maxProjectiles) continue;
                    Projectile other = Main.projectile[id];
                    if (!other.active) continue;
                    if (other.owner != Projectile.owner) continue; // only same-owner masses
                    var mp = other.ModProjectile as BlackGlassMass;
                    if (mp == null) continue;
                    if (mp.TotalMass >= mp.MaxMass) continue; // full already

                    float dsq = Vector2.DistanceSquared(Projectile.Center, other.Center);
                    if (dsq < nearestMassDistSq && dsq <= massSearchRadiusSq)
                    {
                        nearestMassDistSq = dsq;
                        nearestMassId = id;
                    }
                }

                float mergeRadius = 12f;
                float mergeRadiusSq = mergeRadius * mergeRadius;

                if (nearestMassId != -1)
                {
                    // steer toward nearest mass
                    Projectile.timeLeft++; // keep alive while merging
                    Projectile.Center = Vector2.Lerp(Projectile.Center, Main.projectile[nearestMassId].Center, 0.06f);

                    // if close enough, add to mass
                    if (Vector2.DistanceSquared(Projectile.Center, Main.projectile[nearestMassId].Center) <= mergeRadiusSq)
                    {
                        var mass = Main.projectile[nearestMassId].ModProjectile as BlackGlassMass;
                        if (mass != null && mass.TotalMass < mass.MaxMass)
                        {
                            mass.TotalMass += Mass;
                            Projectile.Kill();
                            return;
                        }
                    }

                    return; // already chasing a mass — skip fragment-cluster logic
                }

                // 2) No mass nearby — try cluster creation (only after brief delay and if not claimed)
                if (Time > 30 && Claimed == 0)
                {
                    float clusterRadius = 28f; // cluster detection radius
                    float clusterRadiusSq = clusterRadius * clusterRadius;
                    int threshold = 3; // require at least this many fragments to create a mass (tune up to 4 maybe)

                    List<int> neighbors = new List<int>();
                    neighbors.Add(Projectile.whoAmI);

                    // gather unclaimed neighbors
                    for (int i = 0; i < ActiveFragments.Count; i++)
                    {
                        int id = ActiveFragments[i];
                        if (id == Projectile.whoAmI) continue;
                        if (id < 0 || id >= Main.maxProjectiles) continue;
                        Projectile p = Main.projectile[id];
                        if (!p.active) continue;
                        if (p.owner != Projectile.owner) continue; // only same-owner fragments
                        if (p.localAI[1] != 0f) continue; // already claimed by another founder
                        if (Vector2.DistanceSquared(p.Center, Projectile.Center) <= clusterRadiusSq)
                            neighbors.Add(id);
                    }

                    if (neighbors.Count >= threshold)
                    {
                        // claim them (prevent other fragments from also creating a mass)
                        foreach (int id in neighbors)
                            Main.projectile[id].localAI[1] = 1f;

                        // compute centroid and total mass
                        Vector2 centroid = Vector2.Zero;
                        int total = 0;
                        foreach (int id in neighbors)
                        {
                            centroid += Main.projectile[id].Center;
                            total += 1; // each fragment Mass = 1; if different, read from modProj
                        }
                        centroid /= neighbors.Count;

                        // spawn mass
                        int newID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), centroid, Vector2.Zero,
                            ModContent.ProjectileType<BlackGlassMass>(), 0, 0, Projectile.owner);
                        if (newID >= 0 && Main.projectile[newID].ModProjectile is BlackGlassMass newMass)
                        {
                            newMass.TotalMass = total;
                        }

                        // kill the fragments used
                        foreach (int id in neighbors)
                        {
                            if (Main.projectile[id].active)
                                Main.projectile[id].Kill();
                        }

                        return;
                    }
                }
            }

            // Otherwise: small wandering movement (or searching)
            // (You can apply a very weak attraction to other fragments here as visual effect.)
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glow = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/BlackGlass_Fragments_Glow").Value;

            var texRect = tex.Frame(1, 7, 0, fragIndex);

            Rectangle glowRect = glow.Frame(1, 7, 0, fragIndex);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texRect.Size() / 2f;
            Vector2 GlowOrigin = glowRect.Size() / 2f;


            Color GlowColor = RainbowColorGenerator.GenerateRandomColor();

            float value = (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly + Projectile.whoAmI));
            value = Utils.Remap(value, -1, 1, 0.5f, 1.2f) * 0.25f;
            Main.EntitySpriteDraw(glow, drawPos, glowRect, GlowColor with { A = 0 } * 0.9f, Projectile.rotation, GlowOrigin, Projectile.scale * value, SpriteEffects.None);

            Main.EntitySpriteDraw(tex, drawPos, texRect, Color.AntiqueWhite, Projectile.rotation, origin, Projectile.scale*0.25f, SpriteEffects.None);
            return false;
        }
    }
}
