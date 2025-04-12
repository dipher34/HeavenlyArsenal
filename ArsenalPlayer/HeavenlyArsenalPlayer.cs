using System;
using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using HeavenlyArsenal.Content.Items.Accessories;
using CalamityMod.Projectiles.Typeless;
using Terraria.Audio;
using HeavenlyArsenal.Projectiles.Misc;
using Terraria.Chat;
using HeavenlyArsenal.Content.Items.Armor;
using CalamityMod.CalPlayer.Dashes;







namespace HeavenlyArsenal.ArsenalPlayer
{
    public partial class HeavenlyArsenalPlayer : ModPlayer
    {
        internal bool ElectricVambrace;
        public int AvatarRifleCounter = 7;

        public float CessationHeat = 0;
        //todo: clean this up, its ugly
        public bool CessationHeld;
        public bool HasReducedDashFirstFrame { get; private set; }


        public bool isVambraceDashing
        {
            get;
            set;
        }
        public bool hasAvatarRifle { 
            get; 
            private set; 
        }

        public override void Load()
        {

            PlayerDashManager.TryAddDash(new ElectricVambraceDash());
           

        }

        public override void PostUpdate()

        {
            if (ElectricVambrace)
            {
               
                if (Player.miscCounter % 3 == 2 && Player.dashDelay > 0) // Reduced dash cooldown by 33%
                    Player.dashDelay--;

                //Console.WriteLine(Player.dashDelay);
                
                if (Player.dashDelay == -1)// TODO: prevent working with special dashes, this was inconsitent with my old solution so I didn't keep it. not huge deal)
                {
                    Player.endurance += 0.1f;
                    if (!isVambraceDashing) // Dash isn't reduced, this is used to determine the first frame of dashing
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.4f, PitchVariance = 0.4f }, Player.Center);
                        
                        int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(750));
                        isVambraceDashing = true;
                       

                    }

                    else
                        isVambraceDashing = false;
                }
            }   

            if (hasAvatarRifle)
            {

            }
        }


        public override void PostUpdateMiscEffects()
        {
            if (ElectricVambrace)
            {
                
            }
        }
        public override void ResetEffects()
        {
            CessationHeld = false;
            ElectricVambrace = false;
            hasAvatarRifle = false;
        }
    }
}