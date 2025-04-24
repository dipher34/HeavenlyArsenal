using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class SacredGrass : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;
        Main.tileMerge[TileID.Dirt][Type] = true;
        Main.tileMerge[Type][TileID.Dirt] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Grass"]);

        RegisterItemDrop(ItemID.DirtBlock);

        AddMapEntry(new Color(94, 51, 128));

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.Conversion.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = TileID.Dirt;
        TileID.Sets.CanBeDugByShovel[Type] = true;
    }

    public override void NumDust(int i, int j, bool fail, ref int Type)
    {
        Type = fail ? 1 : 3;
    }
}
