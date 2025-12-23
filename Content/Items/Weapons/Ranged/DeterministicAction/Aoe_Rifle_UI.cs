using SteelSeries.GameSense;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_UI : UIState
    {

        public override void OnInitialize()
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.LocalPlayer.HeldItem.ModItem is not Aoe_Rifle_Item)
                return;
            
            base.Draw(spriteBatch);

        }


        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            var modPlayer = Main.LocalPlayer.GetModPlayer<Aoe_Rifle_Player>();
            // Calculate quotient
            float quotient = (float)modPlayer.BulletCount / 10;//modPlayer.exampleResourceMax2; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
            quotient = Utils.Clamp(quotient, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.

            /*
            // Here we get the screen dimensions of the barFrame element, then tweak the resulting rectangle to arrive at a rectangle within the barFrame texture that we will draw the gradient. These values were measured in a drawing program.
            Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();
            hitbox.X += 12;
            hitbox.Width -= 24;
            hitbox.Y += 8;
            hitbox.Height -= 16;

            // Now, using this hitbox, we draw a gradient by drawing vertical lines while slowly interpolating between the 2 colors.
            int left = hitbox.Left;
            int right = hitbox.Right;
            int steps = (int)((right - left) * quotient);
            for (int i = 0; i < steps; i += 1)
            {
                // float percent = (float)i / steps; // Alternate Gradient Approach
                float percent = (float)i / (right - left);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Lerp(gradientA, gradientB, percent));
            }
            */

            Utils.DrawBorderString(spriteBatch, $"{modPlayer.BulletCount}/10", Main.MouseWorld - Main.screenPosition, Color.White, 1, anchorx: 0.2f,anchory:-1);
        }
    }


    // This class will only be autoloaded/registered if we're not loading on a server
    [Autoload(Side = ModSide.Client)]
    internal class ExampleResourceUISystem : ModSystem
    {
        private UserInterface ExampleResourceBarUserInterface; 

        internal Aoe_Rifle_UI ExampleResourceBar;

        public static LocalizedText ExampleResourceText { get; private set; }

        public override void Load()
        {
            ExampleResourceBar = new();
            ExampleResourceBarUserInterface = new();
            ExampleResourceBarUserInterface.SetState(ExampleResourceBar);

            string category = "UI";
            ExampleResourceText ??= Mod.GetLocalization($"{category}.ExampleResource");
        }

        public override void UpdateUI(GameTime gameTime)
        {
            ExampleResourceBarUserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "ExampleMod: Example Resource Bar",
                    delegate {
                        ExampleResourceBarUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
