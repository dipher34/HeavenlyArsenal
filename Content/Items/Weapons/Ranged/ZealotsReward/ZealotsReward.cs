using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class ZealotsReward : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 4000;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 20;
            Item.shoot = ModContent.ProjectileType<ZealotsHeld>();
           
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Item.type] = true;
        }


        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] < 1)
            {
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, 0, 0, player.whoAmI);
            }
        }
    }
}
