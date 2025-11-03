namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    partial class BloodJelly
    {
        public enum Behavior
        {
            Drift,
            FindTarget,

            DiveBomb,

            StickAndExplode,

            Recycle
        }
        public Behavior CurrentState
        {
            get => (Behavior)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }

        public void StateMachine()
        {
            switch (CurrentState)
            {
                case Behavior.Drift:
                    Drift(); 
                    break;

                case Behavior.FindTarget:
                    FindTarget();
                    break;
                case Behavior.DiveBomb:
                    DiveBomb();
                    break;

                case Behavior.StickAndExplode:
                    break;

                case Behavior.Recycle:
                    break;
            }
        }

        void Drift()
        {

        }

        void FindTarget()
        {

        }
        void DiveBomb()
        {

        }

        void StickAndExplode()
        {

        }

        void Recycle()
        {

        }
    }
}
