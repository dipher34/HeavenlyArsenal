using System.Collections.Generic;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using ReLogic.Graphics;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Rarities;

public sealed class BloodMoonRarityGlobalItem : GlobalItem
{
    public float Time => Main.GlobalTimeWrappedHourly;
    
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.rare == ModContent.RarityType<BloodMoonRarity>();
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod != "Terraria" || line.Name != "ItemName")
        {
            return true;
        }
        
        var text = item.AffixName();
        var font = FontAssets.MouseText.Value;

        var basePos = new Vector2(line.X, line.Y);
        var time = Main.GlobalTimeWrappedHourly;

        Texture2D dropletTex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

        var measured = font.MeasureString(text);
        var nameOrigin = measured * 0.5f;
        var namePos = basePos;

        // Draw the name with a simple border for legibility (colors are example)
        //Utils.DrawBorderStringFourWay(Main.spriteBatch, font, text, namePos, Color.Red, Color.Black, Vector2.Zero, 1f);
        Utils.DrawBorderString(Main.spriteBatch, text, namePos, Color.Red);

        DrawEclipse(namePos + nameOrigin);

        Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomLine;

        var e = Color.Red with
        {
            A = 0
        };

        float Value = 1; // (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly)/5);

        var Scale = new Vector2(0.4f, 0.009f * text.Length);
        var BarOrigin = new Vector2(glow.Width / 2, glow.Width / 2);
        //new Vector2(0, 0);
        var BarDrawPos = namePos + new Vector2(1, 10);

        var rot = MathHelper.ToRadians(-90);
        Main.EntitySpriteDraw(glow, BarDrawPos, null, e, rot, BarOrigin, Scale, SpriteEffects.None);

        float textScaleInterp = 0; //(float)Math.Abs(Math.Sin(time));

        var A = Color.Lerp(new Color(220, 20, 70), Color.Red, (float)Math.Sin(Main.GlobalTimeWrappedHourly));

        Utils.DrawBorderString(Main.spriteBatch, text, namePos, A);

        return false;
    }
    
    private static void DrawEclipse(Vector2 NamePos)
    {
        Texture2D placeholder = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
        var ModifiedPos = NamePos + new Vector2(0, 0);
        var EclipseOrigin = placeholder.Size() * 0.5f;

        var t = new Vector2(1, 0.9f);

        Main.EntitySpriteDraw
        (
            placeholder,
            ModifiedPos,
            null,
            Color.Crimson with
            {
                A = 0
            },
            0,
            EclipseOrigin,
            t,
            SpriteEffects.None
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        var particleDrawCenter = ModifiedPos + new Vector2(0f, 0f);
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        var Scale = new Vector2(0.1f);

        Main.EntitySpriteDraw
        (
            glow,
            particleDrawCenter - Main.screenPosition,
            glow.Frame(),
            Color.Red with
            {
                A = 200
            },
            0,
            glow.Size() * 0.5f,
            new Vector2(0.12f, 0.25f),
            0
        );

        var innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
        var edgeColor = new Color(1f, 0.06f, 0.06f);

        var shader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        
        shader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
        shader.TrySetParameter("baseCutoffRadius", 0.24f);
        shader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
        shader.TrySetParameter("swirlOutwardnessFactor", 3f);
        shader.TrySetParameter("vanishInterpolant", 0.01f);
        shader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        shader.TrySetParameter("edgeColorBias", 0.1f);
        
        shader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        shader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
        
        shader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, particleDrawCenter, null, Color.White, 0 + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, Scale, 0, 0);

        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
    }
}