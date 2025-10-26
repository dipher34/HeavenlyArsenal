using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class ZealotsHeld : ModProjectile
    {
        #region setup
        public ref float ChargeInterp => ref Projectile.ai[2];
        private enum State
        {
            None,
            Charge,
            Fire,
            Recoil
        }
        private State CurrentState = State.None;

        public ref Player Owner => ref Main.player[Projectile.owner];

        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Ranged/ZealotsReward/ZealotsReward";

        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
        }
        Vector2 Offset = new Vector2(0);
        Vector2 RotatedOffset = new Vector2(0, 0);
        public override void OnSpawn(IEntitySource source)
        {

        }
        #endregion
        public override void AI()
        {
            CheckDespawnConditions();
            UpdateOwner();

            UpdateHeldPosition();
            stateMachine();
        }

        #region stateMachine
        private void stateMachine()
        {
            switch (CurrentState)
            {
                case State.None:
                    if (Owner.controlUseItem)
                        CurrentState = State.Charge;
                    break;
                case State.Charge:
                    ManageCharge();
                    break;
                case State.Fire:
                    break;
                case State.Recoil:
                    break;
                default:
                    break;
            }
        }

        void CheckDespawnConditions()
        {
            if (Owner.HeldItem.type != ModContent.ItemType<ZealotsReward>())
            {
                Projectile.Kill();
                return;
            }
        }
        void UpdateHeldPosition()
        {
            RotatedOffset = Vector2.Lerp(RotatedOffset, new Vector2(40, 0), 0.2f);
            Projectile.timeLeft++;
            Projectile.Center = Owner.MountedCenter;
            Projectile.velocity = Projectile.Center.AngleTo(Owner.Calamity().mouseWorld).ToRotationVector2() * 1f;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        void UpdateOwner()
        {

            Owner.heldProj = Projectile.whoAmI;
            if (Projectile.velocity.X != 0)
                Owner.direction = Math.Sign(Projectile.velocity.X);
        }

        void ManageCharge()
        {
            if (!Owner.controlUseItem)
            {
                ChargeInterp = 0;
                CurrentState = State.None;
                return;
            }
            ChargeInterp = float.Lerp(ChargeInterp, 1, 0.1f);

            Offset = Main.rand.NextVector2Square(0, 2) * ChargeInterp;
            if(ChargeInterp > 0.99f)
            {
                RotatedOffset = new Vector2(10, 2);
                ChargeInterp = 0;
            }
        }

        #endregion
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 DrawPos = Projectile.Center + RotatedOffset.RotatedBy(Projectile.rotation) + Offset - Main.screenPosition;

            Vector2 Origin = new Vector2(texture.Width / 2, texture.Height / 2);

            float Rot = Projectile.rotation;

            SpriteEffects flip = Owner.direction == 1 ? SpriteEffects.None :SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(texture, DrawPos, null, lightColor, Rot, Origin, 1f, flip);
            return false;
        }
    }
}
