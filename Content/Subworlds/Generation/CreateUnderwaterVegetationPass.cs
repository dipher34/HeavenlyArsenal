using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class CreateUnderwaterVegetationPass : GenPass
{
    public CreateUnderwaterVegetationPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating underwater vegetation.";

        ushort treeID = (ushort)ModContent.TileType<UnderwaterTree>();
        for (int i = 0; i < ForgottenShrineGenerationConstants.UnderwaterTreeCount; i++)
        {
            int x = WorldGen.genRand.Next(40, Main.maxTilesX - 40);
            int y = Main.maxTilesY - ForgottenShrineGenerationConstants.GroundDepth - 1;
            Main.tile[x, y].TileType = treeID;
            Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
        }
    }
}
