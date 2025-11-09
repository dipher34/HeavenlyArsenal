using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    public class RiftEclipseBloodMoon: ModBiome
    {
        public override string Name => Language.GetTextValue($"Mods.{Mod.Name}.Bestiary.Biome");
        public override Color? BackgroundColor => Color.Black;

        public override string BestiaryIcon => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RiftEclipseBloodMoon";

        public override bool IsBiomeActive(Player player) => false;

        public override float GetWeight(Player player) => 0f;
    }
}
