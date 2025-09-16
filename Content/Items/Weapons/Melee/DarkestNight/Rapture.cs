
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Assets.GennedAssets.Textures;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight
{
    public class Rapture : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = 1;
            Item.useTime = 7;
            Item.useAnimation = 2;
            Item.reuseDelay = 2;
            Item.crit = 10;
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();

            Item.DamageType = DamageClass.Melee;
            Item.shoot = ModContent.ProjectileType<BlackGlass>();
            Item.damage = 49374;
            Item.Size = ModContent.Request<Texture2D>(Texture).Value.Size() * 0.2f;
        }

        public override bool MeleePrefix()
        {
            return true;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            return true;
        }
        public override bool CanShoot(Player player)
        {
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
            
        }

        public override void HoldItemFrame(Player player)
        {
            
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<RoaringNight>()] < 1)
            {
                Projectile a = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<RoaringNight>(), Item.damage, Item.knockBack);
                a.CritChance = Item.crit;
                RoaringNight b = a.ModProjectile as RoaringNight;

                b.CreatorItem = player.HeldItem;
            }
        }
        #region Draws
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            
            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 DrawPos = Item.position - Main.screenPosition;
            Vector2 Origin = tex.Size() * 0.5f;
            Main.EntitySpriteDraw(tex, DrawPos, null, lightColor, rotation, Origin, scale*0.2f, SpriteEffects.None);

            
            return false;//base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }
        #endregion
    }
}