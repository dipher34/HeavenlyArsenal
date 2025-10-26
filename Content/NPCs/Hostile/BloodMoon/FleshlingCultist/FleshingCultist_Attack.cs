using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.FleshlingCultist
{
    partial class FleshlingCultist
    {
        //todo: put this into NPC.ai[2]l
        enum Behaviors
        {
            WillingSacrifice,

            Worship,

            Defend,

            BlindRush
        }

        Behaviors CurrentState { get; set; }


        void StateMachine()
        {
            switch (CurrentState)
            {
                case Behaviors.WillingSacrifice:
                    WillingSacrifice();
                    break;
                case Behaviors.Worship:
                    Worship();
                    break;
                case Behaviors.Defend:

                    break;

                case Behaviors.BlindRush:
                    BlindRush();
                    break;
            }
        }

        void WillingSacrifice()
        {
            
        }

        void Worship()
        {
            if (CultistCoordinator.GetCultOfNPC(NPC) != null)
            {

                NPC thing = CultistCoordinator.GetCultOfNPC(NPC).Leader;
                NPC.velocity.X = NPC.AngleTo(thing.Center).ToRotationVector2().X * 2;
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);


                if (NPC.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
                    CurrentState = Behaviors.BlindRush;
            }
            else
                CurrentState = Behaviors.BlindRush;
        }

        void BlindRush()
        {
            if (playerTarget == null)
                FindPlayer();
            NPC.velocity.X = NPC.velocity.X = NPC.AngleTo(playerTarget.Center).ToRotationVector2().X * 6 * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            float horizontalRange = 100f;

            // Check if horizontally close but vertically offset
            if (Math.Abs(playerTarget.Center.X - NPC.Center.X) < horizontalRange &&
                playerTarget.Center.Y < NPC.Center.Y - 16f && // player is above
                NPC.velocity.Y == 0) // NPC is on the ground
            {
                // Jump
                NPC.velocity.Y = -10f; // Adjust jump strength
                NPC.netUpdate = true; // Sync in multiplayer
            }


        }

        void FindPlayer()
        {
            playerTarget = Main.player[NPC.FindClosestPlayer()];
        }
    }
}
