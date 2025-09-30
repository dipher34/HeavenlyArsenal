using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
// File: WhipControlPipeline.cs
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    public interface IWhipModifier
    {
        /// <summary>
        /// I HATE WRITING COMMENTS!!!
        /// </summary>
        /// <param name="controlPoints">current list of points (motion already filled it)</param>
        /// <param name="projectile">the whip projectile</param>
        /// <param name="segments">number of logical segments for the whip (useful for index->percent)</param>
        /// <param name="rangeMultiplier"> whip range multiplier (already adjusted by player)</param>
        /// <param name="progress"> normalized 0..1 progress through the swing</param>
        void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress);
    }

    public interface IWhipMotion
    {
        // MUST fill controlPoints (clear + add points). Accepts segments, range multiplier, progress.
        void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress);
    }

    public class TwirlModifier : IWhipModifier
    {
        private int startIndex, endIndex;
        private float strength;
        private bool influencedByProgress;

        /// <summary>
        /// Applies a twirl effect to whip control points.
        /// </summary>
        /// <param name="startIndex">Segment index to start twirling from.</param>
        /// <param name="endIndex">Segment index to stop applying twirl directly (later segments inherit).</param>
        /// <param name="strength">Strength of the twirl effect.</param>
        /// <param name="influencedByProgress">If true, twirl is scaled by swing progress (curved bell shape).</param>
        public TwirlModifier(int startIndex, int endIndex, float strength, bool influencedByProgress = true)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.strength = strength;
            this.influencedByProgress = influencedByProgress;
        }

        public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
        {
            if (controlPoints == null || controlPoints.Count == 0) return;

            int s = Math.Clamp(startIndex, 0, controlPoints.Count - 1);
            int e = Math.Clamp(endIndex, s, controlPoints.Count - 1);

            // Progress curve → strongest in the middle, weakest at start/end
            float curvedProgress = influencedByProgress ? (float)Math.Sin(progress * MathHelper.Pi) : 1f;
            float eff = curvedProgress * strength;

            Vector2 pivot = controlPoints[s];
            float lastAngle = 0f;

            // Apply twirl gradually up to the endIndex
            for (int i = s; i <= e; i++)
            {
                Vector2 rel = controlPoints[i] - pivot;
                float angle = eff * (i - s);
                rel = rel.RotatedBy(angle);
                controlPoints[i] = pivot + rel;

                lastAngle = angle;
            }

            // Ensure the rest of the whip continues smoothly with the last rotation
            for (int i = e + 1; i < controlPoints.Count; i++)
            {
                Vector2 rel = controlPoints[i] - pivot;
                rel = rel.RotatedBy(lastAngle);
                controlPoints[i] = pivot + rel;
            }
        }
    }

    public class SmoothSineModifier : IWhipModifier
    {
        private int startIndex, endIndex;
        private float amplitude, frequency, period;

        // Envelope that maps normalized progress (0..1) -> scalar multiplier for amplitude.
        // Default is sin(pi * t) so amplitude ramps up from 0 -> 1 at mid-swing -> 0 at the end.
        public Func<float, float> AmplitudeEnvelope { get; set; } = (t) => MathF.Sin(MathF.PI * MathHelper.Clamp(t, 0f, 1f));

        public SmoothSineModifier(int startIndex, int endIndex, float amplitude, float frequency, float period)
        {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.amplitude = amplitude;
            this.frequency = frequency;
            this.period = period;
        }

        // helper to compute a local perpendicular vector at index i (safe)
        private static Vector2 PerpAt(int i, List<Vector2> pts)
        {
            if (pts == null || pts.Count < 2) return Vector2.Zero;
            Vector2 dir;
            if (i <= 0) dir = pts[1] - pts[0];
            else if (i >= pts.Count - 1) dir = pts[^1] - pts[pts.Count - 2];
            else dir = pts[i + 1] - pts[i - 1];
            if (dir.LengthSquared() < 0.0001f) return Vector2.Zero;
            dir.Normalize();
            return new Vector2(-dir.Y, dir.X);
        }

        public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
        {
            if (controlPoints == null || controlPoints.Count == 0) return;

            int s = Math.Clamp(startIndex, 0, controlPoints.Count - 1);
            int e = Math.Clamp(endIndex, s, controlPoints.Count - 1);
            int denom = Math.Max(1, e - s);

            // compute envelope once (progress assumed normalized 0..1)
            float env = AmplitudeEnvelope != null ? AmplitudeEnvelope(progress) : 1f;
            env = MathHelper.Clamp(env, 0f, 1f);
            float effectiveAmplitude = amplitude * env;

            for (int i = s; i <= e; i++)
            {
                float t = (i - s) / (float)denom; // local 0..1 along modifier
                                                  // phase uses frequency across the mod range, and time progress to animate
                float sine = MathF.Sin((t * frequency + progress) * MathHelper.TwoPi / period);
                Vector2 perp = PerpAt(i, controlPoints);
                if (perp == Vector2.Zero) continue;
                Vector2 v = controlPoints[i];
                v += perp * (sine * effectiveAmplitude);
                controlPoints[i] = v;
            }
        }
    }

    public class VanillaWhipMotion : IWhipMotion
    {
        public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
        {
            if (controlPoints == null) return;

            // Let the engine compute its canonical whip control points.
            // Projectile.FillWhipControlPoints expects Projectile.ai[0] to already track the
            // swing progress (CleanBaseWhip does that), so calling into it yields the vanilla curve.
            controlPoints.Clear();
            Projectile.FillWhipControlPoints(projectile, controlPoints);

            // If the pipeline passed a different segments / rangeMultiplier and you want to
            // respect them rather than the engine defaults, you'd need a custom implementation.
            // In most cases using the engine's points is the correct vanilla behavior.
        }
    }

    public class StraightLineMotion : IWhipMotion
    {
        public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
        {
            controlPoints.Clear();
            Player player = Main.player[projectile.owner];
            Item heldItem = player.HeldItem;

            float distFactor = (float)(ContentSamples.ItemsByType[heldItem.type].useAnimation * 2) * progress * player.whipRangeMultiplier;
            float baseSpeed = projectile.velocity.Length();
            if (baseSpeed < 0.0001f) baseSpeed = 12f;
            float pxPerSegment = baseSpeed * distFactor * rangeMultiplier / Math.Max(1, segments);

            Vector2 origin = Main.GetPlayerArmPosition(projectile);
            controlPoints.Add(origin);

            Vector2 forward = projectile.velocity.LengthSquared() > 0.0001f ? Vector2.Normalize(projectile.velocity) : new Vector2(player.direction, 0f);
            for (int i = 1; i <= segments; i++)
            {
                controlPoints.Add(origin + forward * (pxPerSegment * i));
            }
        }
    }

    /// <summary>
    /// Vanilla swing baseline, blended toward a designer Bezier shape in mid-swing.
    /// The Bezier's ControlPoints are interpreted as LOCAL OFFSETS (p0 must be (0,0), oriented +X).
    /// </summary>
    public class CustomMotion : IWhipMotion
    {
        private readonly BezierCurve template;   // local-space curve (do NOT mutate this instance)
        private readonly int precision;          // arc-length precision when sampling the world curve

        /// <param name="template">
        /// Local-space Bezier: p0=(0,0), points laid out facing +X (right).
        /// e.g., new BezierCurve(new Vector2(0,0), new Vector2(60,-80), new Vector2(160,90), new Vector2(220,0));
        /// </param>
        /// <param name="precision">Arc-length sampling precision for Bezier (default 30).</param>
        public CustomMotion(BezierCurve template, int precision = 30)
        {
            this.template = template;
            this.precision = Math.Max(8, precision);
        }

        public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
        {
            if (controlPoints == null) return;
            if (template == null || template.ControlPoints == null || template.ControlPoints.Length < 2)
            {
                // Fallback: vanilla only if the curve is invalid
                controlPoints.Clear();
                Projectile.FillWhipControlPoints(projectile, controlPoints);
                return;
            }

       

            // 1) Get arm position and vanilla swing
            Vector2 arm = Main.GetPlayerArmPosition(projectile);
            List<Vector2> vanilla = new();
            Projectile.FillWhipControlPoints(projectile, vanilla);
            int totalPoints = vanilla.Count;

            // 2) Calculate swing direction (first segment of vanilla)
            Vector2 baseDir = vanilla[1] - vanilla[0];
            float baseAngle = baseDir.ToRotation();

            // Current vanilla length (sum of segment lengths)
            float vanillaLen = 0f;
            for (int i = 1; i < totalPoints; i++)
                vanillaLen += Vector2.Distance(vanilla[i - 1], vanilla[i]);
            if (vanillaLen < 1e-3f) vanillaLen = 1f;

            // Template length in LOCAL space (sample once per frame)
            List<Vector2> tempLocal = template.GetEvenlySpacedPoints(totalPoints, precision, forceRecalculate: false);
            float templateLen = 0f;
            for (int i = 1; i < tempLocal.Count; i++)
                templateLen += Vector2.Distance(tempLocal[i - 1], tempLocal[i]);
            if (templateLen < 1e-3f) templateLen = 1f;

            float lengthScale = vanillaLen / templateLen;





            // 3) Build world-space Bezier from local offsets
            Vector2[] worldCP = new Vector2[template.ControlPoints.Length];
            for (int i = 0; i < template.ControlPoints.Length; i++)
            {
                Vector2 rel = template.ControlPoints[i];

                // Scale to current whip length
                rel *= lengthScale;

                // Flip horizontally when facing left
                if (projectile.spriteDirection == -1)
                    rel.X = -rel.X;

                // Rotate into the same angle as the vanilla swing
                rel = rel.RotatedBy(baseAngle);

                // Anchor at arm
                worldCP[i] = arm + rel;
            }


            // Create a temporary world curve and sample same count as vanilla
            var worldCurve = new BezierCurve(worldCP);
            List<Vector2> bezier = worldCurve.GetEvenlySpacedPoints(totalPoints, precision, forceRecalculate: true);
            if (bezier.Count != totalPoints)
            {
                // Guard: counts must match to avoid despawn
                controlPoints.Clear();
                controlPoints.AddRange(vanilla);
                return;
            }

            // 3) Bell-curve influence: 0 at ends, 1 at mid-swing
            float bell = MathF.Sin(MathHelper.Clamp(progress, 0f, 1f) * MathHelper.Pi);

            // 4) Blend vanilla ↔ Bezier
            controlPoints.Clear();
            for (int i = 0; i < totalPoints; i++)
                controlPoints.Add(Vector2.Lerp(vanilla[i], bezier[i], bell*0.2f));
        }
    }



    // -------------------------
    // Controller
    // -------------------------
    public class ModularWhipController
    {
        private readonly IWhipMotion motion;
        private readonly List<IWhipModifier> modifiers = new();

        public ModularWhipController(IWhipMotion motion)
        {
            this.motion = motion;
        }

        public void AddModifier(IWhipModifier modifier) => modifiers.Add(modifier);

        // segments & rangeMultiplier must come from Projectile.GetWhipSettings(...) (or similar) so the motion can size itself.
        public void Apply(List<Vector2> controlPoints, Projectile projectile, int segments, float rangeMultiplier, float progress)
        {
            if (controlPoints == null) return;
            // Build base motion (this clears & fills controlPoints)
            motion.Apply(controlPoints, projectile, segments, rangeMultiplier, progress);

            // Apply all modifiers (they mutate in-place)
            foreach (var mod in modifiers)
                mod.Apply(controlPoints, projectile, segments, rangeMultiplier, progress);
        }
    }
}
