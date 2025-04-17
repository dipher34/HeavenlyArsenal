using CalamityMod;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Enums;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Rogue;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Projectiles.Misc;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using System.Security.Principal;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Vambrace;

public class ElectricVambraceDash : PlayerDashEffect
{
    public static new string ID => "ElectricVambraceDash";

    public override DashCollisionType CollisionType => DashCollisionType.NoCollision;

    public override bool IsOmnidirectional => false;
    public int Time = 0;
    public bool AngleSwap = true;

    public override float CalculateDashSpeed(Player player) => 30.4f;

    public override void OnDashEffects(Player player)
    {
        Time = 0;
        SoundEngine.PlaySound(GennedAssets.Sounds.Mars.LightFlickerOn with { PitchVariance = 0.45f, MaxInstances = 0, }, player.Center, null);
        
        
        for (int i = 0; i < Main.rand.Next(1, 5); i++)
        {
            Vector2 lightningPos = player.Center + Main.rand.NextVector2Circular(24, 24);

            HeatLightning particle = HeatLightning.pool.RequestParticle();
            particle.Prepare(lightningPos, player.velocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), 10 + i * 3, Main.rand.NextFloat(0.5f, 1f));
            ParticleEngine.Particles.Add(particle);
        }

        //Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<ShintoArmorDash_Hand>(), 40, 0, -1, 0, 0, 0);  
    }

    public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
    {
        for (int i = 0; i < Main.rand.Next(1, 5); i++)
        {
            Vector2 lightningPos = player.Center + Main.rand.NextVector2Circular(24, 24);

            HeatLightning particle = HeatLightning.pool.RequestParticle();
            particle.Prepare(lightningPos, player.velocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), 10 + i * 3, Main.rand.NextFloat(0.5f, 1f));
            ParticleEngine.Particles.Add(particle);
        }
        Time++;
        dashSpeed = 19f;
    }
}
