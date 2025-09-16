using HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;
using HeavenlyArsenal.Core;
using Humanizer;
using Luminance.Common.Easings;
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
using static tModPorter.ProgressUpdate;

namespace HeavenlyArsenal.Content.Particles
{
    internal class CollapseParticle : BaseParticle
    {
        public static ParticlePool<CollapseParticle> pool = new ParticlePool<CollapseParticle>(500, GetNewParticle<CollapseParticle>);

        public PiecewiseCurve CollapseCurve;

        public Entity attache;
        public Vector2 position;
        public Vector2 Velocity;

        public float t;
        public float BaseProgress;
        public float progress;
        public float Rotation;

        public float Scale;
        public float EndScale;

        public int MaxTime;
        public int TimeLeft;

        public Color GlowColor;
        public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, float scale, float endScale, float startprog, Color glowColor, Entity sap = null)
        {
            this.position = position;
            Velocity = velocity;
            Rotation = velocity.ToRotation() + rotation;
            MaxTime = lifeTime;
            Scale = scale;
            GlowColor = glowColor * 1.1f;

            EndScale = endScale;
            attache = sap;
            BaseProgress = startprog;

        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            Velocity = Vector2.Zero;
            TimeLeft = 0;
            t = 0;
            progress = 0;
           
        }

        public override void Update(ref ParticleRendererSettings settings)
        {
            if(TimeLeft == 0)
            {
                t = BaseProgress;
                CollapseCurve = new PiecewiseCurve()
                    //.Add(EasingCurves.Linear, EasingType.InOut, 0.5f, 0.01f)
                    //.Add(EasingCurves.Exp, EasingType.In, 1f, 1f);
                    .Add(EasingCurves.Exp, EasingType.InOut, 0f, 0.3f, 1f)
                    .Add(EasingCurves.Linear, EasingType.InOut, 0, 0.35f)
                    .Add(EasingCurves.Cubic, EasingType.Out, 1f, 1f);

               // Main.NewText($"Base:{BaseProgress}, t: {t}");
            }
                
            //CollapseCurve = new PiecewiseCurve()
            //      .Add(EasingCurves.Sextic, EasingType.Out, 1f, 0.5f)
            //      .Add(EasingCurves.Quadratic, EasingType.Out, 0f, 1f);


            position += Velocity;
            Velocity *= 0.8f;
            if (attache != null)
                position = attache.Center;

            GlowColor = Color.Lerp(GlowColor, Color.White, 0.02f);

            progress = CollapseCurve.Evaluate(t);

            Rotation += MathHelper.ToRadians(20) * (progress + 0.2f);
           
            t = Math.Clamp(t + 0.01f, 0, 1);

            if (t == 1)
                ShouldBeRemovedFromRenderer = true;

            if (TimeLeft > MaxTime)
                ShouldBeRemovedFromRenderer = true;
            if (attache != null)
            {
                NPC a = attache as NPC;
                if (a.GetGlobalNPC<Collapse>().Collapsing)
                {

                    ShouldBeRemovedFromRenderer = true;
                   
                }
            }

            TimeLeft++;
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.Corona;
            Texture2D Glowtex = GennedAssets.Textures.GreyscaleTextures.BloomCircle;

            Vector2 DrawPos = position - Main.screenPosition;

            Vector2 Origin = tex.Size() * 0.5f;

            float Rot = Rotation;
            float value = progress;
            float adjustedScale = Scale * (1 - progress);

            Color AdjustedColor = GlowColor with { A = 0 } * value;

            Main.EntitySpriteDraw(Glowtex, DrawPos, null, AdjustedColor, 0, Glowtex.Size() * 0.5f, adjustedScale, SpriteEffects.None);
            Main.EntitySpriteDraw(tex, DrawPos, null, AdjustedColor, Rot, Origin, adjustedScale, SpriteEffects.None);
            Main.EntitySpriteDraw(tex, DrawPos, null, AdjustedColor, Rot - MathHelper.Pi, Origin, adjustedScale, SpriteEffects.None);
            //Utils.DrawBorderString(Main.spriteBatch, t.ToString(), DrawPos, Color.AntiqueWhite);
        }
    }
}
