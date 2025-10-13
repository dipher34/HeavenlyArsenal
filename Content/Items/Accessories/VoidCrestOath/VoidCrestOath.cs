using System;
using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Projectiles.Summon;
using HeavenlyArsenal.Content.Items.Accessories.Cosmetic;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static NoxusBoss.Assets.GennedAssets.Sounds;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath
{
    [AutoloadEquip(EquipType.Neck)]
    public class VoidCrestOath : ModItem, ILocalizedModType
    {
        public const string HaloEquippedVariableName = "WearingVoidCrest";
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetStaticDefaults()
        {
            // Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 6));
            // ItemID.Sets.AnimatesAsSoul[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 78;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.accessory = true;
        }
        public override void UpdateVanity(Player player)
        {
            VoidCrestOathPlayer modPlayer = player.GetModPlayer<VoidCrestOathPlayer>();
            modPlayer.Vanity = true;
            if (player.isDisplayDollOrInanimate)
                modPlayer.ResourceInterp = 1;
           
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (!Main.specialSeedWorld)
                return;
            string text = Language.GetTextValue("Mods.HeavenlyArsenal.Items.Accessories.VoidCrestOath.GFBtooltip");

            TooltipLine line = new TooltipLine(Mod, "VoidCrestTroll", text);
            int insertIndex = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name.StartsWith("Tooltip"));
            if (insertIndex == -1)
                tooltips.Add(line);
            else
                tooltips.Insert(insertIndex+2, line);

            

        }
        public override void EquipFrameEffects(Player player, EquipType type)
        {
            player.GetModPlayer<VoidCrestOathPlayer>().Vanity = true;
            if (player.isDisplayDollOrInanimate || Main.gameMenu)
                player.GetModPlayer<VoidCrestOathPlayer>().ResourceInterp = 1;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            //i don't car </3
            VoidCrestOathPlayer modPlayer = player.GetModPlayer<VoidCrestOathPlayer>();
            modPlayer.voidCrestOathEquipped = true;
            modPlayer.Vanity = false;
            if (hideVisual)
            {
                modPlayer.Hide = true;
                modPlayer.ResourceInterp = 0;
            }
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                texture: TextureAssets.Item[Type].Value,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale,
                wantedScale: 0.6f,
                drawOffset: new(0f, -2f)
            );
            return false;
        }
    }

}
