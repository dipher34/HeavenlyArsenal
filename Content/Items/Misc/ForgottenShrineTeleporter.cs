using HeavenlyArsenal.Content.Subworlds;
using NoxusBoss.Content.Items.Debugging;
using NoxusBoss.Core.World.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;

namespace HeavenlyArsenal.Content.Items.Misc
{
    public class ForgottenShrineTeleporter : DebugItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = null;
            Item.rare = ItemRarityID.Blue;
            Item.value = 0;
        }

        public override bool? UseItem(Player p)
        {
            if (Main.myPlayer == NetmodeID.MultiplayerClient || p.itemAnimation != p.itemAnimationMax - 1)
                return false;

            if (SubworldSystem.IsActive<ForgottenShrineSubworld>())
                SubworldSystem.Exit();
            else
            {
                ForgottenShrineSubworld.ClientWorldDataTag = EternalGardenNew.SafeWorldDataToTag("Client", false);
                SubworldSystem.Enter<ForgottenShrineSubworld>();
            }
            return null;
        }
    }
}