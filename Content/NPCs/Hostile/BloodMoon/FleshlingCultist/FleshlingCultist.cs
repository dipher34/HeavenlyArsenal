using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.FleshlingCultist
{
    partial class FleshlingCultist : BloodmoonBaseNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/FleshlingCultist/FleshlingCultist";
        public override int SacrificePrio => 1;
        public override int bloodBankMax => 30;
        public ref float Time => ref NPC.ai[0];
        public override void SetStaticDefaults()
        {
            NPCID.Sets.ReflectStarShotsInForTheWorthy[Type] = true;
            
            Main.npcFrameCount[Type] = 7;

            ContentSamples.NpcBestiaryRarityStars[Type] = 4;
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 40_000;
            NPC.damage = 100;
            NPC.defense = 27;
            NPC.knockBackResist = 0.4f;
            NPC.Size = new Vector2(30, 50);
        }

        public override void OnSpawn(IEntitySource source)
        {
            blood = 0;
            CheckForEmptyCults();
            if (CultistCoordinator.GetCultOfNPC(NPC) != null)
                CurrentState = Behaviors.Worship;
            else
            {
                CurrentState = Behaviors.BlindRush;
            }
        }
        public override void AI()
        {
            StateMachine();

            NPC.direction = (NPC.velocity.X != 0 ? Math.Sign(NPC.velocity.X) : 1);
            NPC.spriteDirection = -NPC.direction;

            if (Time % 120 == 0)
            {
                CheckForEmptyCults();
            }
            Time++;
        }

        void CheckForEmptyCults()
        {
            if (CultistCoordinator.Cults.Count > 0)
            {
                foreach (var kvp in CultistCoordinator.Cults)
                {
                    Cult cult = kvp.Value;

                    if (cult.Leader.Center.Distance(NPC.Center) > 200)
                        continue;

                    if (cult.Cultists.Count < cult.MaxCultists)
                    {
                        CultistCoordinator.AttachToCult(cult.CultID, NPC);
                        break;
                    }


                }

            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit)
        {
            blood += 2;
        }


        public override void FindFrame(int frameHeight)
        {
            // Determine if NPC is moving
            if (Math.Abs(NPC.velocity.X) > 0.1f)
            {
                // Walking animation
                NPC.frameCounter += 0.2; // how fast the animation cycles
                if (NPC.frameCounter >= 6) // number of walking frames
                    NPC.frameCounter = 0;

                // Add 1 since frame 0 is the idle frame
                int frame = 1 + (int)NPC.frameCounter;
                NPC.frame.Y = frame * frameHeight;
            }
            else
            {
                // Idle frame
                NPC.frameCounter = 0;
                NPC.frame.Y = 0; // first frame
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
           // if (NPC.IsABestiaryIconDummy)
           // {
                return base.PreDraw(spriteBatch, screenPos, drawColor);
            //}

            string TextureString = "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/FleshlingCultist/FleshlingCultist";
            Texture2D tex = ModContent.Request<Texture2D>(TextureString).Value;

            Vector2 DrawPos = NPC.Center - screenPos;


            Rectangle frame = tex.Frame(1, 1);
            Vector2 Origin = frame.Size() * 0.5f + new Vector2(0, -1);

            SpriteEffects flip = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            Main.EntitySpriteDraw(tex, DrawPos, null, drawColor, NPC.rotation, Origin, NPC.scale, flip);
            //string debug = "";
            //if (CultistCoordinator.GetCultOfNPC(NPC) != null)
            // {
            //     debug += $"CultID: {CultistCoordinator.GetCultOfNPC(NPC).CultID}\n";


            // }

            //debug += CurrentState.ToString() + $"\n";
            // Utils.DrawBorderString(spriteBatch, debug, NPC.Center - screenPos, Color.AntiqueWhite);

            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }


    }
}
