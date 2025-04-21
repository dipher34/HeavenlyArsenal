using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class SmoothenPass : GenPass
{
    public SmoothenPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Smoothing the world.";

        for (int y = 5; y < Main.maxTilesY - 5; y += 2)
        {
            for (int x = 5; x < Main.maxTilesX - 5; x += 2)
                Tile.SmoothSlope(x, y);
        }
    }
}
