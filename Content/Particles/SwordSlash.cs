using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Particles;

public class SwordSlash : BaseParticle
{
    public static ParticlePool<SwordSlash> pool = new ParticlePool<SwordSlash>(500, GetNewParticle<SwordSlash>);

    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public int MaxTime;
    public int TimeLeft;
    public Color ColorTint;
    public Color ColorGlow;
    public float Scale;
    private int Style;
    private int SpriteEffect;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, Color color, Color glowColor, float scale)
    {
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        MaxTime = lifeTime;
        ColorTint = color;
        ColorGlow = glowColor;
        Scale = scale;
        Style = Main.rand.Next(3);
        SpriteEffect = Main.rand.Next(2);
        Main.NewText($"SwordSlash Drawn!", Color.AntiqueWhite);
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Velocity = Vector2.Zero;
        MaxTime = 1;
        TimeLeft = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Position += Velocity;
        Velocity += new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
        Velocity *= 1.1f;

        TimeLeft++;
        if (TimeLeft > MaxTime)
            ShouldBeRemovedFromRenderer = true;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/swordslash").Value;

        texture.Frame();
        float progress = (float)TimeLeft / MaxTime;

        // Calculate the frame based on the progress
        int frameCount = 4; // Change to 4 frames
        int currentFrame = (int)(progress * frameCount) % frameCount; // Cycle through frames
        Rectangle frame = texture.Frame(1, 1, currentFrame, Style);

        // Calculate the alpha value for fading
        float alpha = 1f - progress;

        // Apply the alpha value to the draw color
        Color drawColor = Color.Lerp(ColorTint, ColorGlow, Utils.GetLerpValue(0.3f, 0.7f, progress, true)) * Utils.GetLerpValue(1f, 0.9f, progress, true) * alpha;

        // Adjust the scale based on the progress
        float widthScale = Scale * (1f - progress); // Decrease the width over time
        float heightScale = Scale; // Keep the height constant

        Vector2 anchorPosition = new Vector2(frame.Width / 2, frame.Height);

        // Draw the particle with the adjusted scale
        spritebatch.Draw(texture, Position + settings.AnchorPosition, frame, drawColor, Rotation, texture.Size() * 0.5f, new Vector2(widthScale, heightScale), (SpriteEffects)SpriteEffect, 0);
    }
}
