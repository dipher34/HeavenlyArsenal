using HeavenlyArsenal.Common.utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    public class RitualBuffNPC : GlobalNPC
    {
        public static List<string> NameModifiers = new()
        {
            "",
            "Undying",
            "Furious",
            "Vigorful",
            "Regenerating",
            "Vampiric"
        };
        public enum RitualBuffType
        {
            None,
            Ressurection,
            Damagebuff,
            CooldownReduction,
            LifeRegen,
            Vampirism
        }
        public override bool InstancePerEntity => true;
        public bool WasRessurectedRecently = false;
        public bool hasRitualBuff = false;
        public int ritualBuffTier;
        public int ritualBuffTimer = 0;
        public int ritualBuffDuration = 60 * 17;
        public RitualBuffType BuffType;
        public NPC BuffGranter = null;

        public Rope BuffString;

        public bool IsRessurecting = false;
        public int Ressurectiontimer = 0;

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if(hasRitualBuff && BuffType == RitualBuffType.LifeRegen)
            {
                npc.lifeRegenCount += 13000;
            }
        }



        private void ManageRessurection(NPC npc)
        {
            Ressurectiontimer++;

            if (Ressurectiontimer > 120)
            {
                npc.life = npc.lifeMax;
                npc.immortal = false;
                ClearRitualBuff(npc);
                Ressurectiontimer = 0;
                IsRessurecting = false;
                WasRessurectedRecently = true;
                CombatText.NewText(npc.getRect(), CombatText.HealLife, npc.lifeMax, true);
            }
            


        }
      
        public override bool CheckDead(NPC npc)
        {
            if ((hasRitualBuff && BuffType == RitualBuffType.Ressurection) || IsRessurecting)
            {
                npc.life = 2;
                IsRessurecting = true;
                npc.immortal = true;
                return false;//prevent death because FUCK YOU
            }
            return base.CheckDead(npc);
        }

        public override bool PreAI(NPC npc)
        {
            if (npc.immortal && npc.life <= 2 && WasRessurectedRecently && !IsRessurecting)
            {
                Main.NewText(npc.FullName);

                npc.immortal = false;
            }
            if (BuffGranter == null || !BuffGranter.active || BuffGranter.life <= 0)
            {
                DestroyBuffString();
                hasRitualBuff = false;

            }

            if (IsRessurecting && !WasRessurectedRecently)
            {
                ManageRessurection(npc);
                return false;
            }

            ApplyEffects(npc);
          
                return base.PreAI(npc);
        }

        public void ApplyEffects(NPC npc)
        {
            if(BuffType == RitualBuffType.Damagebuff)
            {
                npc.damage = npc.defDamage * 3;
            }
            if (BuffType == RitualBuffType.CooldownReduction)
            {
                npc.velocity *= 2;
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            if (hasRitualBuff && BuffType == RitualBuffType.Vampirism)
                npc.life = Math.Clamp( npc.life + (int)(hurtInfo.Damage * 1.2f), 0, npc.lifeMax);
        }
        public override void PostAI(NPC npc)
        {
            if (hasRitualBuff)
            {
                ManageBuffstring(npc);
                if (ritualBuffTimer > 0)
                    ritualBuffTimer--;

                if (ritualBuffTimer <= 0)
                    hasRitualBuff = false;
            }
        }


        private void ManageBuffstring(NPC npc)
        {
            Vector2 StringStart = BuffGranter.Top;

            if (BuffString == null)
            {
                BuffString = new Rope(StringStart, npc.Center, 40, 10, Vector2.Zero);
            }

            for (int i = 1; i < BuffString.segments.Length - 1; i++)
            {
                if (Main.rand.NextBool(15) && i < BuffString.segments.Length - BuffString.segments.Length / 6)
                {
                    Dust blood = Dust.NewDustPerfect(BuffString.segments[i].position, DustID.CrimtaneWeapons, new Vector2(0, -3f), 10, Color.Crimson, 1);
                    blood.noGravity = true;
                    blood.rotation = Main.rand.NextFloat(-89, 89);

                }
                if (i == BuffString.segments.Length / 2)
                {
                    for (int u = -3; u < 6; u++)
                        BuffString.segments[i + u].position += new Vector2(0, -1f);
                }

            }
            BuffString.segments[0].position = npc.Center;
            BuffString.segments[^1].position = StringStart;
            BuffString.Update();
        }
        private void DestroyBuffString()
        {
            BuffString = null;
        }

        #region apply/remove buff
        public void ApplyRitualBuff()
        {

            int buffType = Main.rand.Next(1, 6);
            hasRitualBuff = true;
            BuffType = RitualBuffType.Ressurection;// (RitualBuffType)buffType;
            ritualBuffTimer = ritualBuffDuration;
            
        }

        public void ClearRitualBuff(NPC npc)
        {
            hasRitualBuff = false;
            BuffType = 0;
            BuffGranter = null;
            
            if (npc.immortal)
                npc.immortal = false;
        }
        #endregion


        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (BuffString != null && hasRitualBuff)
            {
                Texture2D a = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
                Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomLine2;
                float theMagicFactorThatMakesEveryElectricShineEffectSoMuchBetter = MathF.Sin(Main.GlobalTimeWrappedHourly*7 + npc.whoAmI);
                Color color = Color.Lerp(drawColor.MultiplyRGBA(Color.Red), Color.Red, theMagicFactorThatMakesEveryElectricShineEffectSoMuchBetter);

                List<Vector2> points = new List<Vector2>();
                points.AddRange(BuffString.GetPoints());
                points.Add(npc.Center);
                for (int i = 1; i < points.Count - 2; i++)
                {
                    Vector2 DrawPos = points[i] - Main.screenPosition;
                    float rot = points[i].AngleTo(points[i - 1]);

                    Vector2 element = points[i];
                    Vector2 diff = points[i + 1] - element;
                    Vector2 scale = new Vector2(diff.Length() + 2f, 2f);
                    Main.EntitySpriteDraw(a, DrawPos, null, color, rot, a.Size() / 2, scale, SpriteEffects.None);
                    Main.EntitySpriteDraw(Glow, DrawPos, null, color with { A = 0 }, rot + MathHelper.PiOver2, Glow.Size() / 2, new Vector2(scale.X*0.03f, scale.Y * 0.00905f), SpriteEffects.None);
                    //Utils.DrawBorderString(spriteBatch, i.ToString(), DrawPos, Color.AntiqueWhite);
                }
                /*
                string r = "null";
                if (BuffGranter != null)
                {
                    r = BuffGranter.FullName;
                }
                if (npc.type != ModContent.NPCType<RitualAltar>())
                {
                  
                      // Utils.DrawBorderString(spriteBatch, $"{hasRitualBuff}, Buffgranter: {r},\n Timer: {ritualBuffTimer},\n type:{BuffType.ToString()}", Vector2.UnitX * -60 + npc.Center - Main.screenPosition - Vector2.UnitY * -30, Color.Red);

                  
                }*/
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

    }
}
