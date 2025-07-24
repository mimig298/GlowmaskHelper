using Terraria;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class ItemGlowmasks : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return GlowmaskLoader.GetGlowmaskSlot(entity) != -1;
    }

    public override void SetDefaults(Item entity)
    {
        entity.glowMask = GlowmaskLoader.GetGlowmaskSlot(entity.type, typeof(Item));
    }
}
