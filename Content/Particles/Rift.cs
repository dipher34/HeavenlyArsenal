using HeavenlyArsenal.Core;
using HeavenlyArsenal.Content.Particles;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using CalamityMod;
using Terraria.Graphics.Renderers;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Particles;

public class Rift : BaseParticle
{
    public static ParticlePool<Rift> pool = new ParticlePool<Rift>(200, GetNewParticle<Rift>);

    private Vector2 Squish;
    private Color BaseColor;
    private float OriginalScale;
    private float FinalScale;
    private float Opacity;
    private float Rotation;

    public Vector2 Position;
    public Vector2 Velocity;
    public float Scale;
    public int MaxTime;
    public int TimeLeft;

    public void Prepare(Vector2 position, Vector2 velocity, Color color, Vector2 squish, float rotation, float originalScale, float finalScale, int lifetime)
    {
        Position = position;
        Velocity = velocity;
        BaseColor = color;
        Squish = squish;
        Rotation = rotation;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        MaxTime = lifetime;
        TimeLeft = lifetime;
        Scale = originalScale;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Velocity = Vector2.Zero;
        MaxTime = 40;
        TimeLeft = 0;
        Scale = 1f;
        Rotation = 0f;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        TimeLeft--;
        if (TimeLeft <= 0)
        {
            ShouldBeRemovedFromRenderer = true;
            return;
        }

        float completion = 1f - (TimeLeft / (float)MaxTime);

        // Smooth scaling using piecewise animation
        float scaleLerp = CalamityUtils.PiecewiseAnimation(completion, new CalamityUtils.CurveSegment(CalamityUtils.EasingType.PolyOut, 0f, 0f, 1f, 4));
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, scaleLerp);

        // Sinusoidal opacity
        Opacity = (float)Math.Sin(MathF.PI / 2f + completion * (MathF.PI / 2f));
        Rotation = -Velocity.ToRotation();
        //Velocity *= 0.95f;
        Position += Velocity;
       
        // Optional: Add light (if desired)
        Lighting.AddLight(Position, BaseColor.ToVector3() * Opacity * 0.6f);
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch)
    {
       
        Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

       // float vanishTime = Utils.GetLerpValue(0, 20, 40, true) * Utils.GetLerpValue(0, 20, 4, true);

        
        Vector2 offset = Velocity.SafeNormalize(Vector2.Zero);
        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        Main.EntitySpriteDraw(glow, Position + offset - Main.screenPosition, glow.Frame(), Color.Red with { A = 200 } , Rotation, glow.Size() * 0.5f, new Vector2(0.12f, 0.25f) , 0, 0);

        Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
        Color edgeColor = new Color(1f, 0.06f, 0.06f);
        float timeOffset = Main.myPlayer * 2.5552343f;

        ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f + timeOffset);
        riftShader.TrySetParameter("baseCutoffRadius", 0.3f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
        riftShader.TrySetParameter("vanishInterpolant", 0.01f );
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0.1f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, Position + offset - Main.screenPosition, null, Color.White, Rotation + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, new Vector2(0.2f, 0.4f), 0, 0);

        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        
    }
}
