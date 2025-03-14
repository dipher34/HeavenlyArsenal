using HeavenlyArsenal.Content.Items.Weapons.Summon;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Luminance.Common.Utilities;

using static Luminance.Common.Utilities.Utilities;
using System;
//using HeavenlyArsenal.Common.utils;
//using static NoxusBoss.Assets.GennedAssets;


namespace HeavenlyArsenal.Content.Items.Weapons.Summon;

public class AntishadowAssassinSlash : ModProjectile
{
    /// <summary>
    /// How long this slash should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(0.25f);

    /// <summary>
    /// How long this slash has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = Lifetime / 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 50;
        Projectile.height = 50;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Lifetime;
        Projectile.penetrate = -1;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 1;
        Projectile.tileCollide = false;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.MaxUpdates = 2;
    }

    public override void AI()
    {
        Time++;
        Projectile.scale =
            Convert01To010
            (Time / Lifetime + 0.001f) * 1.5f;

        int fireBrightness = Main.rand.Next(0, 15);
        Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
        if (Main.rand.NextBool(6))
            fireColor = new Color(220, 20, Main.rand.Next(60), 0);

        if (Time % 2f == 0f)
            AntishadowFireParticleSystemManager.ParticleSystem.CreateNew(Projectile.Center, Main.rand.NextVector2Circular(50f, 50f), Vector2.One * Main.rand.NextFloat(40f, 90f), fireColor);
    }

    private float TrailWidthFunction(float completionRatio) => Projectile.scale * 50f;

    private Color TrailColorFunction(float completionRatio)
    {
        Color baseColor = new Color(220, 20, 60);
        float lifetimeRatio = Time / Lifetime;
        return baseColor * Projectile.Opacity * Utils.GetLerpValue(2f, 0.75f, lifetimeRatio);
    }

    Texture2D PerlinNoise = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/Iridescence").Value;

    public override bool PreDraw(ref Color lightColor)
    {
        float lifetimeRatio = Time / Lifetime;
        ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinSlashShader");
        trailShader.TrySetParameter("sheenEdgeColorWeak", new Vector4(255f, 0.02f, lifetimeRatio * 0.6f, 1f));
        trailShader.TrySetParameter("sheenEdgeColorStrong", new Vector4(0.75f, 0.75f, 0.75f, 1f));
        trailShader.SetTexture(PerlinNoise, 1, SamplerState.LinearWrap);
        trailShader.SetTexture(TextureAssets.Extra[201], 2, SamplerState.LinearWrap);

        UnifiedRandom rng = new UnifiedRandom(Projectile.identity);

        float swingArc = lifetimeRatio * -MathHelper.Pi + rng.NextFloat(MathHelper.TwoPi);
        float slashOffset = rng.NextFloat(150f, 550f);

        float zOffset = MathF.Sin(lifetimeRatio * MathHelper.TwoPi) * 10f; // Example Z offset logic.
        trailShader.TrySetParameter("zOffset", zOffset);

        Vector2[] points = new Vector2[26];
        Matrix transformation = Matrix.CreateRotationX(rng.NextFloatDirection() * 1.3f) *
                                Matrix.CreateRotationY(rng.NextFloatDirection() * 1.2f) *
                                Matrix.CreateTranslation(0, 0, zOffset); // Apply Z offset.
        for (int i = 0; i < points.Length; i++)
        {
            float trailInterpolant = i / (float)points.Length;
            Vector2 offset =
                (MathHelper.Pi * trailInterpolant + swingArc).ToRotationVector2();
            points[i] = Projectile.Center + Vector2.Transform(offset, transformation) * slashOffset * 0.5f;
        }

        PrimitiveRenderer.RenderTrail(points, new PrimitiveSettings(default, default, Shader: trailShader)
        {
            WidthFunction = TrailWidthFunction,
            ColorFunction = TrailColorFunction
        }, 32);
        return false;
    }

}
