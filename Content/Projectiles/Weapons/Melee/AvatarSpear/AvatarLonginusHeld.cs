
using HeavenlyArsenal.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;

public class AvatarLonginusHeld : ModProjectile
{
    public ref Player Player => ref Main.player[Projectile.owner];

    public bool IsEmpowered { get; set; }

    public enum AttackStates
    {
        Idle,
        RapidStabs,
        LungeStab,
        ThrowTeleport,
        WhipSlash,

        // Empowered attacks
        Empowered_RapidStabs,
        Empowered_HeavyThrust,
        Empowered_RipOut,
        Empowered_Castigation
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.hide = true;
        Projectile.noEnchantmentVisuals = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public ref float Time => ref Projectile.ai[0];
    public ref float AttackState => ref Projectile.ai[1];

    public bool canHit;
    public bool throwMode;

    public override void AI()
    {
        Projectile.extraUpdates = 3;
        Projectile.timeLeft = 2;

        if (Player.HeldItem.type != ModContent.ItemType<AvatarLonginus>() || Player.CCed || Player.dead)
        {
            Projectile.Kill();
            return;
        }

        Player.heldProj = Projectile.whoAmI;

        throwMode = false;
        canHit = false;
        Vector2 offset = Vector2.Zero;
        Vector2 handPosition = Player.MountedCenter;

        switch (AttackState)
        {
            default:
            case (int)AttackStates.Idle:

                Time = 0;

                Projectile.scale = 1f;
                Projectile.velocity = Vector2.Zero;

                float motionBob = Player.velocity.X * 0.02f - Player.velocity.Y * 0.015f * Player.direction;
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, -MathHelper.PiOver2 + 1f * Player.direction + motionBob, 0.2f);
                Projectile.spriteDirection = Player.direction;

                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, -0.3f * Player.direction + motionBob * 0.3f);
                Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2 * Player.direction + motionBob * 1.2f);
                handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, -0.3f * Player.direction);

                if (Player.controlUseItem)
                {
                    AttackState = (int)AttackStates.RapidStabs;
                }

                break;

            case (int)AttackStates.RapidStabs:

                Player.SetDummyItemTime(5);

                const int RapidWindUp = 50;
                const int RapidStabCount = 3;
                const int RapidWindDown = 50;

                int RapidStabTime = 10 + (int)(50 * Player.GetAttackSpeed(DamageClass.Melee));

                if (Time < 2)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.velocity = Player.DirectionTo(Main.MouseWorld) * 20f;
                        Projectile.netUpdate = true;
                    }
                }

                if (Time < RapidWindUp)
                {
                    float windProgress = Time / (RapidWindUp - 1f);

                    float wiggle = MathF.Sin(MathF.Pow(windProgress, 2f) * MathHelper.Pi) * -0.4f * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, 0.9f);
                    offset = new Vector2(MathHelper.SmoothStep(0, -50, windProgress), 0).RotatedBy(Projectile.rotation);

                    Projectile.scale = 1f - windProgress * 0.33f;
                }
                else if (Time < RapidWindUp + RapidStabTime - 1) // -1 here because the modulo is a bit quirky
                {
                    float windDownProgress = Utils.GetLerpValue(RapidWindDown, 0f, Time - RapidWindUp - RapidStabTime, true);
                    if (Time < RapidWindUp + RapidStabTime)
                        canHit = true;

                    float stabProgress = Utils.GetLerpValue(0, RapidStabTime / RapidStabCount, (Time - RapidWindUp) % (RapidStabTime / RapidStabCount));
                    float stabCurve = Utils.GetLerpValue(0, 0.7f, stabProgress, true);

                    if (Time % (RapidStabTime / RapidStabCount) == 0)
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.IntroScreenSlice with { Pitch = 1f, MaxInstances = 0 }, Projectile.Center);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Projectile.velocity = Player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.3f) * 20f;
                            Projectile.netUpdate = true;
                        }
                    }

                    Projectile.rotation = Projectile.velocity.ToRotation();

                    offset = new Vector2(stabCurve * 200 - 50, 0).RotatedBy(Projectile.rotation);

                    Projectile.scale = 1f + stabCurve * 0.5f;
                }
                else
                {
                    float windDownProgress = Utils.GetLerpValue(RapidWindDown / 4f, RapidWindDown, Time - RapidWindUp - RapidStabTime, true);
                    offset = new Vector2(150, 0).RotatedBy(Projectile.rotation) * MathF.Cbrt(1f - windDownProgress);
                    Projectile.scale = 1.5f - MathF.Pow(windDownProgress, 3);
                }

                Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                Time++;

                if (Time > RapidWindUp + RapidStabTime + RapidWindDown / 2)
                {
                    if (Player.controlUseItem)
                    {
                        AttackState = (int)AttackStates.ThrowTeleport;
                        Time = 0;
                    }
                    else if (Time > RapidWindUp + RapidStabTime + RapidWindDown)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AttackStates.ThrowTeleport:

                const int ThrowWindUp = 30;
                const int ThrowTime = 50;
                const int TPTime = 30;

                if (Time < 2)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.velocity = Player.DirectionTo(Main.MouseWorld) * 20f;
                        Projectile.netUpdate = true;
                    }
                }

                Player.velocity *= 0.5f;
                Player.SetImmuneTimeForAllTypes(60);

                if (Time < ThrowWindUp)
                {
                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    float windProgress = Time / (ThrowWindUp - 1f);

                    float wiggle = (windProgress * -0.5f - 0.5f) * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, windProgress);
                    offset = new Vector2(MathHelper.SmoothStep(0, -80, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f;
                }
                else if (Time < ThrowWindUp + ThrowTime)
                {
                    if (Time == ThrowWindUp)
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.DaggerBurst with { Pitch = 1f, MaxInstances = 0 }, Projectile.Center);
                        Projectile.Center = Player.MountedCenter;
                    }

                    Projectile.extraUpdates = 8;

                    canHit = true;
                    throwMode = true;
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 16;
                    Projectile.rotation = Projectile.velocity.ToRotation();

                    if (Collision.SolidCollision(Projectile.Center - new Vector2(10) + Projectile.velocity, 20, 20) && Time < ThrowWindUp + ThrowTime)
                    {
                        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                        Time = ThrowWindUp + ThrowTime;
                        Projectile.velocity *= 0.01f;
                    }
                }
                else
                {
                    Projectile.velocity *= 0.9f;

                    if (Main.myPlayer == Projectile.owner)
                        Main.SetCameraLerp(0.1f, 20);

                    canHit = true;
                    throwMode = true;
                    Player.Center = Vector2.Lerp(Player.Center, Projectile.Center, 0.15f);
                }

                Time++;

                if (Time > ThrowWindUp + ThrowTime + TPTime)
                {
                    if (Player.controlUseItem)
                    {
                        AttackState = (int)AttackStates.LungeStab;
                        Time = 0;
                    }
                    else if (Time > ThrowWindUp + ThrowTime + TPTime)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AttackStates.LungeStab:

                const int LungeWindUp = 80;
                int LungeThrustTime = 10 + (int)(30 * Player.GetAttackSpeed(DamageClass.Melee));
                int LungeWindDown = 10 + (int)(20 * Player.GetAttackSpeed(DamageClass.Melee));

                if (Time < 2)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.velocity = Player.DirectionTo(Main.MouseWorld) * 20f;
                        Projectile.netUpdate = true;
                    }
                }

                if (Time < LungeWindUp)
                {
                    float windProgress = Time / (LungeWindUp - 1f);

                    float wiggle = (windProgress * -0.5f - 0.5f) * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, windProgress);
                    offset = new Vector2(MathHelper.SmoothStep(0, -80, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f;
                }
                else
                {
                    if (Time == LungeWindUp + 1)
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.DaggerGrazeEcho with { MaxInstances = 0 }, Projectile.Center);

                    canHit = true;
                    float thrustProgress = Utils.GetLerpValue(0, LungeThrustTime, Time - LungeWindUp, true);
                    float windDown = Utils.GetLerpValue(0, LungeWindDown, Time - LungeWindUp - LungeThrustTime, true);

                    float thrustCurve = Utils.GetLerpValue(0, 0.2f, thrustProgress, true);
                    float windDownCurve = MathF.Cbrt(windDown);

                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() - (thrustCurve - 1f) * 0.3f * Projectile.direction, 0.5f - thrustCurve * 0.2f);
                    offset = new Vector2(MathHelper.SmoothStep(0, 150, thrustCurve) * (1f - windDownCurve), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1.5f - windDownCurve * 0.5f;
                }

                Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                Time++;

                if (Time > LungeWindUp + LungeThrustTime + LungeWindDown - 2)
                {
                    if (Player.controlUseItem)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                    else if (Time > LungeWindUp + LungeThrustTime - LungeWindDown)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                }
                    
                break;
        }

        if (!throwMode)
            Projectile.Center = Player.RotatedRelativePoint(handPosition) + offset - Projectile.velocity;

        if (canHit)
        {
            for (int i = 0; i < 3; i++)
            {
                int size = (int)(110 * MathF.Exp(-i));
                Projectile.EmitEnchantmentVisualsAt(Projectile.Center + new Vector2((120 + 40 * i) * Projectile.scale, 0).RotatedBy(Projectile.rotation) - new Vector2(size / 2), size, size);
            }
        }
    }

    public void HandleEmpowerment()
    {

    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? CanCutTiles() => false;

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (canHit)
        {
            Vector2 offset = new Vector2(150 * Projectile.scale, 0).RotatedBy(Projectile.rotation);
            float _ = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center - offset, Projectile.Center + offset, 100f, ref _);
        }    

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //Add to percentage. probably add a check for if the next hit should restore the spear to normal/empowered state
        if (AttackState == (int)AttackStates.LungeStab)
        {
            Player.SetImmuneTimeForAllTypes(40);
        }
    }

    //public string StringTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Lantern_String";
    //public string LanternTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Lantern";
    //public string SpearTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Holdout";
    //public string EmpoweredTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Holdout_Empowered";

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        int direction = Projectile.spriteDirection;
        SpriteEffects flipEffect = direction > 0 ? 0 : SpriteEffects.FlipVertically;
        int gripDistance = throwMode ? 30 : 0;
        Vector2 origin = new Vector2(texture.Width / 2 - gripDistance, texture.Height / 2 + (gripDistance + 2) * Player.gravDir * direction);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), Color.White, Projectile.rotation + MathHelper.PiOver4 * direction, origin, Projectile.scale, flipEffect, 0);

        return false;
    }
}
