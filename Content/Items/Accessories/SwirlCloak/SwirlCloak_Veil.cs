using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.SwirlCloak
{
    internal class SwirlCloak_Veil : ModProjectile
    {
        public PiecewiseCurve SwirlCurve;
        public ref float SwirlCloakInterp => ref Projectile.ai[0];
        public ref float t => ref Projectile.ai[1];
        public ref Player Owner => ref Main.player[Projectile.owner];

        public int StarDamage = 600;

        public HashSet<int> TrappedProjectiles = new HashSet<int>();
        private float CaptureRadius = 300f;
        private Vector2 orbitRadius = new Vector2(10, 0);

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 3;
            Projectile.scale = 1f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (SwirlCurve == null)
            {
                SwirlCurve = new PiecewiseCurve()
                    .Add(EasingCurves.Sextic, EasingType.Out, 1f, 1f, 0f);
            }
        }
        public override void AI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = Owner.Center;
            if (SwirlCloakInterp >= 0.999f)
            {
                Projectile.Kill();
                return;
            }
            else
                Projectile.timeLeft++;

            SwirlCloakInterp = SwirlCurve.Evaluate(t);
            if (t < 1) 
                t = Math.Clamp(t + 0.0001f, 0, 1);

            TrapProjectiles();
            doCaptureLogic();


        }

        void TrapProjectiles()
        {
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (!proj.active || proj.type == ModContent.ProjectileType<SwirlCloak_Veil>() && !proj.friendly)
                    continue;

                float distance = Vector2.Distance(proj.Center, Projectile.Center);
                if (distance < CaptureRadius)
                {
                  
                    // Store trapped state
                    if (!TrappedProjectiles.Contains(proj.whoAmI))
                        TrappedProjectiles.Add(proj.whoAmI);
                }
            }

        }
        void doCaptureLogic()
        {
            var trappedList = new List<int>(TrappedProjectiles);
            for (int i = trappedList.Count - 1; i >= 0; i--)
            {
                Projectile trapped = Main.projectile[trappedList[i]];
                trapped.GetGlobalProjectile<VortexCaptureGlobal>().BeginCapture(trapped, this.Projectile, 0, 300);

            }
        } 
            void ConvertProjectiles()
        {
            // Convert HashSet<int> to a List<int> for indexed access
            var trappedList = new List<int>(TrappedProjectiles);
            for (int i = trappedList.Count - 1; i >= 0; i--)
            {
                Projectile trapped = Main.projectile[trappedList[i]];
                if (!trapped.active)
                {
                    TrappedProjectiles.Remove(trappedList[i]);
                    continue;
                }
               
               // float distance = Vector2.Distance(trapped.Center, Projectile.Center);
               // if (distance < 16f)
               /*
                {
                    // Convert
                    trapped.Kill();
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<SwirlCloak_Star>(),
                        StarDamage,
                        0f,
                        Projectile.owner
                    );

                    TrappedProjectiles.Remove(trappedList[i]);
                }
               */
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D veilTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Accessories/SwirlCloak/SwirlCloak_Veil").Value;

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            Rectangle frame = veilTexture.Frame(1, 1, 0, 0);

            float Rot = MathHelper.ToRadians(360) * SwirlCloakInterp;
            Vector2 Scale = new Vector2(1) * SwirlCloakInterp;
            Main.EntitySpriteDraw(veilTexture, DrawPos, frame, Color.White * (1 - SwirlCloakInterp), Rot, frame.Size() * 0.5f, Scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
