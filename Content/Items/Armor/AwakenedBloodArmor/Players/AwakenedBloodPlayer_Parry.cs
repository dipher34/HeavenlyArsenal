using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players
{
    internal class AwakenedBloodPlayer_Parry : ModPlayer
    {
        public void HandleParry()
        {

        }
        public int ParryTime { get; set; }
        internal const int bloodThornParry = 30;
        public bool IsParrying
        {
            get => ParryTime > 0;
        }
        public override void PostUpdateMiscEffects()
        {
            if(ParryTime>0)
                ParryTime--;
            
        }
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if(IsParrying)
            {

            }
        }
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if(IsParrying)
            {

            }
        }
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if(IsParrying)
            {
                modifiers.FinalDamage *= 0f;
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if(IsParrying)
            {
                modifiers.FinalDamage *= 0f;
            }
        }

        public static void AttemptParry(Player player)
        {
            if(player.GetModPlayer<AwakenedBloodPlayer>().CurrentForm != AwakenedBloodPlayer.Form.Defense)
                return;
            player.GetModPlayer<AwakenedBloodPlayer_Parry>().ParryTime = bloodThornParry;
        }
    }
}
