using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Common.Graphics;
using Terraria.Map;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue
{
    class FlowerShuriken_Proj : ModProjectile
    {
        private float attachedRotationSpeed = 0f;
        private const float maxScale = 1f;
        private const float detectionRadius = 300f;
        private float lockOnRadius = 100f;
        private int gravityTimer = 120;
        private float visualRotation; // Visual rotation for drawing

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
            Projectile.aiStyle = -1;

            Projectile.scale = 0.2f;
            Main.projFrames[Projectile.type] = 4;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[0] = -1f;
            Projectile.ai[1] = 0f;
            Projectile.localAI[0] = 0f;
            Projectile.localAI[1] = 0f;
            Projectile.localAI[2] = 0f;
            visualRotation = 0f; // Initialize visual rotation
        }

        private void AttachToNPC(NPC target)
        {
            Projectile.localAI[0] = Projectile.Center.X - target.position.X;
            Projectile.localAI[1] = Projectile.Center.Y - target.position.Y;
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0] = target.whoAmI;
            Projectile.ai[1] = 0f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;

            attachedRotationSpeed = 0.3f; // Starting rotation speed
            Projectile.localAI[2] = 0f;

            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SliceTelegraph with { MaxInstances = 16, PitchVariance = 0.3f }, Projectile.Center).WithVolumeBoost(0.5f);
        }

        public override void AI()
        {
            lockOnRadius = Projectile.scale * 100f; // Update lock-on radius based on current scale
            visualRotation += 0.3f; // Update visual rotation regardless of attachment status

            if (Projectile.ai[0] < 0) // If not attached to an NPC
            {
                NPC closestNPC = null;
                float closestDistance = detectionRadius;

                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.immortal && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(Projectile.Center, npc.Center);
                        if (distance <= closestDistance)
                        {
                            closestNPC = npc;
                            closestDistance = distance;
                        }
                    }
                }

                if (closestNPC != null) // If a valid NPC is found
                {
                    Vector2 direction = Vector2.Normalize(closestNPC.Center - Projectile.Center) * 10f; // Increased speed factor
                    Projectile.velocity = (Projectile.velocity * 0.8f) + (direction * 0.2f); // Adjusted homing effect

                    if (closestDistance <= lockOnRadius) // Check if within lock-on range
                    {
                        AttachToNPC(closestNPC); // Attach to the NPC
                    }
                }
                else if (gravityTimer <= 0) // After timer, apply gravity if not attached
                {
                    Projectile.velocity.Y += 0.3f; // Apply gravity
                }
                else
                {
                    gravityTimer--; // Decrease gravity timer
                }
            }
            else // If attached to an NPC
            {
                int targetIndex = (int)Projectile.ai[0]; // Get the target NPC's index
                if (targetIndex < Main.npc.Length && Main.npc[targetIndex].active) // Check if the NPC is still active
                {
                    NPC target = Main.npc[targetIndex]; // Reference to the target NPC

                    if (target.immortal || target.dontTakeDamage) // If the NPC is immune, detach and ignore
                    {
                        Projectile.ai[0] = -1f; // Reset the attachment
                        return; // Exit the AI method
                    }

                    Projectile.Center = target.position + new Vector2(Projectile.localAI[0], Projectile.localAI[1]); // Stick to the NPC
                    attachedRotationSpeed = MathHelper.Lerp(attachedRotationSpeed, 0.93f, 0.1f); // Ease the rotation speed
                    Projectile.rotation += attachedRotationSpeed; // Update actual rotation

                    float sawIncrement = 1f;
                    if (Projectile.timeLeft < 100)
                        sawIncrement *= Projectile.timeLeft / 100f; // Slow down saw ticks when near death

                    Projectile.localAI[2] += sawIncrement; // Increase cumulative attached time for scale growth
                    Projectile.scale = 1f - 0.8f * (float)Math.Pow(0.5, Projectile.localAI[2] / 5f); // Exponential growth
                    if (Projectile.scale > maxScale)
                        Projectile.scale = maxScale; // Clamp the scale to the maximum

                    lockOnRadius = Projectile.scale * 100f; // Update lock-on radius based on current scale

                    Projectile.ai[1] += sawIncrement;
                    if (Projectile.ai[1] >= 5) // Apply damage every 5 ticks
                    {
                        int damage = Projectile.damage; // Get projectile damage
                        NPC.HitInfo hitInfo = new NPC.HitInfo
                        {
                            Damage = damage,
                            Knockback = 0f,
                            HitDirection = 0
                        };
                        target.StrikeNPC(hitInfo); // Deal damage to the target NPC
                        Projectile.ai[1] = 0f; // Reset saw timer
                        SwordSlash darkParticle = SwordSlash.pool.RequestParticle();
                        darkParticle.Prepare(
                            //position
                            new Vector2 (Projectile.localAI[0], Projectile.localAI[1] ),
                            //velocity
                            Projectile.velocity,//.ToRotation(),
                                                //rotaiton
                            Projectile.velocity.ToRotation() + MathHelper.PiOver2,// + Main.rand.NextFloat(-1f, 1f),
                                                                                  //lifetime
                            30,//Main.rand.Next(20, 40),
                               //color normal
                            Color.Crimson,//Color.DarkCyan * 0.5f,
                                          //color glow
                             Color.AntiqueWhite,//Color.Black * 0.33f,
                                                //scale
                            1f);


                        ParticleEngine.Particles.Add(darkParticle);
                        //target.AddBuff(BuffID.OnFire, 300); // Apply On Fire debuff for 5 seconds
                       // Dust.NewDust(Projectile.Center, 10, 10, DustID.FungiHit, 0f, 0f, 150, Color.Orange, 1.2f);
                        //SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SliceTelegraph with { MaxInstances = 16, PitchVariance = 0.3f }, Projectile.Center).WithVolumeBoost(0.5f);
                    }
                }
                else
                {
                    Projectile.Kill(); // Remove projectile if NPC disappears
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Projectile.ai[0] >= 0 ? false : base.Colliding(projHitbox, targetHitbox);
        }

        public override void Kill(int timeLeft)
        {
            if (Projectile.ai[0] >= 0) // If attached to an NPC
            {
                int targetIndex = (int)Projectile.ai[0]; // Get the target NPC's index
                if (targetIndex < Main.npc.Length && Main.npc[targetIndex].active) // Check if the NPC is still active
                {
                    Main.npc[targetIndex].AddBuff(BuffID.OnFire, 300); // Apply On Fire debuff for 5 seconds
                }
            }

            // Explosion dust effects on projectile death
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Adamantite);
            }
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FeatherDisappear with { MaxInstances = 16, PitchVariance = 0.3f }, Projectile.Center).WithVolumeBoost(0.5f);
            // Play explosion sound
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.AntiqueWhite;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value; 
            Vector2 drawPosition = Projectile.Center - Main.screenPosition; 
            Rectangle frame = texture.Frame(1, 1, 0, Projectile.frame);
            SpriteEffects spriteEffects = SpriteEffects.None; 
            Vector2 origin = new Vector2(frame.Width / 2, frame.Height / 2); 

            Main.EntitySpriteDraw(texture, drawPosition, frame, lightColor, visualRotation, origin, Projectile.scale/4, spriteEffects, 0);



            Texture2D SwordTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/swordslash").Value;
            SwordTexture.Frame();
            Rectangle Sframe = SwordTexture.Frame(1, 4, 0, 0);

            Vector2 Sorigin = new Vector2(Sframe.Width / 2, Sframe.Height / 2);
            int swordcount = 4;

            for (int i = 0; i < swordcount; i++)
            {
                Main.EntitySpriteDraw(SwordTexture, drawPosition, Sframe, lightColor, visualRotation+i*MathHelper.ToRadians(360/swordcount), Sorigin, Projectile.scale*1.31f, spriteEffects, 0);
            }
           
          

            
            return false; 
        }
    }
}
