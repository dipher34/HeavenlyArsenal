using CalamityMod;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Armor.NewFolder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    class BloodNeedle : ModProjectile
    {
       
        public override string Texture => "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/BloodNeedle_Head";
        //TODO: sync
        public float TimeInner;
        public ref float Time => ref Projectile.ai[0];
        public ref float Attack => ref Projectile.ai[1];
        public ref float Index => ref Projectile.ai[2];
        public ref Player Player => ref Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.damage = 4000;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            if (Player.dead || !Player.active || Player.GetModPlayer<BloodArmorPlayer>().CurrentForm != BloodArmorForm.Offense)
            {
               // Projectile.Kill();
                return;
            }
            int count = Main.projectile.Count(n => n.active && n.type == Type && n.owner == Player.whoAmI && n.whoAmI != Projectile.whoAmI);
            Index = 0;

            foreach (Projectile proj in Main.projectile.Where(n => n.active && n.owner == Player.whoAmI && n.type == Type))
            {
                if (proj.whoAmI > Projectile.whoAmI)
                {
                    Index++;
                }

                if (proj.whoAmI == Projectile.whoAmI)
                {
                    continue;
                }

                if (Projectile.Distance(proj.Center) < 40)
                {
                    Projectile.velocity -= Projectile.DirectionFrom(proj.Center).SafeNormalize(Vector2.Zero) * 13;
                    proj.velocity -= proj.DirectionFrom(Projectile.Center).SafeNormalize(Vector2.Zero) * 13;
                }
            }

            Projectile.timeLeft = 20;
            Projectile.rotation = Projectile.AngleFrom(Player.MountedCenter) - MathHelper.PiOver2;
            float trackSpeed = 1f;
            NPC target = Projectile.FindTargetWithinRange(320);
            float sideOffset = (Index == 0 ? -110f : 110f);
            Vector2 homePos = Player.MountedCenter
                          + new Vector2(sideOffset, 0f)
                          - Player.velocity * 10f;

           
            if (target != null)
            {
                Projectile.rotation = Player.AngleTo(target.Center) - MathHelper.PiOver2;

            }
            if (target == null)
            {
                Projectile.Center += Player.velocity * 0.5f;
                TimeInner = 0;
            }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(homePos) * Projectile.Distance(homePos) * 0.1f, trackSpeed);

            if (Projectile.Distance(homePos) < 3f)
            {
                Projectile.Center = homePos;
            }
            if (tentacle == null)
            {
                tentacle = new Rope(Projectile.Center, Player.MountedCenter, 30, 10f, Vector2.Zero);
                //tentacle.segments[0].pinned = false;
                //tentacle.segments[^1].pinned = false;
            }
            for (int i = 0; i < tentacle.segments.Length; i++)
            {
                if (Main.rand.NextBool(100))
                {
                    Dust blood = Dust.NewDustPerfect(tentacle.segments[i].position, DustID.CrimtaneWeapons, new Vector2(0, -3f), 10, Color.Crimson, 1);
                    blood.noGravity = true;
                    blood.rotation = Main.rand.NextFloat(-20, 20);
                }
                if(i <= tentacle.segments.Length/2 && i > 3)
                {
                    tentacle.segments[i].velocity += new Vector2(13*Player.direction,6);
                }
            }
            tentacle.segments[0].position = Projectile.Center;
            tentacle.segments[^1].position = Player.MountedCenter;
            tentacle.gravity = -Vector2.UnitX * Player.direction * 0.05f - Vector2.UnitY * 0.01f;
            tentacle.damping = Utils.GetLerpValue(20, 0, Player.velocity.Length(), true) * 0.05f;
            tentacle.Update();

            Projectile.timeLeft = 20;
        }

        public Rope tentacle;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            base.ModifyDamageHitbox(ref hitbox);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D head = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D body = GennedAssets.Textures.GreyscaleTextures.WhitePixel.Value;

            Main.EntitySpriteDraw(head, Projectile.Center - Main.screenPosition, head.Frame(), Color.White, Projectile.rotation, head.Size() * 0.5f, new Vector2(1.5f, 1.5f), 0, 0);

            if (tentacle != null)
            {
                List<Vector2> points = new List<Vector2>();
                points.AddRange(tentacle.GetPoints());
                points.Add(Player.MountedCenter);

                for (int i = points.Count - 1; i > 0; i--)
                {
                    

                    float rot = points[i].AngleTo(points[i - 1]) - MathHelper.PiOver2;
                    Vector2 stretch = new Vector2((1.1f - (float)i / points.Count * 0.6f) * Projectile.scale, i > points.Count - 2 ? points[i].Distance(points[i - 1]) / (body.Height - 2f) : 1.1f);

                    stretch = new Vector2(Projectile.scale * 0.6f, points[i].Distance(points[i - 1]) / (body.Height - 2f) * 1.2f);
                    Main.EntitySpriteDraw(body, points[i] - Main.screenPosition, body.Frame(), Color.White, rot, body.Size(), stretch, 0, 0);
                   
                    //Main.EntitySpriteDraw(texture, points[i] - Main.screenPosition, tentacleGlowFrame, glowColor.MultiplyRGBA(Color.Lerp(light, Color.White, 1f - (float)i / points.Count)) * 1.5f, rot, tentacleFrame.Size() * new Vector2(0.5f, 0f), stretch, 0, 0);

                }

                Utils.DrawBorderString(Main.spriteBatch, Index.ToString(), Projectile.Center - Vector2.UnitY * 50 - Main.screenPosition, Color.White);
            }


            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return base.CanHitNPC(target);
        }
    }
}
