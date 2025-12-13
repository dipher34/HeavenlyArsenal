using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho
{
    partial class Cryonophore
    {
        public override void DrawBehind(int index)
        {
            base.DrawBehind(index);
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            base.PostDraw(spriteBatch, screenPos, drawColor);
        }


        void RenderCore(Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/sipho/Cryonophore_Core").Value;

            Vector2 DrawPos = NPC.Center - screenPos;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height - 12);
            Main.EntitySpriteDraw(texture, DrawPos, null, drawColor, 0, origin, 2, 0);
        }
        void RenderLimbs(Vector2 screenPos, Color drawColor)
        {
            foreach(var plac in OwnedZooids)
            {
                var limb = plac.Value.Item2;
                if (limb != null)
                    return;
                //todo: replace wiht appropriate texture paths later
                Texture2D tex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
                Vector2 drawPos = NPC.Center - screenPos - new Vector2(plac.Value.Item1.id * 10 - 20, 0);
                float rot = MathHelper.ToRadians(MathF.Sin(Main.GlobalTimeWrappedHourly + plac.Value.Item1.id) * 30);
                Main.EntitySpriteDraw(tex, drawPos, null, drawColor with { A = 0 }, rot, new Vector2(tex.Width/2, 0), new Vector2(2,40), 0);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            RenderCore(screenPos, drawColor);
            RenderLimbs(screenPos, drawColor);
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
