using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    internal class ViscousWhip_Proj : CleanBaseWhip
    {

        public override SoundStyle? WhipSound =>  GennedAssets.Sounds.Common.Glitch with { Volume = 0.5f, PitchVariance = 0.2f};
        private ModularWhipController _controller;
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            Vector2 arm = Main.GetPlayerArmPosition(Projectile);
            Vector2 c1 = arm + new Vector2(50 * Projectile.spriteDirection, -80f);
            Vector2 c2 = arm + new Vector2(150 * Projectile.spriteDirection, 100f);
            Vector2 end = arm + new Vector2(200 * Projectile.spriteDirection, 0f);

            var curve = new BezierCurve(new Vector2(0, 0), new Vector2(60, 80), new Vector2(160, 90), new Vector2(220, 0));


            _controller = new ModularWhipController(new CustomMotion(curve));
            _controller.AddModifier(new SmoothSineModifier(0, 18, 6f, 2f, 1f));
            _controller.AddModifier(new TwirlModifier(5, 12, 0.15f));
        }


        
        public override void ModifyControlPoints(List<Vector2> controlPoints)
        {
            // get whip settings (segments and range multipliers)
            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int segments, out float rangeMultiplier);
            rangeMultiplier *= Main.player[Projectile.owner].whipRangeMultiplier;

            // IMPORTANT: use the normalized progress from the CleanBaseWhip (0..1)
            float progress = FlyProgress; // this is Projectile.ai[0] / flyTime inside CleanBaseWhip
            progress = MathHelper.Clamp(progress, 0f, 1f);

            // now apply the pipeline
            _controller.Apply(controlPoints, Projectile, segments, rangeMultiplier, progress);
        }
        /*
        public override void ModifyControlPoints(List<Vector2> controlPoints)
        {
            controlPoints.Clear();
            Projectile proj = Projectile;
            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int segments, out float rangeMultiplier);

            Player player = Main.player[proj.owner];
            rangeMultiplier *= player.whipRangeMultiplier;

            float timePercent = proj.ai[0] / timeToFlyOut;
            float hDistancePercent = timePercent * 1.5f;
            float retractionPercent = 0f;
            if (hDistancePercent > 1f)
            {
                retractionPercent = (hDistancePercent - 1f) / 0.5f;
                hDistancePercent = MathHelper.Lerp(1f, 0f, retractionPercent);
            }

            Item heldItem = player.HeldItem;
            float distFactor = (float)(ContentSamples.ItemsByType[heldItem.type].useAnimation * 2) * timePercent * player.whipRangeMultiplier;
            float pxPerSegment = proj.velocity.Length() * distFactor * hDistancePercent * rangeMultiplier / segments;
            Vector2 playerArmPosition = Main.GetPlayerArmPosition(proj);
            Vector2 prev_p = playerArmPosition;
            float rot = -1.5707964f;
            Vector2 prev_p2 = prev_p;
            float rot2 = 1.5707964f + 1.5707964f * proj.spriteDirection;
            Vector2 prev_p3 = prev_p;
            float rot3 = 1.5707964f;

            controlPoints.Add(playerArmPosition);

            for (int i = 0; i < segments; i++)
            {
                float segmentPercent = (float)i / segments;
                float thisRotation = 3.7070792f * MathF.Sin(2f * segmentPercent - 3.42f * timePercent + 0.75f * hDistancePercent) * (-(float)proj.spriteDirection) + 1.5707964f;
                Vector2 p = prev_p + Utils.ToRotationVector2(rot) * pxPerSegment * 1.2f;
                Vector2 p2 = prev_p3 + Utils.ToRotationVector2(rot3) * (pxPerSegment * 2f);
                Vector2 vector8 = prev_p2 + Utils.ToRotationVector2(rot2) * (pxPerSegment * 2f);
                float invHDistance = 1f - hDistancePercent;
                float smoothHDistPercent = 1f - invHDistance * invHDistance;
                Vector2 value = Vector2.Lerp(p2, p, smoothHDistPercent * 0.9f + 0.1f);
                Vector2 vector7 = Vector2.Lerp(vector8, value, smoothHDistPercent * 0.7f + 0.3f);
                Vector2 vector9 = playerArmPosition + (vector7 - playerArmPosition) * new Vector2(1.7f, 1.65f);
                float smoothRetractPercent = retractionPercent;
                smoothRetractPercent *= smoothRetractPercent;
                Vector2 item = Utils.RotatedBy(vector9, proj.rotation + 0f * smoothRetractPercent * player.direction, playerArmPosition);
                controlPoints.Add(item);
                rot = thisRotation;
                rot3 = thisRotation;
                rot2 = thisRotation;
                prev_p = p;
                prev_p3 = p2;
                prev_p2 = vector8;
            }
        }
        */
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.MaxUpdates = 10;

        }

        // Keep track of the last tip position for lighting/visuals
        public Vector2 lastTop = Vector2.Zero;

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // Set the whip settings (segments and range multiplier)
        protected override void ModifyWhipSettings(ref float outFlyTime, ref int outSegments, ref float outRangeMult)
        {
            base.ModifyWhipSettings(ref outFlyTime, ref outSegments, ref outRangeMult);
            outSegments = 49;
            outRangeMult = 3f;
        }

        // Additional per-tick behavior (particles / lighting).
        // We re-query control points each tick to perform visuals.
        // Use ModifyControlPoints(...) as the canonical source of control points for visuals/particles
        protected override void WhipAI()
        {
            Player owner = Main.player[Projectile.owner];

            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (swingTime <= 0f) swingTime = 20f * Projectile.MaxUpdates;
            float swingProgress = Timer / swingTime;

            
            List<Vector2> points = new();
            ModifyControlPoints(points);
            if (points.Count == 0) return;

            lastTop = points[^1];
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // rectangle centered on lastTop with 72x72 area
            Rectangle rect = new Rectangle(((int)lastTop.X - 36), ((int)lastTop.Y - 36), 42, 42);
            if (rect.Intersects(target.Hitbox))
            {
                modifiers.SourceDamage *= 1.25f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            target.AddBuff(ModContent.BuffType<BloodwhipBuff>(), 240);
            
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            // reduce projectile's base damage to mimic the original code
            Projectile.damage = (int)(Projectile.damage * 0.9f);

            SoundEngine.PlaySound(SoundID.Item14, target.Center);


        }

        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(0f, 0.5f);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation();
                Color color = Color.Crimson;
                Vector2 scale = new Vector2(diff.Length() + 2f, 2f);
                if (i == list.Count - 2)
                {
                    scale.X -= 5f;
                }

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
                //Utils.DrawBorderString(Main.spriteBatch, i.ToString(), pos - Main.screenPosition, Color.AntiqueWhite, 0.5f);
                pos += diff;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new();
            ModifyControlPoints(list);
            if (list.Count == 0) return false;


            DrawLine(list);

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 pos = list[0];

            Texture2D thing = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;
            for (int i = 0; i < list.Count - 1; i++)
            {
                // These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
                // You can change them if they don't!
                Rectangle frame = new Rectangle(0, 0, 10, 26); // The size of the Handle (measured in pixels)
                Vector2 origin = new Vector2(5, 8); // Offset for where the player's hand will start measured from the top left of the image.
                float scale = 1;

                // These statements determine what part of the spritesheet to draw for the current segment.
                // They can also be changed to suit your sprite.
                if (i == list.Count - 2)
                {
                    // This is the head of the whip. You need to measure the sprite to figure out these values.
                    frame.Y = 74; // Distance from the top of the sprite to the start of the frame.
                    frame.Height = 18; // Height of the frame.


                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    Vector2 a = list[list.Count - 2] - list[2];
                    float rot = a.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.

                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                    Vector2 thingOffset = thing.Size() * 0.5f + new Vector2(-50, 100);
                    //Main.spriteBatch.End();
                    //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                    Main.EntitySpriteDraw(thing, pos - Main.screenPosition, null, Color.Crimson with { A = 0 }, rot, thingOffset, scale * 0.05f, SpriteEffects.None);
                    //Main.spriteBatch.End();
                    //Main.spriteBatch.Begin();
                }
                else if (i > 10)
                {
                    // Third segment
                    frame.Y = 58;
                    frame.Height = 16;
                }
                else if (i > 5)
                {
                    // Second Segment
                    frame.Y = 42;
                    frame.Height = 16;
                }
                else if (i > 0)
                {
                    // First Segment
                    frame.Y = 26;
                    frame.Height = 16;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                pos += diff;
            }

            return false;
        }


    }

}
