using Luminance.Assets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.Temp
{
    public class Bounty : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.timeLeft = 540;
            Projectile.Size = new Vector2(20, 20);
        }

        public override void AI()
        {
            Projectile.Center = Owner.Center;

        }

        public override bool PreDraw(ref Color lightColor)
        {


            return false;
        }
    }
}
