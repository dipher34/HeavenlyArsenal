using CalamityMod;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue
{
    class LifeCessationEnergy : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        public ref float Size => ref Projectile.ai[1];

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults() 
        { 
        
        
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = Projectile.width;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = 0;
            Projectile.damage = 300;
            Projectile.timeLeft = 2;
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
           
        }

        public override void AI()
        {
            
            Projectile.timeLeft = 2;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            //Projectile.velocity = Projectile.velocity.SafeDirectionTo(Owner.Center) * Projectile.velocity.Length();
        }
        public override bool? CanCutTiles()
        {
            return true;
        }



        public override bool? Colliding(Rectangle projHitbox, Microsoft.Xna.Framework.Rectangle targetHitbox)
        {
            return targetHitbox.IntersectsConeFastInaccurate(Projectile.Center, Size, Projectile.rotation, MathHelper.Pi / 7f);   
        }

    }
}
