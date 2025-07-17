using System;
using System.Collections.Generic;
using System.Linq;
    

using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    class Bloodproj : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];
        public int BulFram;
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.timeLeft = 400;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.damage = 300;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CanHitPastShimmer[Projectile.type] = true;
            
        }

        public override void AI()
        {
            Projectile.velocity *= 0.9999f;


            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 100 * (float)Math.Sin(Time);

            //Projectile.rotation = MathHelper.PiOver2+Projectile.velocity.ToRotation();
            if(Projectile.velocity.Length() <= Vector2.One.Length())
            {
                Projectile.velocity = Vector2.Zero;

            }
            
        }
        public override void PostAI()
        {
            
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int value = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;
            int FrameCount = 7;

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 origin = new Vector2(texture.Width/2, (texture.Height / FrameCount) / 2);

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            //games
            Rectangle SquidFrames = new Rectangle(0, value * (texture.Height / FrameCount), texture.Width, texture.Height / FrameCount);

            float Rot = (Projectile.rotation) + MathHelper.PiOver2;

            Main.EntitySpriteDraw(texture, DrawPos, SquidFrames, lightColor, Rot, origin, 1, SpriteEffects.None); 
            return false;
        }
    }
}
