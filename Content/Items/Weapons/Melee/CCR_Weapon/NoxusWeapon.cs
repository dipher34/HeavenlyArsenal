using CalamityMod.Projectiles;
using CalamityMod;
using HeavenlyArsenal.Common;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using CalamityMod.Projectiles.Ranged;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.CCR_Weapon
{
    public enum NoxusWeaponState
    {
        Charge,
        Slash,
        Stab
    }
    class NoxusWeapon : ModItem
    {
        public static HeavenlyArsenalServerConfig Config => ModContent.GetInstance<HeavenlyArsenalServerConfig>();

        public override bool IsLoadingEnabled(Mod mod)
        {
            // Check config setting
            bool enabledInConfig = ModContent.GetInstance<HeavenlyArsenalServerConfig>().EnableSpecialItems;
            bool isOtherModLoaded = ModLoader.HasMod("CalRemix");

            return enabledInConfig || isOtherModLoaded;
        }



        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.WoodenSword);
            Item.shoot = ModContent.ProjectileType<AstrealArrow>();
            Item.DamageType = DamageClass.Ranged;
            Item.useAmmo = AmmoID.Arrow;
            Item.useStyle = ItemUseStyleID.Shoot;
            if (ModLoader.TryGetMod("CalRemix", out Mod CalamityRemix))
            {

                Item.DamageType = CalamityRemix.Find<DamageClass>("StormbowDamageClass");
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float arrowSpeed = Item.shootSpeed;
            Vector2 realPlayerPos = player.RotatedRelativePoint(player.MountedCenter, true);
            float mouseXDist = (float)Main.mouseX + Main.screenPosition.X - realPlayerPos.X;
            float mouseYDist = (float)Main.mouseY + Main.screenPosition.Y - realPlayerPos.Y;
            if (player.gravDir == -1f)
            {
                mouseYDist = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - realPlayerPos.Y;
            }
            float mouseDistance = (float)Math.Sqrt((double)(mouseXDist * mouseXDist + mouseYDist * mouseYDist));
            if ((float.IsNaN(mouseXDist) && float.IsNaN(mouseYDist)) || (mouseXDist == 0f && mouseYDist == 0f))
            {
                mouseXDist = (float)player.direction;
                mouseYDist = 0f;
                mouseDistance = arrowSpeed;
            }
            else
            {
                mouseDistance = arrowSpeed / mouseDistance;
            }

            realPlayerPos = new Vector2(player.position.X + (float)player.width * 0.5f + (-(float)player.direction) + ((float)Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
            realPlayerPos.X = (realPlayerPos.X + player.Center.X) / 2f;
            realPlayerPos.Y -= 100f;
            mouseXDist = (float)Main.mouseX + Main.screenPosition.X - realPlayerPos.X;
            mouseYDist = (float)Main.mouseY + Main.screenPosition.Y - realPlayerPos.Y;
            if (mouseYDist < 0f)
            {
                mouseYDist *= -1f;
            }
            if (mouseYDist < 20f)
            {
                mouseYDist = 20f;
            }
            mouseDistance = (float)Math.Sqrt((double)(mouseXDist * mouseXDist + mouseYDist * mouseYDist));
            mouseDistance = arrowSpeed / mouseDistance;
            mouseXDist *= mouseDistance;
            mouseYDist *= mouseDistance;
            float speedX4 = mouseXDist;
            float speedY5 = mouseYDist;
            int shotArrow = Projectile.NewProjectile(source, realPlayerPos.X, realPlayerPos.Y, speedX4, speedY5, ModContent.ProjectileType<AstrealArrow>(), damage, knockback, player.whoAmI);
            Main.projectile[shotArrow].noDropItem = true;
            Main.projectile[shotArrow].tileCollide = false;
            CalamityGlobalProjectile cgp = Main.projectile[shotArrow].Calamity();
            cgp.allProjectilesHome = true;
            return false;
        }


        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
        }
        public override bool AltFunctionUse(Player player)
        {
            return base.AltFunctionUse(player);
        }


    }
    public class NoxusWeaponProjectile : ModProjectile
    {
        #region Setup
        
        public ref float Time => ref Projectile.ai[0];

        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Melee/CCR_Weapon/NoxusWeapon";
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }
        public override void SetStaticDefaults()
        {

        }
        #endregion

        #region AI
        /*
         * stormbow. its perfect.
         * 
         */
        public override void AI()
        {
           
        }
        #endregion

        public override bool PreDraw(ref Color lightColor)
        {
            return base.PreDraw(ref lightColor);
        }
    }

}

