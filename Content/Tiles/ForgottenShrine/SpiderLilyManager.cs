using HeavenlyArsenal.Content.Tiles.Generic;
using Terraria.GameContent.Drawing;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class SpiderLilyManager : WorldOrientedTileObjectManager<SpiderLilyData>
{
    public override void OnModLoad() => On_TileDrawing.ClearLegacyCachedDraws += RenderLilies;

    private void RenderLilies(On_TileDrawing.orig_ClearLegacyCachedDraws orig, TileDrawing self)
    {
        orig(self);
        if (tileObjects.Count <= 0)
            return;

        foreach (SpiderLilyData lily in tileObjects)
            lily.Render();
    }
}
