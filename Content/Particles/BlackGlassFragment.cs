using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Particles
{
    internal class BlackGlassFragment : BaseParticle
    {
        public static ParticlePool<BlackGlassFragment> pool = new ParticlePool<BlackGlassFragment>(500, GetNewParticle<BlackGlassFragment>);



        public Vector2 position;
        public Vector2 Velocity;
        public Vector2 Variance;

        public float progress;
        public float Rotation;

        public float Scale;
        public float EndScale;

        public int MaxTime;
        public int TimeLeft;

        public int fragIndex;

        public Color GlowColor;
        public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, Color glowColor,
            float scale = 0, float endScale = 0, int FragIndex = -1)
        {
            this.position = position;
            Velocity = velocity;
            Rotation = velocity.ToRotation() + rotation;
            MaxTime = lifeTime;
            Scale = scale;
            GlowColor = glowColor * 1.1f;

            EndScale = endScale;
            if (FragIndex == -1)
            {
                fragIndex = Main.rand.Next(1, 8);
            }
            else
                fragIndex = FragIndex;

        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            Velocity = Vector2.Zero;
            TimeLeft = 0;
            progress = 0;
            //Variance = Rotation.ToRotationVector2()*10;
            
        }

        public override void Update(ref ParticleRendererSettings settings)
        {
            Scale = float.Lerp(Scale, EndScale, 0.02f);
            Velocity *= Main.rand.NextFloat(0.9f, 0.99f);
            position += Velocity;
          
               

            


            Rotation += MathHelper.ToRadians(1);
            TimeLeft++;
            //Main.NewText($"T: {t}, Progress: {progress}, Rotation: {MathHelper.ToDegrees(Rotation)}");

            if (TimeLeft > MaxTime)
                ShouldBeRemovedFromRenderer = true;

        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/BlackGlass_Fragments").Value;
            Texture2D texGlow = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/BlackGlass_Fragments_Glow").Value;

            Rectangle texRect = tex.Frame(1, 7, 0, fragIndex);
            Rectangle GlowtexRect = texGlow.Frame(1, 7, 0, fragIndex);

            Vector2 DrawPos = position - Main.screenPosition;

            Vector2 Origin = new Vector2(texRect.Width/2, texRect.Height/2);
            Vector2 GlowOrigin = new Vector2(GlowtexRect.Width / 2, GlowtexRect.Height/2 );

            float Rot = Rotation;
            float value = progress;
            float adjustedScale = Scale * 1.4f;

            Color AdjustedColor = GlowColor *1.5f;


            Main.EntitySpriteDraw(texGlow, DrawPos, GlowtexRect, AdjustedColor, Rot, Origin, adjustedScale, SpriteEffects.None);

            Main.EntitySpriteDraw(tex, DrawPos, texRect, Color.AntiqueWhite, Rot, Origin, adjustedScale, SpriteEffects.None);

            //Texture2D Debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            //Main.EntitySpriteDraw(Debug, DrawPos, null, Color.AntiqueWhite, 0, Debug.Size() * 0.5f, 3, SpriteEffects.None);
        }
    }
}
