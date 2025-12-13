using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho
{
    partial class CryonophoreLimb : BloodMoonBaseNPC
    {
        public override bool canBeSacrificed => false;
        public override bool canBebuffed => false;

        public NPC Owner
        {
            get => Main.npc[OwnerIndex] != null ? Main.npc[OwnerIndex] : default;
        }

        public int OwnerIndex;
        public CryonophoreZooid self;
        public override void SetDefaults()
        {
            NPC.lifeMax = 64_000;
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(30, 30);
            NPC.noGravity = true;
        }

        public override void AI()
        {

            if (currentTarget == null)
            {
                Cryonophore d = Owner.ModNPC as Cryonophore;
                currentTarget = d.currentTarget;
                NPC.Center = Owner.Center;
                
            }
            else
                StateMachine();
            Time++;
        }
        void StateMachine()
        {
            switch (self.type)
            {
                case ZooidType.basic:
                    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.AngleTo(currentTarget.Center).ToRotationVector2()* NPC.velocity.Length(),0.4f);
                    break;

                case ZooidType.Ranged:
                    ManageRanged();
                    break;
            }
        }
        
        void ManageRanged()
        {
            NPC.Center = currentTarget.Center + Main.rand.NextVector2CircularEdge(70, 70);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            Main.EntitySpriteDraw(debug, NPC.Center - screenPos, null, Color.AntiqueWhite, 0, debug.Size() / 2, 10, 0);
            Utils.DrawBorderString(spriteBatch, self.type.ToString(), NPC.Center - screenPos, Color.AntiqueWhite, 0.4f);
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
