using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    internal class RiftEclipseBloodMoon :  IBestiaryInfoElement
    {
        public string _FilterImage;
        public UIElement GetFilterImage()
        {
            Texture2D Texture = GennedAssets.Textures.GraphicalUniverseImager.EclipseSelectionBox_BloodMoon;//ModContent.Request<Texture2D>("SRPTerraria/icon_small").Value;
            if (_FilterImage != null)
                if (ModContent.RequestIfExists(_FilterImage, out Asset<Texture2D> Asset))
                    Texture = Asset.Value;
            return new UIImage(Texture)
            {
                HAlign = 0.5f,
                VAlign = 0.5f
            };
        }
        public UIElement ProvideUIElement(BestiaryUICollectionInfo info) => null;
    }
}
