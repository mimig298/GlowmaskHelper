using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
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

internal class HandsOffGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.HandsOff, drawInfo.drawPlayer.handoff) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.OffhandAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.HandsOff, drawInfo.drawPlayer.handoff)];

        // Copied from PlayerDrawLayers.DrawPlayer_12_SkinComposite_BackArmShirt():
        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
        Vector2 positionOffset = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
        positionOffset.Y -= 2f;
        position += positionOffset * -drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
        position.Y += drawInfo.torsoOffset;
        float bodyRotation = drawInfo.drawPlayer.bodyRotation;
        Vector2 bodyVect = drawInfo.bodyVect;
        Vector2 compositeOffset_BackArm = new(6 * ((!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) ? 1 : (-1)), 2 * ((!drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically)) ? 1 : (-1))); ;
        position += compositeOffset_BackArm;
        bodyVect += compositeOffset_BackArm;
        float rotation = bodyRotation + drawInfo.compositeBackArmRotation;

        DrawData data = new(glowmaskTexture.Value, position, drawInfo.compBackArmFrame, Color.White, rotation, bodyVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cHandOff };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class HandsOnGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.HandsOn, drawInfo.drawPlayer.handon) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HandOnAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.HandsOn, drawInfo.drawPlayer.handon)];

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

        DrawData data = new(glowmaskTexture.Value, position, drawInfo.compFrontArmFrame, Color.White, rotation, bodyVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cHandOn };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class BackAccGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Back, drawInfo.drawPlayer.back) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Back, drawInfo.drawPlayer.back)];

        Vector2 position = drawInfo.Position - Main.screenPosition + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + new Vector2(0f, 4f);
        position = position.Floor();

        DrawData data = new(glowmaskTexture.Value, position, drawInfo.drawPlayer.bodyFrame, Color.White, drawInfo.drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cBack };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class WaistAccGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Waist, drawInfo.drawPlayer.waist) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.WaistAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Waist, drawInfo.drawPlayer.waist)];

        Rectangle frame = ArmorIDs.Waist.Sets.UsesTorsoFraming[drawInfo.drawPlayer.waist] ? drawInfo.drawPlayer.bodyFrame : drawInfo.drawPlayer.legFrame;
        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.legFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f)) + drawInfo.drawPlayer.legPosition + drawInfo.legVect;

        DrawData data = new(glowmaskTexture.Value, position, frame, Color.White, drawInfo.drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cWaist };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class NeckAccGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Neck, drawInfo.drawPlayer.neck) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.NeckAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Neck, drawInfo.drawPlayer.neck)];

        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
        
        DrawData data = new(glowmaskTexture.Value, position, drawInfo.drawPlayer.bodyFrame, Color.White, drawInfo.drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cNeck };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class FaceAccGlowmaskLayer : PlayerDrawLayer
{
    public override void Load()
    {
        On_PlayerDrawLayers.DrawPlayer_21_Head_TheFace += DrawGlowmaskInFaceLayer;
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Face, drawInfo.drawPlayer.face) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FaceAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[drawInfo.drawPlayer.face])
            return;

        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Face, drawInfo.drawPlayer.face)];

        Vector2 position = Vector2.Zero;
        if (drawInfo.drawPlayer.mount.Active && drawInfo.drawPlayer.mount.Type == 52)
            position = new Vector2(28f, -2f);
        position *= drawInfo.drawPlayer.Directions;
        position += new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect;

        DrawData data = new(glowmaskTexture.Value, position, drawInfo.drawPlayer.bodyFrame, Color.White, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cFace };

        drawInfo.DrawDataCache.Add(data);
    }

    private static void DrawGlowmaskInFaceLayer(On_PlayerDrawLayers.orig_DrawPlayer_21_Head_TheFace orig, ref PlayerDrawSet drawinfo)
    {
        orig(ref drawinfo);

        short glowmaskSlot = GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Face, drawinfo.drawPlayer.face);

        if (drawinfo.drawPlayer.face <= 0 || glowmaskSlot == -1 || !ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[drawinfo.drawPlayer.face])
            return;

        if (drawinfo.drawPlayer.head > 0 && !ArmorIDs.Head.Sets.DrawHead[drawinfo.drawPlayer.head])
            return;

        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[glowmaskSlot];

        Vector2 faceHeadOffsetFromHelmet = drawinfo.drawPlayer.GetFaceHeadOffsetFromHelmet();
        Vector2 position = new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - drawinfo.drawPlayer.bodyFrame.Width / 2 + drawinfo.drawPlayer.width / 2), (int)(drawinfo.Position.Y - Main.screenPosition.Y + drawinfo.drawPlayer.height - drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.headPosition + drawinfo.headVect + faceHeadOffsetFromHelmet;

        DrawData data = new(glowmaskTexture.Value, position, drawinfo.drawPlayer.bodyFrame, Color.White, drawinfo.drawPlayer.headRotation, drawinfo.headVect, 1f, drawinfo.playerEffect)
        { shader = drawinfo.cFace };

        drawinfo.DrawDataCache.Add(data);
    }
}

internal class ShieldGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Shield, drawInfo.drawPlayer.shield) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Shield);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Shield, drawInfo.drawPlayer.shield)];

        Vector2 zero = Vector2.Zero;
        if (drawInfo.drawPlayer.shieldRaised)
            zero.Y -= 4f * drawInfo.drawPlayer.gravDir;

        Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
        Vector2 bodyVect = drawInfo.bodyVect;
        int textureWidth = glowmaskTexture.Width();
        if (bodyFrame.Width != textureWidth)
        {
            bodyFrame.Width = textureWidth;
            bodyVect.X += bodyFrame.Width - textureWidth;
            if (drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally))
                bodyVect.X = bodyFrame.Width - bodyVect.X;
        }

        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2) + zero;

        DrawData data = new(glowmaskTexture.Value, position, bodyFrame, Color.White, drawInfo.drawPlayer.bodyRotation, bodyVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cShield };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class FrontAccGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Front, drawInfo.drawPlayer.front) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FrontAccFront);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Front, drawInfo.drawPlayer.front)];

        Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
        int num = bodyFrame.Width / 2;
        bodyFrame.Width -= num;
        Vector2 bodyVect = drawInfo.bodyVect;
        if (drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally))
            bodyVect.X -= num;
        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
        
        DrawData data = new(glowmaskTexture.Value, position, bodyFrame, Color.White, drawInfo.drawPlayer.bodyRotation, bodyVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cFront };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class ShoeAccGlowmaskLayer : PlayerDrawLayer
{
    private static MethodInfo DrawSittingLegsInfo;

    public override void Load()
    {
        DrawSittingLegsInfo = typeof(PlayerDrawLayers).GetMethod("DrawSittingLegs", BindingFlags.Static | BindingFlags.NonPublic);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Shoes, drawInfo.drawPlayer.shoe) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Shoes);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.drawPlayer.legs > 0 && ArmorIDs.Legs.Sets.OverridesLegs[drawInfo.drawPlayer.legs])
            return;

        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Shoes, drawInfo.drawPlayer.shoe)];

        if (drawInfo.isSitting)
        {
            object[] parameters = [drawInfo, glowmaskTexture.Value, Color.White, drawInfo.cShoe, true];
            DrawSittingLegsInfo.Invoke(null, parameters);
            return;
        }

        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.legFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f)) + drawInfo.drawPlayer.legPosition + drawInfo.legVect;

        DrawData data = new(glowmaskTexture.Value, position, drawInfo.drawPlayer.legFrame, Color.White, drawInfo.drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cShoe };

        drawInfo.DrawDataCache.Add(data);
    }
}

internal class BalloonGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Balloon, drawInfo.drawPlayer.balloon) != -1;
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BalloonAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Balloon, drawInfo.drawPlayer.balloon)];

        if (ArmorIDs.Balloon.Sets.UsesTorsoFraming[drawInfo.drawPlayer.balloon])
        {
            Vector2 pos = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + drawInfo.bodyVect;
            DrawData data = new(glowmaskTexture.Value, pos, drawInfo.drawPlayer.bodyFrame, Color.White, drawInfo.drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect)
            { shader = drawInfo.cBalloon };
            drawInfo.DrawDataCache.Add(data);
            return;
        }

        int num = (Main.hasFocus && (!Main.ingameOptionsWindow || !Main.autoPause)) ? (DateTime.Now.Millisecond % 800 / 200) : 0;
        Vector2 vector = Main.OffsetsPlayerOffhand[drawInfo.drawPlayer.bodyFrame.Y / 56];
        if (drawInfo.drawPlayer.direction != 1)
            vector.X = drawInfo.drawPlayer.width - vector.X;

        if (drawInfo.drawPlayer.gravDir != 1f)
            vector.Y -= drawInfo.drawPlayer.height;

        Vector2 vector2 = new(0f, 14f);
        Vector2 position = drawInfo.Position - Main.screenPosition + vector * new Vector2(1f, drawInfo.drawPlayer.gravDir) + new Vector2(0f, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height) + vector2;
        position = position.Floor();
        Vector2 textureSize = glowmaskTexture.Size();
        Rectangle frame = new(0, (int)textureSize.Y / 4 * num, (int)textureSize.X, (int)textureSize.Y / 4);
        DrawData item = new(glowmaskTexture.Value, position, frame, Color.White, drawInfo.drawPlayer.bodyRotation, new Vector2(26 + drawInfo.drawPlayer.direction * 4, 28f + drawInfo.drawPlayer.gravDir * 6f), 1f, drawInfo.playerEffect)
        { shader = drawInfo.cBalloon };
        drawInfo.DrawDataCache.Add(item);
    }
}

internal class BeardGlowmaskLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Beard, drawInfo.drawPlayer.beard) != -1 && !(drawInfo.drawPlayer.head >= 0 && ArmorIDs.Head.Sets.PreventBeardDraw[drawInfo.drawPlayer.head]);
    }

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_Equip(EquipType.Beard, drawInfo.drawPlayer.beard)];

        Vector2 beardDrawOffsetFromHelmet = drawInfo.drawPlayer.GetBeardDrawOffsetFromHelmet();
        Vector2 position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect;
        position += beardDrawOffsetFromHelmet;

        DrawData data = new(glowmaskTexture.Value, position, drawInfo.drawPlayer.bodyFrame, Color.White, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect)
        { shader = drawInfo.cBeard };
        drawInfo.DrawDataCache.Add(data);
    }
}
