using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged
{
    class AvatarBow : ModItem
    {
        public override void SetDefaults()
        {
           Item.DefaultToBow(Item.width, Item.height);
            Item.damage = 100;
            Item.DamageType = DamageClass.Ranged;
            Item.shootSpeed = 20;
            Item.useAmmo = AmmoID.Arrow;
            
        }


    }
}
