using System.Linq;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Weapons.Summon;

namespace HeavenlyArsenal.Content.Items
{
    public class avatar_FishingRod : ModItem
    {
        public override string Texture => "HeavenlyArsenal/Content/Items/avatar_FishingRod";

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.DamageType = DamageClass.Summon;
            Item.damage = 4445;
            Item.knockBack = 3f;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.autoReuse = true;

            Item.holdStyle = 0; // Custom hold style
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item1;
            Item.channel = true;
            Item.noMelee = true;


            Item.shoot = ModContent.ProjectileType<CnidarianJellyfishOnTheString>();
            Item.shootSpeed = 10f;
            Item.rare = ModContent.RarityType<NamelessDeityRarity>(); 
            Item.value = Item.buyPrice(gold: 2);
        }

        public override bool CanUseItem(Player player)
        {
            return true;//!Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<CnidarianJellyfishOnTheString>());
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Coral, 2)
                .AddTile<GardenFountainTile>()
                .Register();
        }

       public override void HoldItem(Player player)
        {
            Vector2 directionToCursor = Main.MouseWorld - player.Center;
            player.ChangeDir(directionToCursor.X > 0 ? 1 : -1);
            player.itemRotation = directionToCursor.ToRotation();
            
       }


        private void SetItemInHand(Player player, Rectangle heldItemFrame)
        {
            Vector2 directionToCursor = Main.MouseWorld - player.Center;
            player.ChangeDir(directionToCursor.X > 0 ? 1 : -1);
        }

        private void SetPlayerArms(Player player)
        {
            Vector2 cursorDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float armRotation = cursorDirection.ToRotation();

            if (!player.mount.Active)
            {
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation - MathHelper.PiOver2);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation - MathHelper.PiOver2);
            }
        }

     
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 directionToCursor = Main.MouseWorld - player.Center;
            player.ChangeDir(directionToCursor.X > 0 ? 1 : -1);
            player.itemRotation = directionToCursor.ToRotation() * player.gravDir;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            HoldStyle(player, heldItemFrame);
        }
        public override void HoldItemFrame(Player player)
        {
            SetPlayerArms(player);

        }

        public override void UseItemFrame(Player player)
        {
            SetPlayerArms(player);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/avatar_FishingRod").Value;
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
        //i should die
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/avatar_FishingRod").Value;

            Player player = Main.LocalPlayer;
            Vector2 handPosition = player.MountedCenter + new Vector2(10 * player.direction, -6f * player.gravDir);
            Vector2 drawOffset = new Vector2(-20f, -10f);
            Vector2 drawPosition = handPosition + drawOffset - Main.screenPosition;

            float adjustedRotation = rotation;
            if (player.direction == -1)
            {
                adjustedRotation += MathHelper.Pi;
            }

            spriteBatch.Draw(texture, drawPosition, null, lightColor, adjustedRotation, texture.Size() * 0.5f, scale, player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            return true;
        }



    }
}
