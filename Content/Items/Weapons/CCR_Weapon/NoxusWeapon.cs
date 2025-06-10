using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.Projectiles;
using CalamityMod.Rarities;
using HeavenlyArsenal.common;
using HeavenlyArsenal.Common;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.Automators;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon.EntropicCrystal;
using Player = Terraria.Player;
using static Luminance.Common.Utilities.Utilities;
using Terraria.Audio;
using CalRemix.Core.Graphics;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon
{
    class NoxusWeapon : ModItem
    {
        
        public static int AltDamage = 4093;
        public static HeavenlyArsenalServerConfig Config => ModContent.GetInstance<HeavenlyArsenalServerConfig>();

        public override bool IsLoadingEnabled(Mod mod)
        {
            // Check config setting
            bool enabledInConfig = ModContent.GetInstance<HeavenlyArsenalServerConfig>().EnableSpecialItems;
            bool isOtherModLoaded = ModLoader.HasMod("CalRemix");

            return enabledInConfig || isOtherModLoaded;
        }



        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.DaedalusStormbow);
            Item.shoot = ModContent.ProjectileType<EntropicCrystal>();
            Item.DamageType = DamageClass.Ranged;
            Item.useAmmo = AmmoID.Arrow;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.damage = 4900;
            Item.rare = ModContent.RarityType<Violet>();
            Item.shootSpeed = 40;
            if (ModLoader.TryGetMod("CalRemix", out Mod CalamityRemix))
            {

                Item.DamageType = CalamityRemix.Find<DamageClass>("StormbowDamageClass");
            }
        }

        public override float UseSpeedMultiplier(Player player)
        {
            return player.GetModPlayer<NoxusWeaponPlayer>().CrystalSpeedMulti;
        }
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2) 
            {
                TriggleExplosion(player);
                return true; 
            }
            else

                return base.UseItem(player);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                return false;
            float arrowSpeed = Item.shootSpeed;
            Vector2 realPlayerPos = player.RotatedRelativePoint(player.MountedCenter, true);
            float mouseXDist = Main.mouseX + Main.screenPosition.X - realPlayerPos.X;
            float mouseYDist = Main.mouseY + Main.screenPosition.Y - realPlayerPos.Y;
            if (player.gravDir == -1f)
            {
                mouseYDist = Main.screenPosition.Y + Main.screenHeight - Main.mouseY - realPlayerPos.Y;
            }
            float mouseDistance = (float)Math.Sqrt((double)(mouseXDist * mouseXDist + mouseYDist * mouseYDist));
            if (float.IsNaN(mouseXDist) && float.IsNaN(mouseYDist) || mouseXDist == 0f && mouseYDist == 0f)
            {
                mouseXDist = player.direction;
                mouseYDist = 0f;
                mouseDistance = arrowSpeed;
            }
            else
            {
                mouseDistance = arrowSpeed / mouseDistance;
            }

            realPlayerPos = new Vector2(player.position.X + player.width * 0.5f + -(float)player.direction + (Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
            realPlayerPos.X = (realPlayerPos.X + player.Center.X) / 2f;
            realPlayerPos.Y -= 100f;
            mouseXDist = Main.mouseX + Main.screenPosition.X - realPlayerPos.X;
            mouseYDist = Main.mouseY + Main.screenPosition.Y - realPlayerPos.Y;
            if (mouseYDist < 0f)
            {
                mouseYDist *= -1f;
            }
            if (mouseYDist < 20f)
            {
                mouseYDist = 20f;
            }
            mouseDistance = (float)Math.Sqrt((double)(mouseXDist * mouseXDist + mouseYDist * mouseYDist));
            mouseDistance = arrowSpeed / mouseDistance;
            mouseXDist *= mouseDistance;
            mouseYDist *= mouseDistance;
            float speedX4 = mouseXDist;
            float speedY5 = mouseYDist;
            int shotArrow = Projectile.NewProjectile(source, realPlayerPos.X, realPlayerPos.Y, speedX4, speedY5, ModContent.ProjectileType<EntropicCrystal>(), damage, knockback, player.whoAmI);
            Main.projectile[shotArrow].noDropItem = true;
            Main.projectile[shotArrow].tileCollide = false;
            //CalamityGlobalProjectile cgp = Main.projectile[shotArrow].Calamity();
            //cgp.allProjectilesHome = true;
            return false;
        }


        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
        }
        public override bool AltFunctionUse(Player player){return true;}
       
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit += 1.5f*player.GetModPlayer<NoxusWeaponPlayer>().CrystalCount;
            base.ModifyWeaponCrit(player, ref crit);
        }

        /// <summary>
        /// detonates all entropic crystals owned by the player.
        /// 6/9/2025, 7:58 AM EST
        /// </summary>
        private void TriggleExplosion(Player player)
        {
            
            if (!player.dead)
            {
                for (int i = 0; i < player.ownedProjectileCounts[ModContent.ProjectileType<EntropicCrystal>()]; i++)
                    
                {
                    //todo: fix this code only triggering the first crystal it finds, instead of all of them.
                    var crystals = Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<EntropicCrystal>() && p.owner == player.whoAmI);
                    foreach (var crystal in crystals)
                    {
                        if (crystal != null && crystal.ai[1] == (float)EntropicCrystalState.PostHit)
                        {
                            crystal.ai[1] = (float)EntropicCrystalState.Exploding;
                        }
                    }
                   
                }
            }
        }

    }
    public class NoxusWeaponPlayer: ModPlayer
    {
        /// <summary>
        /// track the amount of crystals currently lodged in NPCs for the purposes of crit scaling and maybe some other fun things
        /// </summary>
        public int CrystalCount
        {
            get;
            set;
        }

        public int CrystalCap = 100;
        public float CrystalSpeedMulti = 1;
        public override void PostUpdateMiscEffects()
        { 
            var crystals = Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<EntropicCrystal>() && p.owner == Player.whoAmI);
            
            foreach (var crystal in crystals)
            {
                //if crystal exists, and crystal isn't falling or disipating:
                if(crystal != null && (crystal.ai[1] != (float)EntropicCrystalState.PreHit && crystal.ai[1] != (float)EntropicCrystalState.DisipateHarmlessly))
                {
                    CrystalCount++;
                }
            }
            if (CrystalCount > CrystalCap)
            {
                Projectile oldestCrystal = crystals.Last();
                if (oldestCrystal != null && oldestCrystal.ai[1] != (float)EntropicCrystalState.DisipateHarmlessly)
                {
                    oldestCrystal.ai[1] = (float)EntropicCrystalState.DisipateHarmlessly;
                    oldestCrystal.netUpdate = true;
                }
            }
            float CrystalSpeedInterpolant = 0;

            CrystalSpeedInterpolant = Math.Clamp((CrystalCount / 16),0,2);


            CrystalSpeedMulti = MathHelper.Lerp(CrystalSpeedMulti, 2f, CrystalSpeedInterpolant);

            //Main.NewText($"Crystal Count:{CrystalCount}, CrystalSpeedMulti: {CrystalSpeedMulti}, interp: {CrystalSpeedInterpolant}");
        }

        public override void ResetEffects()
        {
            CrystalCount = 0;
            CrystalSpeedMulti = 1;
        }
    }
    public class NoxusWeaponNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            
        }
    }
    class EntropicCrystal : ModProjectile
    {
        public Entity StuckNPC
        {
            get;
            set;
        }
        public enum EntropicCrystalState
        {
            PreHit,
            PostHit,
            Exploding,
            DisipateHarmlessly
        }
        public bool Toxic = false;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        //store in ai[1] to avoid bugs
        public EntropicCrystalState CurrentState => (EntropicCrystalState)Projectile.ai[1];
        //just for fun
        public ref float StoredNPC => ref Projectile.ai[2];
        public ref float Fram => ref Projectile.localAI[1];
        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/EntropicCrystal";
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            
        }
        public override void SetStaticDefaults()
        {
        }

        //public override void onSpawn(IEntitySource source)
        //{
        // Fram = main.rand.next(0,4+1);
        //}
        public override void AI()
        {
            Fram = 0;
            HandleState();

            Time++;

            
        }
        private void HandleState()
        {
            switch(CurrentState)
            {
                case EntropicCrystalState.PreHit:
                    HandlePreHit();
                    break;
                case EntropicCrystalState.PostHit:
                    HandlePostHit();
                    break;
                case EntropicCrystalState.Exploding:
                    HandlePostHit();
                    HandleExplosion();
                    break;
                case EntropicCrystalState.DisipateHarmlessly:
                    HandleDisipateHarmlessly();
                    break;
            }
        }
        /// <summary>
        /// this is the crystal before hitting anything. its just falling from the sky right now, and isn't doing all too much
        /// </summary>
        private void HandlePreHit()
        {
            //Projectile.velocity.Y += 0.1f;
            //Projectile.velocity.Y *= 1.01f;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            //todo: make the projectile home in on enemies to compensate for the smaller hitbox size.
        }
        /// <summary>
        /// this is the crystal after hitting something. it embeds itself in the target, and deals DOT while waiting for an explosion call.
        /// </summary>
        private void HandlePostHit() 
        {
            Projectile.timeLeft = 4;
            // Stick to the hit NPC
            Projectile.velocity = Vector2.Zero;
            if (Projectile.ai[2] == 0)
            {
                if(StuckNPC != null)
                    Projectile.ai[2] = StuckNPC.whoAmI + 1; 
                Projectile.netUpdate = true;
            }
            int npcIndex = (int)Projectile.ai[2] - 1;
            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC stuckNpc = Main.npc[npcIndex];
                if (stuckNpc.active && !stuckNpc.dontTakeDamage && !stuckNpc.friendly)
                {
                    Projectile.velocity = stuckNpc.velocity;
                }
                else
                {
                    // If NPC is dead or invalid, dissipate harmlessly
                    Projectile.ai[1] = (float)EntropicCrystal.EntropicCrystalState.DisipateHarmlessly;
                    Projectile.netUpdate = true;
                }
            }


            if (Toxic)
            {
                if(Time == 60*3 - 1)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                        voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));
                        HeavySmokeParticle darkGas = new(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextVector2Circular(1f, 1f), voidColor, 11, Projectile.scale * 1.24f, Projectile.Opacity * 0.6f, Main.rand.NextFloat(0.02f), true);
                        GeneralParticleHandler.SpawnParticle(darkGas);
                    }
                }
                if(Time > 60 * 3)
                {
                    Time = 0;
                    if (Main.rand.NextBool(1))
                    {
                        float gasSize = Utils.GetLerpValue(-3f, 25f, Time, true) * Projectile.width * 0.68f;
                        float angularOffset = -Projectile.rotation;
                        NoxusGasMetaball.CreateParticle(Projectile.Center + Projectile.velocity * 2f, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity.RotatedBy(angularOffset).RotatedByRandom(0.6f) * 0.26f, gasSize);
                    }
                    Main.npc[npcIndex].SimpleStrikeNPC(400, 0, false, 0, default, false, default, default);
                   
                }



            }
        }
        /// <summary>
        /// ended up splitting this into two projectiles. this just creates the portal.
        /// </summary>
        private void HandleExplosion()
        {
            int Crystals = 4*Owner.GetModPlayer<NoxusWeaponPlayer>().CrystalCount;
            Vector2 offset = Main.rand.NextVector2CircularEdge(200 + Crystals, 200 + Crystals);
            // Only create one portal per crystal
            if (!Projectile.localAI[0].Equals(1f))
            {
                Projectile.localAI[0] = 1f;
                int portal = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + offset,
                    Vector2.Zero,
                    ModContent.ProjectileType<EntropicBlast>(),
                    Projectile.damage,
                    0,
                    Projectile.owner
                );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void HandleDisipateHarmlessly()
        {
            for (int i = 0; i < 2; i++)
            {
                Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));
                HeavySmokeParticle darkGas = new(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextVector2Circular(1f, 1f), voidColor, 11, Projectile.scale * 1.24f, Projectile.Opacity * 0.6f, Main.rand.NextFloat(0.02f), true);
                GeneralParticleHandler.SpawnParticle(darkGas);
            }
            Projectile.alpha--;
            if (Projectile.alpha >= 0)
                Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled with { Pitch = 0.3f, PitchVariance = 0.4f });
            Projectile.ai[1] = (float)EntropicCrystalState.PostHit;
            Projectile.timeLeft = 180;
            StuckNPC = target;
            Toxic = true;   
            Projectile.netUpdate = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
           
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 ScaleEffects = new Vector2(0.9f, 1);

            int FrameCount = 4;
            Rectangle Frame = new Rectangle(0,(int)Fram, texture.Width, texture.Height/FrameCount);

            Vector2 origin = new Vector2(texture.Width / 2f, (texture.Height /FrameCount)/ 1.3f);


            float wind = AperiodicSin(Main.GlobalTimeWrappedHourly * 0.1f + Projectile.Center.X + Projectile.Center.Y) *
              //clamps rotation kinda
              0.0333f
              ;
            if (Toxic)
            {
                float thing = MathHelper.SmoothStep(0.8f, 1f, (float) Math.Cos(Main.GlobalTimeWrappedHourly * 0.1f));

                ScaleEffects = new Vector2(thing, 1);
                Main.EntitySpriteDraw(texture, drawPos, Frame, lightColor, Projectile.rotation+wind, origin, ScaleEffects, effects);


            }
            else 
                Main.EntitySpriteDraw(texture, drawPos, Frame, lightColor, Projectile.rotation, origin, ScaleEffects, effects);


            Utils.DrawBorderString(Main.spriteBatch, Projectile.whoAmI.ToString() + ", ai[2]: " + Projectile.ai[2], drawPos - Vector2.UnitY * 100, Color.AntiqueWhite);
            return false;
        }
    }
    class EntropicBlast : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        private Vector2 SpawnPos;
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        private enum BlastStage
        {
            portal,
            Bolt
        }
        private BlastStage CurrentStage => (BlastStage)Projectile.ai[1];
        public ref float Time => ref Projectile.ai[0];
        public ref float thing => ref Projectile.ai[2];
        private Projectile Crystal;

        private bool PortalOpen = true;
        private float portalInterp = 0f;
        private bool HasHit;
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.penetrate = - 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 240;
            Projectile.damage = NoxusWeapon.AltDamage;
            Projectile.width = Projectile.height = 60;
            
            
        }
        public override bool? CanDamage() { return false; }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        }

        public override void OnSpawn(IEntitySource source)
        {
            
            SpawnPos = Projectile.Center;
            portalInterp = 0;
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj)
            {
                if (parentProj.type == ModContent.ProjectileType<EntropicCrystal>())
                {
                    Crystal = parentProj;
                    
                }
            }
        }
        public override void AI()
        {
            ManageState();
            portalInterp =(float)Math.Clamp(Math.Round(MathHelper.Lerp(portalInterp, PortalOpen ? 1.1f : -0.1f, PortalOpen ? 0.3f : 0.05f),2),0,1);
            if (Crystal.active == false)
            {
                PortalOpen = false;
            }
            else
            {
                //todo: set the spawn pos and normal velocity to be offest in comparison to the crystal, while still allowing the projectile to move under its own power.
                

            }

            if (portalInterp == 0 && PortalOpen == false)
            {
                Projectile.Kill();
            }
            //Main.NewText($"Portal: {PortalOpen}, Interpolant: {portalInterp}");
            Time++;
        }
        private void ManageState()
        {
            switch (CurrentStage)
            {
                case BlastStage.portal:
                    HandlePortal();
                    break;
                case BlastStage.Bolt:
                    HandleBolt();
                    break;
            }
        }
        private void HandleBolt()
        {
            if(Time== 1) 
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), SpawnPos,
                Projectile.rotation.ToRotationVector2() * 40, ModContent.ProjectileType<EntropicComet>(),
                NoxusWeapon.AltDamage,0,default, default,default, Crystal.whoAmI);
        }
        private void HandlePortal()
        {
            Projectile.Opacity = (float)Math.Pow(Projectile.scale, 2.6f);
            if (Crystal != null && Crystal.ai[2] > 0)
            {
                int npcIndex = (int)Crystal.ai[2] - 1;
                if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
                {
                    NPC targetNpc = Main.npc[npcIndex];
                    if (targetNpc != null && targetNpc.active)
                    {
                        Projectile.rotation = (targetNpc.Center - Projectile.Center).ToRotation();
                        thing = npcIndex;
                    }
                }
            }
            // Create particles that converge in on the portal.
            for (int i = 0; i < 3; i++)
            {
                
                Vector2 lightAimPosition = SpawnPos + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloatDirection() * Projectile.scale * 50f + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 lightSpawnPosition = SpawnPos + Projectile.velocity * 75f + Projectile.velocity.RotatedByRandom(2.83f) * Main.rand.NextFloat(700f);
                Vector2 lightVelocity = (lightAimPosition - lightSpawnPosition) * 0.06f;
                SquishyLightParticle light = new(lightSpawnPosition, lightVelocity, 0.33f * portalInterp, Color.Pink, 19, 0.04f, 3f, 8f);
                GeneralParticleHandler.SpawnParticle(light);
            }
            if (Time > 60)
            {
                for (int i = 0; i < 2; i++)
                {
                    Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                    voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));
                    HeavySmokeParticle darkGas = new(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextVector2Circular(1f, 1f), voidColor, 11, Projectile.scale * 1.24f, Projectile.Opacity * 0.6f, Main.rand.NextFloat(0.02f), true);
                    GeneralParticleHandler.SpawnParticle(darkGas);
                }
                Projectile.ai[1] = (float)BlastStage.Bolt;
                Time = 0;
            }
        }
        public void DrawPortal(Vector2 DrawPos)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            ManagedShader PortalShader = ShaderManager.GetShader("HeavenlyArsenal.PortalShader");

            PortalShader.TrySetParameter("circleStretchInterpolant", portalInterp);
            PortalShader.TrySetParameter("transformation", (Matrix.CreateScale(3f, 1f, 1f)));
            PortalShader.TrySetParameter("aimDirection", Projectile.velocity.ToRotation());
            PortalShader.TrySetParameter("uColor", Color.MediumPurple);
            //PortalShader.TrySetParameter("uSecondaryColor", Color.White);
            PortalShader.TrySetParameter("edgeFadeInSharpness", 20.3f);
            PortalShader.TrySetParameter("aheadCircleMoveBackFactor", 0.67f);
            PortalShader.TrySetParameter("aheadCircleZoomFsctor", 0.9f);
            //PortalShader.TrySetParameter("uProgress", portalInterp * Main.GlobalTimeWrappedHourly);
            PortalShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);
            
            PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.StarDistanceLookup, 0);
            PortalShader.SetTexture(GennedAssets.Textures.Noise.TurbulentNoise, 1);
            PortalShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 2);
            PortalShader.SetTexture(GennedAssets.Textures.Extra.Void, 3);
            //PortalShader.SetTexture(GennedAssets.Textures.Noise.TurbulentNoise, 4);
            
            
            PortalShader.Apply();
            Texture2D pixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            float maxScale = 5f;
            Vector2 textureArea = Projectile.Size / pixel.Size() * maxScale;
            float scaleMod = 1f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 15f + Projectile.identity) * 0.012f);
            textureArea *= scaleMod;

            Main.spriteBatch.Draw(pixel, DrawPos, null, Projectile.GetAlpha(Color.MediumPurple), Projectile.rotation, pixel.Size() * 0.5f, textureArea, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 PortalDraw = SpawnPos - Main.screenPosition; 
            if(portalInterp > 0)
            DrawPortal(PortalDraw);

            Utils.DrawBorderString(Main.spriteBatch, "Projectile whoami:" + Projectile.whoAmI.ToString() + ", " + "ai[2]: " + thing.ToString(), PortalDraw - Vector2.UnitY * 120, Color.AntiqueWhite, 1);

            return false;
        }
    }
    class EntropicComet : ModProjectile
    {
        private Projectile Portal;
        private Projectile Crystal;
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ModContent.ProjectileType<EntropicBlast>());
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        }
        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/DarkComet";
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];

        public ref float OwnerID => ref Projectile.ai[2];
        private bool HasHit;
        public override void OnSpawn(IEntitySource source)
        {
            //todo: upon being spawned, the projectile already has it's owner's ID. 
            //all that needs to happen is the Crystal Projectile has to be set to that ID, after ensuring it exists.

            if (OwnerID >= 0 && OwnerID < Main.maxProjectiles)
            {
                Projectile possibleCrystal = Main.projectile[(int)OwnerID];
                if (possibleCrystal != null && possibleCrystal.active && possibleCrystal.type == ModContent.ProjectileType<EntropicCrystal>() && possibleCrystal.owner == Projectile.owner)
                {
                    Crystal = possibleCrystal;
                }
            }
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj)
            {
                if (parentProj.type == ModContent.ProjectileType<EntropicBlast>())
                {
                    Portal = parentProj;
                }
            }
        }
        public override void AI()
        {
            HandleBolt();
            Time++;
        }
        private void HandleBolt()
        {
            if (Time == 1)
            {
                for (int i = 0; i < 30; i++)
                    NoxusGasMetaball.CreateParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), Projectile.velocity.RotatedByRandom(0.68f) * Main.rand.NextFloat(0.9f), Main.rand.NextFloat(13f, 56f));

                SoundEngine.PlaySound(SoundID.Item103, Projectile.Center);
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch with { MaxInstances = 0, Volume = 0.1f, PitchVariance = 0.4f });
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.EarRinging with { MaxInstances = 0, Volume = 0.05f });
                float screenshakePower = 1;//MathHelper.Lerp(Owner.GetModPlayer<NoxusWeaponPlayer>().CrystalCount, 10, 3);
                ScreenShakeSystem.SetUniversalRumble(screenshakePower, MathHelper.TwoPi, null, 0.2f);
                Main.NewText($"screenshakePower: {screenshakePower}, {Owner.GetModPlayer<NoxusWeaponPlayer>().CrystalCount}");
            }

            if (Time <= 10 || !HasHit)
            {
                if(Portal is not null)
                Projectile.velocity = Portal.rotation.ToRotationVector2() * 40;


                // Add a mild amount of slithering movement.
                float slitherOffset = (float)Math.Sin(Time / 6.4f + Projectile.identity) * Utils.GetLerpValue(10f, 25f, Time, true) * 6.2f;
                Vector2 perpendicularDirection = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Projectile.Center += perpendicularDirection * slitherOffset;

                Projectile.rotation = Projectile.velocity.ToRotation();
               
                // Spawn particles.
                for (int i = 0; i < 20; i++)
                {
                    Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                    voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));
                    HeavySmokeParticle darkGas = new(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextVector2Circular(1f, 1f), voidColor, 11, Projectile.scale * 1.24f, Projectile.Opacity * 0.6f, Main.rand.NextFloat(0.02f), true);
                    GeneralParticleHandler.SpawnParticle(darkGas);

                }
                for (int i = 0; i < 4; i++)
                {
                    float gasSize = Utils.GetLerpValue(-3f, 25f, Time, true) * Projectile.width * 0.68f;
                    float angularOffset = (float)Math.Sin(Time / 5f) * 0.77f;
                    NoxusGasMetaball.CreateParticle(Projectile.Center + Projectile.velocity * 2f, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity.RotatedBy(angularOffset).RotatedByRandom(0.6f) * 0.26f, gasSize);
                }
            }
            else
            {

                Projectile.velocity *= 0.8f;
                Projectile.scale *= 0.99f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
           Crystal.active = false;
           HasHit = true;
            if (Portal != null && Portal.active && Portal.ModProjectile is EntropicBlast entropicBlast)
            {
               
                var hasHitField = typeof(EntropicBlast).GetField("HasHit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (hasHitField != null)
                {
                    hasHitField.SetValue(entropicBlast, true);
                }
            }

           
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Color Drawcolor = Color.Purple;
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/DarkComet").Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Drawcolor, Projectile.rotation, texture.Size() * 0.5f, 1f, SpriteEffects.None);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Drawcolor);


            Utils.DrawBorderString(Main.spriteBatch, Crystal.whoAmI.ToString() + "Time: "+ Time.ToString(), Projectile.Center - Main.screenPosition - Vector2.UnitY * 120, Color.AntiqueWhite);
            return false;
        }
    }
}

