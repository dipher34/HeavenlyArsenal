using CalamityMod;
using HeavenlyArsenal.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    partial class RitualAltar
    {
        #region Limb System
        private const float LimbReach = 125f;
        private const float LimbSearchRadius = LimbReach * 0.8f;
        private const int LimbCount = 4;

        private RitualAltarLimb[] _limbs;
        private Vector2[] _limbBaseOffsets;
        private readonly HashSet<Point> _claimedTiles = new();


        private int _limbStepTimer;
        private bool _stepRightSide;
        private int _activeLimbIndex;
        private Vector2 _forward;
        private Vector2 _right;
        bool TouchingGround(Vector2 target)
        {
            Point tile = target.ToTileCoordinates();
            // Check the tile directly below the anchor point
            Point below = new Point(tile.X, tile.Y);
            if (!WorldGen.InWorld(below.X, below.Y, 10))
                return false;

            Tile t = Framing.GetTileSafely(below);
            return t.HasTile && Main.tileSolid[t.TileType];
        }
        void UpdateGravity()
        {
            float Strength = 0f;
            for (int i = 0; i < _limbs.Length; i++)
            {
                if (_limbs[i].IsTouchingGround)
                {
                    Strength++;
                }
            }
            if (Strength > 0.2f)
                Strength /= LimbCount;
            NPC.velocity.Y = -Strength;// float.Lerp(NPC.velocity.Y, NPC.velocity.Y - Strength, 0.6f);

        }

        void UpdateLimbMotion()
        {
            //UpdateLimbRhythm();
            UpdateLimbTargets();
        }
        void UpdateLimbTargets()
        {
            float speed = NPC.velocity.Length();
            const float baseReach = 80f;
            float reachRelax = baseReach * 1.1f;
            float forwardBias = MathHelper.Clamp(speed * 30f, 40f, 200f);
            float maxSearchDown = 300f;
            int holdTime = (int)MathHelper.Clamp(50f - speed * 10f, 20f, 50f);

            bool idle = speed < 0.1f;
            int groundedCount = 0;
            for (int i = 0; i < LimbCount; i++)
                if (_limbs[i].IsTouchingGround)
                    groundedCount++;

            if (idle && groundedCount<2)
                idle = false; 
            Vector2 moveDir = (speed > 0.1f) ? NPC.velocity.SafeNormalize(Vector2.UnitY) : Vector2.UnitY;
            for (int i = 0; i < LimbCount; i++)
            {
                ref var limb = ref _limbs[i];
                Vector2 basePos = NPC.Center + _limbBaseOffsets[i];

                if (limb.Cooldown > 0)
                    limb.Cooldown--;

                float dist = Vector2.Distance(basePos, limb.TargetPosition);
                bool aboutToOverstretch = dist > baseReach * 0.9f;
                bool tooFar = dist > reachRelax;

              

                // If idle, but *none* of the limbs are grounded, treat it as not idle
                

                if (idle)
                {
                    // Re-evaluate if the current end position is on ground
                    bool grounded = false;

                    Point tilePos = (limb.EndPosition / 16f).ToPoint();
                    Tile t = Framing.GetTileSafely(tilePos.X, tilePos.Y + 1);
                    if (t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
                        grounded = true;

                    limb.IsTouchingGround = grounded;
                    limb.HasTarget = grounded;
                    limb.IsAnchored = grounded;

                    UpdateLimbState(ref limb, basePos, 0.15f + speed * 0.02f, 6f);
                    continue;
                }


                if ((aboutToOverstretch && limb.Cooldown <= 0) || tooFar)
                {
                    limb.IsAnchored = false;
                    limb.HasTarget = false;
                    limb.IsTouchingGround = false;
                }

                if (!limb.IsAnchored && limb.Cooldown <= 0)
                {
                    Vector2 difference = basePos - NPC.Center + moveDir;
                    
                    Vector2 probe = difference + NPC.Center + moveDir * forwardBias;
                    probe += Main.rand.NextVector2Circular(30f, 1f);

                    // Raycast straight down from probe
                    Vector2 end = probe + Vector2.UnitY * maxSearchDown;
                    Point? hit = LineAlgorithm.RaycastTo(
                        (int)(probe.X / 16f),
                        (int)(probe.Y / 16f),
                        (int)(end.X / 16f),
                        (int)(end.Y / 16f)
                    );

                    bool found = false;

                    if (hit.HasValue)
                    {
                        Point tilePos = hit.Value;
                        Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);

                        if (tile.HasTile && tile.IsTileSolid())
                        {
                            // Convert tile coordinate to world position
                            Vector2 desiredPosition = new Vector2(tilePos.X * 16f, tilePos.Y * 16f + 8f);
                            
                            // stay away from other limbs
                            int spacedCount = 0;
                            for (int x = 0; x < LimbCount; x++)
                            {
                                if (_limbs[x].IsAnchored && Vector2.Distance(desiredPosition, _limbs[x].TargetPosition) > 10f)
                                    spacedCount++;
                            }

                            if (spacedCount >= 3)
                            {
                                limb.TargetPosition = desiredPosition;
                                limb.HasTarget = true;
                                limb.IsTouchingGround = true;

                                limb.Cooldown = holdTime;
                                found = true;
                            }
                        }
                    }

                    // Fallback if no hit
                   
                    




                }

                float followSpeed = MathHelper.Clamp(0.08f + speed * 0.02f, 0.08f, 0.19f);
                limb.EndPosition = Vector2.Lerp(limb.EndPosition, limb.TargetPosition, followSpeed);

               

                UpdateLimbState(ref limb, basePos, 0.15f + speed * 0.02f, 1f);
            }
        }
        void UpdateLimbRhythm()
        {
            //swap so that it looks different

            if (--_limbStepTimer <= 0)
            {
                _limbStepTimer = Main.rand.Next(25, 45);
                _activeLimbIndex = _stepRightSide ? 1 : 0;
                _stepRightSide = !_stepRightSide;
            }

            // Update direction vectors based on current movement or rotation
            _forward = NPC.velocity.SafeNormalize(Vector2.UnitY);
            _right = _forward.RotatedBy(MathHelper.PiOver2);
        }
        void SetFlailTarget(ref RitualAltarLimb limb, Vector2 basePos, float reach, int limbIndex)
        {
            //float sway = (float)Math.Sin(Time * 5f + limbIndex * 1.37f) * 12f;
            // Keep it under the base, within reach
            limb.TargetPosition = limb.EndPosition;
            limb.IsAnchored = false;
            limb.HasTarget = false;
            if(!TouchingGround(limb.EndPosition))
            limb.IsTouchingGround = false;
            //limb.TargetTile = Point.Zero;
        }
        #endregion
    }
}
