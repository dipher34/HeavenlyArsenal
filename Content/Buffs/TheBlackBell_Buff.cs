using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using HeavenlyArsenal.Content.Projectiles.Weapons.Summon;

namespace HeavenlyArsenal.Content.Buffs
{
    public class TheBlackBell_Buff : ModBuff
    {
        // Texture => GetAssetPath("Content/Buffs", Name);



        public override string Texture => base.Texture;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false; // This buff won't save when you exit the world
            Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // If the minions exist reset the buff time, otherwise remove the buff from the player
            if (player.ownedProjectileCounts[ModContent.ProjectileType<TheBlackBell_Projectile>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                //player.DelBuff(buffIndex);
               // buffIndex--;
            }
        }
    }


}