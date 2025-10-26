using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal partial class RitualAltar
{
    static void DrawArm(ref RitualAltarLimb RitualAltarLimb, Color drawColor, SpriteEffects effects)
    {
        var armTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarArm").Value;
        var defaultForearmFrame = new Rectangle(0, 0, 84, 32);
        var anchoredForearmFrame = new Rectangle(0, 32, 84, 32);

        var currentFrame = RitualAltarLimb.IsAnchored ? anchoredForearmFrame : defaultForearmFrame;

        Main.spriteBatch.Draw(
            armTexture,
            RitualAltarLimb.Skeleton.Position(0) - Main.screenPosition,
            new Rectangle(94, 0, 48, 24),
            drawColor,
            (RitualAltarLimb.Skeleton.Position(0) - RitualAltarLimb.Skeleton.Position(1)).ToRotation(),
            new Vector2(134 - 94, 12),
            1f,
            effects,
            0f
        );

        Main.spriteBatch.Draw(
            armTexture,
            RitualAltarLimb.Skeleton.Position(1) - Main.screenPosition,
            currentFrame,
            drawColor,
            (RitualAltarLimb.Skeleton.Position(1) - RitualAltarLimb.Skeleton.Position(2)).ToRotation(),
            new Vector2(72, 14),
            1f,
            effects,
            0f
        );
    }


    static void DrawLeg(ref RitualAltarLimb nightgauntLimb, Color drawColor, SpriteEffects effects)
    {
        var legTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarArm").Value;
        var defaultLowerLegFrame = new Rectangle(0, 0, 80, 26);
        var anchoredLowerLegFrame = new Rectangle(0, 26, 80, 26);

        var currentFrame = nightgauntLimb.IsAnchored ? anchoredLowerLegFrame : defaultLowerLegFrame;

        Main.spriteBatch.Draw(
            legTexture,
            nightgauntLimb.Skeleton.Position(0) - Main.screenPosition,
            new Rectangle(84, 0, 66, 26),
            drawColor,
            (nightgauntLimb.Skeleton.Position(0) - nightgauntLimb.Skeleton.Position(1)).ToRotation(),
            new Vector2(134 - 74, 12),
            1f,
            effects,
            0f
        );

        Main.spriteBatch.Draw(
            legTexture,
            nightgauntLimb.Skeleton.Position(1) - Main.screenPosition,
            currentFrame,
            drawColor,
            (nightgauntLimb.Skeleton.Position(1) - nightgauntLimb.Skeleton.Position(2)).ToRotation(),
            new Vector2(72, 14),
            1f,
            effects,
            0f
        );
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy) return false;

        Texture2D A = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarConcept").Value;


        SpriteEffects sp = 0;
        float rot = NPC.rotation + MathHelper.PiOver2;
        Vector2 Origin = A.Size() * 0.5f + new Vector2(0, 30);
        Main.EntitySpriteDraw(A, NPC.Center - Main.screenPosition, null, drawColor, rot, Origin, 1, sp);
        for (int i = 0; i < _limbs.Length; i++)
        {

            SpriteEffects a = NPC.direction == 1 ? 0 : SpriteEffects.FlipHorizontally;
            a = Math.Sign((_limbs[i].EndPosition - _limbs[i].TargetPosition).Length()) == 1 ? 0 : SpriteEffects.FlipVertically;
            DrawArm(ref _limbs[i], drawColor, a);
        }

       
        string msg = $"Buffed NPCS: \n";
        foreach(NPC npc in RitualSystem.BuffedNPCs)
        {
            msg += $"{npc.FullName},";
        }
        //Utils.DrawBorderString(spriteBatch, msg, NPC.Center - screenPos, Color.AntiqueWhite);
        
        

        for (int i = 0; i < _limbs.Length; i++)
        {
            Vector2 DrawPos = _limbs[i].TargetPosition - screenPos;
            //spriteBatch.Draw(GennedAssets.Textures.GreyscaleTextures.WhitePixel, DrawPos, new Rectangle(0, 0, 5, 5), Color.Lime);
            string msgd = $"{i + 1}\n {_limbs[i].IsTouchingGround}";
           // Utils.DrawBorderString(Main.spriteBatch, msgd, DrawPos, Color.AntiqueWhite);

        }
        //Texture2D debugArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/Jellyfish_DebugArrow").Value;
        //Main.EntitySpriteDraw(debugArrow, NPC.Center - Main.screenPosition, null, Color.AntiqueWhite, rot, new Vector2(debugArrow.Width / 2, 0), new Vector2(MathF.Tanh(NPC.velocity.X), MathF.Tanh(NPC.velocity.Y)), SpriteEffects.FlipVertically);
        // Utils.DrawBorderString(Main.spriteBatch, NPC.velocity.ToString(), NPC.Center - Main.screenPosition, Color.AntiqueWhite);


       //Utils.DrawBorderString(Main.spriteBatch, (blood / (float)bloodBankMax).ToString(), NPC.Top - screenPos, Color.AntiqueWhite);


        string b = "";
        b += $"CultID: {CultistCoordinator.GetCultOfNPC(NPC).CultID.ToString()}\n";
        b += $"MaxCultists: {CultistCoordinator.GetCultOfNPC(NPC).MaxCultists}\n";
        b += currentAIState.ToString() + $"\n";
        b += blood / (float)bloodBankMax;
       // Utils.DrawBorderString(Main.spriteBatch, b, NPC.Top - screenPos, Color.AntiqueWhite);
        return false;
    }
}