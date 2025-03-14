using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

using static Luminance.Common.Utilities.Utilities;
using HeavenlyArsenal.Content.Buffs;

using NoxusBoss.Core.Physics.VerletIntergration;
using NoxusBoss.Core.Graphics.RenderTargets;
using System;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Utilities;
using Terraria.GameContent;


//using static NoxusBoss.Assets.GennedAssets;
using NoxusBoss.Assets;


namespace HeavenlyArsenal.Content.Items.Weapons.Summon;

public class AntishadowAssassin : ModProjectile
{

    /// <summary>
    /// Return a shorthand path for a given texture content prefix and name.
    /// </summary>
    public enum AssassinState
    {
        StayNearOwner,
        DissipateToHuntTarget,
        SliceTargetRepeatedly,
        EmergeNearTarget
    }

    /// <summary>
    /// The state of this assassin.
    /// </summary>
    public AssassinState State
    {
        get;
        set;
    }

    /// <summary>
    /// The type of mask used by this assassin.
    /// </summary>
    public int MaskVariant
    {
        get;
        set;
    }

    /// <summary>
    /// The index of the target of this assassin.
    /// </summary>
    public int TargetIndex
    {
        get;
        set;
    } = -1;

    /// <summary>
    /// The starting position of this assassin during their dash.
    /// </summary>
    public Vector2 DashStart
    {
        get;
        set;
    }

    /// <summary>
    /// The position of this assassin's mask.
    /// </summary>
    public Vector2 MaskPosition => Projectile.Center + new Vector2(Projectile.spriteDirection * 1f, -73f).RotatedBy(Projectile.rotation) * Projectile.scale;

    /// <summary>
    /// The owner of this assassin.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    /// The rope for the assassin's first rope.
    /// </summary>
    public VerletSimulatedRope BeadRopeA
    {
        get;
        set;
    }

    /// <summary>
    /// The rope for the assassin's first rope.
    /// </summary>
    public VerletSimulatedRope BeadRopeB
    {
        get;
        set;
    }

    public VerletSimulatedRope BeadRopeC
    {
        get;
        set;
    }


    public VerletSimulatedRope BeadRopeD
    {
        get;
        set;
    }
    /// <summary>
    /// The rotation of the assassin's left arm.
    /// </summary>
    public ref float LeftArmRotation => ref Projectile.localAI[0];

    /// <summary>
    /// The rotation of the assassin's right arm.
    /// </summary>
    public ref float RightArmRotation => ref Projectile.localAI[1];

    /// <summary>
    /// The movement interpolant of this assassin during its stay near owner state.
    /// </summary>
    public ref float MovementInterpolant => ref Projectile.ai[0];

    /// <summary>
    /// How long this assassin has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    /// The amount of mask variants that exist for this assassin.
    /// </summary>
    public static int TotalMasks => 5;

    /// <summary>
    /// The maximum search range this assassin can examine.
    /// </summary>
    public static float TargetingRange => 756f;

    /// <summary>
    /// The render target that holds the contents of this assassin.
    /// </summary>
    public static InstancedRequestableTarget MyTarget
    {
        get;
        private set;
    }

    public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Summon/AntishadowAssassin";

    public override void SetStaticDefaults()
    {
        MyTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(MyTarget);

        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 74;
        Projectile.height = 90;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.minionSlots = AntishadowBead.MinionSlotRequirement;
        Projectile.timeLeft = 90000;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((int)State);
        writer.Write(TargetIndex);
        writer.WriteVector2(DashStart);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        State = (AssassinState)reader.ReadInt32();
        TargetIndex = reader.ReadInt32();
        DashStart = reader.ReadVector2();
    }

    public override void AI()
    {
        // Initialize beads if necessary.
        if (BeadRopeA is null || BeadRopeB is null || BeadRopeC is null|| BeadRopeD is null)
        {
            BeadRopeA = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, 6, 12.5f);
            BeadRopeB = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, 6, 18.5f);
            BeadRopeC = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, 6, 29f);
            BeadRopeD = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, 2, 1f);
        }

        HandleMinionBuffs();

        switch (State)
        {
            case AssassinState.StayNearOwner:
                DoBehavior_StayNearOwner();
                break;
            case AssassinState.DissipateToHuntTarget:
                DoBehavior_DissipateToHuntTarget();
                break;
            case AssassinState.SliceTargetRepeatedly:
                DoBehavior_SliceTargetRepeatedly();
                break;
            //case AssassinState.ShadowLunge:
             //   DoBehavior_ShadowLunge();
             //   break;
        }

        ResetVisuals();
        MoveBeadRops();

        Time++;
    }

    private void DoBehavior_ShadowLunge()
    {
        throw new NotImplementedException();
    }

    private void DoBehavior_StayNearOwner()
    {
        HandleRepositionMotion(Owner.Center + new Vector2(-Owner.direction * 60f, -40f));

        // Fly up and down by default.
        // This gets functionally nullified if a reposition is ongoing.
        float sinusoidalSpeed = MathF.Cos(MathHelper.TwoPi * Time / 550f) * 0.13f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.UnitY * sinusoidalSpeed, 0.5f);

        Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(Owner.Center);

        // Search for targets.
        int targetIndex = TargetIndex;
        Projectile.Minion_FindTargetInRange((int)TargetingRange, ref targetIndex, false);
        if (targetIndex >= 0)
        {
            Time = 0f;
            TargetIndex = targetIndex;
            State = AssassinState.DissipateToHuntTarget;
            Projectile.netUpdate = true;
        }
    }

    private void DoBehavior_DissipateToHuntTarget()
    {
        if (!EnsureTargetIsAlive())
            return;

        NPC target = Main.npc[TargetIndex];
        if (Time <= 1f)
        {
            if (Projectile.WithinRange(target.Center, 300f))
                Time = 4f;
            else
                MovementInterpolant = 1f;
            Projectile.netUpdate = true;
        }
        if (Time <= 3f)
        {
            HandleRepositionMotion(target.Center + target.SafeDirectionTo(Projectile.Center) * 75f);

            if (MovementInterpolant >= 0.01f)
                Time = 2f;
        }

        float armRotation = 1.3f;
        LeftArmRotation = LeftArmRotation.AngleLerp(armRotation, 0.56f);
        RightArmRotation = RightArmRotation.AngleLerp(-armRotation, 0.56f);

        // Look at the target.
        Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(target.Center);

        // Vanish.
        Projectile.scale = MathHelper.SmoothStep(0f, 1f, InverseLerp(15f, 5f, Time));
        if (Time >= 15f)
        {
            Time = 0f;
            State = AssassinState.SliceTargetRepeatedly;
            Projectile.netUpdate = true;
        }
    }

    private void DoBehavior_SliceTargetRepeatedly()
    {
        if (!EnsureTargetIsAlive())
            return;

        int slashTime = 120;
        NPC target = Main.npc[TargetIndex];
        if (Time <= slashTime)
        {
            if (Time % 5f == 0f)
                SoundEngine.PlaySound((target.Organic() ? Murasama.OrganicHit : Murasama.InorganicHit) with { MaxInstances = 0 });

            Projectile.Center = target.Center;
            Projectile.velocity = Vector2.Zero;
            DashStart = Projectile.Center;
            ScreenShakeSystem.StartShakeAtPoint(target.Center, 2f);

            if (Main.myPlayer == Projectile.owner)
            {
                float slashMaxOffset = 50f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 slashSpawnPosition = target.Center + Main.rand.NextVector2Circular(slashMaxOffset, slashMaxOffset);
                    Vector2 slashDirection = slashSpawnPosition.SafeDirectionTo(target.Center);

                    int slash = Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashSpawnPosition, slashDirection, ModContent.ProjectileType<AntishadowAssassinSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Main.projectile[slash].originalDamage = Projectile.originalDamage;
                    Main.projectile[slash].netUpdate = true;
                    Projectile.netUpdate = true;
                }
            }
        }

        Projectile.scale = InverseLerp(slashTime, slashTime + 15f, Time);
        if (Time >= slashTime + 15f)
        {
            Time = 0f;
            State = AssassinState.StayNearOwner;
            Projectile.netUpdate = true;
        }
    }

    private bool EnsureTargetIsAlive()
    {
        if (TargetIndex <= -1 || TargetIndex >= Main.maxNPCs || !Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
        {
            Time = 0f;
            TargetIndex = -1;
            State = AssassinState.StayNearOwner;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Resets visuals for this assassin, such rotations.
    /// </summary>
    private void ResetVisuals()
    {
        float idealArmRotation = 0.15f;
        //ShadowEffect();
        LeftArmRotation = LeftArmRotation.AngleLerp(idealArmRotation, 0.09f);
        RightArmRotation = RightArmRotation.AngleLerp(-idealArmRotation, 0.09f);

        // Rotate based on current horizontal speed.
        float idealRotation = MathHelper.Clamp((Projectile.position - Projectile.oldPosition).X * 0.004f, -0.5f, 0.5f);
        Projectile.rotation = Projectile.rotation.AngleLerp(idealRotation, 0.3f);
    }

    /// <summary>
    /// Handles repositioning motion for this assassin.
    /// </summary>
    private void HandleRepositionMotion(Vector2 destination)
    {
        float minMovementInterpolant = 0.01f;
        bool doneMoving = MovementInterpolant <= minMovementInterpolant;

        // Dash if sufficiently far from the desired position and not already dashing.
        if (doneMoving && !Projectile.WithinRange(destination, 320f))
        {
            RerollMask();
            DashStart = Projectile.Center;
            MovementInterpolant = 1f;
            Projectile.netUpdate = true;
        }

        // Handle motion.
        Projectile.Center = Vector2.SmoothStep(Projectile.Center, destination, MathF.Pow(Convert01To010(MovementInterpolant), 4f) * 0.8f);
        MovementInterpolant *= 0.91f;
        if (MovementInterpolant < 0.6f)
            MovementInterpolant *= 0.93f;

        // Scale down when moving.
        Projectile.scale = InverseLerp(0.05f, minMovementInterpolant, MovementInterpolant) + InverseLerp(0.75f, 1f, MovementInterpolant);

        if (MovementInterpolant >= 0.2f)
        {
            float armRotation = 1.3f;
            LeftArmRotation = LeftArmRotation.AngleLerp(armRotation, 0.56f);
            RightArmRotation = RightArmRotation.AngleLerp(-armRotation, 0.56f);

            CreateMotionVisuals();
        }
    }

    private void GetMaskInfo(out Texture2D? texture, out int goreID)
    {
        texture = null;
        goreID = 0;
        if (Main.netMode != NetmodeID.Server)
        {
            switch (MaskVariant)
            {
                case 0:
                    texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Gores/AntishadowAssassinMask1").Value;
                    goreID = ModContent.Find<ModGore>(Mod.Name, "AntishadowAssassinMask1").Type;
                    break;
                case 1:
                    texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Gores/AntishadowAssassinMask2").Value;
                    goreID = ModContent.Find<ModGore>(Mod.Name, "AntishadowAssassinMask2").Type;
                    break;
                case 2:
                    texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Gores/AntishadowAssassinMask3").Value;
                    goreID = ModContent.Find<ModGore>(Mod.Name, "AntishadowAssassinMask3").Type;
                    break;
                case 3:
                    texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Gores/AntishadowAssassinMask4").Value;
                    goreID = ModContent.Find<ModGore>(Mod.Name, "AntishadowAssassinMask4").Type;
                    break;
                case 4:
                    texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Gores/AntishadowAssassinMask5").Value;
                    goreID = ModContent.Find<ModGore>(Mod.Name, "AntishadowAssassinMask5").Type;
                    break;
            }
        }
    }

    /// <summary>
    /// Rerolls the mask variant for this assassin.
    /// </summary>
    private void RerollMask()
    {
        CreateMaskGore();

        int oldMaskVariant = MaskVariant;
        do
        {
            MaskVariant = Main.rand.Next(TotalMasks);
        }
        while (MaskVariant == oldMaskVariant);
    }

    /// <summary>
    /// Drops this assassin's mask as a gore.
    /// </summary>
    private void CreateMaskGore()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        GetMaskInfo(out _, out int goreID);
        
        Gore.NewGore(Projectile.GetSource_FromThis(), MaskPosition, Vector2.Zero, goreID, Projectile.scale);
    }



    private void ShadowEffect()
    {
       
        Vector2 Something = new Vector2(Projectile.Center.Y - Projectile.height / 2);
        int fireBrightness = Main.rand.Next(0, 20);
        Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
        Color bigColorColor = fireColor;
        AntishadowSmokeParticleSystemManager.ParticleSystem.CreateNew(new Vector2(Projectile.Center.X,Projectile.Center.Y+Projectile.height/2), Main.rand.NextVector2Circular(60f, 60f), Vector2.One * Main.rand.NextFloat(20f, 90f) * 3f, bigColorColor);

    }






    /// <summary>
    /// Ensures that all motion visuals for when this assassin is dashing to its destination.
    /// </summary>
    private void CreateMotionVisuals()
    {
        Vector2 previous = DashStart;
        Vector2 current = Projectile.Center;
        Vector2 directionalForce = (current - previous).RotatedByRandom(0.4f) * 0.1f;
        float scaleFactor = Utils.Remap(current.Distance(previous), 30f, 125f, 1f, 0.5f) * 0.6f;
        int fireBrightness = Main.rand.Next(0, 20);
        Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
        Color bigColorColor = fireColor;

        if (MovementInterpolant <= 0.35f)
        {
            scaleFactor *= 2f;
            bigColorColor = new Color(0, 0, 0);
        }

        // Create fire.
        AntishadowFireParticleSystemManager.ParticleSystem.CreateNew(current, Main.rand.NextVector2Circular(7f, 7f) + directionalForce, Vector2.One * Main.rand.NextFloat(100f, 175f) * scaleFactor, bigColorColor);
        if (!current.WithinRange(previous, 40f))
        {
            int steps = (int)(current.Distance(previous) / 21f);
            for (float i = 0; i < steps; i++)
            {
                Color loopFireColor = fireColor;
                if (i % 6 == 0)
                    loopFireColor = new Color(174, 0, Main.rand.Next(16), 0);

                Vector2 position = Vector2.Lerp(previous, current, i / steps);
                AntishadowFireParticleSystemManager.ParticleSystem.CreateNew(position, Main.rand.NextVector2Circular(27f, 27f) + directionalForce * 0.45f, Vector2.One * Main.rand.NextFloat(30f, 175f) * scaleFactor, loopFireColor);

                if (Main.rand.NextBool(4))
                {
                    Dust fire = Dust.NewDustPerfect(position, 261);
                    fire.velocity = Main.rand.NextVector2Circular(30f, 30f) - directionalForce * 0.4f;
                    fire.color = Color.Red;
                    fire.noGravity = true;
                    fire.scale *= 1.2f;
                }
            }
        }
    }

    /// <summary>
    /// Moves the ropes responsible for the motion of this assassin's kasa hat beads.
    /// </summary>
    private void MoveBeadRops()
    {
        float ropeGravity = 0.5f;
        Vector2 force = Vector2.UnitX * (AperiodicSin(Time * 0.024f) * 0.4f + Main.WindForVisuals);
        BeadRopeA.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * -36f, -84f).RotatedBy(Projectile.rotation) * Projectile.scale, ropeGravity, force);
        force = Vector2.UnitX * (AperiodicSin(Time * 0.024f + 5.1f) * 0.4f + Main.WindForVisuals);
        BeadRopeB.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * 26f, -84f).RotatedBy(Projectile.rotation) * Projectile.scale, ropeGravity, force);
        force = Vector2.UnitX * (AperiodicSin(Time * 0.024f + 5.1f) * 0.4f + Main.WindForVisuals);
        BeadRopeC.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * 26f, -84f).RotatedBy(Projectile.rotation) * Projectile.scale, ropeGravity, force);
        force = Vector2.UnitX * (AperiodicSin(Time * 0.024f + 5.1f) * 0.4f + Main.WindForVisuals);
        BeadRopeD.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * 40f, -84f).RotatedBy(Projectile.rotation) * Projectile.scale, ropeGravity, force);
    }

    /// <summary>
    /// Handles the application of minion buffs for this assassin, making it vanish if the buffs are not present or the owner player has died.
    /// </summary>
    private void HandleMinionBuffs()
    {
        Owner.AddBuff(ModContent.BuffType<AntishadowAssassinBuff>(), 3600);
        Referenced<bool> hasMinion = Owner.GetValueRef<bool>("HasAntishadowAssassin");
        if (Owner.dead)
            hasMinion.Value = false;
        if (hasMinion.Value)
            Projectile.timeLeft = 2;
    }

    /// <summary>
    /// Draws a katana for this assassin.
    /// </summary>
    private void DrawKatana(Vector2 bladeDrawStartingPosition, bool flip, float angle)
    {
        float katanaWidthFunction(float completionRatio) => Projectile.Opacity * Projectile.scale * 4f;
        Color katanaColorFunction(float completionRatio) => Projectile.GetAlpha(Color.Red);

        float appearanceInterpolant = 1f;
        ManagedShader katanaShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowKatanaShader");
        katanaShader.TrySetParameter("flip", flip);
        katanaShader.TrySetParameter("appearanceInterpolant", appearanceInterpolant);
        katanaShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.PointWrap);

        PrimitiveSettings katanaPrimitiveSettings = new(katanaWidthFunction, katanaColorFunction, Shader: katanaShader);

        Vector2 katanaReach = angle.ToRotationVector2() * appearanceInterpolant * Projectile.spriteDirection * MathF.Sqrt(Projectile.scale) * -138f;
        Vector2 orthogonalOffset = (angle + flip.ToDirectionInt() * -MathHelper.PiOver2).ToRotationVector2() * appearanceInterpolant * 75;
        
        Vector2[] katanaPositions = new Vector2[8];
        for (int i = 0; i < katanaPositions.Length; i++)
        {
            float completionRatio = i / (float)(katanaPositions.Length - 1f);
            katanaPositions[i] = bladeDrawStartingPosition + katanaReach * completionRatio + Main.screenPosition;
            katanaPositions[i] += orthogonalOffset * completionRatio.Squared();
        }

        PrimitiveRenderer.RenderTrail(katanaPositions, katanaPrimitiveSettings, 40);
    }

    /// <summary>
    /// Draws the main body for the purpose of rendering into the RT responsible for this assassin.
    /// </summary>
    /// 
    /// <summary>
    /// Returns the size of the game's main <see cref="Viewport"/>.
    /// </summary>
    public static Vector2 ViewportSize => new Vector2(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height);
    private void DrawIntoTarget()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, CullOnlyScreen);

        Texture2D body = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/AntishadowAssassin").Value;
        Texture2D arm = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/AntishadowAssassinArm").Value;
        Vector2 drawPosition = ViewportSize * 0.5f;

        // Draw the right arm behind the body.
        float armScale = 0.76f;
        Vector2 rightArmDrawPosition = drawPosition + new Vector2(6f, -50f);
        Main.spriteBatch.Draw(arm, rightArmDrawPosition, null, Color.White, RightArmRotation, new Vector2(arm.Width - 32f, 10f), armScale, SpriteEffects.FlipHorizontally, 0f);

        // Draw the body.
        Main.spriteBatch.Draw(body, drawPosition, null, Color.White, 0f, body.Size() * 0.5f, 1f, 0, 0f);

        // Draw the left arm.
        Vector2 leftArmDrawPosition = drawPosition + new Vector2(-14f, -50f);
        Main.spriteBatch.Draw(arm, leftArmDrawPosition, null, Color.White, LeftArmRotation, new Vector2(32f, 10f), armScale, 0, 0f);

        Main.spriteBatch.End();
    }

    /// <summary>
    /// Draws beads and rope attached to the kasa hat for this assassin.
    /// </summary>
    /// 

    private void DrawBeads()
    {
        Texture2D beadTextureA = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Beads3").Value;    //GennedAssets.Textures.SecondPhaseForm.Beads3;
        Texture2D beadTextureB = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Beads2").Value;
        Texture2D beadTextureC = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Beads1").Value;
        Texture2D beadTextureD = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Content/NPCs/Bosses/Avatar/SecondPhaseForm/Beads1").Value;//GennedAssets.Textures.SecondPhaseForm.Beads2;
        Color ropeColor = new Color(210, 13, 16);
        BeadRopeA?.DrawProjection(TextureAssets.MagicPixel.Value, -Main.screenPosition, false, _ => ropeColor, widthFactor: Projectile.scale);
        BeadRopeB?.DrawProjection(TextureAssets.MagicPixel.Value, -Main.screenPosition, false, _ => ropeColor, widthFactor: Projectile.scale);
        BeadRopeC?.DrawProjection(TextureAssets.MagicPixel.Value, -Main.screenPosition, false, _ => ropeColor, widthFactor: Projectile.scale);
        BeadRopeD?.DrawProjection(TextureAssets.MagicPixel.Value, -Main.screenPosition, false, _ => ropeColor, widthFactor: Projectile.scale);

        if (BeadRopeA is not null)
        {
            float beadRotation = (BeadRopeA.Rope[^2].Position - BeadRopeA.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;

            Main.spriteBatch.Draw(beadTextureA, BeadRopeA.Rope[^6].Position - Main.screenPosition, null, Color.White, beadRotation, new Vector2(24f, 6f), Projectile.scale * 0.08f, 0, 0f);
        }
        if (BeadRopeB is not null)
        {
            float beadRotation = (BeadRopeB.Rope[^2].Position - BeadRopeB.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureB, BeadRopeB.Rope[^4].Position - Main.screenPosition, null, Color.White, beadRotation, new Vector2(30f, 243f), Projectile.scale * 0.1f, 0, 0f);
        }
        if (BeadRopeC is not null)
        {
            float beadRotation = (BeadRopeC.Rope[^2].Position - BeadRopeC.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureC, BeadRopeC.Rope[^4].Position - Main.screenPosition, null, Color.White, beadRotation, new Vector2(30f, 243f), Projectile.scale * 0.1f, 0, 0f);
        }
        if (BeadRopeD is not null)
        {
            float beadRotation = (BeadRopeC.Rope[^2].Position - BeadRopeC.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureC, BeadRopeC.Rope[^4].Position - Main.screenPosition, null, Color.White, beadRotation, new Vector2(30f, 243f), Projectile.scale * 0.1f, 0, 0f);
        }

    }

    /// <summary>
    /// Draws the mask for this assassin.
    /// </summary>
    private void DrawMask()
    {
        GetMaskInfo(out Texture2D? mask, out _);
        if (mask is null)
            return;

        Vector2 faceDrawPosition = MaskPosition - Main.screenPosition;
        Main.spriteBatch.Draw(mask, faceDrawPosition, null, Color.White, Projectile.rotation, mask.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0f);
    }

    /// <summary>
    /// Draws the kasa hat for this assassin.
    /// </summary>
    private void DrawHat()
    {
        Texture2D hat = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/AntishadowAssassinKasa").Value; //GennedAssets.Textures.Weapons.AntishadowAssassinKasa;
        Vector2 hatDrawPosition = Projectile.Center - Main.screenPosition + new Vector2(Projectile.spriteDirection * -1f, -94f).RotatedBy(Projectile.rotation) * Projectile.scale;
        Main.spriteBatch.Draw(hat, hatDrawPosition, null, Color.White, Projectile.rotation+0.05f*Projectile.spriteDirection, hat.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        MyTarget.Request(400, 400, Projectile.identity, DrawIntoTarget);
        if (MyTarget.TryGetTarget(Projectile.identity, out RenderTarget2D? target) && target is not null)
        {
            DrawBeads();

            float leftKatanaAngle = Projectile.rotation - LeftArmRotation + 0.3f;
            float rightKatanaAngle = Projectile.rotation + RightArmRotation;
            if (Projectile.spriteDirection == 1)
                rightKatanaAngle += 0.3f;
            else
                leftKatanaAngle -= 0.3f;

            Vector2 rightShoulderPosition = Projectile.Center + new Vector2(6f, -50f).RotatedBy(Projectile.rotation) * Projectile.scale;
            Vector2 rightHandEnd = rightShoulderPosition + new Vector2(12f, 84f).RotatedBy(Projectile.rotation + RightArmRotation) * Projectile.scale;
            DrawKatana(rightHandEnd - Main.screenPosition, true, rightKatanaAngle);

            Vector2 leftShoulderPosition = Projectile.Center + new Vector2(-14f, -50f).RotatedBy(Projectile.rotation) * Projectile.scale;
            Vector2 leftHandEnd = leftShoulderPosition + new Vector2(-12f, 84f).RotatedBy(Projectile.rotation + LeftArmRotation) * Projectile.scale;
            DrawKatana(leftHandEnd - Main.screenPosition, true, leftKatanaAngle);

            Main.spriteBatch.PrepareForShaders();

            Vector3[] palette = new Vector3[]
            {
                new Vector3(1.5f),
                new Vector3(0f, 1f, 1.2f),
                new Vector3(1f, 0f, 0f),
            };
            Texture2D PerlinNoise = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/PerlinNoise").Value;
            ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinShader");
            overlayShader.TrySetParameter("eyeScale", Cos01(Main.GlobalTimeWrappedHourly * 7.5f + Projectile.identity) * 0.2f + 1f);
            overlayShader.TrySetParameter("gradient", palette);
            overlayShader.TrySetParameter("gradientCount", palette.Length);
            overlayShader.TrySetParameter("textureSize", target.Size());
            overlayShader.TrySetParameter("edgeColor", new Vector3(1f, 0f, 0f));
            overlayShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
            overlayShader.Apply();

            Main.spriteBatch.Draw(target, Projectile.Center - Main.screenPosition, null, Color.Black, Projectile.rotation, target.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0);

            Main.spriteBatch.UseBlendState(BlendState.NonPremultiplied);
            DrawMask();
            DrawHat();
            Main.spriteBatch.ResetToDefault();
        }

        return false;
    }

    public override bool? CanDamage() => Projectile.scale >= 0.56f;
}
