using System;
using System.Collections.Generic;
using System.Linq;
using HeavenlyArsenal.Content.Projectiles;
using Microsoft.Xna.Framework;
using static Luminance.Luminance;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath
{
    public class VoidCrestOathPlayer : ModPlayer
    {
        /*
        public List<T> Blacklist
        {
            get;
            set;
        }
        */
        /// <summary>
        /// True if the accessory is actively equipped in an accessory slot.
        /// (Not in a vanity slot.)
        /// </summary>
        public bool voidCrestOathEquipped;

        /// <summary>
        /// If true, the accessory is in a vanity slot and only provides cosmetic effects.
        /// </summary>
        public bool NotVanity;

        /// <summary>
        /// True if the conflicting accessory WarBannerOftheSun is equipped.
        /// </summary>
        public bool warBannerOftheSunEquipped;

        /// <summary>
        /// The resource used to "pay" for intercepts.
        /// It will decrease when an interception happens and regenerate slowly.
        /// </summary>
        public float InterceptCount;

        /// <summary>
        /// Maximum resource value for intercepting hostile projectiles.
        /// </summary>
        public float MaxInterceptCount = 100f;

        /// <summary>
        /// How much resource is consumed per intercept.
        /// </summary>
        public float InterceptCost = 1f;

        /// <summary>
        /// How much resource is regenerated per tick if no intercept is occurring.
        /// </summary>
        public float InterceptRegenRate = 10f;

        /// <summary>
        /// A list to keep track of hostile projectile indices that we are watching.
        /// We rebuild this every tick.
        /// </summary>
        public List<int> trackedProjectileIndices = new List<int>();

        /// <summary>
        /// Detection radius (in pixels) for adding enemy projectiles to the tracking list.
        /// </summary>
        private float TrackingRadius = 650f;

        /// <summary>
        /// Distance at which an intercept is triggered (in pixels).
        /// </summary>
        private float InterceptDistance = 300f;

        /// <summary>
        /// Interceptor projectile type. 
        /// </summary>
        private int interceptorType => ModContent.ProjectileType<VoidCrestInterceptorProjectile>();

        /// <summary>
        /// The current Projectile to be intercepted
        /// </summary>
        public int TobeDestroyed
        {
            //todo: output the proj so that i can handle the destroy logic in the interceptor because i hate everyone
            get;
            set;
        }
        public override void ResetEffects()
        {
            
            voidCrestOathEquipped = false;
           
            warBannerOftheSunEquipped = false;
        }

        public override void PostUpdate()
        { 

            trackedProjectileIndices.Clear();
            // If the conflicting accessory is equipped, skip all interception logic.
            if (warBannerOftheSunEquipped|| !voidCrestOathEquipped || NotVanity)
                return;
            

            // Rebuild the tracking list each tick.
            
            //shit implementation i know

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                // Only consider projectiles that:
                //  • Are hostile (enemy projectiles)
                //  • Are not friendly (not shot by player)
                //  • Were not spawned by the player (owner check)
                Projectile proj = Main.projectile[i];
                if (proj.active&& proj.hostile && proj.owner != Player.whoAmI )
                    continue;
                float distance = Vector2.Distance(proj.Center, Player.Center);

                if (proj.hostile && proj.type != interceptorType && !proj.friendly && distance <= TrackingRadius) //&& proj.owner != Player.whoAmI)//&& proj.owner != Player.whoAmI)
                {
                    trackedProjectileIndices.Add(i);
                }
            }

            if (Main.myPlayer == Player.whoAmI)
            {
               
            }

           // Main.NewText($"{trackedProjectileIndices}");
            //SHUT UP
            bool interceptedSomethingThisTick = false;

            //sort the tracked projectiles by distance to the player
            trackedProjectileIndices.Sort((a, b) => Vector2.Distance(Main.projectile[a].Center, Player.Center).CompareTo(Vector2.Distance(Main.projectile[b].Center, Player.Center)));  
            


            foreach (int index in trackedProjectileIndices.ToList())
            { 

                Projectile proj = Main.projectile[index];
                if (proj != null && proj.owner != Player.whoAmI && proj.type != interceptorType)
                    continue;

                float distance = Vector2.Distance(proj.Center, Player.Center);

                //Main.NewText($"tracking:{proj.whoAmI}, count:{trackedProjectileIndices.Count}");
                if (Main.myPlayer == Player.whoAmI)
                {
                    try
                    {

                        //CombatText.NewText(Player.Hitbox, Color.Cyan, $"Tracking: {proj.Name} ({proj.type}) [{distance:F0}px]");
                       //Main.NewText($"Hostile projectiles in range: {trackedProjectileIndices.Count}", Color.Yellow);
                       
                    }
                    catch
                    {
                        // Fails silently to prevent crash
                    }
                }
                //The method you're looking for is Remove I think
                //(But also you can't use that in a foreach since that modifies the state of the iteration in the middle of the loop) 


                if (distance <= InterceptDistance && proj.type != interceptorType && !proj.friendly && proj.active)
                {
                    if (InterceptCount >= InterceptCost)
                    {
                        if (Main.myPlayer == Player.whoAmI)
                        {
                            Main.NewText($"{interceptorType} Intercepted {proj.Name}!", Color.Red);
                        }

                        Projectile.NewProjectile(
                            Player.GetSource_FromThis(),
                            proj.Center,
                            Vector2.Zero,
                            interceptorType,
                            -1,
                            1f,
                            Player.whoAmI
                        );
                        
                        CreateInterceptVisualEffect(proj.Center);
                        InterceptCount -= InterceptCost;
                        Main.NewText($"Projectile {proj} killed");
                        //todo: remove this projectile from the server 
                        proj.Kill();
                        interceptedSomethingThisTick = true;
                        // :derp:
                        
                    }
                    else
                    {
                        
                        if (Main.myPlayer == Player.whoAmI)
                        {
                            Main.NewText("[VoidCrest] Not enough InterceptCount!", Color.OrangeRed);
                        }
                    }
                }
            }

            // If no intercept happened this tick, regenerate the intercept resource.
            if (!interceptedSomethingThisTick)
                RegenerateInterceptCount();
        }

        /// <summary>
        /// Regenerates InterceptCount until it reaches the maximum.
        /// </summary>
        private void RegenerateInterceptCount()
        {
            if (InterceptCount < MaxInterceptCount)
            {
                InterceptCount += InterceptRegenRate;
                if (InterceptCount > MaxInterceptCount)
                {
                    InterceptCount = MaxInterceptCount;
                }
            }
        }

        /// <summary>
        /// Creates a visual effect (dust, particles, etc.) at the specified position.
        /// 
        /// </summary>
        /// <param name="position">The world position for the visual effect.</param>
        private void CreateInterceptVisualEffect(Vector2 position)
        {
            
            // Example using Terraria dust
            for (int d = 0; d < 1; d++)
            {
                int dustIndex = Dust.NewDust(
                    position,
                    10, 10,
                    DustID.AncientLight,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    100,
                    default,
                    1.5f
                );
                Main.dust[dustIndex].noGravity = true;
            }
        }
    }
}
