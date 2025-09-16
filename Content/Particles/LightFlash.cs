using HeavenlyArsenal.Core;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles
{
    internal class LightFlash : BaseParticle
    {
        public static ParticlePool<LightFlash> pool = new ParticlePool<LightFlash>(500, GetNewParticle<LightFlash>);

        public Vector2 position;
        public Vector2 Velocity;
        public float Rotation;
        public float progress;
        public int MaxTime;
        public int TimeLeft;
        public float Scale;
        public Color GlowColor;
        public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, float scale, Color glowColor)
        {
            this.position = position;
            Velocity = velocity;
            Rotation = velocity.ToRotation() + rotation;
            MaxTime = lifeTime;
            Scale = scale;
            GlowColor = glowColor;
            progress = 0;
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
            position += Velocity;
            Velocity *= 0.8f;
            progress = float.Lerp(progress, 20, 0.2f);

            GlowColor *= 1.1f;
            TimeLeft++;
            if (TimeLeft > MaxTime)
                ShouldBeRemovedFromRenderer = true;
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D Spire = GennedAssets.Textures.GreyscaleTextures.ChromaticSpires;
            Texture2D Star = GennedAssets.Textures.GreyscaleTextures.FourPointedStar;
            Texture2D texture = GennedAssets.Textures.GreyscaleTextures.ChromaticBurst;
           


            Vector2 DrawPos = position - Main.screenPosition;
            SpriteEffects flip = SpriteEffects.None;
            Vector2 GlowSize = new Vector2(0.05f, 0.05f) * Scale * (float)Math.Abs(1+ Math.Cos(TimeLeft/60));// * (1f + progress * 0.5f) * 0.05f;

            Vector2 Size = new Vector2(0.055f) * progress * Scale;

            Vector2 GlowOrigin =    Spire.Size() * 0.5f;
            Vector2 TexOrigin = texture.Size() * 0.5f;

            Color Adjusted = GlowColor with { A = 0 } * (20 - progress);

            Main.spriteBatch.Draw(Spire, DrawPos, Spire.Frame(), Adjusted, Rotation, GlowOrigin, Size, flip, 0);

            Main.spriteBatch.Draw(texture, DrawPos, null, Adjusted, Rotation, TexOrigin, Size * 0.5f, flip, 0);
            Main.spriteBatch.Draw(Star, DrawPos, null, Adjusted, Rotation + MathHelper.ToRadians(90), Star.Size() * 0.5f,Size, flip, 0);

        }

        
    }
}
