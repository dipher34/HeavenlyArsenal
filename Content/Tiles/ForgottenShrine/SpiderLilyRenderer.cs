using System.Collections.Generic;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class SpiderLilyRenderer : ModSystem
{
    private static readonly List<SpiderLilyData> lilies = new List<SpiderLilyData>(32);

    public override void OnModLoad() => On_TileDrawing.ClearLegacyCachedDraws += RenderLilies;

    private void RenderLilies(On_TileDrawing.orig_ClearLegacyCachedDraws orig, TileDrawing self)
    {
        orig(self);
        if (lilies.Count <= 0)
            return;

        foreach (SpiderLilyData lily in lilies)
            lily.Render();
    }

    /// <summary>
    /// Registers a new lily into the set of lilies maintained by the world.
    /// </summary>
    public static void Register(SpiderLilyData lily)
    {
        lilies.Add(lily);
    }

    /// <summary>
    /// Removes a given existing lily from the set of lilies maintained by the world.
    /// </summary>
    public static void Remove(SpiderLilyData lily) => lilies.Remove(lily);

    public override void SaveWorldData(TagCompound tag)
    {
        tag["LilyCount"] = lilies.Count;

        TagCompound liliesTag = new TagCompound();
        for (int i = 0; i < lilies.Count; i++)
            liliesTag.Add($"Lily{i}", lilies[i].Serialize());

        tag["Lilies"] = liliesTag;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        lilies.Clear();

        if (!tag.TryGet("LilyCount", out int lilyCount) || !tag.TryGet("Lilies", out TagCompound liliesTag))
            return;

        for (int i = 0; i < lilyCount; i++)
            lilies.Add(SpiderLilyData.Deserialize(liliesTag.GetCompound($"Lily{i}")));
    }

    public override void PostUpdatePlayers()
    {
        for (int i = 0; i < lilies.Count; i++)
        {
            SpiderLilyData lily = lilies[i];
            lily.Update();

            // Account for the case in which a lily gets removed in the middle of the loop.
            if (!lilies.Contains(lily))
                i--;
        }
    }
}
