using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight
{
    public class BlackGlassMass : ModProjectile
    {
        // registry of active masses (only indices)
        public static List<int> ActiveMasses = new List<int>();


        public Color[] GlowColor;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];

        public int MaxMass = 550;
        public int TotalMass
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public override string GlowTexture => "HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow";

        public override void OnSpawn(IEntitySource source)
        {
            GlowColor = new Color[100];
            for (int i = 0; i < 100; i++)
            {
                GlowColor[i] = RainbowColorGenerator.TrailColorFunction(i / 100f);
            }

            if (!ActiveMasses.Contains(Projectile.whoAmI))
                ActiveMasses.Add(Projectile.whoAmI);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.Size = new Vector2(10, 10);
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 0;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            overWiresUI.Add(index);

        }

        public override void AI()
        {
            // merge into other masses nearby
            float mergeRadius = 32f;
            float mergeRadiusSq = mergeRadius * mergeRadius;

            for (int i = ActiveMasses.Count - 1; i >= 0; i--)
            {
                int id = ActiveMasses[i];
                if (id == Projectile.whoAmI) continue;
                if (id < 0 || id >= Main.maxProjectiles)
                {
                    ActiveMasses.RemoveAt(i);
                    continue;
                }
                Projectile other = Main.projectile[id];
                if (!other.active)
                {
                    ActiveMasses.RemoveAt(i);
                    continue;
                }

                if (other.owner != Projectile.owner) 
                    continue;

               
                float dsq = Vector2.DistanceSquared(Projectile.Center, other.Center);
                if (dsq <= mergeRadiusSq)
                {
                    var otherMass = other.ModProjectile as BlackGlassMass;
                    if (otherMass == null) continue;

                    // absorb as much as we can without exceeding MaxMass
                    int transferable = Math.Min(otherMass.TotalMass, MaxMass - TotalMass);
                    if (transferable <= 0) continue;

                    TotalMass += transferable;
                    otherMass.TotalMass -= transferable;

                    if (otherMass.TotalMass <= 0)
                        other.Kill();
                    // if we've reached MaxMass, stop absorbing
                    if (TotalMass >= MaxMass)
                        break;
                }
            }


            float scale = TotalMass/(float)MaxMass;
            Projectile.scale = scale;
        }




        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Base = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D Glow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow").Value;
            Texture2D Glow2 = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 Origin = Base.Size() * 0.5f;
            Vector2 Grigin = Glow.Size() * 0.5f;
            float Rot = Projectile.rotation + MathHelper.PiOver2;

            Vector2 Scale = new Vector2(4) * Projectile.scale;
            Vector2 GlowScale = new Vector2(4f) * Projectile.scale;

            SpriteEffects flip = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;



            float GlowMulti = float.Lerp(0, 1f, (float)Math.Clamp(Time / 20, 0, 1));


           
//            Main.EntitySpriteDraw(Glow2, DrawPos, null, GlowColor[0] * 0.2f, Rot, Glow2.Size() * 0.5f, GlowScale, flip);


//            Main.EntitySpriteDraw(Base, DrawPos, null, Color.AntiqueWhite, Rot, Origin, Scale, flip);

           // Utils.DrawBorderString(Main.spriteBatch, TotalMass.ToString(), DrawPos, Color.AntiqueWhite);
            return false;
        }


    }
}
