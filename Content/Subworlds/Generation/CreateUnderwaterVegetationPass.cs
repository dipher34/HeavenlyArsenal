using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
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
            float xInterpolant = i / (float)(ForgottenShrineGenerationConstants.UnderwaterTreeCount - 1f);
            int x = (int)MathHelper.Lerp(50f, Main.maxTilesX - 50f, xInterpolant) + WorldGen.genRand.Next(-24, 24);
            int y = Main.maxTilesY - ForgottenShrineGenerationConstants.GroundDepth - 1;
            Main.tile[x, y].TileType = treeID;
            Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
            TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEUnderwaterTree>());
        }
    }
}
