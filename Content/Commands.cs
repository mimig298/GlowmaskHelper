using Terraria.Localization;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class ListGlowmasks : ModCommand
{
    public static LocalizedText DescriptionText { get; private set; }

    public override void SetStaticDefaults()
    {
        DescriptionText = Language.GetText("GlowmaskHelper.Commands.ListGlowmasks.Description");
    }

    public override string Command => "listglowmasks";
    public override string Description => DescriptionText.Value;

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        int moddedGlowmaskCount = GlowmaskLoader.GlowmaskCount - GlowmaskLoader.VanillaGlowmaskCount;
        if (moddedGlowmaskCount == 0)
        {
            caller.Reply("No glowmasks found.");
            return;
        }
        caller.Reply($"Found {moddedGlowmaskCount} glowmasks:");

        foreach (var textureSlotPair in GlowmaskLoader.glowmasks)
        {
            caller.Reply($"* {textureSlotPair.Value}: {textureSlotPair.Key}");
        }
    }
}
