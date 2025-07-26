using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class ItemGlowmasks : GlobalItem
{
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
}
