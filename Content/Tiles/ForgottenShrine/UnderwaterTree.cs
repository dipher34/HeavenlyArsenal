using HeavenlyArsenal.Content.Subworlds.Generation;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class UnderwaterTree : ModTile
{
    private static readonly Asset<Texture2D>[] treeTextures =
    [
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/UnderwaterTree1"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/UnderwaterTree2"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/UnderwaterTree3")
    ];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;

        // Prepare necessary setups to ensure that this tile is treated like grass.
        Main.tileCut[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        // Use plant destruction visuals and sounds.
        HitSound = SoundID.Grass;
        DustType = DustID.Grass;

        AddMapEntry(new Color(16, 16, 16));
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CanReplace(int i, int j, int tileTypeBeingPlaced) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        int treeVariant = (i * 4 + j * 17) % treeTextures.Length;
        Texture2D texture = treeTextures[treeVariant].Value;

        UnifiedRandom rng = new UnifiedRandom(i + 74 + j * 113);
        float baseLength = ForgottenShrineGenerationConstants.WaterDepth * 16f;
        float treeLength = baseLength + rng.NextFloat(275f);
        float rotation = rng.NextFloatDirection() * 0.4f + MathHelper.PiOver2;

        // This ensures that rotations do not interfere with the rule that the tree should reach treeLength pixels upwards.
        float treeLengthAccountingForRotation = treeLength / MathF.Sin(rotation);
        float treeScale = treeLengthAccountingForRotation / texture.Height;

        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        Vector2 drawPosition = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y + 2f) + drawOffset;
        SpriteEffects direction = rng.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(texture, drawPosition, null, Color.Black, rotation - MathHelper.PiOver2, new Vector2(0.5f, 1f) * texture.Size(), treeScale, direction, 0f);

        return false;
    }
}
