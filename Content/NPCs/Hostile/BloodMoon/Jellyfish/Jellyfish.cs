using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    public class BloodJelly : BloodmoonBaseNPC
    {
        #region Setup
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/BloodJelly";
        public enum JellyState
        {
            Idle,
            TrackTarget
        }
        public int Time
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public JellyState CurrentState
        {
            get => (JellyState)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        public ref float SquishInterp => ref NPC.localAI[0];

        #endregion
        public override void SetDefaults()
        {
            NPC.lifeMax = 40000;
            NPC.damage = 300;
            NPC.defense = 300;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.Size = new Vector2(40, 80);
            NPC.aiStyle = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SquishInterp = 1;
            
        }

        public override void AI()
        {
            //SquishInterp = Utils.Remap( NPC.oldVelocity.Y - NPC.velocity.Y, 1, -1, 0.8f, 1.4f);

            NPC.velocity = new Vector2(0, 2f * (0.7f-SquishInterp)).RotatedBy(NPC.rotation);
            if(Time > 120)
                BoostUp();
            else
            {
                SquishInterp = float.Lerp(SquishInterp, 1, 0.2f);
            }

                Time++;
        }

        private void BoostUp()
        {
            NPC.velocity = new Vector2(0, -2).RotatedBy(NPC.rotation);
            SquishInterp = float.Lerp(SquishInterp, 0.7f, 0.2f);
            if(Time > 300)
            {
                Time = 0;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if(NPC.IsABestiaryIconDummy)
                return base.PreDraw(spriteBatch, screenPos, drawColor);

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            if (tex == null)
                return false;
            Vector2 DrawPos = NPC.Center - Main.screenPosition;
            Vector2 Origin = tex.Size() * 0.5f;
            Vector2 Squish = new Vector2(1 * SquishInterp, 1) * 0.5f ;

            Main.EntitySpriteDraw(tex, DrawPos, null, drawColor, NPC.rotation, Origin, Squish, 0);

            
            return false;
        }
    }
}
