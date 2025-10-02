using CalamityMod.Items.Weapons.Rogue;
using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.Temp
{
    class AvatarRogue : RogueWeapon
    {

        public override string Texture => MiscTexturesRegistry.PixelPath;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;
        }
        public override void SetDefaults()
        {
            DamageClass d;
            Mod calamity = ModLoader.GetMod("CalamityMod");
            calamity.TryFind("RogueDamageClass", out d);
            Item.DamageType = d;

            Item.damage = 1;
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.channel = true;

            Item.shoot = ModContent.ProjectileType<Bola>();
            Item.shootSpeed = 20;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;


        }

        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);
        }

        
    }
}
