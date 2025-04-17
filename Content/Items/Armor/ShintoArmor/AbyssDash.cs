using System;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Enums;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.GodSlayer;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Particles;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Particle = CalamityMod.Particles.Particle;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Common.Graphics;

namespace HeavenlyArsenal.Content.Items.Armor;

public class AbyssDash : PlayerDashEffect
{
    public static new string ID => "AbyssDash";

    //public static int AbyssDashCooldown = 45;

    public SlotId AbyssDashSlot;

    //public static readonly SoundStyle Impact = new("CalamityMod/Sounds/NPCKilled/DevourerDeathImpact") { Volume = 0.5f };

    public override DashCollisionType CollisionType => DashCollisionType.ShieldSlam;

    public override bool IsOmnidirectional => true;

    public int Time = 0;
    public float Size = 2.2f;
    public bool SoundOnce = true;

    public override float CalculateDashSpeed(Player player) => 80f;

    public override void OnDashEffects(Player player)
    {
        Time = 0;
        Size = 2.2f;
        AbyssDashSlot = SoundEngine.PlaySound(ShintoArmorBreastplate.AbyssDash_Start, player.Center, null);
        SoundOnce = true;

        CalamityMod.Particles.Particle pulse = new DirectionalPulseRing(player.Center, Vector2.Zero, Color.Orchid, new Vector2(2f, 2f), Main.rand.NextFloat(12f, 25f), 0.1f, 12f, 18);
        GeneralParticleHandler.SpawnParticle(pulse);

        for (int i = 0; i <= 15; i++)
        {
            Dust dust = Dust.NewDustPerfect(player.position, 30, -player.velocity.RotatedByRandom(MathHelper.ToRadians(35f)) * Main.rand.NextFloat(0.3f, 0.9f), 0, default, Main.rand.NextFloat(3.1f, 3.9f));
            dust.noGravity = false;
        }
    }

    public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
    {
        if (SoundEngine.TryGetActiveSound(AbyssDashSlot, out var Dashsound) && Dashsound.IsPlaying)
            Dashsound.Position = player.Center;

       
        Size -= 0.04f;
        drawDash(player);
        // Fall way, way, faster than usual. 
        player.maxFallSpeed = 50f;
        
        if (Time == 0)
        {
            Rift darkParticle = Rift.pool.RequestParticle();
            darkParticle.Prepare(player.Center- player.velocity,player.velocity, Color.AntiqueWhite, new Vector2 (0,0),player.fullRotation, 3, 3, 300);


            ParticleEngine.Particles.Add(darkParticle);
            //darkParticle.Update();
        }
        
        if (Time > 20 && Time < 100)
        {
            Particle pulse = new DirectionalPulseRing(player.Center - player.velocity * 0.52f, player.velocity / 1.5f, Color.Fuchsia, new Vector2(1f, 2f), player.velocity.ToRotation(), 0.82f, 0.32f, 60);
            GeneralParticleHandler.SpawnParticle(pulse);
            Particle pulse2 = new DirectionalPulseRing(player.Center - player.velocity * 0.40f, player.velocity / 1.5f * 0.9f, Color.Aqua, new Vector2(0.8f, 1.5f), player.velocity.ToRotation(), 0.58f, 0.28f, 50);
            GeneralParticleHandler.SpawnParticle(pulse2);
            Time = 111;
        }
        if(Time > 111)
        {
            Rift.pool.RequestParticle();
        }

        Time++;
        // Dash at a much, much faster speed than the default value.
        dashSpeed = 10f;
        runSpeedDecelerationFactor = 0.8f;

        // Cooldown for God Slayer Armor dash.
        player.AddCooldown(AbyssDashCooldown.ID, CalamityUtils.SecondsToFrames(ShintoArmorBreastplate.AbyssDash_Cooldown));
        player.Calamity().godSlayerDashHotKeyPressed = false;
    }
    public void drawDash(Player player)
    {
        
    }
    public override void OnHitEffects(Player player, NPC npc, IEntitySource source, ref DashHitContext hitContext)
    {
        if (SoundOnce)
        {
            SoundEngine.PlaySound(ShintoArmorBreastplate.AbyssDash_Start, player.Center);
            SoundOnce = false;
        }
        /*
        for (int i = 0; i <= 25; i++)
        {
            Dust dust = Dust.NewDustPerfect(player.position, Main.rand.NextBool(3) ? 226 : 272, player.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.1f, 0.5f), 0, default, Main.rand.NextFloat(2.1f, 2.9f));
            dust.noGravity = false;
        }
        */
        // Define hit context variables.
        int hitDirection = player.direction;
        if (player.velocity.X != 0f)
            hitDirection = Math.Sign(player.velocity.X);
        hitContext.HitDirection = hitDirection;
        hitContext.PlayerImmunityFrames = AsgardianAegis.ShieldSlamIFrames;

        // Define damage parameters.
        int dashDamage = 3000;
        hitContext.damageClass = player.GetBestClass();
        hitContext.BaseDamage = player.ApplyArmorAccDamageBonusesTo(dashDamage);
        hitContext.BaseKnockback = 15f;

        // God Slayer Dash intentionally does not use the vanilla function for collision attack iframes.
        // This is because its immunity is meant to be completely consistent and not subject to vanilla anticheese.
        hitContext.PlayerImmunityFrames = ShintoArmorBreastplate.AbyssDash_Iframes;

        npc.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 300);
    }
}