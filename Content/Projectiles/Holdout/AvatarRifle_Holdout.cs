using CalamityMod.Projectiles.BaseProjectiles;
using HeavenlyArsenal.Content.Projectiles;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CalamityMod;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;
using ReLogic.Content;
using Terraria.GameContent.UI.BigProgressBar;
using HeavenlyArsenal.Content.Gores;

namespace HeavenlyArsenal.Content.Projectiles.Holdout
{
    public class AvatarRifle_Holdout : BaseIdleHoldoutProjectile
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override int AssociatedItemID => ModContent.ItemType<AvatarRifle>();
        public override int IntendedProjectileType => ModContent.ProjectileType<ParasiteParadiseProjectile>();

        private SoundEffectInstance firingSoundInstance;
        private SoundEffect firingSoundEffect;

        private SoundEffectInstance reloadSoundInstance;
        private SoundEffect reloadSoundEffect;

        private SoundEffectInstance MagEmptySoundInstance;
        private SoundEffect MagEmptySoundEffect;

        private SoundEffectInstance CycleSoundInstance;
        private SoundEffect CycleSoundEffect;




        private AvatarRifleState CurrentState = AvatarRifleState.Firing;
        private int StateTimer = 0;

        private int AmmoCount = 7; // Total shots before reload
        public int ReloadDuration = AvatarRifle.ReloadTime; // Duration for reload (in frames)

        private enum AvatarRifleState
        {
            Firing,  // Firing a shot
            Cycle,   // Cycle before cycling the bolt
            Reload   // Reloading after all shots are fired
        }

        public override void SetDefaults()
        {
            firingSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle_FireWIP2").Value;
            reloadSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle_Cycle").Value; // Add your reload sound here
            CycleSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle_Cycle").Value;



            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = 2;
            Player player = Main.player[Projectile.owner];
        }
        private void CreateDustAtOrigin()
        {
            // Create dust at the center of the sprite
            Vector2 dustPosition = new Vector2 (145,3);
            Dust dust = Dust.NewDustDirect(dustPosition, 1, 1, DustID.Smoke, 0f, 0f, 150, Color.White, 1f);
            dust.velocity *= 0.3f; // Slow the dust movement
            dust.noGravity = true; // Make the dust float
        }
        public override void SafeAI()
        {
            CreateDustAtOrigin();
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            UpdateProjectileHeldVariables(armPosition);
            // Check if the owner is still using the item
            if (Owner.HeldItem.type == ModContent.ItemType<AvatarRifle>())
            {
                // Continue with state handling if the item is being used
                switch (CurrentState)
                {

                    case AvatarRifleState.Firing:
                        HandleFiring();
                        break;

                    case AvatarRifleState.Cycle:
                        HandleCycle();
                        break;

                    case AvatarRifleState.Reload:
                        HandleReload();
                        break;
                }
            }
            else
            {
                // Reset to idle state if the item is not being used
                CurrentState = AvatarRifleState.Firing;
                StateTimer = 0;
            }
        }

        private void HandleFiring()
        {
            if (Owner.channel&& (Owner.HasAmmo(Owner.HeldItem)))
            {
                if (AmmoCount > 0)
                {
                    // Play firing sound
                    firingSoundInstance = firingSoundEffect.CreateInstance();
                    firingSoundInstance.Play();

                    // Decrement ammo count
                    AmmoCount--;
                    //Owner.ConsumeAmmo(Owner.HeldItem);
                    //Owner.Ammo
                    // Call your firing function here (e.g., spawning a projectile)
                    FireProjectile();

                    // Transition to Cycle state for cycling the bolt
                    Cycled = false;
                    CurrentState = AvatarRifleState.Cycle;
                    StateTimer = AvatarRifle.CycleTime; // Adjust delay duration for bolt cycle
                }
                else
                {
                    // Transition to Reload state when ammo is depleted
                    CurrentState = AvatarRifleState.Reload;
                    StateTimer = ReloadDuration;
                }

            }
        }
        public bool Cycled = false;
        private void HandleCycle()
        {
            if (Cycled == false)
            {
                
                CycleSoundInstance = CycleSoundEffect.CreateInstance();
                CycleSoundEffect.Play();
                Cycled = true;
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Left, new Vector2(Projectile.direction * -5f, -10f), ModContent.GoreType<BulletGore>(), 1);

            }
            if (StateTimer > 0)
            {
                StateTimer--; // Count down the delay
            }
            else
            {
               
                // Transition back to Firing state
                CurrentState = AvatarRifleState.Firing;
                
            }
        }

        private void HandleReload()
        {
            if (StateTimer == ReloadDuration)
            {
                // Play reload sound once at the start of reload
                reloadSoundInstance = reloadSoundEffect.CreateInstance();
                reloadSoundInstance.Play();
                //new Vector2 GoreDirection = (-1f, 1f);
                //Gore.NewGore(Projectile.GetSource_FromThis,Projectile.Left, new Vector2(1*Projectile.direction,-1f), ModContent.GoreType<MagEjectGore>,1);
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Left, new Vector2(Projectile.direction*1f, 1f), ModContent.GoreType<MagEjectGore>(), 1);
            
            }

            if (StateTimer > 0)
            {
                StateTimer--; // Count down the reload
            }
            else
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
                // Reset ammo and transition back to Firing state
                AmmoCount = 7;
                CurrentState = AvatarRifleState.Firing;
            }
        }
        private float recoilIntensity = 0f; // Tracks the current recoil intensity
        private const float maxRecoil = 10f; // Maximum recoil amount
        private const float recoilRecoverySpeed = 0.1f; // Speed at which recoil eases out
        private void FireProjectile()
        {
            
            //Player::CheckAmmo
                //player::CheckAmmo
                Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            // Logic for spawning the projectile
            Owner.PickAmmo(Owner.HeldItem, out int projToShoot, out _, out _, out _, out _);



            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                     Projectile.Right,
                                     Projectile.velocity*8,
                                     ProjectileID.Bullet,//AvatarRifle.AmmoType,
                                     Projectile.damage,
                                     Projectile.knockBack,
                                     Projectile.owner);
            recoilIntensity = maxRecoil;
        }

        public void UpdateProjectileHeldVariables(Vector2 armPosition)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                float aimInterpolant = Utils.GetLerpValue(10f, 40f, Projectile.Distance(Main.MouseWorld), true);
                Vector2 oldVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(Main.MouseWorld), aimInterpolant);
                if (Projectile.velocity != oldVelocity)
                {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }
            // Update recoil intensity (ease it out over time)
            if (recoilIntensity > 0f)
            {
                recoilIntensity -= recoilRecoverySpeed;
                if (recoilIntensity < 0f)
                    recoilIntensity = 0f; // Clamp to prevent negative values
            }

            Vector2 recoilOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * recoilIntensity;
            Projectile.position = armPosition - Projectile.Size * 0.4f + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 34f + recoilOffset;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
        }

        public void ManipulatePlayerVariables()
        {
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;


            float frontArmRotation = Projectile.rotation * 0.5f;
            if (Owner.direction == -1)
                frontArmRotation += MathHelper.PiOver2;
            else
                frontArmRotation = MathHelper.PiOver2 - frontArmRotation;
            frontArmRotation += Projectile.rotation + MathHelper.Pi + Owner.direction * MathHelper.PiOver2 + 0.12f;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - MathHelper.PiOver2);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;


            float rotation = Projectile.rotation;
            SpriteEffects direction = SpriteEffects.None;
            if (Math.Cos(rotation) < 0f)
            {
                direction = SpriteEffects.FlipHorizontally;
                rotation += MathHelper.Pi;
            }

            Color stringColor = new(105, 239, 145);


            // Draw a backglow effect as an indicator of charge.
           
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }



        public override bool? CanDamage() => false;
    }
}