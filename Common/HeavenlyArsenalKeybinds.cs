using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using NoxusBoss.Assets;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common
{
    //idunwannahearit
    public class HeavenlyArsenalKeybinds : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            var bloodArmorPlayer = Player.GetModPlayer<BloodArmorPlayer>();
            var modPlayer = Player.GetModPlayer<AwakenedBloodPlayer>();
            if (KeybindSystem.HaemsongBind.JustPressed && modPlayer.AwakenedBloodSetActive)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { Volume = 0.2f, Pitch = -1f }, Player.Center, null);
               
                bloodArmorPlayer.CurrentForm = bloodArmorPlayer.CurrentForm == BloodArmorForm.Offense
                    ? BloodArmorForm.Defense
                    : BloodArmorForm.Offense;
               
                modPlayer.CurrentForm = modPlayer.CurrentForm == AwakenedBloodPlayer.Form.Offsense
                    ? AwakenedBloodPlayer.Form.Defense
                    : AwakenedBloodPlayer.Form.Offsense;
            }

            var ShintoPlayer = Player.GetModPlayer<ShintoArmorPlayer>();
            if (KeybindSystem.ShadowTeleport.JustPressed && ShintoPlayer.SetActive)
            {
                ShintoPlayer.isShadeTeleporting = true;
            }
        }
    }

    public class KeybindSystem : ModSystem
    {
        public static ModKeybind ShadowTeleport { get; private set; }
        public static ModKeybind HaemsongBind { get; private set; }
        public override void Load()
        {
            // We localize keybinds by adding a Mods.{ModName}.Keybind.{KeybindName} entry to our localization files. The actual text displayed to English users is in en-US.hjson
            HaemsongBind = KeybindLoader.RegisterKeybind(Mod, "Toggle Haemsong Mode", "F");
            ShadowTeleport = KeybindLoader.RegisterKeybind(Mod, "Shadow Teleport", "F");
        }
        public override void Unload()
        {
            HaemsongBind = null;
            ShadowTeleport = null;
        }
    }
}