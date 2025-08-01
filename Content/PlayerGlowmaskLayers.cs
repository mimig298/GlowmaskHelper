using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class TorsoGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return !drawInfo.drawPlayer.invis && drawInfo.bodyGlowMask >= GlowmaskLoader.VanillaGlowmaskCount;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[drawInfo.bodyGlowMask];

        // Copied from PlayerDrawLayers.DrawPlayer_17_TorsoComposite():
        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2f + drawInfo.drawPlayer.width / 2f), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
        Vector2 positionOffset = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
        positionOffset.Y -= 2f;
        position += positionOffset * -drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
        float bodyRotation = drawInfo.drawPlayer.bodyRotation;

        DrawData data = new(glowmaskTexture.Value, position, drawInfo.compTorsoFrame, drawInfo.bodyGlowColor, bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cBody };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class ArmGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return !drawInfo.drawPlayer.invis && drawInfo.armGlowMask >= GlowmaskLoader.VanillaGlowmaskCount;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[drawInfo.armGlowMask];

        // Copied from PlayerDrawLayers.DrawPlayer_28_ArmOverItemComposite():
        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2f + drawInfo.drawPlayer.width / 2f), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
        Vector2 positionOffset = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
        positionOffset.Y -= 2f;
        position += positionOffset * -drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
        float rotation = drawInfo.drawPlayer.bodyRotation + drawInfo.compositeFrontArmRotation;
        Vector2 bodyVect = drawInfo.bodyVect;
        Vector2 compositeOffset_FrontArm = new(-5 * ((!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1)), 0f); ;
        bodyVect += compositeOffset_FrontArm;
        position += compositeOffset_FrontArm;
        if (drawInfo.compFrontArmFrame.X / drawInfo.compFrontArmFrame.Width >= 7)
            position += new Vector2((!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1), (!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically)) ? 1 : (-1));

        DrawData data = new(glowmaskTexture.Value, position, drawInfo.compFrontArmFrame, drawInfo.armGlowColor, rotation, bodyVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cBody };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class WingsGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return !drawInfo.drawPlayer.dead && !drawInfo.hideEntirePlayer && drawInfo.drawPlayer.wings > 0;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Wings);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        short glowmaskSlot = GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Wings, drawInfo.drawPlayer.wings);
        if (glowmaskSlot <= GlowmaskLoader.VanillaGlowmaskCount)
            return; // Don't draw glowmask if it is a vanilla one.
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[glowmaskSlot];

        // Copied from PlayerDrawLayers.DrawPlayer_09_Wings():
        Vector2 directions = drawInfo.drawPlayer.Directions;
        Vector2 positionOffset = new(0f, 7f);
        Vector2 position = drawInfo.Position - Main.screenPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + positionOffset;
        position += new Vector2(-9, 2) * directions;

        DrawData item = new(glowmaskTexture.Value, position.Floor(), new Rectangle(0, glowmaskTexture.Height() / 4 * drawInfo.drawPlayer.wingFrame, glowmaskTexture.Width(), glowmaskTexture.Height() / 4), Color.White * drawInfo.stealth * (1f - drawInfo.shadow), drawInfo.drawPlayer.bodyRotation, new Vector2(glowmaskTexture.Width() / 2, glowmaskTexture.Height() / 8), 1f, drawInfo.playerEffect)
        { shader = drawInfo.cWings };
        drawInfo.DrawDataCache.Add(item);
    }
}
