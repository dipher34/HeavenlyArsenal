using HeavenlyArsenal.Content.Items.Accessories.Cosmetic;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering.RenderSteps;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;
using static NoxusBoss.Assets.GennedAssets.Textures;
using static NoxusBoss.Content.NPCs.Bosses.NamelessDeity.NamelessDeityBoss;
using Microsoft.Xna.Framework.Graphics;
using Luminance.Common.Utilities;



namespace HeavenlyArsenal.Content.Items.Accessories
{
    class NDWings : ModItem
    {
        public override void SetDefaults()
        {
        }

        public override void SetStaticDefaults() { }

    }

    class NDWingsPlayer : ModPlayer
    {
       
    /// <summary>
    /// The current rotation of the wings.
    /// </summary>
    public float Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// The previous rotation of the wings.
        /// </summary>
        public float PreviousRotation
        {
            get;
            set;
        }

        /// <summary>
        /// The amount of squish to apply to wings.
        /// </summary>
        public float Squish
        {
            get;
            set;
        }

        /// <summary>
        /// Updates the wings.
        /// </summary>
        /// <param name="motionState">The motion that should be used when updating.</param>
        /// <param name="animationCompletion">The 0-1 interpolant for the animation completion.</param>
        public void Update(WingMotionState motionState, float animationCompletion)
        {
            // Cache the current wing rotation as the previous one.
            PreviousRotation = Rotation;

            // Positive rotations correspond to upward flaps.
            // Negative rotations correspond to downward flaps.
            PiecewiseCurve flap = new PiecewiseCurve().
                Add(EasingCurves.Cubic, EasingType.Out, 0.67f, 0.25f). // Upward rise.
                Add(EasingCurves.Quadratic, EasingType.In, -1.98f, 0.44f). // Flap.
                Add(EasingCurves.MakePoly(1.5f), EasingType.In, 0f, 1f); // Recovery.

            PiecewiseCurve squish = new PiecewiseCurve().
                Add(EasingCurves.Cubic, EasingType.Out, 0.15f, 0.25f). // Upward rise.
                Add(EasingCurves.Quintic, EasingType.In, 0.7f, 0.44f). // Flap.
                Add(EasingCurves.MakePoly(1.3f), EasingType.InOut, 0f, 1f); // Recovery.

            // It's easing curve time!
            switch (motionState)
            {
                case WingMotionState.RiseUpward:
                    Squish = 0f;
                    Rotation = (-0.6f).AngleLerp(0.36f, animationCompletion);
                    break;
                case WingMotionState.Flap:
                    Squish = squish.Evaluate(animationCompletion % 1f);
                    Rotation = flap.Evaluate(animationCompletion % 1f);
                    break;
            }
        }
    }
    class NDWings_Drawlayer : PlayerDrawLayer
    {
        public Texture2D Wings { get; private set; }

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Wings);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(NDWings), EquipType.Wings);

        public override bool IsHeadLayer => false;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<NDWingsPlayer>();
            modPlayer.Update(WingMotionState.Flap, Main.GlobalTimeWrappedHourly % 1f);

            if (Wings == null)
            {
                Wings = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Accessories/Cosmetic/ExampleWings").Value;
            }

            // Fixing the CS1503 error by ensuring the second argument is a Rectangle, not a Vector2.  
            Rectangle destinationRectangle = new Rectangle(
                (int)(drawInfo.drawPlayer.position.X - Main.screenPosition.X),
                (int)(drawInfo.drawPlayer.position.Y - Main.screenPosition.Y),
                Wings.Width,
                Wings.Height
            );

            Main.spriteBatch.Draw(Wings, destinationRectangle, null, Color.White, modPlayer.Rotation, Wings.Size() * 0.5f, SpriteEffects.None, 0); 
        }
    }
}
