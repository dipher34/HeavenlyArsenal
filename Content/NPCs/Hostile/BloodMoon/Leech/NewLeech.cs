using System;
using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Items.Accessories;
using HeavenlyArsenal.Common.utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.World.GameScenes.AvatarAppearances;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    public enum UmbralLeechAI
    {
        Idle,
        SeekTarget,
        FeedOnTarget,
        FlyAway,
        DeathAnim
    }

    public class NewLeech : ModNPC
    {
       
        #region StolenCode
        /*
                                                 .:-:                             
                                               .--.--                             
                                              :=-:.==                             
                                     .:::.. .:-:-=:=-                             
                                     .:.::::....-=:::                             
                                     .:-::::::. =-.:-.                            
                                       ::.      ..:.:=-.                          
                                        .-:.      ..:..=:                         
                                          .. .:-+=::=+=.                          
                                           .-::-=-:..                             
                                          .:-:.                                   
                                     .:::.:+=:::.                                 
                              .:==++++-::-=+-::-+++==-:                           
                            .:=+=----::.....      ..:===:.                        
                        .::=++==+**+==:....           .:=+-::.                    
                        :-====**=:.:::-:. .:.   .:::..   .:=-.                    
                      .-+=-=++-... .:   :+##*-.   .. ...    :=:.                  
                     .-+---..:....::   -%%=.=#+:..    ...    .=:.                 
                     :+--........:. .-#%##+. :*%+::--:...     .-:                 
                    .--:...:..:....-*#%%%%*:  .-*#+:..:--:     :..                
                    .==. .:.::..:=##*#%%%#+:    .-+*+-:.:=:    :-.                
                   .-=-  .::--+#%%%@@%#%%*=:       .-+**+*=.   :=-.               
                  .::=-..:+%@####%%%##%##**+:         .-+%%-   =-::.              
                    .-*:.:#@@##%%%%**+++++*-.            *%:..-+-.                
                     :%%@#*%@%%%#%#*+==++**+-    .....  .*%=#@%#-                 
                      -@%#%@@%%%@@%@@@@@%%+:--=@@@@@@@+..#@=-+%-                  
                      .*#%#@%%%**@@@@@@@%*=:...*@@@@@+. :#%=*%+...                
                    ....+%%#%@%**#%%##*-=*=.::..:---:.  -#+*#=..                  
                        :-+@@@%#**###*-:+#=.    .  .... =%%+..                    
                        .-=#@@######**+#%#=.       .:. .*@*-.                     
                        .:=*%@%%##%%###%%#=..:.   ...  :#*==:.                    
                        .--##@@%#**%%%%%%@@%+:.   .:. :#@+==-.                    
                         .-*#@@@###@%%%%####=:....:: -%@%+*=.                     
                        .-:****%@%#%%%%#+:..:-:  .. =#*=+++-::.                   
                        ...-**+-+@@@##%###*+=:.  .=%@+.=+=:...                    
                           .-*++*%#@@@%#%%#-   :*%#=*=.--:.                       
                            -#+:+@##%@@@%#*++*#%#=..**:--..                       
                         .:=*@@%%@%%%%%#*+=+==:.   .*%#%%#=:.                     
        */
        #endregion
        #region setup
        /// <summary>
        /// A general‐purpose timer. Used by segments to wobble and by death animation.
        /// </summary>
        public ref float Time => ref NPC.ai[0];

        public ref float WiggleTime => ref NPC.localAI[0];
        /// <summary>
        /// Reference into NPC.ai[0]: stores the NPC ID of the head segment.
        /// All non‐head segments use this to know which NPC is the head.
        /// </summary>
        public ref float HeadID => ref NPC.ai[1];

        /// <summary>
        /// For child segments: the index (1, 2, 3, …) of this segment in the chain.
        /// For the head, SegmentNum is 0.
        /// </summary>
        public ref float SegmentNum => ref NPC.ai[2];

        /// <summary>
        /// Total number of segments (not counting head). Stored in NPC.localAI[0] for head,
        /// and copied to each spawned segment’s localAI[0] so that everyone knows the count.
        /// </summary>
        public int SegmentCount;

        /// <summary>
        /// Current AI state (Idle, Seek, Feed, etc.)
        /// </summary>
        private UmbralLeechAI CurrentState;

        /// <summary>
        /// The player or other entity we’re currently “feeding on” or chasing.
        /// </summary>
        private Entity currentTarget;

        /// <summary>
        /// Keeps track of each segment’s “ideal” position in world‐coordinates,
        /// so that body/tail segments smoothly interpolate/follow the segment in front.
        /// We fill this list in AI() each tick.
        /// </summary>
        private List<Vector2> SegmentPositions;

        private List<NPC> Segments;
        /// <summary>
        /// Only the head will fill this every tick.
        /// headPositions[0] = head.Center,
        /// headPositions[i] = “ideal” world position for the i-th segment (1..SegmentCount).
        /// </summary>
        public Vector2[] headPositions;


        // This boolean toggles “feeding mode” so bodies know to lash harder.
        private bool isFeeding = false;

        public static readonly SoundStyle Bash = new SoundStyle(
            "HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Bash_", 3
        );

        public static readonly SoundStyle Explode = new SoundStyle(
            "HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Head_Crush_3"
        );

        public static readonly SoundStyle GibletDrop = new SoundStyle(
            "HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Giblet_Drop_3"
        );

       
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech";

        public override void SetStaticDefaults()
        {
            // Prevent this NPC from dropping coins or souls, and from despawning to inactivity.
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.CannotDropSouls[Type] = true;
            NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 30;
            NPC.lifeMax = 50000;
            NPC.damage = 200;
            NPC.defense = 330;
            NPC.npcSlots = 0f;   // Doesn’t occupy standard NPC slots so you can spawn many segments.
            NPC.noGravity = true;
            NPC.aiStyle = -1;    // We use custom AI only.
            CurrentState = UmbralLeechAI.Idle;
            NPC.knockBackResist = 0;
            // Initialize our segment‐position list to empty. We will size it at runtime.
            SegmentPositions = new List<Vector2>();
        }

        public override void Load()
        {
            ribbonTexture = AssetDirectory.Textures.UmbralLeechTendril;
        }
        #endregion
        public float feedtime = 0;
        /// <summary>
        /// When the head (SegmentNum == 0) spawns, choose a random segment count,
        /// store that in localAI[0], then spawn each segment with AI parameters:
        /// - ai[0] = head NPC ID
        /// - ai[1] = this segment’s index (1..count)
        /// Also copy SegmentCount into each child’s localAI[0].
        public override void OnSpawn(IEntitySource source)
        {
            // Ensure that each segment spawned shares the same segment count, for placement of things later on.
            int count;
            if (SegmentNum == 0)
            {
                // Head segment: choose random segment count and store in localAI[0]
                count = Main.rand.Next(6, 16);
                SegmentCount = count;
                NPC.localAI[0] = count;

                HeadID = NPC.whoAmI;

                // Only the head is running OnSpawn with SegmentNum == 0, so it can allocate:
                headPositions = new Vector2[count + 1]; 

                // Immediately set up our SegmentPositions list to size (count + 1),
                // where index 0 is head’s position, and 1..count are segment positions.
                SegmentPositions = new List<Vector2>(new Vector2[count + 1]);
                Segments = new List<NPC>(new NPC[count + 1]);
                // For each index i = 1..count, spawn a body segment.
                for (int i = 1; i <= count; i++)
                {
                    int newNPC = NPC.NewNPC(
                        NPC.GetSource_FromAI(),
                        (int)SegmentPositions[i].X,
                        (int)SegmentPositions[i].Y,
                        Type,
                        ai0: 0,        // Time (will be set by segment AI)
                        ai1: HeadID,   // HeadID
                        ai2: i         // SegmentNum
                    );

                    if (newNPC >= 0 && newNPC < Main.maxNPCs)
                    {
                        // Set the segment count in the spawned segment's localAI[0]
                        Main.npc[newNPC].localAI[0] = count;
                        Segments.Add(Main.npc[newNPC]);
                    }
                }
            }
            else
            {
                
                SegmentCount = (int)NPC.localAI[0];
                if (SegmentCount == 0 && HeadID >= 0 && HeadID < Main.maxNPCs)
                {
                    NPC head = Main.npc[(int)HeadID];
                    if (head.active && head.type == Type)
                        SegmentCount = (int)head.localAI[0];
                }
            }
        }

        
        private Vector2[] ribbonPoints;
        private Vector2[] ribbonVels;

        public void RibbonPhysics()
        {
            int length = SegmentCount;
            if (ribbonVels != null)
            {
                for (int i = 0; i < ribbonVels.Length; i++)
                {
                    ribbonVels[i] = (NPC.rotation  * NPC.spriteDirection).ToRotationVector2() * MathHelper.Pi;
                }
            }
            else
            {
                ribbonVels = new Vector2[length];
            }

            if (ribbonPoints != null)
            {
                float drawScale = NPC.scale;
                // Incorporate wiggle timer so while it's near 0 or 80, flare outwards, but draw inwards as it approaches 40.
                // We'll use a sine curve to modulate the flare amount based on WiggleTime.
                float maxCycle = 80f;
                float t = WiggleTime % maxCycle;
                // Flare factor: 1 at t=0 or t=80, 0 at t=40
                float flare = (float)Math.Sin(Math.PI * t / maxCycle);
                float flareAmount = MathHelper.Lerp(0.8f, 1.3f, Math.Abs(flare)); // 0.8 (inward) to 1.3 (outward)

                ribbonPoints[0] = NPC.Center + new Vector2(4, -5 * NPC.spriteDirection).RotatedBy(NPC.rotation) * drawScale * flareAmount;

                for (int i = 1; i < ribbonPoints.Length; i++)
                {
                    ribbonPoints[i] += ribbonVels[i];
                    if (ribbonPoints[i].Distance(ribbonPoints[i - 1]) > 10)
                    {
                        ribbonPoints[i] = Vector2.Lerp(
                            ribbonPoints[i],
                            ribbonPoints[i - 1] + new Vector2(10 * flareAmount, 0).RotatedBy(ribbonPoints[i - 1].AngleTo(ribbonPoints[i])),
                            0.8f
                        );
                    }
                }
            }
            else
            {
                ribbonPoints = new Vector2[length];
                for (int i = 0; i < ribbonPoints.Length; i++)
                {
                    ribbonPoints[i] = NPC.Center;
                }
            }
        }
        #region AI
        private const float PlayerDetectionRange = 4000f;

        public override void AI()
        {
            if(ribbonPoints != null)
                RibbonPhysics();
            if (currentTarget != null)
            {
                NPC.direction = Math.Sign(NPC.Center.X - currentTarget.Center.X);
            }
            //wiggle wiggle

            if (SegmentNum == 0f)
            {
                int total = (int)SegmentCount;
                if (headPositions == null || headPositions.Length != total + 1)
                    headPositions = new Vector2[total + 1];

                
                headPositions[0] = NPC.Center;

                // Compute head’s base velocity direction
                Vector2 headVelDir = Vector2.Zero;
                if (NPC.velocity.LengthSquared() > 0.001f)
                    headVelDir = Vector2.Normalize(NPC.velocity);
                NPC.rotation = NPC.velocity.ToRotation();

                
                // If we just entered FeedOnTarget, reset Time so sine wave restarts
                if (CurrentState == UmbralLeechAI.FeedOnTarget && !isFeeding)
                {
                    
                    isFeeding = true;
                }
                else if (CurrentState != UmbralLeechAI.FeedOnTarget && isFeeding)
                {
                    // We’ve left feeding state → reset flag
                    isFeeding = false;
                }

                

                // Keep Time within a cycle—say a max of 40 ticks in feed, 80 in normal
                float maxCycle = isFeeding ? 60f : 80f;
                if (WiggleTime > maxCycle)
                    WiggleTime -= maxCycle;

                // Now build the chain of positions:
                for (int i = 1; i <= total; i++)
                {
                    Vector2 basePoint = headPositions[i - 1] - headVelDir * 40f;
                    Vector2 perp = headVelDir.RotatedBy(MathHelper.PiOver2);
                    
                    float phaseShift = i * (isFeeding ? 2.5f : 2f);
                    float sineValue;

                    if (isFeeding){sineValue = (float)Math.Sin(Math.PI * (WiggleTime + phaseShift) /1f); headPositions[i] = basePoint + perp * (sineValue * 18f); }
                    else {sineValue = (float)Math.Sin(Math.PI * (WiggleTime + phaseShift) / 10f); headPositions[i] = basePoint + perp * (sineValue * 18f);}
                }
            }

            WiggleTime++;
            //body
            if (SegmentNum >= 1f)
            {
                
                int headIndex = (int)HeadID;
                NPC.direction = Main.npc[headIndex].direction;
                if (headIndex < 0 || headIndex >= Main.maxNPCs || !Main.npc[headIndex].active)
                {
                    NPC.active = false;
                    return;
                }
                NewLeech head = (NewLeech)Main.npc[headIndex].ModNPC;
                int idx = (int)SegmentNum;
                int total = (int)head.SegmentCount;

                NPC precedingNPC = null;
                // The preceding segment is the one with SegmentNum == (this.SegmentNum - 1) and same HeadID
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC candidate = Main.npc[i];
                    if (candidate.active && candidate.type == Type && candidate.whoAmI != NPC.whoAmI)
                    {
                        if (candidate.ai[1] == HeadID && candidate.ai[2] == SegmentNum - 1)
                        {
                            precedingNPC = candidate;
                            break;
                        }
                    }
                }

                if (precedingNPC == null)
                {
                    NPC.active = false;
                    return;
                }

                if (head.headPositions == null || head.headPositions.Length != total + 1)
                {
                    // If something’s missing, just fall back to a simple snap
                    Vector2 fallbackDir = Main.npc[headIndex].Center - NPC.Center;
                    if (fallbackDir.Length() > 6f)
                        NPC.velocity = Vector2.Normalize(fallbackDir) * 109f;
                    return;
                }

                
                Vector2 toMe = NPC.Center - precedingNPC.Center;
                Vector2 dirAway;
                if (toMe.LengthSquared() > 0.001f)
                    dirAway = Vector2.Normalize(toMe);
                else
                    dirAway = Vector2.UnitY;

                Vector2 basePoint = precedingNPC.Center + dirAway * 21f;

                //shiggy with it time
                Vector2 prevVel = precedingNPC.velocity;
                Vector2 prevDir = prevVel.LengthSquared() > 0.005f ? Vector2.Normalize(prevVel) : Vector2.UnitX;
                Vector2 perp = prevDir.RotatedBy(MathHelper.PiOver2);
                float phaseShift = SegmentNum * (isFeeding ? 2.5f : 2.5f);
                float sineVal = (float)Math.Sin(Math.PI * (WiggleTime + phaseShift) / (isFeeding ? 5f : 10f));

                // Amplitude tapers at the start and end of the segment chain
                float baseAmplitude = isFeeding ? 5f : (SegmentCount)/5;
                float t = (float)idx / (float)total;
                // Use a smoothstep curve for tapering: 0 at ends, 1 in the middle
                float taper = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((float)Math.Sin(Math.PI * t), 0f, 1f));
                float amplitude = baseAmplitude * taper;

                Vector2 wiggleOffset = perp * (sineVal * amplitude);

                // 4) Final desired position
                Vector2 desired = basePoint + wiggleOffset;

                // 5) Lerp toward desired
                float lerpFactor = isFeeding ? 0.35f : 1f;
                NPC.Center = Vector2.Lerp(NPC.Center, desired, lerpFactor);

                // 6) Rotate to face preceding NPC
                NPC.rotation = (precedingNPC.Center - NPC.Center).ToRotation();

                // 7) Sync life
                NPC.life = Main.npc[headIndex].life;

               
                return;
            }
          
            //Actual AI
            if (SegmentNum == 0f)
            {
                if (NPC.life > 0)
                {
                    
                    
                    switch (CurrentState)
                    {
                        case UmbralLeechAI.Idle:
                            // Look for a player
                            Player p = FindClosestPlayer(PlayerDetectionRange);
                            if (p != null)
                            {
                                currentTarget = p;
                                NPC.target = p.whoAmI;
                                CurrentState = UmbralLeechAI.SeekTarget;
                            }
                            break;

                        case UmbralLeechAI.SeekTarget:
                            if (currentTarget != null && currentTarget.active)
                            {
                                Vector2 toTarget = currentTarget.Center - NPC.Center;
                                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Normalize(toTarget) * (8f + (float)SegmentCount/12), 0.05f);

                                if (Vector2.Distance(NPC.Center, currentTarget.Center) < 40f)
                                {
                                    CurrentState = UmbralLeechAI.FeedOnTarget;
                                    Time = 0f;       // reset Time right when feeding starts
                                    feedtime = 0;
                                }
                            }
                            else
                            {
                                CurrentState = UmbralLeechAI.Idle;
                            }
                            break;

                        case UmbralLeechAI.FeedOnTarget:
                            
                            if (currentTarget != null && currentTarget.active)
                            {
                                feedtime++;
                                // (1) Snap head to player center + small jitter
                                Vector2 toPlayer = currentTarget.Center - NPC.Center;
                                NPC.Center = Vector2.Lerp(NPC.Center, currentTarget.Center, 0.5f);

                                // (2) Shake the head itself: small perpendicular wiggle
                                Vector2 dir = Vector2.Normalize(toPlayer);
                                Vector2 perp = dir.RotatedBy(MathHelper.PiOver2);
                                float shake = (float)Math.Sin(Math.PI * Time / 4f) * 4f;
                                NPC.Center += perp * shake;

                                // After a short feed duration, transition to FlyAway
                                if (feedtime > 60f)
                                {
                                    Time = 0f;
                                    CurrentState = UmbralLeechAI.FlyAway;
                                }
                            }
                            else
                            {
                                CurrentState = UmbralLeechAI.Idle;
                            }
                            break;

                        case UmbralLeechAI.FlyAway:
                            if (Time == 0f)
                            {
                                NPC.velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 12f;
                            }
                            if (Time > 90f)
                            {
                                Time = 0f;
                                CurrentState = UmbralLeechAI.Idle;
                                feedtime = 0;
                            }
                            
                            break;

                        case UmbralLeechAI.DeathAnim:
                            DoDeathAnimation();
                            break;
                    }
                }
                else
                {
                    CurrentState = UmbralLeechAI.DeathAnim;
                    DoDeathAnimation();
                }
            }
            if (SegmentNum < 1f)
            {
                //todo: if the head is dying, the segemnts are also dying.
                if (Main.npc[(int)HeadID].life <= 0)
                {
                    CurrentState = UmbralLeechAI.DeathAnim;
                    DoDeathAnimation();
                }
            }
            Time++;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon)
                return SpawnCondition.OverworldNightMonster.Chance * 0.1f;
            return 0f;
            
        }


        /// <summary>
        /// Return the closest active, alive player within maxDist, or null if none.
        /// </summary>
        private Player FindClosestPlayer(float maxDist)
        {
            Player best = null;
            float bestDist = maxDist;
            foreach (var p in Main.player)
            {
                if (!p.active || p.dead)
                    continue;
                float d = Vector2.Distance(p.Center, NPC.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = p;
                }
            }
            return best;
        }


        /// <summary>
        /// Prevent the NPC from fully dying immediately. Instead, set life = 1, switch to DeathAnim,
        /// and keep it active so that DoDeathAnimation can play out for all segments.
        /// </summary>
        public override bool CheckDead()
        {
            if (SegmentNum == 0f)
            {
                // Head should go into DeathAnim.
                NPC.life = 1;
                CurrentState = UmbralLeechAI.DeathAnim;
                NPC.active = true;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;

                
                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;

                return false; // Return false so tModLoader does not actually remove the NPC yet.
            }
            else
            {
                // Segments themselves never “die”—they mirror head’s life (set in AI).
                return false;
            }
        }

        private float DeathAnimationTimer;

        /// <summary>
        /// Plays a multi‐segment death sequence. Slows itself down, shakes, then finally explodes into gore.
        /// </summary>
        private void DoDeathAnimation()
        {
            // 1) Slow all segments by a factor of 0.1.
            for (int i = 0; i < SegmentCount; i++)
            {
                NPC.velocity *= 0.1f;
                SegmentPositions[i] += new Vector2();
            }
            for(int i = 0; i< SegmentPositions.Count; i++)
            {
                SegmentPositions[i] += new Vector2(Main.rand.NextFloat(-10,10),Main.rand.NextFloat(-10,10));
            }

            if (DeathAnimationTimer % 5 == 0 && Main.netMode != NetmodeID.Server)
            {
                Player local = Main.LocalPlayer;
                if (local.WithinRange(NPC.Center, 4800f))
                    SoundEngine.PlaySound(Bash with { Volume = 1.65f });
            }

            if (DeathAnimationTimer == 92f)
            {
            }

            if (Main.netMode == NetmodeID.Server && DeathAnimationTimer % 45f == 44f)
            {
                NPC.netUpdate = true;
                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;
            }

            if (DeathAnimationTimer >= 125f)
            {
                // Only the head needs to call HitEffect() and NPCLoot(); segments will vanish automatically.
                NPC.active = false;
                NPC.HitEffect();
                NPC.NPCLoot();
                NPC.netUpdate = true;

                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;
            }

            DeathAnimationTimer++;
        }

        #endregion
   
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (SegmentNum <= 1f)
            {
                // Near head: apply small knockback.
                NPC.velocity -= NPC.DirectionTo(item.Center) * item.knockBack * 0.2f;
            }
            else
            {
                // Body/tail: redirect damage to head.
                int headIndex = (int)HeadID;
                if (headIndex >= 0 && headIndex < Main.maxNPCs)
                {
                    Main.npc[headIndex].life -= damageDone;
                    // Make sure our health matches the head’s after redirecting.
                    NPC.life = Main.npc[headIndex].life;
                }
            }
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (SegmentNum <= 1f)
            {
                NPC.velocity -= NPC.SafeDirectionTo(projectile.Center, Vector2.Zero) * projectile.knockBack * 0.2f;//NPC.DirectionTo(projectile.Center) * projectile.knockBack * 0.2f;
            }
            else
            {
                int headIndex = (int)HeadID;
                if (headIndex >= 0 && headIndex < Main.maxNPCs)
                {
                    Main.npc[headIndex].life -= damageDone;
                    NPC.life = Main.npc[headIndex].life;
                }
            }
        }

        #region drawcode
        public static Asset<Texture2D> ribbonTexture;
        private void DrawRibbon(Color lightColor, Vector2 Offset)
        {
            if (ribbonPoints != null && ribbonPoints.Length > 1)
            {
                // Anchor the first point to a point on the NPC determined by the offset.
                // This ensures the ribbon starts at a specific spot relative to the NPC.
                ribbonPoints[0] = NPC.Center + Offset.RotatedBy(NPC.rotation);

                for (int i = 0; i < ribbonPoints.Length - 1; i++)
                {
                    int style = 0;
                    if (i == ribbonPoints.Length - 3)
                    {
                        style = 1;
                    }
                    if (i > ribbonPoints.Length - 3)
                    {
                        style = 2;
                    }

                    Rectangle frame = ribbonTexture.Value.Frame(1, 3, 0, style);
                    float rotation = ribbonPoints[i].AngleTo(ribbonPoints[i + 1]);
                    Vector2 stretch = new Vector2(0.25f + Utils.GetLerpValue(0, ribbonPoints.Length, i, true),
                        ribbonPoints[i].Distance(ribbonPoints[i + 1]) / (frame.Height - 5)
                    );
                    Main.EntitySpriteDraw(
                        ribbonTexture.Value,
                        ribbonPoints[i] - Main.screenPosition + Offset,
                        frame,
                        lightColor.MultiplyRGBA(Color.Lerp(Color.DimGray, Color.White, (float)i / ribbonPoints.Length)),
                        rotation - MathHelper.PiOver2,
                        frame.Size() * new Vector2(0.5f, 0f),
                        stretch,
                        0,
                        0
                    );
                }
            }
        }
        private void DrawWhisker()
        {

        }

        /// <summary>
        /// Draw the leech’s body (head/body/tail) using a 3×8 sprite sheet.
        /// We pick the correct frame based on ai[1] (SegmentNum) and animation timer.
        /// </summary>
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Load our 3 (horizontal) × 8 (vertical) sprite sheet. Each row is a different segment‐type frame.
            Texture2D texture = 
                ModContent.Request<Texture2D>(
                "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/NewLeech"
            ).Value;

            //Texture2D outline = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech2").Value;
            SpriteEffects Leech = NPC.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            int row = (SegmentNum < 1f) ? 2 : 1;
           

            if (!NPC.IsABestiaryIconDummy & SegmentNum == 0)
            {
                Utils.DrawBorderString(Main.spriteBatch, "|FeedTime: " + feedtime.ToString()+"|", NPC.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| " + CurrentState +"| Time: "+ Time.ToString() + " | wiggletime: " + WiggleTime.ToString(), NPC.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);
            }
            if (SegmentNum % 2 == 0)
            {
                //Utils.DrawBorderString(Main.spriteBatch, "| Direction : " + NPC.direction, NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
                //Utils.DrawBorderString(Main.spriteBatch, "| DeathTimer: " + DeathAnimationTimer.ToString(), NPC.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);


            }
           
            if (SegmentNum == SegmentCount)
            {
                row = 0;
                // Draw tendrils at the tail segment
                if (ribbonPoints == null || ribbonPoints.Length == 0)
                {
                    // Initialize ribbon points if not already done
                    int length = SegmentCount;
                    ribbonPoints = new Vector2[length];
                    for (int i = 0; i < length; i++)
                        ribbonPoints[i] = NPC.Center;
                }
                //Utils.DrawBorderString(Main.spriteBatch, "|Test " , NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
                DrawRibbon(drawColor, new Vector2(0, -1*NPC.direction*-15));
                DrawRibbon(drawColor, new Vector2(0, -1*NPC.direction*5));
            }
            else if (SegmentNum == SegmentCount -1)
            {
                row = 1;
            }
            else if(SegmentNum == 0)
            {
                row = 4;
            }
                int animation = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;
            Rectangle sourceRect = texture.Frame(5, 1, row, 0);

            
            Vector2 origin = sourceRect.Size() / 2f;
          
            // Draw with rotation and chosen frame. Lighting.GetColor for proper lighting.
            Main.EntitySpriteDraw(
                texture,
                NPC.Center - Main.screenPosition,
                sourceRect,
                Lighting.GetColor(NPC.Center.ToTileCoordinates()),
                NPC.rotation,
                origin,
                1f,
                Leech
            );

            return false; // Skip default draw
        }

        #endregion
    }
}
