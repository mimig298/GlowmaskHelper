using ReLogic.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class ItemGlowmasks : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.ModItem?.GetType().GetAttribute<AutoloadGlowmask>() != null;
    }

    public override void SetDefaults(Item entity)
    {
        entity.glowMask = GlowmaskLoader.GetGlowmaskSlot(entity.type, typeof(Item));
    }
}
