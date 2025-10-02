using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.Automators;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.BlackKnife
{
    internal class KnifeSlash : ModProjectile, IDrawSubtractive
    {
        public int KnifeFrame
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        public ref float SlashType => ref Projectile.ai[2];

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults()
        {
        }

        private int Maxtime = 110;
        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(30);
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.timeLeft = Maxtime;
            Projectile.extraUpdates = 5;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled);
        }
        public override void AI()
        {
            if (Time == 0)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Vector2.Zero;

            }
            Projectile.Center = Main.player[Projectile.owner].Center;

            if (Time % (Maxtime / 7) == 0)
                KnifeFrame++;
            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float dist = 200f;
            Vector2 offset = new Vector2(dist * Projectile.scale * 1, 0).RotatedBy(Projectile.rotation);
            float _ = 0;
            if (KnifeFrame > 1 && KnifeFrame < 6)
                return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center - offset / 2, Projectile.Center + offset, 120f, ref _);
            else
                return false;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
           // hitbox.Inflate(100, 50);
           // hitbox.Location += new Vector2(100 * Projectile.scale, 0).RotatedBy(Projectile.rotation).ToPoint();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration = AddableFloat.Zero + 50;
            modifiers.DisableKnockback();

        }
        public override bool PreDraw(ref Color lightColor)
        {
            string Val = "HeavenlyArsenal/Content/Items/Weapons/BlackKnife/KnifeSwing";
            Texture2D tex = ModContent.Request<Texture2D>(Val + (SlashType + 1).ToString()).Value;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            Rectangle fram = tex.Frame(1, 7, 0, KnifeFrame);
            Vector2 Origin = new Vector2(0, fram.Height / 2);
            SpriteEffects sprit = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(tex, DrawPos, fram, Color.White, Projectile.rotation, Origin, 1, sprit);
            return false;
        }

        public void DrawSubtractive(SpriteBatch spriteBatch)
        {
          
            string Val = $"HeavenlyArsenal/Content/Items/Weapons/BlackKnife/KnifeSwing{SlashType+1}_Glow{SlashType+1}";
            Texture2D tex = ModContent.Request<Texture2D>(Val).Value;


            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle fram = tex.Frame(1, 7, 0, KnifeFrame);
            float offset = SlashType == 0 ?80 : 40 ;
            Vector2 Origin = new Vector2(offset, fram.Height / 2);
            SpriteEffects sprit = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            spriteBatch.Draw(tex, drawPosition, fram, Color.White, Projectile.rotation, Origin, Projectile.scale * 1.25f, sprit, 0f);

        }
    }
}
