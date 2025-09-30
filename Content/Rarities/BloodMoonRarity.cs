using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;
using static NoxusBoss.Assets.GennedAssets.Sounds;
using Item = Terraria.Item;

namespace HeavenlyArsenal.Content.Rarities
{
    public class BloodMoonRarity : ModRarity
    {
        public override Color RarityColor => new Color(220, 20, 70);
            
        

    }

    public class BloodRarityTooltip : GlobalItem
    {
        // simple droplet state type
        private struct Droplet
        {
            public Vector2 anchor;
            public float size;   // x anchor (relative to name left), initial
            public float VerticalOffset;             // vertical offset (starts negative, increases downward)
            public float speed;         // falling speed
                   
            public float phase;
            public float alpha;     
            
            public float FallInterp;

            /*
             * 
            public Vector2 anchor;// x anchor (relative to name left), initial
            public Vector2 Scale;
          
            public float VerticalOffset;
            public float CurrentSpeed;           // falling speed      
            public float FallInterp;
            public float alpha;       
             */
            // opacity
        }
        public float Time => Main.GlobalTimeWrappedHourly;

        // map by item type -> droplet list (persist across frames)
        private static readonly Dictionary<int, List<Droplet>> dropletMap = new();

        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {

            if (item.rare == ModContent.RarityType<BloodMoonRarity>() && line.Mod == "Terraria" && line.Name == "ItemName")
            {
                string text = item.AffixName();
                DynamicSpriteFont font = FontAssets.MouseText.Value;

                Vector2 basePos = new Vector2(line.X, line.Y);  
                float time = Main.GlobalTimeWrappedHourly;

               
                Texture2D dropletTex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

                
                EnsureDroplets(item.type, text, font);

                Vector2 measured = font.MeasureString(text);
                Vector2 nameOrigin = measured * 0.5f;
                Vector2 namePos = basePos;

                
                // Draw the name with a simple border for legibility (colors are example)
                //Utils.DrawBorderStringFourWay(Main.spriteBatch, font, text, namePos, Color.Red, Color.Black, Vector2.Zero, 1f);
                Utils.DrawBorderString(Main.spriteBatch, text, namePos, Color.Red, 1f);
                // Update & draw droplets
                List<Droplet> list = dropletMap[item.type];
                Vector2 nameLeft = namePos - nameOrigin;
                DrawEclipse(namePos + nameOrigin);


                for (int i = 0; i < list.Count; i++)
                {
                    Droplet d = list[i];

                    // update
                    // tweak these multipliers to taste
                    float fallMultiplier = 0.5f;    // base falling speed multiplier
                    float dt = 1f; // per-tick increment; tooltip updates once per frame/tick in Terraria
                    d.VerticalOffset += d.speed * fallMultiplier * dt;
                    

                    float sway = (float)System.Math.Sin(time * 3f + d.phase) * 2f;

                    float resetThreshold = measured.Y + 20f + d.size * 6f;
                    if (d.VerticalOffset > resetThreshold)
                    {
                        
                        d.VerticalOffset = Main.rand.NextFloat(-10f, -2f);
                        d.speed = Main.rand.NextFloat(10f, 28f); // randomize fall speed
                        d.phase = Main.rand.NextFloat(MathHelper.TwoPi);
                        d.size = Main.rand.NextFloat(4f, 9f);
                        d.alpha = Main.rand.NextFloat(0.6f, 1f);
                    }
                    

                    Vector2 drawPos = nameLeft + new Vector2(d.anchor.X + 60 + sway, d.VerticalOffset + measured.Y *1f);

                    // draw the droplet: dropletTex is 1x1 so we scale width/height
                    // we'll make the droplet a vertical oval pointing down
                    float dropletWidth = d.size * 0.45f;
                    float dropletHeight = d.size * 1.1f;
                    Vector2 drawScale = new Vector2(dropletWidth, dropletHeight);

                    // draw with origin at texture center horizontally, top vertically so it looks like hanging
                    Vector2 origin = new Vector2(0.5f, 0f);
                    Color color = Color.Lerp(Color.DarkRed, Color.Red, 0.4f) * d.alpha;

                    Main.spriteBatch.Draw(
                        dropletTex,
                        drawPos,
                        null,
                        color,
                        0f,
                        origin,
                        drawScale,
                        SpriteEffects.None,
                        0f
                    );

                        
                    Vector2 charBaseline = nameLeft + new Vector2(d.anchor.X + 60, measured.Y * 0.0f);
                    Vector2 TrailOrigin = new Vector2(0.5f, -0.115f);

                    float trailLength = d.VerticalOffset;
                    if (trailLength > 0.5f)
                    {
                        Vector2 trailPos = charBaseline + new Vector2(100f, 0f);
                        Vector2 trailScale = new Vector2(0.6f, trailLength);
                        Main.spriteBatch.Draw(
                            dropletTex,
                            trailPos,
                            null,
                            Color.DarkRed * (d.alpha * 0.6f),
                            0f,
                            TrailOrigin,
                            trailScale,
                            SpriteEffects.None,
                            0f
                        );
                    }
                }

                Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomLine;
                
                Color e = Color.Red with { A = 0 };

                float Value = 1;// (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly)/5);

                Vector2 Scale = new Vector2(0.4f, 0.009f * text.Length);
                Vector2 BarOrigin = new Vector2(glow.Width/2, glow.Width / 2);
                //new Vector2(0, 0);
                Vector2 BarDrawPos = namePos + new Vector2(1, 10);

                float rot = MathHelper.ToRadians(-90);
                Main.EntitySpriteDraw(glow, BarDrawPos, null, e, rot, BarOrigin, Scale, SpriteEffects.None);


                float textScaleInterp = 0;//(float)Math.Abs(Math.Sin(time));
              

                Color A = Color.Lerp(new Color(220, 20, 70), Color.Red, (float)Math.Sin(Main.GlobalTimeWrappedHourly));

                Utils.DrawBorderString(Main.spriteBatch, text, namePos, A);
                return false;
            }

            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        public void DrawEclipse(Vector2 NamePos)
        {
            Texture2D placeholder = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
            Vector2 ModifiedPos = NamePos + new Vector2(0   , 0);
            Vector2 EclipseOrigin = placeholder.Size() * 0.5f;

            Vector2 t = new Vector2(1,0.9f);
            Main.EntitySpriteDraw(placeholder, ModifiedPos, null, Color.Crimson with { A = 0}, 0, EclipseOrigin, t, SpriteEffects.None);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
      
            Vector2 particleDrawCenter = ModifiedPos + new Vector2(0f, 0f);
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

            Vector2 Scale = new Vector2(0.1f);

            Main.EntitySpriteDraw(glow, particleDrawCenter - Main.screenPosition, glow.Frame(), Color.Red with { A = 200 }, 0, glow.Size() * 0.5f, new Vector2(0.12f, 0.25f), 0, 0);
            Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
            Color edgeColor = new Color(1f, 0.06f, 0.06f);
           
            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
            riftShader.TrySetParameter("baseCutoffRadius", 0.24f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
            riftShader.TrySetParameter("vanishInterpolant", 0.01f );
            riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
            riftShader.TrySetParameter("edgeColorBias", 0.1f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            Main.spriteBatch.Draw(innerRiftTexture, particleDrawCenter, null, Color.White, 0 + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, Scale, 0, 0);

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);


        }
        private void EnsureDroplets(int itemType, string text, DynamicSpriteFont font)
        {
            // If we already have a list and its length matches number of letters (or is greater), do nothing.
            if (dropletMap.TryGetValue(itemType, out var existing))
            {
                // allow reusing if same length; otherwise recreate
                if (existing.Count >= text.Length)
                    return;
            }

            // create a new list
            var list = new List<Droplet>();

            // We'll create at least one droplet per character, and sometimes add extra random droplets
            for (int i = 0; i < text.Length; i++)
            {
                // measure the substring up to this character to get a horizontal anchor
                Vector2 measureBefore = font.MeasureString(text.Substring(0, i));
                Vector2 measureChar = font.MeasureString(text.Substring(i, 1));
                float charCenterX = measureBefore.X + measureChar.X * 0.5f;

                // primary droplet attached to this char
                var d = new Droplet()
                {
                    anchor = new Vector2(charCenterX, 0f),
                    VerticalOffset = Main.rand.NextFloat(-20f, -2f),
                    speed = Main.rand.NextFloat(12f, 24f),
                    size = Main.rand.NextFloat(4f, 8.5f),
                    phase = Main.rand.NextFloat(MathHelper.TwoPi),
                    alpha = Main.rand.NextFloat(0.5f, 1f)
                };
                list.Add(d);

                // randomly add an occasional extra droplet near this char
                if (Main.rand.NextBool(6))
                {
                    var d2 = new Droplet()
                    {
                        anchor = new Vector2(charCenterX + Main.rand.NextFloat(-4f, 6f), 0f),
                        VerticalOffset = Main.rand.NextFloat(-30f, -4f),
                        speed = Main.rand.NextFloat(8f, 30f),
                        size = Main.rand.NextFloat(3f, 7f),
                        phase = Main.rand.NextFloat(MathHelper.TwoPi),
                        alpha = Main.rand.NextFloat(0.35f, 0.95f)
                    };
                    list.Add(d2);
                }
            }

            dropletMap[itemType] = list;
        }
    }
}