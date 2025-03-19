using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Magic;

public class avatar_FishingRodVoid : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 2;
        Projectile.timeLeft = 240;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.manualDirectionChange = true;
        Projectile.minion = true;
    }

    public override void AI()
    {
        
    }

    public override bool? CanCutTiles() => false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}