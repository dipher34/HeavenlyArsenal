using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Misc.GoodAppleSlingFolder
{
    class GoodAppleProj : ModProjectile
    {
        private Texture2D _apple;
        private bool _initialized = false;

        public Texture2D Apple
        {
            get
            {
                // If not yet initialized, determine the proper apple texture.
                if (!_initialized)
                {
                    Texture2D goodApple = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Content/Items/GoodApple").Value;
                    Texture2D bittenApple = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Content/Items/GoodAppleBitten").Value;
                    Texture2D badApple = null;

                    if (ModLoader.TryGetMod("CalamityHunt", out Mod calamityHunt) &&
                        calamityHunt.HasAsset("Assets/Textures/Items/Misc/BadApple"))
                    {
                        badApple = calamityHunt.Assets.Request<Texture2D>("Assets/Textures/Items/Misc/BadApple").Value;
                    }

                    // Cache the chosen texture based on whether its quite good or not
                    _apple = good ? goodApple : (badApple ?? bittenApple);
                    _initialized = true;
                }
                return _apple;
            }

            set
            { _apple = value; }
        }

        // This flag is set during OnSpawn based on ammo consumed.
        public bool good { get; set; }
        // Used to check if the apple projectile is still held on a slingshot.
        public bool IsOnSling { get; set; }
        // BounceCount is stored in ai[2]
        public ref float Time => ref Projectile.ai[1];
        public ref float BounceCount => ref Projectile.ai[2];

        public override string Texture => "NoxusBoss/Assets/Textures/Content/Items/GoodApple";

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.width = Projectile.height = 16;
            Projectile.knockBack = 4;
            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;
            //defaults to good apple
            Projectile.ai[1] = 0f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Use ai[0] to set whether this apple is "good" or not.
            // The spawning code (from the item) should pass 1f for a good apple and 0f for a bad apple.
            good = Projectile.ai[1] == 1f;

            // Set the bounce count based on the state: good apples bounce more.
            BounceCount = good ? 3f : 1f;
        }

        public override void AI()
        {
            good = true;
            if (IsOnSling)
            {
                Projectile.timeLeft++;
                // If on a slingshot, hold position.
                Projectile.velocity = Vector2.Zero;
            }
            else
            {
                
                if (Time > 10)
                {
                    Projectile.velocity.X *= 0.99f;
                    Projectile.velocity.Y += 0.2f;
                }

                // Optionally: Apply simple friction when on the ground.
                if (Projectile.velocity.Y == 0f)
                {
                    Projectile.velocity.X *= 0.98f;
                }
            }
            Time++;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Decrement the bounce count each time we collide.
            


            // If there are still bounces left, bounce off the tile.
            if (BounceCount > 0)
            {
                // Reflect velocity with damping.
                if (Projectile.velocity.X != oldVelocity.X)
                {
                    Projectile.velocity.X = -oldVelocity.X * Main.rand.NextFloat(0.75f,1.4f);
                }
                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y * Main.rand.NextFloat(0.53f, 0.6f);
                }
                BounceCount--;
                return false; // Do not kill the projectile.
            }
            else
            {
                // If no bounces are left and the apple is a bad apple, trigger an explosion.
                if (!good)
                {
                    ExplosionEffect();
                }
                // Then allow the projectile to be killed.
                BounceCount--;
                return true;
            }
           
        }

        // This method spawns a basic explosion effect.
        private void ExplosionEffect()
        {
            // Spawn dust particles for a visual explosion.
            for (int i = 0; i < 30; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
                Main.dust[dustIndex].velocity *= 2f;
            }

            float explosionRadius = 200f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && Vector2.Distance(npc.Center, Projectile.Center) < explosionRadius)
                {
                    npc.AddBuff(BuffID.Poisoned, 600); // Poison for 10 seconds.
                    // Adjusted StrikeNPC call to use the correct overload.
                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage=(int) Math.Pow(Projectile.damage,6),
                        Knockback = 0f,
                        HitDirection = 0,
                        Crit = false
                    };
                    npc.StrikeNPC(hitInfo);
                }
            }
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarExplode with { MaxInstances = 10, PitchVariance = 1.4f }, Projectile.position);
        }

        public override bool CanHitPlayer(Player target)
        {
            
            return base.CanHitPlayer(target);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float rot = 0f;

            if (IsOnSling)
            {
                rot = Projectile.velocity.ToRotation();
            }
            else
            {
                rot = Projectile.rotation+MathHelper.ToRadians(Math.Sign(Projectile.velocity.X) * (Time*14));
            }

            Main.spriteBatch.Draw(Apple, Projectile.Center - Main.screenPosition, null, Color.White, rot, Apple.Size() * 0.5f, new Vector2(0.5f, 0.5f), SpriteEffects.None, 0);
            
            return false;
        }
    }

    class GoodAppleNPC : GlobalNPC
    {
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        { 
            if (projectile.ModProjectile is GoodAppleProj goodAppleProj)
            {
                if (goodAppleProj.good)
                {
                    npc.lifeMax -= 1;
                }
                else
                {
                    // Increase the max health of the hit NPC but deal 13 damage.
                    npc.lifeMax += 1;
                    npc.life -= 13;
                }
            }
        }
    }
}
