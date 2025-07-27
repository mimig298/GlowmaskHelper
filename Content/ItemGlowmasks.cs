using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class ItemGlowmasks : GlobalItem
{
    public override void Load()
    {
        On_PlayerDrawLayers.DrawCompositeArmorPiece += DrawTorsoGlowmask;
    }

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return GlowmaskLoader.GetGlowmaskSlot_Item(entity.type) > 0;
    }

    public override void SetDefaults(Item entity)
    {
        entity.glowMask = GlowmaskLoader.GetGlowmaskSlot_Item(entity.type);
    }

    public override void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
    {
        glowMask = GlowmaskLoader.GetGlowmaskSlot_Equip(type, slot);
        glowMaskColor = Color.White;
    }

    public override void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color)
    {
        glowMask = GlowmaskLoader.GetGlowmaskSlot_Equip_Arms(slot);
        color = Color.White;
    }

    private static void DrawTorsoGlowmask(On_PlayerDrawLayers.orig_DrawCompositeArmorPiece orig, ref PlayerDrawSet drawinfo, CompositePlayerDrawContext context, DrawData data)
    {
        orig(ref drawinfo, context, data);

        if (context != CompositePlayerDrawContext.Torso || drawinfo.bodyGlowMask < GlowmaskLoader.VanillaGlowmaskCount)
            return;

        DrawData item = data;
        item.texture = TextureAssets.GlowMask[drawinfo.bodyGlowMask].Value;
        item.color = drawinfo.bodyGlowColor;
        Rectangle rectangle = item.sourceRect.Value;
        rectangle.Y += 224;
        item.sourceRect = rectangle;

        drawinfo.DrawDataCache.Add(item);
    }
}
