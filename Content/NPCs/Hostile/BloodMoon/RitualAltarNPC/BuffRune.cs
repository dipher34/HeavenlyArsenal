using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    internal class BuffRune : BaseParticle
    {
        public static ParticlePool<BuffRune> pool = new ParticlePool<BuffRune>(500, GetNewParticle<BuffRune>);
        public int TimeLeftMax;
        public int TimeLeft;
        public Vector2 position;
        public int Variation;
        public void Prepare(Vector2 Pos, int Variation, int maxTime)
        {
            position = Pos;
            TimeLeftMax = maxTime;
            this.Variation = Variation;
            TimeLeft = 0;
        }
        public override void FetchFromPool()
        {
            TimeLeft = 0;
        }
        public override void Update(ref ParticleRendererSettings settings)
        {
            position.Y -= 1f;

            Main.NewText(TimeLeft);
            TimeLeft++;
            if (TimeLeft > TimeLeftMax)
                ShouldBeRemovedFromRenderer = true;
        }
        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D Tex = AssetDirectory.Textures.Particles.RuneParticle.Value;
            Vector2 DrawPos = position - Main.screenPosition;

            Main.EntitySpriteDraw(Tex, DrawPos, null, Color.AntiqueWhite, 0, Tex.Size() * 0.5f, 1, 0);

        }
    }
}
