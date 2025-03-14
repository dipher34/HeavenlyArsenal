using CalamityMod;
using HeavenlyArsenal.Content;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using HeavenlyArsenal.Content.Buffs;
using HeavenlyArsenal.Content.Projectiles.Weapons.Summon;


namespace HeavenlyArsenal.Content.Items.Weapons.Summon
{
    class TheBlackBell : ModItem
    {
        public override void SetDefaults()
        {

            Item.rare = ModContent.RarityType<NamelessDeityRarity>();

            Item.damage = 50000;
            Item.DamageType = DamageClass.Summon;
            Item.shootSpeed = 40f;
            Item.width = 40;
            Item.height = 32;
            Item.useTime = 4;
            Item.reuseDelay = 0;




            Item.useAnimation = 0;
            Item.noUseGraphic = true;
            Item.useTurn = false;
            Item.channel = false;


            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;

            //Item.UseSound = SoundID.Item1;
            
            Item.shoot = ModContent.ProjectileType<TheBlackBell_Projectile>();
            Item.buffType = ModContent.BuffType<TheBlackBell_Buff>();

            Item.buffTime = 60;


            Item.ChangePlayerDirectionOnShoot = false;
           
            Item.noMelee = true;
            Item.Calamity().devItem = true;

        }


    }
}
