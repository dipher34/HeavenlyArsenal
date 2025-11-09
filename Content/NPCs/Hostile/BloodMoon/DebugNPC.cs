using HeavenlyArsenal.Common.IK;
using HeavenlyArsenal.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    internal class DebugNPC : BloodMoonBaseNPC
    {

        // Configuration constants
        const float LimbSearchRadius = 100f;       // max distance to search for ground
        const float FootLerpSpeed = 0.2f;          // how quickly feet move toward targets (0.0 to 1.0)
        const float AnchorThreshold = 4f;          // how close foot must be to target to count as anchored
        const float StepThreshold = 80f;           // when to lift foot (distance from base > threshold)

        // Leg structures
        struct Limb
        {
            public IKSkeleton Skeleton;    // IK solver for the leg (e.g. 2-joint FABRIK)
            public Vector2 TargetPosition; // current intended foot target
            public Vector2 EndPosition;    // current foot position (moves toward target)
            public bool IsAnchored;
        }
        Limb[] limbs;
        Vector2[] limbBaseOffsets;  // attachment points relative to NPC center

        public override void SetDefaults()
        {
            // Initialize limbs (example with 2-segment legs)
            int limbCount = 4;  // say 4 legs
            limbs = new Limb[limbCount];
            limbBaseOffsets = new Vector2[limbCount];
            // Define base offsets around the bottom of the NPC (e.g. spread around center)
            float width = NPC.width * 0.3f;
            limbBaseOffsets[0] = new Vector2(-width, NPC.height / 2);   // back-left
            limbBaseOffsets[1] = new Vector2(width, NPC.height / 2);   // back-right
            limbBaseOffsets[2] = new Vector2(-width / 2, NPC.height / 2);  // front-left
            limbBaseOffsets[3] = new Vector2(width / 2, NPC.height / 2);  // front-right
                                                                          // Create IK skeletons for each limb (each with two segments of specified lengths)
            for (int i = 0; i < limbCount; i++)
            {
                limbs[i].Skeleton = new IKSkeleton(
                    (36f, new IKSkeleton.Constraints()),            // upper leg bone length 36
                    (60f, new IKSkeleton.Constraints())             // lower leg bone length 60
                );
                limbs[i].EndPosition = NPC.Center + limbBaseOffsets[i] + new Vector2(0, 40);
                limbs[i].TargetPosition = limbs[i].EndPosition;  // start with foot below body
                limbs[i].IsAnchored = true;  // start anchored (assuming on ground initially)
            }
        }

        public override void AI()
        {
            Vector2 npcVelocity = NPC.velocity;
            bool anyFootAnchored = false;

            for (int i = 0; i < limbs.Length; i++)
            {
                Vector2 basePos = NPC.Center + limbBaseOffsets[i];  // current world pos of leg’s base
                Limb limb = limbs[i];  // copy for ease (if not using ref)

                // Decide if we need a new target for this leg
                if (limb.IsAnchored)
                {
                    // Check if leg stretched too far from base (or NPC changed direction)
                    float distBaseToFoot = Vector2.Distance(basePos, limb.EndPosition);
                    if (distBaseToFoot > StepThreshold)
                    {
                        // Un-anchor the foot to find a new placement
                        limb.IsAnchored = false;
                    }
                    // Optionally, also unanchor if NPC changed direction rapidly or foot is behind body, etc.
                }

                if (!limb.IsAnchored)
                {
                    // Perform raycast downward, biased forward by velocity
                    float forwardBias = 0f;
                    if (npcVelocity.X != 0f)
                    {
                        forwardBias = Math.Sign(npcVelocity.X) * (LimbSearchRadius * 0.5f);
                    }
                    Vector2 rayStart = basePos;
                    rayStart.X += forwardBias;  // shift starting point forward
                    Vector2 rayEnd = rayStart + new Vector2(0f, LimbSearchRadius);  // straight down

                    // Convert world coordinates to tile coordinates for raycast
                    Point startTile = rayStart.ToTileCoordinates();
                    Point endTile = rayEnd.ToTileCoordinates();
                    Point? hit = LineAlgorithm.RaycastTo(startTile.X, startTile.Y, endTile.X, endTile.Y);

                    if (hit.HasValue)
                    {
                        // We hit a solid tile – set target to the top of that tile
                        int tileX = hit.Value.X;
                        int tileY = hit.Value.Y;
                        float worldX = tileX * 16 + 8;    // center of tile
                        float worldY = tileY * 16;       // top of tile (Tiles origin at top-left)
                        limb.TargetPosition = new Vector2(worldX, worldY);
                    }
                    else
                    {
                        // No ground found within radius – extend foot to ray end (dangling)
                        limb.TargetPosition = rayEnd;
                    }
                }

                // Smoothly move foot towards target
                limb.EndPosition = Vector2.Lerp(limb.EndPosition, limb.TargetPosition, FootLerpSpeed);
                // Update IK solver for this leg
                limb.Skeleton.Update(basePos, limb.EndPosition);
                // Check if foot reached target
                if (Vector2.Distance(limb.EndPosition, limb.TargetPosition) < AnchorThreshold)
                {
                    limb.IsAnchored = true;
                }

                limbs[i] = limb;  // store back the updated limb
                if (limb.IsAnchored) anyFootAnchored = true;
            }

            // Apply vertical support if needed
            if (anyFootAnchored)
            {
                if (NPC.velocity.Y > 0f)
                {
                    // Dampen downward velocity to simulate support from legs
                    NPC.velocity.Y *= 0.5f;
                }
                // Optionally, you could even stop falling completely when multiple legs anchored:
                //if (NPC.velocity.Y > 0 && enoughFeetAnchored) NPC.velocity.Y = 0;
            }

            // ... (rest of NPC AI such as movement, attacking, etc.)
        }


    }
}
