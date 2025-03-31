using CalamityMod.Rarities;
using HeavenlyArsenal.Content.Projectiles.Weapons.Melee.Nadir2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee
{
    internal class AvatarSpear : ModItem
    {
        public override void SetDefaults()
        {
            Item.rare = ModContent.RarityType<HotPink>();

            Item.damage = 10000;
            Item.shootSpeed = 40f;
            Item.width = 40;
            Item.height = 32;
            Item.useTime = 40;
            Item.reuseDelay = 40;

            ;
            Item.useAnimation = 0;
            Item.noUseGraphic = false;
            Item.useTurn = true;
            Item.channel = true;


            //Item.useStyle = 3;
            Item.knockBack = 6;


            Item.autoReuse = true;


            Item.ChangePlayerDirectionOnShoot = true;
            Item.noMelee = false;
        }


        //TODO: replace the current sound styles with different ones more actualized of the spear

        public static readonly SoundStyle ThrustSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_charge3");

        public static readonly SoundStyle HitSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_fire");

        public static readonly SoundStyle FullyChargedSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_FullyCharged");



        private bool spearOut = false;

        public override void HoldItem(Player player)
        {
            // Check if the spear projectile already exists
            bool projectileExists = false;

            //dough... 
            foreach (Projectile projectile in Main.projectile)
            {
                
                if (projectile.active && projectile.type == ModContent.ProjectileType<AvatarSpear_Holdout>() && projectile.owner == player.whoAmI)
                {
                    projectileExists = true;
                    break; 
                }
            }

            if (!spearOut && !projectileExists)
            {
                // Spawn the projectile
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center.X, player.Center.Y, 0f, 0f, ModContent.ProjectileType<AvatarSpear_Holdout>(), Item.damage, Item.knockBack, player.whoAmI);
                spearOut = true; // Set the flag to true to prevent further spawns
            }
        }

        public override void UpdateInventory(Player player)
        {
            // Check if the item is no longer being held or in the inventory
            if (player.HeldItem.type != Item.type)
            {
                spearOut = false;

                // Find the spear projectile and kill it
                foreach (Projectile projectile in Main.projectile)
                {
                    if (projectile.active && projectile.type == ModContent.ProjectileType<AvatarSpear_Holdout>() && projectile.owner == player.whoAmI)
                    {
                        projectile.Kill(); // Despawn the spear projectile
                        break; // Only kill the first matching projectile
                    }
                }
            }
        }

    }
}
