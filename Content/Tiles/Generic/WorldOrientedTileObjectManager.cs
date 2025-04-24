using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.Generic;

public abstract class WorldOrientedTileObjectManager<TTileObject> : ModSystem where TTileObject : WorldOrientedTileObject, new()
{
    protected readonly List<TTileObject> tileObjects = new List<TTileObject>(32);

    /// <summary>
    /// Registers a new tile object into the set maintained by the world.
    /// </summary>
    public virtual void Register(TTileObject tileObject) => tileObjects.Add(tileObject);

    /// <summary>
    /// Removes a given existing tile object from the set maintained by the world.
    /// </summary>
    public void Remove(TTileObject tileObject) => tileObjects.Remove(tileObject);

    public override void SaveWorldData(TagCompound tag)
    {
        tag["ObjectCount"] = tileObjects.Count;

        TagCompound objectsTag = new TagCompound();
        for (int i = 0; i < tileObjects.Count; i++)
            objectsTag.Add($"Object{i}", tileObjects[i].Serialize());

        tag["Objects"] = objectsTag;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        tileObjects.Clear();

        if (!tag.TryGet("ObjectCount", out int objectCount) || !tag.TryGet("Objects", out TagCompound objectsTag))
            return;

        TTileObject scuffed = new();
        for (int i = 0; i < objectCount; i++)
            tileObjects.Add((TTileObject)scuffed.Deserialize(objectsTag.GetCompound($"Object{i}")));
    }

    public sealed override void PostUpdatePlayers()
    {
        for (int i = 0; i < tileObjects.Count; i++)
        {
            TTileObject tileObject = tileObjects[i];
            tileObject.Update();

            // Account for the case in which an object gets removed in the middle of the loop.
            if (!tileObjects.Contains(tileObject))
                i--;
        }
    }
}
