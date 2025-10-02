using CalamityMod;
using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;
using HeavenlyArsenal.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common.Players
{
    internal class DebugPlayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player Owner = drawInfo.drawPlayer;



            string msg = "";
            msg += $"Empowered Attack Count: {Owner.GetModPlayer<VoidCrestOathPlayer>().InterceptCount}\n";
            /* if (Owner.HeldItem != null)
                 msg = $"Held Item: {Owner.HeldItem.Name} \n" +
                 $"Use Time: {Owner.HeldItem.useTime} \n" +
                 $"Use Animation: {Owner.HeldItem.useAnimation} \n" +
                 $"Use Style: {Owner.HeldItem.useStyle} \n" +
                 $"Attack Speed: {Owner.GetWeaponAttackSpeed(Owner.HeldItem)} \n" +
                 $"Empowered Attack Count: {Owner.GetModPlayer<VoidCrestOathPlayer>().InterceptCount}\n" +

                 $"{Owner.GetModPlayer<VoidCrestOathPlayer>().MaxInterceptCount} \n";
             else
                 msg = "No Held Item";

           */
            for (int i = 0; i < Owner.GetModPlayer<VoidCrestOathPlayer>().trackedProjectileIndices.Count; i++)
            {
                int projIndex = Owner.GetModPlayer<VoidCrestOathPlayer>().trackedProjectileIndices[i];
                if (Main.projectile[projIndex] != null && Main.projectile[projIndex].active)
                {
                    msg += $"Proj {i}: Name: {Main.projectile[projIndex].Name} WhoamI: {Main.projectile[projIndex].whoAmI},Damage:{Main.projectile[projIndex].damage}\n";
                }
                else
                {
                    msg += $"Proj {i}: NULL\n";
                }
            }
            // msg = $"Cooldown: {Owner.GetModPlayer<VoidCrestOathPlayer>().Cooldown}\n"+ $"InterceptCount: {Owner.GetModPlayer<VoidCrestOathPlayer>().InterceptCount}";




            msg = $"modStealth: {Owner.Calamity().modStealth} \n"
                + $"rogueStealth: {Owner.Calamity().rogueStealth}\n"
                + $"{Owner.Calamity().accStealthGenBoost}";
            Utils.DrawBorderString(Main.spriteBatch, msg, Owner.Center - Main.screenPosition, Color.AntiqueWhite, 1, 0.2f, -0.2f);
        }
    }
    public class DebugProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override void PostDraw(Projectile projectile, Color lightColor)
        {
           // if (!projectile.isAPreviewDummy && projectile.type != ModContent.ProjectileType<VoidCrest_Spear>())
            //    Utils.DrawBorderString(Main.spriteBatch, $"{projectile.whoAmI}", projectile.Center - Main.screenPosition, Color.AntiqueWhite, 1, 0.2f, -0.2f);
        }
    }
}
