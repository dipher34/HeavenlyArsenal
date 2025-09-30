using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
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
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath
{
    internal class VoidCrest_DisintegrateParticle : BaseParticle
    {
        public static ParticlePool<VoidCrest_DisintegrateParticle> pool = new ParticlePool<VoidCrest_DisintegrateParticle>(500, GetNewParticle<VoidCrest_DisintegrateParticle>);

        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public int MaxTime;
        public int TimeLeft;
        public float Scale;
        public float Opacity;
        public int Frame;
        public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime)
        {
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            MaxTime = lifeTime;
            Scale = 0;
            Opacity = 0;
            Frame = Main.rand.Next(0, 30);
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
            Scale = float.Lerp(Scale, 1, 0.15f);
            if(TimeLeft < MaxTime/2)
                Opacity = float.Lerp(Opacity, 1, 0.2f);
            else
               Opacity = float.Lerp(Opacity, 0, 0.2f);
            TimeLeft++;
            if (TimeLeft > MaxTime)
                ShouldBeRemovedFromRenderer = true;
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
            Texture2D texture = AssetDirectory.Textures.Items.Accessories.VoidCrestOath.VoidSigil.Value;
            Texture2D texture2 = AssetDirectory.Textures.Items.Accessories.VoidCrestOath.VoidSigil2.Value;
            Vector2 DrawPos = Position - Main.screenPosition;

            Rectangle frame = texture.Frame(1, 30, 0, Frame);
            Rectangle frame2 = texture2.Frame(1, 30, 0, Frame);

            Vector2 Origin = new Vector2(frame.Width / 2, frame.Height / 2);
            Vector2 Origin2 = new Vector2(frame2.Width / 2, frame2.Height / 2);

            Vector2 scale = new Vector2(5) * (1 - Scale);

            Color BaseColor = Color.Purple with { A = 0 };
            Color A = BaseColor  * Opacity;

            Color B = BaseColor * Opacity;
            float Rot = MathHelper.ToRadians(90) * 10*(1 - Scale);

            Main.EntitySpriteDraw(Glow, DrawPos, null, BaseColor, Rot, Glow.Size() * 0.5f, scale *0.5f, SpriteEffects.None);

            Main.EntitySpriteDraw(texture, DrawPos, frame, A, Rot, Origin, scale, SpriteEffects.None);

            Main.EntitySpriteDraw(texture2, DrawPos, frame, B, Rot, Origin, scale, SpriteEffects.None);
        }
    }
}
