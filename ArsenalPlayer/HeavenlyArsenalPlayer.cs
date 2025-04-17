using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityMod.CalPlayer.Dashes;
using HeavenlyArsenal.Content.Items.Accessories.Vambrace;







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

        }

        public override void PostUpdate()

        {
            if (ElectricVambrace)
            {
               
                if (Player.miscCounter % 8 == 7 && Player.dashDelay > 0) // Reduced dash cooldown by 38%
                    Player.dashDelay--;

                //Console.WriteLine(Player.dashDelay);
                
                if (Player.dashDelay == -1){
                    
                    Player.endurance += 0.20f;
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