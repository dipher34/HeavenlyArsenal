using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.LeverAction
{
    partial class AvatarRifle_Held
    {

        void RenderAmmunition()
        {
            if (CurrentState != State.Reload)
                return;
    
            int bulletAMMO = AmmoID.Bullet;

            Texture2D BulletValue = TextureAssets.Item[bulletAMMO].Value;
            Vector2 DrawPos = Owner.GetBackHandPositionImproved(Owner.compositeBackArm) - Main.screenPosition;
            Main.EntitySpriteDraw(BulletValue, DrawPos, null, Color.AntiqueWhite, 0, BulletValue.Size() * 0.5f, 0.5f, 0);
        
        }
        void RenderLever(float Rot)
        {
            Texture2D lever = ModContent.Request<Texture2D>(Texture+"_Held_Lever").Value;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition + new Vector2(20,0).RotatedBy(Rot);
            Vector2 Origin = new Vector2(lever.Width, 0);

            float AdjustedRot = Rot + MathHelper.ToRadians(-30*LeverCurveOutput);
            Main.EntitySpriteDraw(lever, DrawPos, null, Color.Purple, AdjustedRot, Origin, 1, 0);
            


        }
        public override bool PreDraw(ref Color lightColor)
        {
            
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 4, texture.Height / 2);
            SpriteEffects flip = 0;
            
            Main.EntitySpriteDraw(texture, DrawPos, null, Color.AntiqueWhite, Projectile.rotation, origin, 1, flip);
            RenderLever(Projectile.rotation);
                RenderAmmunition();
            return false;
        }
    }
}
