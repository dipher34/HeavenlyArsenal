using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using Humanizer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    public class ViscousWhip_Item : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(BloodwhipBuff.TagDamage);

        public override string LocalizationCategory => "Items.Weapons.Summon";
        public int SwingStage = 0;
        public override bool MeleePrefix()
        {
            return true;
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.shootSpeed = 4f;
            Item.knockBack = 3f;

            Item.rare = ModContent.RarityType<Rarities.BloodMoonRarity>();
            Item.value = Item.buyPrice(0, 46, 30, 2);
            Item.shoot = ModContent.ProjectileType<ViscousWhip_Proj>();
            Item.damage = 1200;
            Item.Size = new Vector2(40, 40);
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;
            

            Item.DefaultToWhip(ModContent.ProjectileType<ViscousWhip_Proj>(), Item.damage, Item.knockBack, 4, 42);


            Item.crit = 12;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string text = $"Swingstage: {SwingStage}";
          
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (BlacklistedProjectiles.BlackListedProjectiles.Contains(projectile.type))
                    continue;

                if (projectile.sentry)
                    continue;


                if (projectile.DamageType != DamageClass.Summon)
                    continue;

                if (projectile.owner != Main.LocalPlayer.whoAmI)
                    continue;
                int Damage = projectile.originalDamage / 4 + projectile.damage / 2;

                text += $"\n {projectile.Name}=> Spit Damage: {Damage}";
            }
            TooltipLine line = new TooltipLine(Mod, "Debug", text);
            tooltips.Add(line);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if(player.altFunctionUse != 2) {
            Projectile Whip = Projectile.NewProjectileDirect(source, position, velocity, Item.shoot, Item.damage, knockback, ai1:SwingStage);
            SwingStage++;
                if (SwingStage > 3)
                    SwingStage = 0;
            }
            return false;
        }
        public override bool AltFunctionUse(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ViscousWhip_Proj>()] < 1)
            {
                return true;
            }
            else
                return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<UmbralLeechDrop>(), 4)
                .AddDecraftCondition(Condition.EclipseOrBloodMoon);
        }
    }
}
