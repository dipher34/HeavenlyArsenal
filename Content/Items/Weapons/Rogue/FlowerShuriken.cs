using CalamityMod;

using HeavenlyArsenal.Content.Projectiles.Weapons.Rogue;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue
{
    class FlowerShuriken : ModItem
    {
        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Magic/avatar_FishingRod";
        public override void SetDefaults()
        {
            //Item.DamageType = RogueDamageClass.Instance;
            Item.DamageType= DamageClass.Throwing;
            Item.crit = 70;
            Item.damage = 4000;
            Item.useStyle = 1;
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 4;
            Item.reuseDelay = 4;
            Item.noMelee = true;
            Item.Calamity().devItem = true;
            Item.useAnimation = 4;

            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<FlowerShuriken_Proj>();
            Item.shootSpeed = 20;

        }



    }
}
