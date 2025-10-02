using Luminance.Assets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.Temp
{
    public class AvatarRogueProjectile : ModProjectile
    {
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
            

        }

        public override bool PreDraw(ref Color lightColor)
        {


            return false;
        }
    }
}
