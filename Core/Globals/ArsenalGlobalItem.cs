using CalamityMod.UI.CalamitasEnchants;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace HeavenlyArsenal.Core.Globals;

public class ArsenalGlobalItem : GlobalItem
{
    public delegate void ModifyItemLootDelegate(Item item, ItemLoot loot);

    public static event ModifyItemLootDelegate? ModifyItemLootEvent;

    public static List<Enchantment> EnchantmentList { get; internal set; } = new List<Enchantment>();


    public override void ModifyItemLoot(Item item, ItemLoot loot)
    {
        ModifyItemLootEvent?.Invoke(item, loot);
    }

    

    // TODO: try to mess around with the Items name while empowered
    /*
    public override void SetDefaults(Item item)
    {
          
        if (item.netID == ModContent.ItemType<AvatarLonginus>())
        {
            foreach (Player.GetModPlayer<AvatarSpearHeatPlayer>().Active in )
            {
                if (Player.GetModPlayer<AvatarSpearHeatPlayer>().Active)
                    item.SetNameOverride("");
            }
            
        }
    }
    */
}
