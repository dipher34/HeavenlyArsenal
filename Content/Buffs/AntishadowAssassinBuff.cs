using HeavenlyArsenal.Content.Items.Weapons.Summon;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Buffs;

public class AntishadowAssassinBuff : ModBuff
{
    public override string Texture => base.Texture;

    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;

        PlayerDataManager.ResetEffectsEvent += ResetMinionState;
    }

    private void ResetMinionState(PlayerDataManager p)
    {
        p.Player.GetValueRef<bool>("HasAntishadowAssassin").Value = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        Referenced<bool> hasMinion = player.GetValueRef<bool>("HasAntishadowAssassin");
        if (player.ownedProjectileCounts[ModContent.ProjectileType<AntishadowAssassin>()] > 0)
            hasMinion.Value = true;

        if (!hasMinion.Value)
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
        else
        {
            player.buffTime[buffIndex] = 18000;
        }
    }
}
