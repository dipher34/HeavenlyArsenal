using CalamityMod.Rarities;
using Luminance.Assets;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.ManaTransfusion
{
    internal class LifeToMana : ModItem
    {
        public override string Texture => MiscTexturesRegistry.PixelPath;
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.rare = ModContent.RarityType<Violet>();
            Item.value = 43840;
           
        }

        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ManaTransfusionPlayer>().Active = true;
            player.statManaMax2 += 60;
        }
    }
    public class ManaReplacement : GlobalItem
    {
        public bool Active
        {
            get => Main.LocalPlayer.GetModPlayer<ManaTransfusionPlayer>().Active;
        }
        public override bool ConsumeItem(Item item, Player player)
        {
            //disable healing potions. 
            if (Active && item.healLife > 0)
            {
                return false;
            }
            return base.ConsumeItem(item, player);
        }

        public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
        {
            if (Active)
            {
                healValue = (int)(healValue * 1.5f);
            }
            base.GetHealMana(item, player, quickHeal, ref healValue);
        }
        public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
        {
            if (Active)
            {
                healValue = 0;
            }
            base.GetHealLife(item, player, quickHeal, ref healValue);
        }
    }
    public class ManaTransfusionPlayer : ModPlayer
    {
        public bool Active;
       
        public override void ResetEffects()
        {
            Active = false;
        }
        public override void PostUpdate()
        {
            //TODO: IF THIS ACCESSORY IS EQUIPPED, PLAYER'S LIFE IS SET TO THEIR MANA.
            // IN ESSENCE, THEIR MAX HEALTH IS THEIR MAX MANA, AND THEY CANNOT HEAL LIFE.
            if (!Active)
                return;

            string DebugText = $"\n";
            DebugText += $"Player.manaRegen: {Player.manaRegen}\n";
            DebugText += $"Player.manaRegenBonus: {Player.manaRegenBonus}\n";
            DebugText += $"Player.manaRegenCount: {Player.manaRegenCount}\n";
            DebugText += $"Player.manaRegenDelay: {Player.manaRegenDelay}\n";
            DebugText += $"Player.manaRegenDelayBonus: {Player.manaRegenDelayBonus}\n";

            Main.NewText(DebugText);
            Player.statLifeMax2 = Player.statManaMax2;
            Player.statLife = Player.statMana;
            Player.lifeRegen = 0;
            Player.manaRegenDelayBonus = 40;

            Player.manaRegen = (int)(Player.manaRegen * 0.5f);
        }
        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            if (!Active)
                return;

            mult = 0.56f;
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            Player.statMana -= info.Damage;
            Player.manaRegenDelay = Math.Clamp(Player.manaRegenDelay + 160, 0, 300);
        }
        public override void UpdateLifeRegen()
        {
            Player.lifeRegen *= 0;
        }
        public override void NaturalLifeRegen(ref float regen)
        {
            if (!Active)
                return;
            regen *= 0;
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (!Active)
                return;


        }
        public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText)
        {
            if (Active)
            {
                chatText = "You need a Mage, not a surgeon.";
                return false;
            }

            return base.ModifyNurseHeal(nurse, ref health, ref removeDebuffs, ref chatText);
        }
    }
}
