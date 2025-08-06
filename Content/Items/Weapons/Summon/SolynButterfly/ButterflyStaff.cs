using HeavenlyArsenal.Content.Projectiles.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles.SolynProjectiles;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles.SolynProjectiles;
using NoxusBoss.Content.NPCs.Bosses.Draedon.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Assets.GennedAssets.Textures;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly
{
    [LegacyName("SolynWhip_Item")]
    public class ButterflyStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {

            Item.damage = 4000;

            Item.shootSpeed = 4;
            Item.rare = ItemRarityID.Master;
            Item.useTime = 30;
            Item.useStyle = 1;
            Item.useAnimation = 30;
            Item.channel = true;
            Item.noMelee = true;
            Item.mana = 60;
            
            Item.shoot = ModContent.ProjectileType<ButterflyMinion>();
        }
        public override bool CanShoot(Player player) => true;//player.ownedProjectileCounts[Item.shoot] < 0;
        public override void HoldItem(Player player)
        {
            
        }


    }
    public enum ButterflyMinionState
    {
        Shield,
        Laser
    }
    public class ButterflyMinionPlayer : ModPlayer
    {
        public ButterflyMinionState CurrentState;

        public bool IsMinionActive => Player.ownedProjectileCounts[ModContent.ProjectileType<ButterflyMinion>()] > 0;

        public Projectile Butterfly;

        public bool ButterflyBarrierActive = true;
        public int ButterflyBarrierCurrentHealth;
        public int ButterflyBarrierMaxHealth = 600;
        public int butterflyBarrierRegenRate = 10;
        public int butterflyBarrierTimeSinceLastHit = 0;
        
        public override void Initialize()
        {
            ButterflyBarrierActive = false;

        }
        public override void ResetEffects()
        {
            ButterflyBarrierActive = false;
            ButterflyBarrierCurrentHealth = 0;
        }

        public override void UpdateBadLifeRegen()
        {
            if (ButterflyBarrierActive)
            {
                butterflyBarrierTimeSinceLastHit++;
                if(CurrentState == ButterflyMinionState.Shield)
                {
                    
                }
            }
        }


        public override bool FreeDodge(Player.HurtInfo info)
        {
            if(ButterflyBarrierCurrentHealth > 0 && ButterflyBarrierActive)
            {
               
                ButterflyBarrierCurrentHealth -= info.Damage;
                butterflyBarrierTimeSinceLastHit = 0; // Reset the time since last hit
                CombatText.NewText(Player.Hitbox, Color.AliceBlue, info.Damage);
                if (ButterflyBarrierCurrentHealth <= 0)
                {
                    ButterflyBarrierActive = false; // Deactivate the barrier if health is depleted
                }
                return true; 
            }
            else
                return base.FreeDodge(info);
        }
    }

    public class ButterflyMinion : ModProjectile
    {
        //public override string Texture =>  "HeavenlyArsenal/Assets/Textures/Extra/BlackPixel";
        
        public enum ButterflyMinionState
        {
            Defend, Attack
        }
        public ButterflyMinionState AttackState = ButterflyMinionState.Defend;
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.damage = 0;
            Projectile.knockBack = 0;
            Projectile.Size = new Vector2(32, 32);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            

        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This allows the minion to target enemies\
            ProjectileID.Sets.MinionShot[Projectile.type] = true; // This allows the minion to shoot projectiles
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public ButterflyMinionState CurrentState;
        public Player Owner => Main.player[Projectile.owner];
        public NPC targetNPC;
        public Projectile Barrier;

        public ref float Time => ref Projectile.ai[0];
        public ref float AttackInterp => ref Projectile.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            Owner.GetModPlayer<ButterflyMinionPlayer>().Butterfly = Projectile;
        }
        public void UseStarFlyEffects()
        {
            // Release star particles.
            int starPoints = Main.rand.Next(3, 9);
            float starScaleInterpolant = Main.rand.NextFloat();
            int starLifetime = (int)float.Lerp(11f, 30f, starScaleInterpolant);
            float starScale = float.Lerp(0.2f, 0.4f, starScaleInterpolant) * Projectile.scale;
            Color starColor = Color.Lerp(new(1f, 0.41f, 0.51f), new(1f, 0.85f, 0.37f), Main.rand.NextFloat());

          

            Vector2 starSpawnPosition = Projectile.Center + new Vector2(Projectile.spriteDirection * 10f, 8f) + Main.rand.NextVector2Circular(16f, 16f);
            Vector2 starVelocity = Main.rand.NextVector2Circular(3f, 3f) + Projectile.velocity * (1f - GameSceneSlowdownSystem.SlowdownInterpolant);
            TwinkleParticle star = new TwinkleParticle(starSpawnPosition, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
            star.Spawn();

           
        }
        public Vector2 CenterWithOffset(float Multi = 1)
        {
            float Val = (float)Math.Sin(Main.GlobalTimeWrappedHourly)*Multi;
            Vector2 Offset = Projectile.Center + new Vector2(0, Val);

            return Offset;
        }
        public override void AI()
        {

            
            Projectile.timeLeft++;
            CurrentState = ButterflyMinionState.Attack;
            //Projectile.NewProjectileBetter(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SolynSentientStar>(),0, 0);
            UseStarFlyEffects();
            switch (CurrentState)
            {
                case ButterflyMinionState.Defend:
                    HandleDefend();
                    break;
                case ButterflyMinionState.Attack:
                    Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center - new Vector2(-40, 40), 0.4f);
                    HandleAttack();
                    break;
            }

        }
        private void FindTarget()
        {
            targetNPC = null;
            float maxDistance = 2000f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy() && Projectile.Distance(npc.Center) < maxDistance)
                {
                    targetNPC = npc;
                    maxDistance = Projectile.Distance(npc.Center);
                }
            }
        }
       
        
        private float orbitRadius = 40f;
        private float angleLerpSpeed = 0.1f;    // How fast the angle catches up
        private float stopThreshold = 0.01f;     // Velocity sqr mag threshold
        private float orbitAngle = -MathHelper.PiOver2; // Start “at top”
        private void HandleDefend()
        {

            if (Barrier == null || !Barrier.active || Barrier.type != ModContent.ProjectileType<SolynButterflyBarrier>())
            {
                int idx = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Owner.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<SolynButterflyBarrier>(),
                    0, 0,
                    Projectile.owner);
                Barrier = Main.projectile[idx];
            }

            // 2) compute desiredAngle

            float desiredAngle = MathHelper.Pi + MathHelper.ToRadians(5) * -Owner.velocity.X/4;

            Vector2 Offset = new Vector2(0,
               Barrier.height/2.7f
                ).RotatedBy(desiredAngle);
            Projectile.rotation = MathHelper.Pi+desiredAngle;
            //Main.NewText($"{MathHelper.ToDegrees(desiredAngle)}");
            
            Vector2 worldTarget = Barrier.Center + Offset;

            Projectile.spriteDirection = Math.Sign(Owner.Center.X - Projectile.Center.X);
            // 5) move the projectile there
            Projectile.Center = Vector2.Lerp(CenterWithOffset(10),worldTarget, 0.7f);
        }
        
        

        private void HandleAttack()
        {
            if (targetNPC == null || !targetNPC.active || targetNPC.life <= 0)
            {
                FindTarget();
            }
            else
            {
                if(AttackInterp < 0.5 && AttackInterp > 0.9)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Mars.SolynStarBeamChargeUp);
                }
                if(AttackInterp > 0.9)
                {
                    
                    if (Owner.ownedProjectileCounts[ModContent.ProjectileType<SolynButterflyBeam>()] <= 0)
                    {
                       
                        Vector2 ButterflytoTarget = Projectile.Center.AngleTo(targetNPC.Center).ToRotationVector2() * 100;
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, ButterflytoTarget, ModContent.ProjectileType<SolynButterflyBeam>(), 10000, Projectile.knockBack, Projectile.owner, 0f, 0f);

                    }
                   
                }
               
                AttackInterp = float.Lerp(AttackInterp, 1, 0.04f);
                
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            

            //float immunityPulse = 1f - Cos01(TwoPi * ImmunityFrameCounter / ImmunityFramesGrantedOnHit * 2f);
            Color baseColor = Color.Lerp(drawColor, Color.White, 0.2f);
            Color immunityColor = Color.Lerp(drawColor, new(255, 0, 50), 0.9f);
            Color color = Color.Lerp(baseColor, immunityColor, 0) * float.Lerp(1f, 0.3f, 0);
            return color * Projectile.Opacity ;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Butterfly = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/SolynButterfly/ButterflyMinion").Value;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;


            Vector2 Origin = new Vector2(Butterfly.Width / 2, Butterfly.Height);
            float Val = (float)Math.Sin(Main.GlobalTimeWrappedHourly);
            float Rot = Projectile.rotation - MathHelper.ToRadians(Val);
            DrawSpinningStar(Rot);
            SpriteEffects Flip = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            DrawAfterimages(Flip);
            Main.EntitySpriteDraw(Butterfly, DrawPos, null, Color.AntiqueWhite, Rot, Origin, Projectile.scale, Flip, 0);
           

            return false; // Prevent default drawing
        }
        private void DrawAfterimages(SpriteEffects effect)
        {
            Texture2D Butterfly = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/SolynButterfly/ButterflyMinion").Value;
            for (int i = 0; i< Projectile.oldPos.Length -1; i++)
            {
                float Val = (float)Math.Sin(Main.GlobalTimeWrappedHourly*1.1f);
                float Rot = Projectile.rotation - MathHelper.ToRadians(Val);

                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition;
                drawPos += Butterfly.Size() / 2.5f;
                float alpha = 1f - (i / (float)Projectile.oldPos.Length);
                Color color = new Color(255, 255, 255, (int)(alpha * 255));
                Vector2 Origin = new Vector2(Butterfly.Width / 2, Butterfly.Height);

                Main.EntitySpriteDraw(Butterfly, drawPos, null, color, Rot, Origin, Projectile.scale * alpha, effect, 0);
            }
        }
        private void DrawSpinningStar(float Rotation)
        {
            Texture2D StarTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/SolynButterfly/StarSpin").Value;
            int FrameCount = 13;

            //Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_Baby").Value;
            //Rectangle lech = new Rectangle(0, val * (texture.Height / FrameCount), texture.Width, texture.Height / FrameCount);

            //Vector2 origin = new Vector2(texture.Width / 2f, (texture.Height / FrameCount) / 2f);

            int val = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;

            Rectangle Frame = new Rectangle(0, val * (StarTexture.Height / FrameCount), StarTexture.Width, StarTexture.Height / FrameCount);

            Vector2 Origin = new Vector2(StarTexture.Width / 2, StarTexture.Height/11 / 2);

            float Rot = Rotation - MathHelper.PiOver2 * (1 - AttackInterp);
            Vector2 DrawPos = Projectile.Center - Main.screenPosition + new Vector2(5, (float)Math.Cos(Main.GlobalTimeWrappedHourly)).RotatedBy(Rot);

            Main.EntitySpriteDraw(StarTexture, DrawPos, Frame, Color.Cornsilk, Rot, Origin, 2 * (1+AttackInterp), SpriteEffects.None, 0);

        }
    }
}