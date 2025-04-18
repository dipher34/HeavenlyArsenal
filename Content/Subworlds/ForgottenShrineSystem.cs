using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineSystem : ModSystem
{
    public override void PreUpdateEntities()
    {
        if (!SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return;

        ModContent.GetInstance<ForgottenShrineBackground>().ShouldBeActive = true;
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return;

        tileColor = new Color(0.6f, 0.4f, 0.4f);
    }
}
