using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace HeavenlyArsenal.ArsenalPlayer
{
    class ShintoArmorPlayer : ModPlayer
    {
        public bool active;
        public int maxBarrier = 560;
        public int barrier = 0;
        public int timeSinceLastHit;
        public int rechargeDelay = 30;
        public int rechargeRate = 100;
        public float barrierDamageReduction = 0.5f;
        public bool ShadowShieldVisible = false;


        public override void PostUpdateMiscEffects()
        {
            if (active)
            {
                //Main.NewText($"Barrier: {barrier}, TimeSinceLastHit: {timeSinceLastHit}",Color.AntiqueWhite);
                Player.buffImmune[BuffID.Silenced] = true;
                Player.buffImmune[BuffID.Cursed] = true;
                Player.buffImmune[BuffID.OgreSpit] = true;
                Player.buffImmune[BuffID.Frozen] = true;
                Player.buffImmune[BuffID.Webbed] = true;
                Player.buffImmune[BuffID.Stoned] = true;
                Player.buffImmune[BuffID.VortexDebuff] = true;
                Player.buffImmune[BuffID.Electrified] = true;
                Player.buffImmune[BuffID.Burning] = true;
                Player.buffImmune[BuffID.Stinky] = true;
                Player.buffImmune[BuffID.Dazed] = true;
                Player.buffImmune[BuffID.Venom] = true;
                Player.buffImmune[BuffID.CursedInferno] = true;
                if (ModLoader.TryGetMod("Calamity", out Mod CalamityMod))
                {
                    Mod calamity = ModLoader.GetMod("CalamityMod");
                    Player.buffImmune[calamity.Find<ModBuff>("Clamity").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("Dragonfire").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("DoGExtremeGravity").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("FishAlert").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("GlacialState").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("GodSlayerInferno").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("HolyFlames").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("IcarusFolly").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("MiracleBlight").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("Nightwither").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("Plague").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("VulnerabilityHex").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("Warped").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("WeakPetrification").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("WhisperingDeath").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("FabsolVodkaBuff").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("FrozenLungs").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("PopoNoselessBuff").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("SearingLava").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("ShellfishClaps").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("BrimstoneFlames").Type] = true;
                    calamity.Call("SetWearingRogueArmor", Player, true);
                    calamity.Call("SetWearingPostMLSummonerArmor", Player, true);
                }
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (barrier > 0)
            {
                modifiers.ModifyHurtInfo += ModifyDamage;
                timeSinceLastHit = 0;
            }
        }

        private void ModifyDamage(ref Player.HurtInfo info)
        {
            if (barrier > 0)
            {
                int incoming = info.Damage;
                CombatText.NewText(Player.Hitbox, Color.Cyan, incoming);

                // Subtract the full incoming damage from the barrier.
                barrier -= incoming;
                if (barrier < 0)
                {
                    barrier = 0;
                }

                // Cancel all damage to the player.
                info.Damage = 0;
            }
        }
        public override void PostUpdateEquips()
        {
            if (barrier > 0)
            {
                Player.statDefense += 30;
            }
        }

        public override void UpdateBadLifeRegen()
        {
            if (maxBarrier > 0)
                timeSinceLastHit++;

            if (timeSinceLastHit >= rechargeDelay && barrier < maxBarrier)
            {
                int rechargeRateWhole = rechargeRate / 60;
                barrier += Math.Min(rechargeRateWhole, maxBarrier - barrier);

                if (rechargeRate % 60 != 0)
                {
                    int rechargeSubDelay = 60 / (rechargeRate % 60);
                    if (timeSinceLastHit % rechargeSubDelay == 0 && barrier < maxBarrier)
                        barrier++;
                }
            }
        }

        public override void ResetEffects()
        {
            //barrier = 0;
            active = false;
        }
    }
}
