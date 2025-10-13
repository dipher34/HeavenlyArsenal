using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    public class PerversionOfFaith : BloodmoonBaseNPC
    {
        public int Time
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public override int bloodBankMax
        {
            get => base.bloodBankMax;
            set => base.bloodBankMax = value;
        }
        public override void SetDefaults()
        {
            
            NPC.aiStyle = -1;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.Size = new Vector2(30, 30);
            NPC.defense = 400;
            NPC.damage = 300;
            NPC.lifeMax = 400;


        }
        public override void OnSpawn(IEntitySource source)
        {

        }
        public override void AI()
        {
            this.playerTarget = Main.player[NPC.FindClosestPlayer()];
            NPC.velocity = NPC.Center.AngleTo(playerTarget.Center).ToRotationVector2();
            NPC.velocity += new Vector2(0, MathF.Sin(Time));

            Time++;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Mouth").Value;

            Vector2 DrawPos = NPC.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            SpriteEffects effects = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(tex, DrawPos, null, drawColor, 0, origin, 1, effects);

            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
