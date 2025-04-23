using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class SpiderLilyData
{
    private static readonly Asset<Texture2D> lilyTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/SpiderLily");

    /// <summary>
    /// A general-purpose timer used for wind movement of this lily.
    /// </summary>
    public float WindTime
    {
        get;
        set;
    }

    /// <summary>
    /// The scale of this lily.
    /// </summary>
    public float Scale
    {
        get;
        set;
    }

    /// <summary>
    /// The direction of this lily.
    /// </summary>
    public SpriteEffects Direction
    {
        get;
        set;
    }

    /// <summary>
    /// The position of the lily, in world coordinates.
    /// </summary>
    public readonly Point Position;

    public SpiderLilyData(Point position)
    {
        Position = position;
        Scale = MathHelper.Lerp(0.33f, 1f, MathF.Pow(Main.rand.NextFloat(), 4f));
        Direction = Main.rand.NextFromList(SpriteEffects.None, SpriteEffects.FlipHorizontally);
    }

    /// <summary>
    /// Updates this lily.
    /// </summary>
    public void Update()
    {
        WindTime = (WindTime + MathF.Abs(Main.windSpeedCurrent) * 0.11f) % (MathHelper.TwoPi * 5000f);
        Point tilePosition = new Point(Position.X / 16, Position.Y / 16);
        if (!WorldGen.SolidTile(tilePosition) || Main.tile[tilePosition].Slope != SlopeType.Solid)
            SpiderLilyRenderer.Remove(this);
    }

    /// <summary>
    /// Renders this lily.
    /// </summary>
    public void Render()
    {
        if (!Main.LocalPlayer.WithinRange(Position.ToVector2(), 2350f))
            return;

        Color color = Lighting.GetColor(Position.ToVector2().ToTileCoordinates());
        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        int windPushTime = 30;
        Main.instance.TilesRenderer.Wind.GetWindTime(Position.X / 16, Position.Y / 16, windPushTime, out int windTimeLeft, out int direction, out _);
        float windGrindPush = LumUtils.Convert01To010(windTimeLeft / (float)windPushTime);
        float rotation = LumUtils.AperiodicSin(WindTime + Position.X * 10f + Position.Y * 20f) * 0.3f + direction * windGrindPush * 0.45f;

        Texture2D texture = lilyTexture.Value;
        Vector2 lilyDrawPosition = Position.ToVector2() - Main.screenPosition + Vector2.UnitY * 2f + drawOffset;
        Main.spriteBatch.Draw(texture, lilyDrawPosition, null, color, rotation, texture.Size() * new Vector2(0.5f, 1f), Scale, 0, 0f);
    }

    /// <summary>
    /// Serializes this lily data as a tag compound for world saving.
    /// </summary>
    public TagCompound Serialize()
    {
        return new TagCompound()
        {
            ["Start"] = Position
        };
    }

    /// <summary>
    /// Deserializes a tag compound containing data for a lily back into said lily.
    /// </summary>
    public static SpiderLilyData Deserialize(TagCompound tag)
    {
        SpiderLilyData lily = new SpiderLilyData(tag.Get<Point>("Start"));
        return lily;
    }
}
