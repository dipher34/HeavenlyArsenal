using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Renderers;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Particles;

public class SoundBarrierParticle : BaseParticle
{
    public static ParticlePool<SoundBarrierParticle> pool = new ParticlePool<SoundBarrierParticle>(500, GetNewParticle<SoundBarrierParticle>);

    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public Color ColorTint;
    public int MaxTime;
    public int TimeLeft;
    public float Scale;
    private int Style;
    private Vector2 Offset;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, Color color, float scale)
    {
        Position = position;
        Velocity = velocity;
        Rotation = velocity.ToRotation() + rotation;
        ColorTint = color;
        MaxTime = (int)(20 * scale);
        Scale = scale;
        Offset = Main.rand.NextVector2Circular(10f, 10f);
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Velocity = Vector2.Zero;
        MaxTime = 40;
        TimeLeft = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Position += Velocity;
        Velocity *= 0.8f;

        TimeLeft++;
        if (TimeLeft > MaxTime)
            ShouldBeRemovedFromRenderer = true;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = TextureAssets.BlackTile.Value;
        float progress = MathF.Sqrt((float)TimeLeft / MaxTime);
        Main.spriteBatch.Draw(texture, Position - Main.screenPosition, texture.Frame(), ColorTint, Rotation, texture.Size() * 0.5f, Scale * progress, 0, 0);
    }
}
