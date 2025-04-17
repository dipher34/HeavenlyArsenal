using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Vambrace
{
   
    public class ElectricVambrace : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 56;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.accessory = true;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ElectricVambracePlayer>().ElectricVambrace = true;
            var modPlayer = player.Calamity();
            //modPlayer.DashID = ElectricVambraceDash.ID;
        }


        public override void AddRecipes()
        {
            CreateRecipe().
            AddIngredient<SlagsplitterPauldron>(1).
            AddIngredient<LeviathanAmbergris>(1).
            AddIngredient<AscendantSpiritEssence>(8).
            AddTile<CosmicAnvil>().
            Register();
        }
    }
    public class ElectricVambracePlayer : ModPlayer
    {
        internal bool ElectricVambrace;
        public bool HasReducedDashFirstFrame 
        { 
            get; 
            private set; 
        }
        public bool isVambraceDashing
        {
            get;
            set;
        }

        public override void Load()
        { 

        }
        public override void PostUpdate()
        {
            if (ElectricVambrace)
            {
                if (Player.miscCounter % 8 == 7 && Player.dashDelay > 0) // Reduced dash cooldown by 38%
                    Player.dashDelay--;

                //Console.WriteLine(Player.dashDelay);

                if (Player.dashDelay == -1)
                {
                    if (Player.miscCounter % 6 == 0 && Player.velocity != Vector2.Zero)
                    {
                        int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(170));
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + Player.velocity * 1.5f, Vector2.Zero, ModContent.ProjectileType<VambraceDash>(), damage, 10f, Player.whoAmI);
                    }
                    Player.endurance += 0.20f;
                    if (!isVambraceDashing) // Dash isn't reduced, this is used to determine the first frame of dashing
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.4f, PitchVariance = 0.4f }, Player.Center);

                        int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(750));
                        isVambraceDashing = true;


                    }

                    else
                        isVambraceDashing = false;
                }
            }
            /*
            if (Pauldron)
            {
                if (Player.dashDelay == -1)// TODO: prevent working with special dashes, this was inconsitent with my old solution so I didn't keep it. not huge deal)
                {
                    Player.endurance += 0.1f;
                    if (!HasReducedDashFirstFrame) // Dash isn't reduced, this is used to determine the first frame of dashing
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.4f, PitchVariance = 0.4f }, Player.Center);
                        int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(67));

                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + Player.velocity * 1.5f, Vector2.Zero, ModContent.ProjectileType<PauldronDash>(), damage, 16f, Player.whoAmI);
                        HasReducedDashFirstFrame = true;
                    }
                    float numberOfDusts = 10f;
                    float rotFactor = 180f / numberOfDusts;
                    for (int i = 0; i < numberOfDusts; i++)
                    {
                        float rot = MathHelper.ToRadians(i * rotFactor);
                        Vector2 offset = new Vector2(MathF.Min(Player.velocity.X * Player.direction * 0.7f + 8f, 20f), 0).RotatedBy(rot * Main.rand.NextFloat(4f, 5f));
                        Vector2 velOffset = Vector2.Zero;
                        Dust dust = Dust.NewDustPerfect(Player.Center + offset + Player.velocity, Main.rand.NextBool() ? 35 : 127, new Vector2(velOffset.X, velOffset.Y));
                        dust.noGravity = true;
                        dust.velocity = velOffset;
                        dust.alpha = 100;
                        dust.scale = MathF.Min(Player.velocity.X * Player.direction * 0.08f, 1.2f);
                    }
                    float sparkscale = MathF.Min(Player.velocity.X * Player.direction * 0.08f, 1.2f);
                    Vector2 SparkVelocity1 = Player.velocity.RotatedBy(Player.direction * -3, default) * 0.1f - Player.velocity / 2f;
                    SparkParticle spark = new SparkParticle(Player.Center + Player.velocity.RotatedBy(2f * Player.direction) * 1.5f, SparkVelocity1, false, Main.rand.Next(11, 13), sparkscale, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark);
                    Vector2 SparkVelocity2 = Player.velocity.RotatedBy(Player.direction * 3, default) * 0.1f - Player.velocity / 2f;
                    SparkParticle spark2 = new SparkParticle(Player.Center + Player.velocity.RotatedBy(-2f * Player.direction) * 1.5f, SparkVelocity2, false, Main.rand.Next(11, 13), sparkscale, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark2);

                    if (Player.miscCounter % 6 == 0 && Player.velocity != Vector2.Zero)
                    {
                        int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(170));
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + Player.velocity * 1.5f, Vector2.Zero, ModContent.ProjectileType<PauldronDash>(), damage, 10f, Player.whoAmI);
                    }
                }
                else
                    HasReducedDashFirstFrame = false;
            */
            }


        public override void PostUpdateMiscEffects()
        {
            if (ElectricVambrace)
            {

            }
        }
        public override void ResetEffects()
        {
            ElectricVambrace = false;

        }
    }
}
