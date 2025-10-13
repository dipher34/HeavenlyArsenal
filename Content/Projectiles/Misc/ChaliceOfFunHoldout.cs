using CalamityMod;
using CalamityMod.Items.Weapons.Magic;
using HeavenlyArsenal.Content.Buffs.Stims;
using HeavenlyArsenal.Content.Items.Consumables.CombatStim;
using HeavenlyArsenal.Content.Items.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Misc
{
    class ChaliceOfFunHoldout : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Rancor>();
        public Player Owner => Main.player[Projectile.owner];

        public bool InUse => Owner.controlUseItem && Owner.altFunctionUse == 0;
        public ref float Time => ref Projectile.ai[0];

        public ref float drinkProgress => ref Projectile.ai[1];

        public bool isDraining;
        public override void SetDefaults()
        {

            Projectile.width = Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.timeLeft = 90000;
            Projectile.aiStyle = 0;
            Projectile.noEnchantmentVisuals = true;
            Projectile.manualDirectionChange = true;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

        }
        public override void AI()
        {
            AdjustPlayerHoldValues();

            float rot = (float.Pi - MathHelper.PiOver4) * drinkProgress * -Owner.direction;
            Projectile.rotation = rot + MathHelper.PiOver2;
            Player.CompositeArmData arm = new Player.CompositeArmData(true, Player.CompositeArmStretchAmount.Full, rot);
            Vector2 armPosition = Owner.GetFrontHandPositionImproved(arm);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (float.Pi - MathHelper.PiOver4) * drinkProgress * -Owner.direction);
            Projectile.Center = armPosition + new Vector2(0, 10 * -Owner.direction).RotatedBy(Projectile.rotation);// + Vector2.UnitX * Owner.direction * 8f;
            Projectile.velocity = Vector2.Zero;

            Owner.heldProj = Projectile.whoAmI;

            if (Owner.HeldItem?.type != ModContent.ItemType<ChaliceOfFun>())
            {
                Projectile.Kill();
                return;
            }


            Lighting.AddLight(Projectile.Center, Color.Crimson.ToVector3());
            Time++;

            if (InUse)
            {
                Drink(Owner);
            }
            if (!InUse)
            {
                drinkProgress = 0;
            }


        }


        public void Drink(Player player)
        {
            drinkProgress = float.Lerp(drinkProgress, 1, 0.2f);
            if (drinkProgress >= 0.99f)
            {
                player.GetModPlayer<StimPlayer>().Addicted = false;
                player.GetModPlayer<StimPlayer>().Withdrawl = false;
                player.GetModPlayer<StimPlayer>().stimsUsed = 0;
                player.ClearBuff(ModContent.BuffType<StimAddicted_Debuff>());




                //Main.NewText($"Dust: {dustLocation}", Color.AntiqueWhite);
                //drained();
            }
        }
        public void drained()
        {
            drinkProgress = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;
            Texture2D Juice = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Misc/ChaliceOfFun_Juice").Value;

            float scale = 0.75f;
            Vector2 offset = new Vector2(0, 0);

            Vector2 origin = texture.Size() * 0.5f;
            Vector2 Gorigin = new Vector2(glow.Width / 2 + 125 * -Owner.direction, glow.Height - 1.6f * glow.Height / 4);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float rotation = Projectile.rotation;

            SpriteEffects direction = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipVertically;
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, scale, direction, 0f);

            Vector2 glowPosition = (Projectile.Center) - Main.screenPosition;
            //Main.spriteBatch.Draw(glow, drawPosition, null, Projectile.GetAlpha(lightColor).MultiplyRGB(Color.Crimson), rotation, Gorigin, 0.1f, direction, 0f);



            Main.spriteBatch.Draw(Juice, drawPosition, null, Projectile.GetAlpha(lightColor).MultiplyRGB(Color.Crimson), rotation, origin, scale, direction, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);



            return false;
        }

        public void AdjustPlayerHoldValues()
        {
            Projectile.spriteDirection = Owner.direction;
            Projectile.timeLeft = 2;
            Owner.heldProj = Projectile.whoAmI;
        }

        public override bool? CanDamage() => false;
    }
}

