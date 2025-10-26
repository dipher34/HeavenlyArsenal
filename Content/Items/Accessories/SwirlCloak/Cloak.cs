using CalamityMod;
using Luminance.Assets;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.SwirlCloak
{
    internal class Cloak : ModItem, ILocalizedModType
    {
        public override string LocalizationCategory => "Items.Accessories";
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.value = Terraria.Item.buyPrice(0, 57, 40, 2);
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            //if(player.velocity.Length()< 0.01f)
            //    player.Calamity().accStealthGenBoost += 2;
            player.GetModPlayer<CloakPlayer>().Active = true;
        }
    }
}
