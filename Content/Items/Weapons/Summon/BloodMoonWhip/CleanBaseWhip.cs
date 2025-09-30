using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    /// <summary>
    /// A cleaned-up, easier-to-extend base class for whip projectiles.
    /// Subclasses override virtual properties/methods to change visuals/behavior.
    /// Key improvements:
    /// - owner item type is captured at spawn time (safer than string compares)
    /// - control points are calculated only when needed
    /// - clearer FlyProgress and HitboxActive semantics
    /// - fewer allocations, clearer override points
    /// </summary>
    public abstract class CleanBaseWhip : ModProjectile
    {
        protected virtual int DefaultSegments => 12;
        protected virtual float DefaultRangeMult => 1.6f;
        protected virtual int DefaultHandleHeight => 20;
        protected virtual int DefaultSegHeight => 16;
        protected virtual int DefaultEndHeight => 20;
        protected virtual int DefaultSegTypes => 2;
        public virtual SoundStyle? WhipSound => SoundID.Item153;
        public virtual Color StringColor => Color.White;

        // ----- runtime state -----
        private bool _initialized;
        private int _ownerItemType = ItemID.None; // saved on spawn
        private float _flyTime; // in ticks (accounting for MaxUpdates below)
        private readonly List<Vector2> _controlPoints = new();

        // Expose counts/settings via properties so subclasses can override
        protected virtual int Segments => DefaultSegments;
        protected virtual float RangeMult => DefaultRangeMult;
        protected virtual int handleHeight => DefaultHandleHeight;
        protected virtual int segHeight => DefaultSegHeight;
        protected virtual int endHeight => DefaultEndHeight;
        protected virtual int segTypes => DefaultSegTypes;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.penetrate = -1;
            // sensible defaults already assigned via virtual props
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            // capture the player's held item at spawn if available
            if (Main.player[Projectile.owner].HeldItem != null)
                _ownerItemType = Main.player[Projectile.owner].HeldItem.type;
        }

        // Helper: compute fly time once per relevant change
        protected virtual float ComputeFlyTime()
        {
            Player p = Main.player[Projectile.owner];
            // itemAnimationMax corresponds to how long the player's attack lasts
            int baseAnim = p.itemAnimationMax > 0 ? p.itemAnimationMax : 20;
            return baseAnim * Projectile.MaxUpdates;
        }

        // Public progress 0..1
        public float FlyProgress => (_flyTime <= 0) ? 0f : (Projectile.ai[0] / _flyTime);

        // Whether the whip's hitbox should currently be active.
        // Subclasses can override for other behavior.
        protected virtual bool HitboxActive(float progress)
        {
            // Example: active between 10% and 70% of the swing, strongest near 50%
            return progress >= 0.1f && progress <= 0.7f;
        }

        // Called by PreAI to let subclasses alter control points before drawing/collision
        public virtual void ModifyControlPoints(List<Vector2> points) { }

        // Let subclasses change seg/range/time settings
        protected virtual void ModifyWhipSettings(ref float outFlyTime, ref int outSegments, ref float outRangeMult)
        {
            outSegments = Segments;
            outRangeMult = RangeMult;
            outFlyTime = ComputeFlyTime();
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];

            if (!_initialized)
            {
                _initialized = true;
                // recompute fly time & store
                float ft = 0; int segs = 0; float range = 0;
                ModifyWhipSettings(ref ft, ref segs, ref range);
                _flyTime = ft > 0 ? ft : ComputeFlyTime();
            }

            // increment animation progress
            Projectile.ai[0]++;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);

            // Make the whip face the same direction as the player
            Projectile.spriteDirection = player.direction;

            // Update rotation — add Pi if facing left so it’s not upside-down
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += MathHelper.Pi;

            // Keep the projectile’s position anchored to the player’s arm
            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * FlyProgress;

            // (Optional) Keep sync with the player’s item animation if needed
            player.heldProj = Projectile.whoAmI;
            // rotation for sprite
            if (Projectile.velocity.LengthSquared() > 0.0001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            player.heldProj = Projectile.whoAmI;

            // kill when done
            if (Projectile.ai[0] >= _flyTime)
            {
                Projectile.Kill();
                return false;
            }

            // Fill control points once per tick and let subclass alter them
            _controlPoints.Clear();
            Projectile.FillWhipControlPoints(Projectile, _controlPoints);
            ModifyControlPoints(_controlPoints);

            // Play sound at the tip of the whip (example: at mid-swing)
            if (Math.Abs(Projectile.ai[0] - _flyTime / 2f) < 1f)
            {
                if (WhipSound != null && _controlPoints.Count > 0)
                    SoundEngine.PlaySound(WhipSound.Value, _controlPoints[^1]);
            }

            // Cut tiles if hitbox active
            if (HitboxActive(FlyProgress))
            {
                var half = new Vector2(Projectile.width * Projectile.scale * 0.5f, 0f);
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
                    Utils.PlotTileLine(_controlPoints[i] - half, _controlPoints[i] + half, Projectile.height * Projectile.scale, DelegateMethods.CutTiles);
                }
            }

            // call subclass AI hook
            WhipAI();
            
            return false;
        }

        // Override this for custom per-tick behavior
        protected virtual void WhipAI() { }

        // Called by engine when hitting NPC
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // set player's target for minions — maintain old behavior
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            // If we have owner item type saved, use that as the tag identity (safer than comparing strings)
            if (_ownerItemType != ItemID.None)
            {
                if (Main.player[Projectile.owner].HeldItem == null)
                    return;

                /*
                // Example: apply a "whip tag" effect using owner item type
                var global = target.GetGlobalNPC<WhipDebuffNPC>();
                if (global != null)
                {
                    // Check existing tags for this item type
                    bool found = false;
                    foreach (var tp in global.Tags)
                    {
                        if (tp.ItemType == _ownerItemType) // using int ID
                        {
                            found = true;
                            // refresh time if needed (example assumes bw.TagTime accessible via item)
                            var maybeItem = Main.player[Projectile.owner].HeldItem.ModItem as BaseWhipItem;
                            if (maybeItem != null && tp.TimeLeft < maybeItem.TagTime)
                                tp.TimeLeft = maybeItem.TagTime;
                            break;
                        }
                    }
                    if (!found)
                    {
                        var maybeItem = Main.player[Projectile.owner].HeldItem.ModItem as BaseWhipItem;
                        if (maybeItem != null)
                        {
                            global.Tags.Add(new WhipTag(_ownerItemType, maybeItem.TagTime, maybeItem.TagDamage, maybeItem.TagDamageMult, maybeItem.TagCritChance, GetTagEffectName));
                        }
                    }
                }*/
            }
        }

        // allow subclasses to give a name for UI/tooltip (optional)
        public virtual string GetTagEffectName => "";

        // Drawing
        public override bool PreDraw(ref Color lightColor)
        {
            // refill (defensive) and allow subclass to modify control points before drawing
            _controlPoints.Clear();
            Projectile.FillWhipControlPoints(Projectile, _controlPoints);
            ModifyControlPoints(_controlPoints);

            DrawStrings(_controlPoints);
            DrawSegs(_controlPoints);

            return false; // we've drawn it
        }

        protected virtual void DrawStrings(List<Vector2> points)
        {
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 mid = Vector2.Lerp(points[i - 1], points[i], 0.5f);
                Color color = Color.Lerp(Lighting.GetColor((int)(mid.X / 16f), (int)(mid.Y / 16f), StringColor), StringColor, Projectile.light);
                Utils.DrawLine(Main.spriteBatch,points[i - 1], points[i], color, color, 2 * Projectile.scale);
            }
        }

        protected virtual void DrawSegs(List<Vector2> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                int frameY = 0, frameHeight = 0;
                Vector2 origin = Vector2.Zero;
                GetFrame(i, points.Count, ref frameY, ref frameHeight, ref origin);
                float drawScale = Projectile.scale * GetSegScale(i, points.Count);

                Vector2 lightPos = i == 0 ? points[i] : Vector2.Lerp(points[i - 1], points[i], 0.5f);
                Color color = Color.Lerp(Lighting.GetColor((int)(lightPos.X / 16f), (int)(lightPos.Y / 16f)), StringColor, Projectile.light);

                float rot;
                if (i == points.Count - 1)
                    rot = (points[i] - points[i - 1]).ToRotation();
                else
                    rot = (points[i + 1] - points[i]).ToRotation();
                rot -= MathHelper.PiOver2;

                Texture2D whipTex = ModContent.Request<Texture2D>(Texture).Value;

                Main.EntitySpriteDraw(whipTex, points[i] - Main.screenPosition, new Rectangle(0, frameY, whipTex.Width, frameHeight), color, rot, origin, drawScale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
        }

        // Same frame logic as original but exposed to be overridden
        public virtual void GetFrame(int segIndex, int segCount, ref int frameY, ref int frameHeight, ref Vector2 origin)
        {
            Texture2D whipTex = ModContent.Request<Texture2D>(Texture).Value;
            if (segIndex == 0)
            {
                frameY = 0;
                frameHeight = handleHeight;
                origin = new Vector2(whipTex.Width, handleHeight) * 0.5f;
            }
            else if (segIndex == segCount - 1)
            {
                frameY = whipTex.Height - endHeight;
                frameHeight = endHeight;
                origin = new Vector2(whipTex.Width, endHeight) * 0.5f;
            }
            else
            {
                frameY = handleHeight + (segIndex - 1) % segTypes * segHeight;
                frameHeight = segHeight;
                origin = new Vector2(whipTex.Width, segHeight) * 0.5f;
            }
        }

        public virtual float GetSegScale(int segIndex, int segCount) => 1f;
    }
}
