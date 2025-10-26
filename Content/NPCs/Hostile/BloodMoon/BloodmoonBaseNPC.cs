using CalamityMod;
using CalamityMod.NPCs.NormalNPCs;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.FleshlingCultist;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss;
using NoxusBoss.Content.NPCs.Bosses.CeaselessVoid;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.Graphics.SwagRain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    public class BlackListProjectileNPCs : ModSystem
    {
        //blacklisted NPCs are to be ignored as potential targets.
        public static HashSet<int> BlackListedNPCs = new HashSet<int>();

        //todo: create a modsystem that does this for us, and then write it back to this npc upon loading the world or some shit
        
        public override void PostSetupContent()
        {
            //doubtless doesn't work, but you know what who gaf, i'm writing in github i can fix it later.
            //the whole point is to have something in place already to work off of.
            for (int i = 0; i < NPCLoader.NPCCount; i++)
            {
                
                if (NPCID.Sets.ProjectileNPC[i])
                {
                    BlackListedNPCs.Add(i);
                }
            }
            BlackListedNPCs.Add(ModContent.NPCType<Solyn>());
            BlackListedNPCs.Add(ModContent.NPCType<CeaselessVoidRift>());

            BlackListedNPCs.Add(ModContent.NPCType<SuperDummyNPC>());
        }
    }

    public class BloodmoonSpawnControl : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            // Only affect surface, night, Blood Moon
            if (Main.bloodMoon && !Main.dayTime && spawnInfo.Player.ZoneOverworldHeight && RiftEclipseBloodMoonRainSystem.EffectActive)
            {
                // Clear all vanilla spawns
                pool.Clear();

                // Add your custom NPCs to the pool
                // The float value is the spawn weight relative to others in the pool
                pool[ModContent.NPCType<ArtilleryCrab>()] = SpawnCondition.OverworldNightMonster.Chance * 0.12f;
                pool[ModContent.NPCType<UmbralLeech>()] = SpawnCondition.OverworldNightMonster.Chance * 0.074f;
                pool[ModContent.NPCType<Umbralarva>()] = SpawnCondition.OverworldNightMonster.Chance * 0.14f;

                pool[ModContent.NPCType<RitualAltar>()] = SpawnCondition.OverworldNightMonster.Chance * 0.03f;
                pool[ModContent.NPCType<FleshlingCultist.FleshlingCultist>()] = SpawnCondition.OverworldNightMonster.Chance * 0.22f;
               
            }
        }
            
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (Main.bloodMoon && !Main.dayTime && player.ZoneOverworldHeight)
            {
                // spawnRate is how often spawns happen (lower = more frequent)
                // maxSpawns is how many can exist near the player at once
                spawnRate = 20; // vanilla ~600; lowering makes spawns faster
                maxSpawns = 30; 
            }
        }
    }
    public abstract class BloodmoonBaseNPC : ModNPC
    {
        public override void ModifyTypeName(ref string typeName)
        {
            if (NPC.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
            {
                int val = (int)NPC.GetGlobalNPC<RitualBuffNPC>().BuffType;
                string Modifier = $"{RitualBuffNPC.NameModifiers[val]}";
                typeName = $"{Modifier} {typeName}";
            }
        }
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override string LocalizationCategory => "NPCs";

        public virtual bool ResistantToTrueMelee => true;

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {

            if(modifiers.DamageType == ModContent.GetInstance<TrueMeleeDamageClass>() && ResistantToTrueMelee)
            {
                modifiers.CritDamage *= 0.75f;
                modifiers.FinalDamage *= 0.55f;
            }

        }
        

        ///<summary>
        /// the current blood in this npc.
        ///</summary>
        public virtual int blood { get; set; } = 0;
        ///<summary>
        /// the total cap of blood this npc can hold.
        ///</summary>
        public virtual int bloodBankMax { get; set; } = 100;


        //todo: a target NPC, a target Player (maybe use entity? and just exclude projectiles)
        public Player playerTarget = null;

        public NPC NPCTarget = null;

        public Entity currentTarget = null;
        
        #region Snackrifice:tm:
        /// <summary>
        /// How likely this npc is to recieve a buff compared to it's neighbors when another npc is sacrificed.
        /// </summary>
        public virtual float buffPrio => 0;

        /// <summary>
        /// Determine whether this npc can be sacrificed.
        /// this is a virtual because I feel like the ability to be sacrificed should be adjustable.
        /// </summary>
        public virtual bool canBeSacrificed
        {
            get; 
            set; 
        } = true;

        /// <summary>
        /// Basically the priority of an npc to be sacrificed. if this is zero, they basically will almost never be sacrificed.
        /// </summary>
        public virtual int SacrificePrio
        {
            get => !canBeSacrificed ? 0 : default;
            set => canBeSacrificed = value > 0;
        }
        ///<summary>
        /// calculate the value of the sacrificed npc. we'll later multiply this depending on their value, but im coding blind at the time of writing this, so that should best be left for later.
        ///</summary>
        public virtual float calculateSacrificeValue(NPC npc)
        {
            float bloodPercent = blood / bloodBankMax;
            float lifePercent = npc.life / (float)npc.lifeMax;
            
            float value = Utils.Clamp(bloodPercent + lifePercent, 0, 1);
            return value;
        }
        #endregion


        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //Utils.DrawBorderString(spriteBatch, $"{blood}/{bloodBankMax}%, {blood}/{bloodBankMax}", NPC.Center - screenPos + new Vector2(0, -40), Color.Red);

            /*
            string Msg = "";
            var a = NPC.GetGlobalNPC<RitualBuffNPC>();
            Msg += $"{a.BuffType.ToString()}\n";
            Msg += $"{a.ritualBuffTimer}\n";
            if (NPC.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
            {
                Utils.DrawBorderString(spriteBatch, Msg, NPC.Center - screenPos, Color.AntiqueWhite, anchory: -2);
            }
            */
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    
    }
}
