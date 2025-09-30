using HeavenlyArsenal.Common.Utilities;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.Accessories.VanityEffects;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Utilities;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;
using static NoxusBoss.Core.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath
{
    public class Voidcrest_DrawLayer : PlayerDrawLayer
    {
        /// <summary>
        /// The render target responsible for rendering the Halo for players.
        /// </summary>
        /// <remarks> directly stolen from portal skirt. thanks, Lucille!!</remarks>
        public static InstancedRequestableTarget HaloTarget
        {
            get;
            private set;
        }
        public override void Load()
        {
            On_PlayerDrawLayers.DrawPlayer_08_Backpacks += What;
            //On_Player.UpdateItemDye += FindSkirtItemDyeShader;

            HaloTarget = new InstancedRequestableTarget();
            Main.ContentThatNeedsRenderTargets.Add(HaloTarget);
        }
        /// <summary>
        /// I love code! 
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="drawinfo"></param>
        private void What(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawinfo)
        {
            if (drawinfo.drawPlayer.GetValueRef<bool>(VoidCrestOath.HaloEquippedVariableName))
            {
                Draw(ref drawinfo);
                return;
            }

            orig(ref drawinfo);
        }
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Backpacks);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetModPlayer<VoidCrestOathPlayer>().voidCrestOathEquipped || drawInfo.drawPlayer.GetModPlayer<VoidCrestOathPlayer>().Vanity;
        public override bool IsHeadLayer => true;
    

        private static void RenderIntoTarget()
        {
            if (GennedAssets.Textures.Noise.WavyBlotchNoise.Uninitialized || !GennedAssets.Textures.Noise.WavyBlotchNoise.Asset.IsLoaded)
                return;
            if (GennedAssets.Textures.Noise.BurnNoise.Uninitialized || !GennedAssets.Textures.Noise.BurnNoise.Asset.IsLoaded)
                return;


            float Rot = MathHelper.ToRadians(45);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null);
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;
            Main.EntitySpriteDraw(glow, ViewportSize * 0.5f, glow.Frame(),
                Color.Red with { A = 200 }, Rot, glow.Size() * 0.5f, new Vector2(0.1f, 0.05f) * 0.275f, 0, 0);
            Texture2D innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
            riftShader.TrySetParameter("baseCutoffRadius", 0.24f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
            riftShader.TrySetParameter("vanishInterpolant", 0.01f);
            riftShader.TrySetParameter("edgeColor", new Vector4(1f, 0.08f, 0.08f, 1f));
            riftShader.TrySetParameter("edgeColorBias", 0.15f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            Main.spriteBatch.Draw(innerRiftTexture, ViewportSize * 0.5f, null, new Color(77, 0, 2), 0f, innerRiftTexture.Size() * 0.5f, ViewportSize / innerRiftTexture.Size() * new Vector2(0.1f, 0.05f), 0, 0f);
            Main.spriteBatch.End();
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
                return;

            HaloTarget.Request(208, 100, drawInfo.drawPlayer.whoAmI, RenderIntoTarget);
            if (!HaloTarget.TryGetTarget(drawInfo.drawPlayer.whoAmI, out RenderTarget2D? portalTexture) || portalTexture is null)
                return;

            float val = (float)Math.Sin(Main.GlobalTimeWrappedHourly / 2) * 2;
            float Rot = drawInfo.drawPlayer.fullRotation + MathHelper.ToRadians(drawInfo.drawPlayer.direction * -45);
            Vector2 position = drawInfo.HeadPosition() + new Vector2(0, -20f + val).RotatedBy(Rot);


            DrawData rift = new DrawData(portalTexture, position, null, Color.White, Rot + MathHelper.Pi, portalTexture.Size() * 0.5f, 1f, 0, 0f)
            {
               
            };
            drawInfo.DrawDataCache.Add(rift);

            Player Owner = drawInfo.drawPlayer;

            //Draw_Halo(ref drawInfo, Owner);
            drawRift(ref drawInfo);
        }
        public void drawRift(ref PlayerDrawSet drawInfo)
        {

            float val = (float)Math.Sin(Main.GlobalTimeWrappedHourly / 2) * 2;
            float Rot = drawInfo.drawPlayer.fullRotation + MathHelper.ToRadians(drawInfo.drawPlayer.direction * -45);

            Vector2 position = drawInfo.HeadPosition() + new Vector2(0, -20f + val).RotatedBy(Rot); 
            Vector2 particleDrawCenter = position + new Vector2(0f, 0f); 
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value; 
            Main.EntitySpriteDraw(glow, particleDrawCenter, glow.Frame(), 
                Color.Red with { A = 200 }, Rot, glow.Size() * 0.5f, new Vector2(0.25f, 0.12f) * 0.275f, 0, 0);
            Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value; Color edgeColor = new Color(1f, 0.06f, 0.06f);
            float timeOffset = Main.myPlayer * 2.5552343f; ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f + timeOffset); 
            riftShader.TrySetParameter("baseCutoffRadius", 0.24f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 3f); 
            riftShader.TrySetParameter("vanishInterpolant", 0.01f);
            riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4()); 
            riftShader.TrySetParameter("edgeColorBias", 0.1f); 
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap); riftShader.Apply(); 
            Main.spriteBatch.Draw(innerRiftTexture, particleDrawCenter, null, Color.White, Rot + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, new Vector2(0.1f, 0.05f), 0, 0); 
          
            Main.spriteBatch.ResetToDefault();
        }
        protected void Draw_Halo(ref PlayerDrawSet drawInfo, Player Owner)
        {
            Texture2D Glow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/Metaballs/BasicCircle").Value;
            float val = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2);
            Vector2 DrawPos = drawInfo.HeadPosition() + new Vector2(22 * -Owner.direction, val - 10);
            Vector2 origin = Glow.Size() * 0.5f;
            Color BaseheadColor = Color.AntiqueWhite;
            Vector2 Scale = new Vector2(1) * 0.267f;

            DrawData value = new DrawData(Glow, DrawPos, null, BaseheadColor with { A = 0 }, 0f, origin, Scale, SpriteEffects.None);
            drawInfo.DrawDataCache.Add(value);

        }
    }
}
