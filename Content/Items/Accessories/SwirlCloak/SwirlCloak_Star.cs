using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.SwirlCloak
{
    class SwirlCloak_Star : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 2;
            Projectile.scale = 1f;
        }
        public override void AI()
        {
            Projectile.velocity *= 1.02f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D a = GennedAssets.Textures.GreyscaleTextures.BloomCircle;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(a, DrawPos, null, lightColor, 0, a.Size() * 0.5f, 0.1f, 0);

            return base.PreDraw(ref lightColor);
        }
    }
}
