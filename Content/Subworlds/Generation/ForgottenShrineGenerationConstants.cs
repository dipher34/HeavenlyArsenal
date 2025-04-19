using Terraria;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public static class ForgottenShrineGenerationConstants
{
    /// <summary>
    /// The depth of ground beneath the shrine's shallow water.
    /// </summary>
    public static int GroundDepth => (int)(Main.maxTilesY * 0.35f);

    /// <summary>
    /// The depth of water beneath the shrine.
    /// </summary>
    public static int WaterDepth => 33;

    /// <summary>
    /// The height of beams for the bridge before the shrine.
    /// </summary>
    public static int BridgeBeamHeight => 11;

    /// <summary>
    /// The width of arches on the bridge.
    /// </summary>
    public static int BridgeArchWidth => 75;

    /// <summary>
    /// The amount of horizontal coverage that ropes underneath the bridge have, in tile coordinates.
    /// </summary>
    public static int BridgeUndersideRopeWidth => (int)(BridgeArchWidth * 0.6f);

    /// <summary>
    /// The amount of vertical coverage that ropes beneath the bridge should have due to sag, in tile coordinates.
    /// </summary>
    public static int BridgeUndersideRopeSag => (int)(BridgeBeamHeight * 0.7f);

    /// <summary>
    /// The maximum height of arches on the bridge.
    /// </summary>
    public static int BridgeArchHeight => 4;

    /// <summary>
    /// The vertical thickness of the bridge.
    /// </summary>
    public static int BridgeThickness => 5;

    /// <summary>
    /// The amount of tree structures to generate underwater.
    /// </summary>
    public static int UnderwaterTreeCount => Main.maxTilesX / 60;

    /// <summary>
    /// The amount of cattails to generate underwater.
    /// </summary>
    public static int CattailCount => Main.maxTilesX / 26;

    /// <summary>
    /// The maximum height of cattails above the water line.
    /// </summary>
    public static int MaxCattailHeight => 7;

    /// <summary>
    /// The amount of lilypads to generate atop the water.
    /// </summary>
    public static int LilypadCount => 0;
}
