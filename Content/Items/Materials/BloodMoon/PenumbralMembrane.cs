using HeavenlyArsenal.Content.Rarities;
using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon
{
    internal class PenumbralMembrane : ModItem
    {
        public override string Texture => MiscTexturesRegistry.PixelPath;
        public override string LocalizationCategory => "Items.Misc";
        public override void SetDefaults()
        {
            Item.maxStack = Terraria.Item.CommonMaxStack;
            Item.width = 38;
            Item.height = 32;
            Item.BestiaryNotes = "Crab";
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Terraria.Item.sellPrice(0, 38, 20, 5);
        }

    }
}
