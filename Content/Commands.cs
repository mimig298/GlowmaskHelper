using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class ListGlowmasks : ModCommand
{
    public static LocalizedText DescriptionText { get; private set; }

    public override void SetStaticDefaults()
    {
        DescriptionText = Language.GetText("Mods.GlowmaskHelper.Commands.ListGlowmasks.Description");
    }

    public override string Command => "listglowmasks";
    public override string Description => DescriptionText.Value;

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        int moddedGlowmaskCount = GlowmaskLoader.GlowmaskCount - GlowmaskLoader.VanillaGlowmaskCount;
        if (moddedGlowmaskCount == 0)
        {
            caller.Reply("No glowmasks found.", Color.Yellow);
            return;
        }
        caller.Reply($"Found {moddedGlowmaskCount} glowmasks:", Color.Yellow);

        foreach (var textureSlotPair in GlowmaskLoader.glowmasks)
        {
            caller.Reply($"* {textureSlotPair.Value}: {textureSlotPair.Key}");

            if (GlowmaskLoader.itemToGlowmask.Values.Contains(textureSlotPair.Value))
            {
                ICollection<int> itemTypes = GetKeysForValue(GlowmaskLoader.itemToGlowmask, textureSlotPair.Value);
                if (itemTypes.Count > 0)
                    caller.Reply($"  - Assigned to the item types: {string.Join(", ", itemTypes)}");
            }
            if (GlowmaskLoader.npcToGlowmask.Values.Contains(textureSlotPair.Value))
            {
                ICollection<int> npcTypes = GetKeysForValue(GlowmaskLoader.npcToGlowmask, textureSlotPair.Value);
                if (npcTypes.Count > 0)
                    caller.Reply($"  - Assigned to the NPC types: {string.Join(", ", npcTypes)}");
            }
            ICollection<int> tileTypes = [];
            for (int i = 0; i < Main.tileGlowMask.Length; i++)
            {
                if (Main.tileGlowMask[i] == textureSlotPair.Value)
                    tileTypes.Add(i);
            }
            if (tileTypes.Count > 0)
                caller.Reply($"  - Assigned to the tile types: {string.Join(", ", tileTypes)}");
            if (GlowmaskLoader.equipToGlowmask.Values.Contains(textureSlotPair.Value))
            {
                ICollection<Tuple<EquipType, int>> equipTypes = GetKeysForValue(GlowmaskLoader.equipToGlowmask, textureSlotPair.Value);
                if (equipTypes.Count > 0)
                {
                    string[] formattedEquipTypeList = new string[equipTypes.Count];
                    int i = 0;
                    foreach (var equipType in equipTypes)
                    {
                        formattedEquipTypeList[i] = $"{equipType.Item1} {equipType.Item2}";
                        i++;
                    }
                    caller.Reply($"  - Assigned to the equipment types: {string.Join(", ", formattedEquipTypeList)}");
                }
            }
            if (GlowmaskLoader.equipArmsToGlowmask.Values.Contains(textureSlotPair.Value))
            {
                ICollection<int> equipSlots = GetKeysForValue(GlowmaskLoader.equipArmsToGlowmask, textureSlotPair.Value);
                if (equipSlots.Count > 0)
                    caller.Reply($"  - Assigned as an arm texture for the equipment body types: {string.Join(", ", equipSlots)}");
            }
            if (GlowmaskLoader.wallToGlowmask.Values.Contains(textureSlotPair.Value))
            {
                ICollection<int> wallTypes = GetKeysForValue(GlowmaskLoader.wallToGlowmask, textureSlotPair.Value);
                if (wallTypes.Count > 0)
                    caller.Reply($"  - Assigned to the wall types: {string.Join(", ", wallTypes)}");
            }
        }
    }

    private static ICollection<TKey> GetKeysForValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TValue value)
    {
        ICollection<TKey> keys = [];
        foreach (var keyValuePair in dictionary)
        {
            if (keyValuePair.Value.Equals(value))
            {
                keys.Add(keyValuePair.Key);
            }
        }
        return keys;
    }
}
