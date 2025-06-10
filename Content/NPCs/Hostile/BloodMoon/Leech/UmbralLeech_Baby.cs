using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{

    public enum BabyLeechState
    {
        disperse,

        Dead
    }
    class UmbralLeech_Baby : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;

        }
        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.EyeballFlyingFish);
            NPC.waterMovementSpeed = 10;
            NPC.lifeMax = 4873;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            NPC.hide = false;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            base.SetBestiary(database, bestiaryEntry);
        }

        public override void AI()
        {
            base.AI();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            int value = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;
            int FrameCount = 5;
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_Baby").Value;
            Rectangle lech  = new Rectangle(0, value, texture.Width, texture.Height/FrameCount);

            Vector2 origin = new Vector2(texture.Width / 2f, (texture.Height /FrameCount)/2f);

            Vector2 drawPos = NPC.Center - Main.screenPosition;
            //Main.NewText($"{value}");
            if (!NPC.IsABestiaryIconDummy)
            {
                // Have a shader prepared, only special thing is that it uses a normalized matrix
                ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.thing");
                trailShader.SetTexture(GennedAssets.Textures.Noise.SwirlNoise, 0, SamplerState.PointClamp);
                trailShader.TrySetParameter("Time", Main.GlobalTimeWrappedHourly);
                trailShader.TrySetParameter("Resolution",1);
                //trailShader.TrySetParameter("uColor", Color.White.ToVector4() * 0.66f);
                trailShader.Apply();
            }
            Main.EntitySpriteDraw(texture, drawPos, lech, drawColor, NPC.rotation, origin , 1, SpriteEffects.None, 0f);
            return false;
        }
    }
}
