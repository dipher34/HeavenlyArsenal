using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.CalPlayer;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Cooldowns;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.NPCs.TownNPCs;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Buffs;
using HeavenlyArsenal.Content.Items.Armor;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Particles.Metaballs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.TentInterior.Cutscenes;
using ReLogic.Content;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.ArsenalPlayer
{
    public class ShintoArmorPlayer : ModPlayer
    {
        public bool SetActive;
        public int maxBarrier;
        public int barrier;
        public int timeSinceLastHit;
        public int Iframe;
        public int rechargeDelay;
        public int rechargeRate;
        public float barrierDamageReduction = 0.5f;
        public float barrierSizeInterp = 0;
        public bool ShadowShieldVisible = true;
        public bool ShadowVeil;
        internal float barrierShieldPartialRechargeProgress = 0f;

        public bool IsDashing;
        public bool empoweredDash;

        public float EnrageInterp = 0;

        public bool Enraged;
        #region s
        /*
            +++++++++++++++++++++++++++++++++++++*+*
            ++++++++++++++++++++++++++++++++++++++++
            ++++++++++++++++++++++++++++++++++++++++
            ++++++++++++++++++++++++++++++++++++++++
            +++++++++++++++++%@%+-...:=%@%++++++++++
            +++++++++++++*%*:.:::.:.-+=.:-=%#+++++++
            +++++++++%@@%-...:+-=-:*++-.:==.:##+++++
            ++++++#@%%@-..:=*+=%=..:=#..:==...+%++++
            ++++#@%#%%....##%@@@=..:=*:..==..*@@*+++
            +++%%##%%.......::*@*@*-=*:..===@+:+%+++
            ++@%###@:.....:=%@+-%::-+++.*%@+-..+%+++
            +@%###%#...:-=*%:+%=+#+:++...=++:.:*#+++
            #@####%*.....-::%=#+..::++:-===-...@%++*
            @#####%*.......::#-:..::++...===...@@+++
            @#####%#.....:+**#:...::=+. .--=.=@@@*++
            @#####%@...::..+-+=...::=+...--=:..#@#++
            %%#####@*.......-*=...::=+. .:==-...%*++
            +@%#####@-.......:::=*=::-...:=-=*.+@*++
            ++%@####%@-.......+#-..::++=+++==..%#+++
            +++*@%####@*........:..::-+::---===*++++
            +++++*%%###%@*..........:-+-.::-:*%+++++
            ++++++++*####%%@#=............:#@#++++++
            +++++*%###########%@@*-@%+++#@@@@%++++++
            +++#%#########################*+*%@@%+++
            +#@%##################*#*+++++++++*****#
            @%#####**++++++**++++*##################
            ++++++++++++*###########################
         */
        #endregion
        public Dictionary<string, CooldownInstance> cooldowns;
        
        public int ShadowVeilImmunity = 0;
        public bool ChestplateEquipped = false;

        public bool VoidBeltEquipped = false;
        public override void Initialize()
        {

            cooldowns = new Dictionary<string, CooldownInstance>();
            maxBarrier = ShintoArmorBreastplate.ShieldDurabilityMax;
            barrier = 0;
            timeSinceLastHit = 0;
            Iframe = 0;
            rechargeDelay = ShintoArmorBreastplate.ShieldRechargeDelay;
            rechargeRate = ShintoArmorBreastplate.ShieldRechargeRate;
           

            ChestplateEquipped = false;
            IsDashing = false;
            empoweredDash = false;
            VoidBeltEquipped = false;
            Enraged = false;

        }
        public override void Load()
        {
           
            PlayerDashManager.TryAddDash(new ShintoArmorDash());
            

        }
        public override void ArmorSetBonusActivated()
        {
            if(SetActive)
            Enraged = Enraged == true ? false : true;
        }
        public override void PostUpdateMiscEffects()
        {
            ManageImmunity();
            ManageBarrier();
            ManageEnraged();
            AntishadowHealing();
            if (Iframe > 0)
            {
                Iframe--;
            }
            if (ShadowVeil)
            {
                CalamityPlayer modPlayer = Player.Calamity();
                bool wearingRogueArmor = modPlayer.wearingRogueArmor;
                float rogueStealth = modPlayer.rogueStealth;
                float rogueStealthMax = modPlayer.rogueStealthMax;
                int chaosStateDuration = 900;

                if (CalamityKeybinds.SpectralVeilHotKey.JustPressed && ShadowVeil && Main.myPlayer == Player.whoAmI && rogueStealth >= rogueStealthMax * 0.25f &&
                    wearingRogueArmor && rogueStealthMax > 0)
                {
                    if (!Player.chaosState)
                    {
                        Vector2 teleportLocation;
                        teleportLocation.X = Main.mouseX + Main.screenPosition.X;
                        if (Player.gravDir == 1f)
                            teleportLocation.Y = Main.mouseY + Main.screenPosition.Y - Player.height;
                        else
                            teleportLocation.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY;

                        teleportLocation.X -= Player.width * 0.5f;
                        Vector2 teleportOffset = teleportLocation - Player.position;
                        if (teleportOffset.Length() > SpectralVeil.TeleportRange)
                        {
                            teleportOffset = teleportOffset.SafeNormalize(Vector2.Zero) * SpectralVeil.TeleportRange;
                            teleportLocation = Player.position + teleportOffset;
                        }
                        if (teleportLocation.X > 50f && teleportLocation.X < (float)(Main.maxTilesX * 16 - 50) && teleportLocation.Y > 50f && teleportLocation.Y < (float)(Main.maxTilesY * 16 - 50))
                        {
                            if (!Collision.SolidCollision(teleportLocation, Player.width, Player.height))
                            {
                                rogueStealth -= rogueStealthMax * 0.25f;

                                Player.Teleport(teleportLocation, 1);
                                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, (float)Player.whoAmI, teleportLocation.X, teleportLocation.Y, 1, 0, 0);

                                int duration = chaosStateDuration;
                                Player.AddBuff(BuffID.ChaosState, duration, true);
                                Player.AddCooldown(ChaosState.ID, duration, true, "spectralveil");

                                int numDust = 40;
                                Vector2 step = teleportOffset / numDust;
                                for (int i = 0; i < numDust; i++)
                                {
                                    int dustIndex = Dust.NewDust(Player.Center - (step * i), 1, 1, DustID.VilePowder, step.X, step.Y);
                                    Main.dust[dustIndex].noGravity = true;
                                    Main.dust[dustIndex].noLight = true;
                                }

                                ShadowVeilImmunity = ShintoArmorHelmetAll.ShadowVeilIFrames;
                            }
                        }
                    }
                }
            }

            
            
        }
        private void ManageEnraged()
        {
            if (SetActive)
            {
                if (Enraged)
                {
                    //there's gotta be a better way to do it than this.

                    Player.Calamity().RoverDriveShieldDurability =(int) float.Lerp(Player.Calamity().RoverDriveShieldDurability, -1, 0.4f);
                    Player.Calamity().cooldowns.Remove(WulfrumRoverDriveDurability.ID);
                    Player.Calamity().cooldowns.Remove(WulfrumRoverDriveRecharge.ID);

                    Player.Calamity().pSoulShieldDurability = (int)float.Lerp(Player.Calamity().pSoulShieldDurability, -1, 0.4f);
                    Player.Calamity().cooldowns.Remove(ProfanedSoulShield.ID);
                    Player.Calamity().cooldowns.Remove(ProfanedSoulShieldRecharge.ID);

                    Player.Calamity().SpongeShieldDurability = (int)float.Lerp(Player.Calamity().SpongeShieldDurability, -1, 0.4f);

                    Player.Calamity().cooldowns.Remove(SpongeRecharge.ID);
                    Player.Calamity().cooldowns.Remove(SpongeDurability.ID);
                   
                    Player.GetDamage<GenericDamageClass>() *= 3;
                    EnrageInterp = float.Lerp(EnrageInterp, 1, 0.4f);
                    cooldowns.Clear();
                    barrier = (int)float.Lerp(barrier,-1, 0.14f);
                    if (barrier <= 0)
                        Player.Calamity().cooldowns.Remove(BarrierDurability.ID);
                    barrierSizeInterp = float.Lerp(barrierSizeInterp, -1, 0.14f);
                    if (barrierSizeInterp <= 0)
                        barrierSizeInterp = 0;
                    timeSinceLastHit = 0;
                    Player.Calamity().cooldowns.Remove(BarrierRecharge.ID);
                }
                else
                {
                    if (timeSinceLastHit == 1 && EnrageInterp > 0.9)
                    {
                        barrier = 0;
                        Player.AddCooldown(BarrierRecharge.ID, ShintoArmorBreastplate.ShieldRechargeDelay);
                        if (Player.Calamity().HasAnyEnergyShield)
                        {
                            
                        }
                    }
                    EnrageInterp = float.Lerp(EnrageInterp, 0, 0.1f);
                }
            }
            else Enraged = false;
        }
        private void ManageImmunity()
        {
            if (SetActive)
            {
                //Main.NewText($"Barrier: {barrier}, TimeSinceLastHit: {timeSinceLastHit}", Color.AntiqueWhite);
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
                Player.buffImmune[BuffID.OnFire] = true;
                Player.buffImmune[BuffID.Weak] = true;
                Player.buffImmune[BuffID.BrokenArmor] = true;
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
        private void ManageBarrier()
        {
            if (timeSinceLastHit == 0 && !cooldowns.ContainsKey(BarrierRecharge.ID))
                Player.AddCooldown(BarrierRecharge.ID, ShintoArmorBreastplate.ShieldRechargeDelay);
            if (SetActive)
            {
                

                // Update the durability cooldown display if the shield is active.
                if (barrier > 0 && !cooldowns.ContainsKey(BarrierDurability.ID))
                {
                    if(barrierSizeInterp < 1)
                    {
                        barrierSizeInterp = float.Lerp(barrierSizeInterp, 1, 0.1f);
                    }
                    var durabilityCooldown = Player.AddCooldown(BarrierDurability.ID, ShintoArmorBreastplate.ShieldDurabilityMax);
                    durabilityCooldown.timeLeft = barrier;
                }
                if (barrier <= 0 )
                    Player.Calamity().cooldowns.Remove(BarrierDurability.ID);
                // Only update recharge progress visuals once the recharge delay has passed.
                if (timeSinceLastHit >= rechargeDelay && barrier > 0 && !cooldowns.ContainsKey(BarrierRecharge.ID))
                {
                    barrierShieldPartialRechargeProgress += ShintoArmorBreastplate.ShieldDurabilityMax / (float)ShintoArmorBreastplate.TotalShieldRechargeTime;
                    int pointsActuallyRecharged = (int)MathF.Floor(barrierShieldPartialRechargeProgress);
                    // durabilityCooldown.timeLeft += 
                    // This value is used only for visual display.
                    int displayBarrier = Math.Min(barrier + pointsActuallyRecharged, TheSponge.ShieldDurabilityMax);

                    barrierShieldPartialRechargeProgress -= 400 - timeSinceLastHit;

                    if (cooldowns.TryGetValue(BarrierDurability.ID, out var cdDurability))
                        cdDurability.timeLeft = displayBarrier;
                }
            }
            if(!SetActive)
            {
                barrierSizeInterp = float.Lerp(barrierSizeInterp, 0, 0.2f);
                barrier = 0;
                timeSinceLastHit = 0;
                rechargeDelay = ShintoArmorBreastplate.ShieldRechargeDelay;
                
                Player.Calamity().cooldowns.Remove(BarrierDurability.ID);
                
                Player.Calamity().cooldowns.Remove(BarrierRecharge.ID);
            }
            

        }

        private void AntishadowHealing()
        {
            
            if (!SetActive)
                return;
            if (Player.StandingStill() && Player.velocity.Y == 0 && Player.itemAnimation == 0)
            {

                // Actually apply "standing still" regeneration (the stats are granted even at full health)
                float regenTimeNeededForTurboRegen = 400;//shadeRegen ? 40f : cFreeze ? 60f : honeyDewWorking ? 90f : photosynthesis ? 90f : aAmpoule ? 90f : purity ? 60f : -1f;

                // 4 = vanilla Shiny Stone
                int turboRegenPower = 12 * (1 + (int)Player.lifeRegenTime / 1000);//shadeRegen || cFreeze || purity ? 4 : honeyDewWorking || aAmpoule ? 3 : photosynthesis ? 1 : -1;

                if (turboRegenPower > 0)
                {
                    // After a brief delay determined by your form of standing still regen, min-cap life regen time at 1800 / 3600.
                    // Photosynthesis Potion does not do this at night.
                    if (Player.lifeRegenTime > regenTimeNeededForTurboRegen && Player.lifeRegenTime < 1800f)
                        Player.lifeRegenTime = 1800f;

                    Player.lifeRegen += turboRegenPower;
                    Player.lifeRegenTime += turboRegenPower;
                }

                if (Player.lifeRegen > 0 && Player.statLife < Player.Calamity().actualMaxLife)
                {
                    if (Main.rand.NextBool(1))
                    {
                        AntishadowBlob Blob = ModContent.GetInstance<AntishadowBlob>();
                        for (int i = 0; i < turboRegenPower; i++)
                        {
                            Vector2 bloodSpawnPosition = Player.Center + Main.rand.NextVector2CircularEdge(120 + Main.rand.Next(-10,10), 120 + Main.rand.Next(-10, 10));

                            //var dust = Dust.NewDustPerfect(bloodSpawnPosition, DustID.AncientLight, Vector2.Zero, default, Color.Red);
                            //dust.noGravity = true;
                            Blob.player = Player;

                            Blob.CreateParticle(bloodSpawnPosition, Vector2.Zero, 0, 0);
                        }
                    }
                    

                }

                

            }
            
        }

        #region Barrier stuff
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if(SetActive)
            {
                if (barrier > 0 && Iframe <= 0)
                {
                    modifiers.DisableSound();
                    
                    SoundEngine.PlaySound(ShintoArmorBreastplate.ShieldHurtSound, Player.Center);
                }
                else if (barrier <= 0)
                {
                    if (timeSinceLastHit! < 0)
                    {
                        SoundEngine.PlaySound(ShintoArmorBreastplate.BreakSound, Player.Center);
                        barrier = 0;
                       
                    }


                }

                if(barrier <=0 && Enraged)
                {
                    modifiers.FinalDamage *= 3;
                }
            }
           
        }

        public void BarrierTakeDamageVFX()
        {
            for (int i = 0; i < Main.rand.Next(1, 5); i++)
            {
                Vector2 lightningPos = Player.Center + Main.rand.NextVector2Circular(24, 24);

                HeatLightning particle = HeatLightning.pool.RequestParticle();
                particle.Prepare(lightningPos, Player.velocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), 10 + i * 3, Main.rand.NextFloat(0.5f, 1f));
                ParticleEngine.Particles.Add(particle);
            }
        }

       

        public override void PostUpdateEquips()
        {
            if (barrier > 0 && SetActive)
            {
                Player.statDefense += 30;
            }
        }

        #endregion
        public override bool FreeDodge(Player.HurtInfo info)
        {
            timeSinceLastHit = 0;
            if (barrier > 0 && SetActive)
            {
                int incoming = info.SourceDamage/4;//info.Damage;
                
                if (Iframe <= 0)
                {
                    
                    int TakenDamage = (int)Math.Round(incoming * 0.5f, 0);
                    
                    if(TakenDamage > ShintoArmorBreastplate.ShieldDurabilityMax && barrier == ShintoArmorBreastplate.ShieldDurabilityMax)
                    {
                        GeneralScreenEffectSystem.ChromaticAberration.Start(Player.Center, 3f, 90);
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.AngryDistant with { Volume = 0.05f }, Player.Center);
                       
                    }
                    Iframe = (int)(Player.ComputeHitIFrames(info)*1.25f);
                    if (info.DamageSource.SourceProjectileType.Equals(ModContent.ProjectileType<StolenPlanetoid>()))
                    {
                        
                        
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Clap);
                        barrier = 0;
                        TakenDamage = 999999;
                    }
                    else

                        barrier -= TakenDamage;
                    BarrierTakeDamageVFX();
                    CombatText.NewText(Player.Hitbox, Color.Cyan, TakenDamage);
                    
                   
                }

                

                // Cancel all damage to the player.
                return true;
            }
            else if(barrier <= 0 && timeSinceLastHit < Iframe)
            {
                return true;
            }
            else if (VoidBeltEquipped && barrier <= 0 && Main.rand.NextBool(7))
            {

                VoidBelt();
                return true;
            }
            else
                return false;
        }

        

        public override void UpdateBadLifeRegen()
        {
            // Increment time since last hit only when the shield is active.
            if (maxBarrier > 0 && SetActive)
                timeSinceLastHit++;

            // Only update the actual recharge if the recharge delay has passed and no recharge cooldown is active.
            if (!cooldowns.ContainsKey(BarrierRecharge.ID) && timeSinceLastHit >= rechargeDelay && barrier < maxBarrier && SetActive)
            {
                int rechargeRateWhole = rechargeRate / 60;
                barrier += Math.Min(rechargeRateWhole, maxBarrier - barrier);

                if (rechargeRate % 60 != 0)
                {
                    int rechargeSubDelay = 60 / (rechargeRate % 60);
                    
                    //starts recharging at 400 timesince
                    if (timeSinceLastHit % rechargeSubDelay == 0 && barrier < maxBarrier)
                    {
                        //Main.NewText($"Here: {timeSinceLastHit}", Color.AntiqueWhite);
                        
                        barrier++;
                    }

                }
            }
        }

        public void VoidBelt()
        {
           
            GeneralScreenEffectSystem.RadialBlur.Start(Player.Center, 3f, 90);
            Player.SetImmuneTimeForAllTypes(120);
            Dust.NewDust(Player.Center, Player.width, Player.height, DustID.BunnySlime, 0, 0, 100, Color.Crimson, 1);
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.AngryDistant with { Volume = 0.4f, PitchVariance = 0.3f}, Player.Center);
            for (int i = 0; i <20; i++)
            {
                int fireBrightness = Main.rand.Next(40);
                Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
                if (Main.rand.NextBool(3) && Player.velocity.X > 20)
                    fireColor = new Color(220, 20, Main.rand.Next(16), 255);

                Vector2 position = Player.Center + Main.rand.NextVector2Circular(30f, 30f);
                AntishadowFireParticleSystemManager.CreateNew(Player.whoAmI, false, position, Main.rand.NextVector2Circular(30f, Player.velocity.X * .96f), Vector2.One * Main.rand.NextFloat(30f, 50f), fireColor);
            }

        }


       
  
        public override void ResetEffects()
        {
            IsDashing = false;
            SetActive = false;
            ChestplateEquipped = false;
            VoidBeltEquipped = false;
            

        }
    }
}
