using HeavenlyArsenal.Common.Keybinds;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players
{
    internal class AwakenedBlood_Parry_KeybindHandler : ModPlayer, IKeybindHandler
    {
        public override void Load()
        {
            HAKeybindRegistry.Register(new AwakenedBlood_Parry_KeybindHandler());
        }
        public void Process(Player player)
        {
           AwakenedBloodPlayer_Parry.AttemptParry(player);
        }
    }
}
