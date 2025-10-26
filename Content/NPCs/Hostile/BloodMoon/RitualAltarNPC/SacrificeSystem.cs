using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.FleshlingCultist;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    partial class RitualAltar
    {
        //TODO: CREATE A BETTER SYSTEM THAT TRACKS ALL NPCS.
        //IF THERE IS AN NPC NEARBY, WE CAN CHECK IF ITS SACRIFICABLE, AND IF IT IS, 
        List<NPC> Sacrifices = new List<NPC>(Main.npc.Length);

     
        #region Sacrifice System
        void locateSacrifices()
        {

            foreach (NPC npc in Main.npc)
            {
                if (RitualSystem.BuffedNPCs.Contains(npc))
                    continue;
                if (BlackListProjectileNPCs.BlackListedNPCs.Contains(npc.type))
                    continue;

                if (npc.type == NPC.type)
                    continue;

                if (npc.immortal || npc.dontTakeDamage )
                    continue;

                if (!npc.active)
                    continue;
                if (npc.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
                    continue;
                
                if (npc.ModNPC == null)
                {
                    continue;
                }
                if (npc.ModNPC is not BloodmoonBaseNPC)
                    continue;
                if (npc.Distance(NPC.Center) > 700)
                    continue;

                if (npc.type == ModContent.NPCType<UmbralLeech>() || npc.type == ModContent.NPCType<Umbralarva>())
                    continue;

                if(npc.type == ModContent.NPCType<FleshlingCultist.FleshlingCultist>())
                {
                    var cult = CultistCoordinator.GetCultOfNPC(npc);
                    if (cult != CultistCoordinator.GetCultOfNPC(this.NPC))
                        continue;
;                }


                if (!Sacrifices.Contains(npc))
                {
                    Sacrifices.Add(npc);
                }
            }
        }
        void UpdateList()
        {
            //first: trim the list. if there's an inactive npc in the list, remove it from the list.
            //TODO: sort by BloodmoonBaseNPC BuffPrio (so that lower buff priority targets get selected to be sacrifices)
            // and by distance (so that the first index is always the closest npc)
            Sacrifices.RemoveAll(id => id == null || (!id.active));

            RitualSystem.BuffedNPCs.RemoveWhere(id => !id.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff);
            RitualSystem.BuffedNPCs.RemoveWhere(id => id.type == ModContent.NPCType<RitualAltar>());
            RitualSystem.BuffedNPCs.RemoveWhere(id => !id.active || id.life <= 0);
            //Main.NewText($"BuffedNPCs count: {RitualSystem.BuffedNPCs.Count}");

            //todo: Sort by SacrificePrio - 1 = top priority, 0 = pretty much ignored.
            Sacrifices.Sort((a, b) => Vector2.Distance(a.Center, NPC.Center).CompareTo(Vector2.Distance(b.Center, NPC.Center)));
            
            Sacrifices.Sort((a, b) =>
            {
                float aPrio = a.ModNPC is BloodmoonBaseNPC aBloodmoon ? aBloodmoon.SacrificePrio : 0f;
                float bPrio = b.ModNPC is BloodmoonBaseNPC bBloodmoon ? bBloodmoon.SacrificePrio : 0f;
                return bPrio.CompareTo(aPrio);
            });

        }
        void StateMachine()
        {
            switch (currentAIState)
            {
               
                case AltarAI.lookForBuffTargets:
                    BuffOtherEnemies();
                    if (blood < bloodBankMax)
                    {
                        currentAIState = AltarAI.LookingForSacrifice;
                    }
                    break;
                case AltarAI.Buffing:
                    BuffOtherEnemies();

                    if (blood < bloodBankMax / 5)
                    {
                        currentAIState = AltarAI.LookingForSacrifice;
                    }
                    break;

                case AltarAI.LookingForSacrifice:
                    if (blood < bloodBankMax / 1.25f)
                    {
                        SacrificeNPC();
                    }
                    else
                    {
                        currentAIState = AltarAI.lookForBuffTargets;
                    }
                    break;
                case AltarAI.Sacrificing:
                    SacrificeNPC();
                    if (blood >= bloodBankMax)
                    {
                        currentAIState = AltarAI.lookForBuffTargets;
                    }
                    break;

                case AltarAI.WalkTowardsPlayer:
                    WalkTowardsPlayer();
                    break;
            }
        }
        void BuffOtherEnemies()
        {
            List<NPC> nearbyNpcs = new List<NPC>();
            if (blood > bloodBankMax / 4)
            {
                foreach (NPC npc in Main.npc)
                {
                    if (npc.life <= 1)
                        continue;
                    if (BlackListProjectileNPCs.BlackListedNPCs.Contains(npc.type))
                        continue;
                    if (npc.type == NPC.type) //don't buff yourself or other ritual altars
                        continue;

                    if (npc.Distance(NPC.Center) > 300)
                        continue;

                    if (!npc.active)
                        continue;

                   // if (Sacrifices.Contains(npc))
                   //     continue;
                   if(!RitualSystem.BuffedNPCs.Contains(npc))

                    if (npc.type == ModContent.NPCType<UmbralLeech>())
                    {
                        UmbralLeech a = npc.ModNPC as UmbralLeech;
                        if (a != null)
                        {
                            if (a.HeadID != npc.whoAmI)
                                continue;
                        }
                    }


                    nearbyNpcs.Add(npc);

                }

            }

            if (nearbyNpcs.Count > 0)
            {

                // Sort by buffPrio if the npc is a BloodmoonBaseNPC
                nearbyNpcs.Sort((a, b) =>
                {
                    float aPrio = a.ModNPC is BloodmoonBaseNPC aBloodmoon ? aBloodmoon.buffPrio : 0f;
                    float bPrio = b.ModNPC is BloodmoonBaseNPC bBloodmoon ? bBloodmoon.buffPrio : 0f;
                    return bPrio.CompareTo(aPrio);
                });

                NPC.velocity.X = NPC.AngleTo(nearbyNpcs[0].Center).ToRotationVector2().X * SpeedMulti * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));


                NPC target = nearbyNpcs[0];
                blood -= bloodBankMax / 5;
                if (!RitualSystem.BuffedNPCs.Contains(target))
                {
                    RitualSystem.BuffedNPCs.Add(target);
                    
                    target.GetGlobalNPC<RitualBuffNPC>().BuffGranter = NPC;
                    target.GetGlobalNPC<RitualBuffNPC>().ApplyRitualBuff();
                    CombatText.NewText(target.Hitbox, Color.Red, "Buffed!", true);
                }
            }
            else

                WalkTowardsPlayer();


        }
        void SacrificeNPC()
        {
            if (Sacrifices.Count > 0)
                foreach (NPC npc in Sacrifices)
                {

                    if (RitualSystem.BuffedNPCs.Contains(npc))
                        continue;

                    if (Vector2.Distance(npc.Center, NPC.Center) > 100f)
                        NPC.velocity.X = NPC.AngleTo(npc.Center).ToRotationVector2().X * SpeedMulti * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));

                    if (Vector2.Distance(npc.Center, NPC.Center) < 100f && npc.active && !npc.friendly && !npc.boss)
                    {

                        SacrificeNPC a = npc.GetGlobalNPC<SacrificeNPC>();
                        if (!a.isSacrificed)
                        {
                            a.isSacrificed = true;
                            a.Priest = this;
                            isSacrificing = true;
                            SoundEngine.PlaySound(SoundID.Item3, NPC.position);
                            break;
                        }
                    }

                }
            else
            {
                currentAIState = AltarAI.WalkTowardsPlayer;
            }
        }
        void WalkTowardsPlayer()
        {
            NPC.velocity.X = NPC.AngleTo(Main.LocalPlayer.Center).ToRotationVector2().X * SpeedMulti * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));
            if (Sacrifices.Count > 0)
                currentAIState = AltarAI.LookingForSacrifice;
            
        }
        #endregion
    }
}
