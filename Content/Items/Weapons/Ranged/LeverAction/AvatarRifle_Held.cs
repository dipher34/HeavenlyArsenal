using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.LeverAction
{
    partial class AvatarRifle_Held : ModProjectile
    {
        public AvatarRiflePlayer2 riflePlayer => Owner.GetModPlayer<AvatarRiflePlayer2>();
        public float RotationOffset;
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public int CosmeticTime
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public ref Player Owner => ref Main.player[Projectile.owner];
        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Ranged/LeverAction/AvatarRifle";

        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            CheckDespawnConditions();
            Projectile.Center = Owner.Center;
            if (CurrentState != State.Reload)
                Projectile.rotation = Projectile.rotation.AngleLerp(Owner.Calamity().mouseWorld.AngleFrom(Owner.Center) + RotationOffset,0.7f);
         
            if (CurrentState != State.Cycle && CurrentState != State.Cycle)
                RotationOffset = float.Lerp(RotationOffset, 0, 0.2f);
            StateMachine();

            Projectile.timeLeft++;
            Time++;
        }
        public override void PostAI()
        {
            Owner.heldProj = Projectile.whoAmI;
            CosmeticTime++;
        }
        void CheckDespawnConditions()
        {
            if (Owner.HeldItem.type != ModContent.ItemType<AvatarRifle>()
                || Owner.DeadOrGhost || !Owner.active)
            {
                Projectile.Kill();
            }
            else
                return;
        }
        
       
        
    }
}
