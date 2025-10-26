using HeavenlyArsenal.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Cosmetic
{
    internal class LightHalo : ModItem
    {
        public override void SetDefaults()
        {
            Item.vanity = true;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<LightHalo_Player>().hasHalo = true;
        }

    }

    internal class LightHalo_Player : ModPlayer
    {
        public bool hasHalo = false;
        public override void ResetEffects()
        {
            hasHalo = false;
        }
    }

    public class LightHalo_DrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition()  => new BeforeParent(PlayerDrawLayers.HairBack);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetModPlayer<LightHalo_Player>().hasHalo;
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            ref Player player = ref drawInfo.drawPlayer;
            Texture2D texture = GennedAssets.Textures.NamelessDeity.NamelessDeityEyeFull;//ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Accessories/Cosmetic/LightHalo").Value;

            Vector2 DrawPos = drawInfo.HeadPosition() + new Vector2(10 * -player.direction, -5+MathF.Sin(Main.GlobalTimeWrappedHourly + player.whoAmI*10));

            Vector2 Origin = texture.Size() * 0.5f;
            Vector2 Scale = new Vector2(0.02f, 0.02f);
            float Rot = MathHelper.ToRadians(1.5f) * player.direction;
            DrawData halo = new DrawData(texture, DrawPos, null, Color.AntiqueWhite, Rot, Origin, Scale, 0);

            drawInfo.DrawDataCache.Add(halo);
        }
    }
}
