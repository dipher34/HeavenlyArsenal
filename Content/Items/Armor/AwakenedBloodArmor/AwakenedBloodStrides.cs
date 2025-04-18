using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    [AutoloadEquip(EquipType.Legs)]
    public class AwakenedBloodStrides : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor";
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.defense = 43;
            Item.rare = ModContent.RarityType<PureGreen>();
        }
        public override void SetStaticDefaults()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
                ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlot] = true;
            }
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.17f;
            player.GetDamage<GenericDamageClass>() += 0.1f;
            player.GetCritChance<GenericDamageClass>() += 7;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BloodstoneCore>(13).
                AddIngredient<RuinousSoul>(3).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
