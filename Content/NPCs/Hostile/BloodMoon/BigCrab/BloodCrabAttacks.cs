using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    partial class BloodCrab
    {
        public enum Behavior
        {
            debug,
            CheckVictimRange,

            //if close up, victim range  -> meleeCharge
            MeleeCharge,
            
            FindBombardLocation,
            Bombard,

            AntiAirMeasures,
            
            ReleaseSquids

        }
        public Behavior CurrentState;
    }
}
