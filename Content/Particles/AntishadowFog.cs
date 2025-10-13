using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles
{
    internal class AntishadowFog : BaseParticle
    {
        public static ParticlePool<AntishadowFog> pool = new ParticlePool<AntishadowFog>(500, GetNewParticle<AntishadowFog>);

        public int timeLeft;
        public int timeLeftMax;

        public float Progress = 0;
        public float Scale;
        public Vector2 StartPos;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
       
        public void Prepare(Vector2 Position, Vector2 initialVelocity, int LifeTime, float Scale, Vector2 StartPos)
        {
            this.Position = Position;
            this.Velocity = initialVelocity;
            timeLeftMax = LifeTime;
            this.Scale = Scale;
            this.StartPos = StartPos;
            Rotation = initialVelocity.ToRotation();
        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            timeLeft = 0;
            Progress = 0;


        }
        public override void Update(ref ParticleRendererSettings settings)
        {
            if (StartPos != Vector2.One)
            {
                if(timeLeft/(float)timeLeftMax > 0.2f)
                {
                    Velocity = Vector2.Lerp( Velocity, Position.AngleTo(StartPos).ToRotationVector2()*2, 0.5f);
                }
            }
            Position += Velocity;
            Progress = float.Lerp(Progress, 1, 0.02f);

            if (timeLeft > timeLeftMax)
                ShouldBeRemovedFromRenderer = true;
            timeLeft++;
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D tex = GennedAssets.Textures.Particles.FireParticleA;
            Texture2D Debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            Vector2 DrawPos = Position - Main.screenPosition;

            int columns = 6;
            int rows = 6;
            int totalFrames = columns * rows; // 36 total frames

            // progress = 0 → 1
            float progress = this.Progress;

            // your starting frame offset (e.g., skip the first 12 frames)
            int frameOffset = 1;

            // compute frame index and wrap around if needed
            int frameIndex = (int)(progress * (totalFrames - 1)) + frameOffset;

            // keep it in range 0 → totalFrames-1
            frameIndex = Math.Min(frameIndex, totalFrames - 1);

            // convert to grid coordinates
            int frameX = frameIndex % columns;
            int frameY = frameIndex / columns;

            // now grab the correct rectangle
            Rectangle Frm = tex.Frame(columns, rows, frameX, frameY);



            Color a = Color.DimGray with { A = 0 };
            Vector2 Origin = new Vector2(Frm.Width / 2, Frm.Height / 2 + 45);
            Main.EntitySpriteDraw(tex, DrawPos, Frm, a, Rotation, Origin, Scale, 0);
            //Main.EntitySpriteDraw(Debug, DrawPos, null, a, Rotation, Debug.Size()*0.5f, 14, 0);

            //Utils.DrawBorderString(Main.spriteBatch, (this.timeLeft / (float)timeLeftMax).ToString(), DrawPos, Color.AntiqueWhite);
        }
    }
}
