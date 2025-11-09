using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.LeverAction;

public class AvatarRifle_MuzzleFlash : BaseParticle
{
    public static ParticlePool<AvatarRifle_MuzzleFlash> pool = new ParticlePool<AvatarRifle_MuzzleFlash>(500, GetNewParticle<AvatarRifle_MuzzleFlash>);

    public Vector2 Position;
    
    public float Rotation;
    public int MaxTime;
    public int TimeLeft;


    public void Prepare(Vector2 position, float Rotation, int Maxtime)
    {
        this.MaxTime = Maxtime;
        this.Position = position;
        this.Rotation = Rotation;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        MaxTime = 1;
        TimeLeft = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
       

        TimeLeft++;
        if (TimeLeft > MaxTime)
            ShouldBeRemovedFromRenderer = true;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Ranged/LeverAction/AvatarRifle_MuzzleFLash").Value;
        float progress = (float)TimeLeft / MaxTime;
        int frameCount = (int)MathF.Floor(MathF.Sqrt(progress) * 7);
        Rectangle frame = texture.Frame(1, 7, 0, frameCount);

        float alpha = 1f - progress;

        Color drawColor = Color.AntiqueWhite;
        Vector2 anchorPosition = new Vector2(frame.Width /2, frame.Height/6);

        Vector2 DrawPos = Position - settings.AnchorPosition;
        spritebatch.Draw(texture, DrawPos, frame, drawColor, Rotation, texture.Size() * 0.5f, 1, 0, 0);

    }

}