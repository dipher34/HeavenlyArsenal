
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.Nadir2;
public class AvatarSpear_Holdout : ModProjectile
{
    public ref Player player => ref Main.player[Projectile.owner];

    public bool IsEmpowered = false;
    public int AvatarSpear_EmpoweredPercent;


    //for reference, antishadow is visually just black with a red outline, so not super difficult. its similar to the cloth that avatar has on it. 
    public enum NormalAttackState
    {
        TransitionToNormal, //transititory states, if unnecessary, remove

        RapidStabs,
        LungeStab, //i made the decision to move the lunge to normalattackstate because i feel like it would combo better if the player could use the lunge stab as a mobility tool to gapclose before awakening the spear and doing bigger damage
        //some intermediate attack im unsure
        slash, //like a swing, not like top to bottom, but as if the player was swinging it across their body, so like ctharsis whip from catalyst empowered form where it goes from whip to kind of normal slash. i dont know how to explain this easily tbh.
        //if you have better ideas, go for it
        //also maybe like one more attack, but i cant think of anything right now beucase head empty i love my brain implosion juice, ten thousand grams of pure caffine
        TransitionToEmpowered //transititory states, if unnecessary, remove
    }
    public enum EmpoweredAttackState
    {
        TransitionToEmpowered, //transititory states, if unnecessary, remove

        RapidStabs, //enemies on hit are ravaged by antishadow
        HeavyThrust, //draw back thrust forward powerfully with a lot of force, cracking space as the spearhead passes through space
        RipOut, // if heavy thrust landed successfully, rip the spear out and cause a huge burst of antishadow to leak out, adding to the avatarSpear empowerment meter so that you can keep it going
        CastigateEnemiesOfTheGodHead, //somehting about attacking and causing more rips in space. duplicates of the spear appear from each crack and attack a target. this will not happen if there's nothing to attack, and they will prioritize the last enemy hit by the spear. if that enemy is not found, but there are still enemies (such as when you're using it on a horde of enemies), then strands of antishadow will lash out at nearby enemies instead.

        TransitionToNormal //transititory states, if unnecessary, remove
    }

    public enum SpecialAttackState
    {
        // stuff for left click + right click 
        Default, //maybe you could throw the spear, and then teleport to it and do an aoe? idk, and thinking on it, sounds a lot like that spear from calamity overhaul
        Empowered
    }
    private AvatarSpear_Holdout CurrentState;

    public float rotationDampening = 0.1f;

    public string StringTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Lantern_String";
    public string LanternTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Lantern";
    public string SpearTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Holdout";
    public string EmpoweredTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Holdout_Empowered";

    public Vector2 gravityVector = new Vector2(0f, 1.2f);

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.friendly = true;
        Projectile.hostile = false;
    }

    public override void AI()
    {
        Projectile.timeLeft = 2;
        UpdateProjectileHeldVariables(player.Center);
        ManipulatePlayerVariables(player);
        //probably shit implementation but i dont care, its to help you understand 
        HandleRightClick();
        if (!IsEmpowered)
        {
            //why didnt i make the handle cases? SHUT UP MY WRSITS HURT
        }
        else if (IsEmpowered)
        {
            //i know it could just be an else but i dont care 
            if (AvatarSpear_EmpoweredPercent > 0)
            {
                //forcibly transition the spear from empowered to submissive:tm:
            }
            else //handle cases
            {

            }
        }
    }

    public void UpdateProjectileHeldVariables(Vector2 armPosition)
    {
        // if you can do this better than i can please do so
    }

    public void ManipulatePlayerVariables(Player player)
    {
        // Arm stuff
    }

    public void HandleRightClick()
    {
        if (Main.mouseRight)
        {
            if (AvatarSpear_EmpoweredPercent > 10)
            {
                if (!IsEmpowered)
                {
                    // Empower the spear
                    IsEmpowered = true;
                    //handle empower transition
                }
                else
                {
                    //Cow(subdue) the spear
                    IsEmpowered = false;
                    //handle transition back to normal
                }
            }
         
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //Add to percentage. probably add a check for if the next hit should restore the spear to normal/empowered state
        base.OnHitNPC(target, hit, damageDone);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        //i know its ugly so i killed it sorry :pensive:
        return true;
    }

    public override void OnKill(int timeLeft)
    {
       //maybe put away sound
    }
}
