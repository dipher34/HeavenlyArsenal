using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class UnderwaterTreeRenderer : ModSystem
{
    public override void PostDrawTiles()
    {
        List<TEUnderwaterTree> trees = TileEntity.ByID.Values.Where(te => te is TEUnderwaterTree).Select(te => te as TEUnderwaterTree).ToList();
        if (trees.Count <= 0)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        foreach (TEUnderwaterTree tree in trees)
            tree.Render();

        Main.spriteBatch.End();
    }
}
