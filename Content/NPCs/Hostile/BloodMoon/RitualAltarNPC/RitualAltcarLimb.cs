using HeavenlyArsenal.Common.IK;
using Microsoft.Xna.Framework;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    //credit: https://github.com/mayli4/AllBeginningsMod/blob/main/src/AllBeginningsMod/Content/Bosses/_Nightgaunt/NightgauntNPC.Limbs.cs
    // thanks bozo :3
    internal partial class RitualAltar
    {
        internal record struct RitualAltarLimb(IKSkeleton skeleton, bool anchored = false, bool hasTarget = false)
        {

        
            public IKSkeleton Skeleton = skeleton;
            public Vector2 TargetPosition = Vector2.Zero;
            public Vector2 EndPosition = Vector2.Zero;
            public bool IsAnchored = anchored;

            public bool HasTarget;        
            public Point TargetTile;
            public int Cooldown;
            public bool IsTouchingGround;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateLimbState(ref RitualAltarLimb nightgauntLimb, Vector2 basePos, float lerpSpeed, float anchorThreshold)
        {
            nightgauntLimb.EndPosition = Vector2.Lerp(nightgauntLimb.EndPosition, nightgauntLimb.TargetPosition, lerpSpeed);
            nightgauntLimb.Skeleton.Update(basePos, nightgauntLimb.EndPosition);
            nightgauntLimb.IsAnchored = Vector2.Distance(nightgauntLimb.EndPosition, nightgauntLimb.TargetPosition) < anchorThreshold;
        }

        void CreateLimbs()
        {
            ////_rightArm = new RitualAltarLimb(new IKSkeleton((46f, new()), (60f, new() { MinAngle = -MathHelper.Pi, MaxAngle = 0f })));
            //_knifeArm = new RitualAltarLimb(new IKSkeleton((36f, new()), (60f, new() { MinAngle = MathHelper.Pi, MaxAngle = 0f })));

            //_legOne = new RitualAltarLimb(new IKSkeleton((36f, new()), (60f, new() { MinAngle = MathHelper.Pi, MaxAngle = 0f })));
            _limbs = new RitualAltarLimb[LimbCount];
            _limbBaseOffsets = new Vector2[LimbCount];

            // Equidistant offsets around the bottom of the NPC
            float width = NPC.width * 0.3f;
            _limbBaseOffsets[0] = new Vector2(-width, NPC.height / 2 -  20);
            _limbBaseOffsets[1] = new Vector2(width, NPC.height / 2 - 20);
            _limbBaseOffsets[2] = new Vector2(-width * 0.5f, NPC.height / 2 - 10);
            _limbBaseOffsets[3] = new Vector2(width * 0.5f, NPC.height / 2 - 10);

            for (int i = 0; i < LimbCount; i++)
            {
                _limbs[i] = new RitualAltarLimb(
                    new IKSkeleton(
                        (36f, new IKSkeleton.Constraints()),
                        (60f, new IKSkeleton.Constraints())
                    )
                );
                _limbs[i].TargetPosition = NPC.Center + _limbBaseOffsets[i] + new Vector2(0, 40);
                _limbs[i].EndPosition = _limbs[i].TargetPosition;
            }
        }
    }
}
    
