using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

/// <summary>
/// This class serves as the central place to store and register custom glowmasks, and can be used to access information about them.<br/>
/// Using an <see cref="AutoloadGlowmask"/> attribute in your classes will do all the special logic for you, so you usually don't need to use this class if that's the case.
/// <para/> Currently supports adding glowmasks to <see cref="ModItem"/>s, <see cref="ModNPC"/>s, <see cref="ModTile"/>s and armour items.
/// </summary>
public static class GlowmaskLoader
{
    // Do NOT ever change this value unless you're inside GlowmaskLoader.Load()
    public static int VanillaGlowmaskCount { get; private set; }

    private static short nextGlowmask = (short)TextureAssets.GlowMask.Length;

    internal static IDictionary<string, short> glowmasks = new Dictionary<string, short>();
    internal static IDictionary<int, short> itemToGlowmask = new Dictionary<int, short>();
    internal static IDictionary<int, short> npcToGlowmask = new Dictionary<int, short>();
    internal static IDictionary<Tuple<EquipType, int>, short> equipToGlowmask = new Dictionary<Tuple<EquipType, int>, short>();
    internal static IDictionary<int, short> equipArmsToGlowmask = new Dictionary<int, short>();

    public static int GlowmaskCount => nextGlowmask;

    internal static void Load()
    {
        VanillaGlowmaskCount = TextureAssets.GlowMask.Length;
        nextGlowmask = (short)VanillaGlowmaskCount;

        foreach (ModItem modItem in ModContent.GetContent<ModItem>())
        {
            Type type = modItem.GetType();
            if (type.GetAttribute<AutoloadGlowmask>() == null)
                continue;

            short glowmaskSlot = RegisterGlowmaskTexture(modItem.Texture + "_Glow");
            AssignGlowmaskTexture_Item(glowmaskSlot, modItem.Type);
            modItem.Item.glowMask = glowmaskSlot;
        }

        foreach (ModNPC modNPC in ModContent.GetContent<ModNPC>())
        {
            if (modNPC.GetType().GetAttribute<AutoloadGlowmask>() != null)
            {
                AssignGlowmaskTexture_NPC(RegisterGlowmaskTexture(modNPC.Texture + "_Glow"), modNPC.Type);
            }
        }

        foreach (ModTile modTile in ModContent.GetContent<ModTile>())
        {
            if (modTile.GetType().GetAttribute<AutoloadGlowmask>() != null)
            {
                Main.tileGlowMask[modTile.Type] = RegisterGlowmaskTexture(modTile.Texture + "_Glow");
            }
        }

        FieldInfo equipTexturesFieldInfo = typeof(EquipLoader).GetField("equipTextures", BindingFlags.Static | BindingFlags.NonPublic);
        Dictionary<EquipType, Dictionary<int, EquipTexture>> equipTextures = (Dictionary<EquipType, Dictionary<int, EquipTexture>>)equipTexturesFieldInfo.GetValue(null);
        foreach (var equipTypeTexturePair in equipTextures)
        {
            EquipType equipType = equipTypeTexturePair.Key;
            foreach (var equipSlotTexturePair in equipTypeTexturePair.Value)
            {
                EquipTexture equipTexture = equipSlotTexturePair.Value;
                if (equipTexture.GetType().GetAttribute<AutoloadGlowmask>() != null || (equipTexture.Item != null && equipTexture.Item.GetType().GetAttribute<AutoloadGlowmask>() != null))
                {
                    AssignGlowmaskTexture_Equip(RegisterGlowmaskTexture(equipTexture.Texture + "_Glow"), equipType, equipSlotTexturePair.Key);
                    if (equipType == EquipType.Body)
                        AssignGlowmaskTexture_Equip_Arms(RegisterGlowmaskTexture(equipTexture.Texture + "_Arms_Glow"), equipSlotTexturePair.Key);
                }
            }
        }
    }

    /// <summary>
    /// Registers a glowmask texture but doesn't assign it to any entity type.
    /// </summary>
    /// <param name="glowmaskTexture">The path to the glowmask texture.</param>
    /// <returns>The slot corresponding to the glowmask.</returns>
    public static short RegisterGlowmaskTexture(string glowmaskTexture)
    {
        if (glowmasks.TryGetValue(glowmaskTexture, out short value))
            return value;

        short slot = nextGlowmask++;
        glowmasks[glowmaskTexture] = slot;

        ModContent.Request<Texture2D>(glowmaskTexture);

        return slot;
    }

    /// <summary>
    /// Assigns a glowmask slot to the given item type.
    /// </summary>
    /// <param name="glowmaskSlot">Slot of the glowmask texture to assign.</param>
    /// <param name="type">Type of the item.</param>
    public static void AssignGlowmaskTexture_Item(short glowmaskSlot, int type) => itemToGlowmask[type] = glowmaskSlot;

    /// <summary>
    /// Assigns a glowmask slot to the given NPC type.
    /// </summary>
    /// <param name="glowmaskSlot">Slot of the glowmask texture to assign.</param>
    /// <param name="type">Type of the NPC.</param>
    public static void AssignGlowmaskTexture_NPC(short glowmaskSlot, int type) => npcToGlowmask[type] = glowmaskSlot;

    /// <summary>
    /// Assigns a glowmask texture to the given equip type and slot.
    /// </summary>
    /// <param name="glowmaskSlot">Slot of the glowmask texture to assign.</param>
    /// <param name="equipType">The <see cref="EquipType"/> of the equip texture.</param>
    /// <param name="equipSlot">The slot of the equip texture.</param>
    public static void AssignGlowmaskTexture_Equip(short glowmaskSlot, EquipType equipType, int equipSlot) => equipToGlowmask[Tuple.Create(equipType, equipSlot)] = glowmaskSlot;

    /// <summary>
    /// Assigns a glowmask texture to the given body slot's arms.
    /// </summary>
    /// <param name="glowmaskSlot">Slot of the glowmask texture to assign.</param>
    /// <param name="equipSlotBody">The slot of the corresponding body equip texture</param>
    public static void AssignGlowmaskTexture_Equip_Arms(short glowmaskSlot, int equipSlotBody) => equipArmsToGlowmask[equipSlotBody] = glowmaskSlot;

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given texture path.
    /// </summary>
    /// <param name="texture">The path to the glowmask texture.</param>
    /// <returns>The slot of the glowmask texture or -1 if not found.</returns>
    public static short GetGlowmaskSlot_Texture(string texture) => glowmasks.TryGetValue(texture, out short slot) ? slot : (short)-1;

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given item type.
    /// </summary>
    /// <param name="type">The item type.</param>
    /// <returns>The slot of the glowmask texture or -1 if not found.</returns>
    public static short GetGlowmaskSlot_Item(int type) => itemToGlowmask.TryGetValue(type, out short slot) ? slot : (short)-1;

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given NPC type.
    /// </summary>
    /// <param name="type">The NPC type.</param>
    /// <returns>The slot of the glowmask texture or -1 if not found.</returns>
    public static short GetGlowmaskSlot_NPC(int type) => npcToGlowmask.TryGetValue(type, out short slot) ? slot : (short)-1;

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given equip type and slot.
    /// </summary>
    /// <param name="equipType">The <see cref="EquipType"/> of the equip texture.</param>
    /// <param name="equipSlot">The slot of the equip texture.</param>
    /// <returns>The slot of the glowmask texture or -1 if not found.</returns>
    public static short GetGlowmaskSlot_Equip(EquipType equipType, int equipSlot)
    {
        Tuple<EquipType, int> key = Tuple.Create(equipType, equipSlot);
        return equipToGlowmask.TryGetValue(key, out short slot) ? slot : (short)-1;
    }

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given body equip's arms.
    /// </summary>
    /// <param name="equipSlotBody">The slot of the corresponding body equip texture.</param>
    /// <returns>The slot of the glowmask texture or -1 if not found.</returns>
    public static short GetGlowmaskSlot_Equip_Arms(int equipSlotBody) => equipArmsToGlowmask.TryGetValue(equipSlotBody, out short slot) ? slot : (short)-1;

    internal static void ResizeAndFillArrays()
    {
        Array.Resize(ref TextureAssets.GlowMask, nextGlowmask);

        foreach (string texture in glowmasks.Keys)
        {
            TextureAssets.GlowMask[glowmasks[texture]] = ModContent.Request<Texture2D>(texture);
        }
    }

    internal static void Unload()
    {
        nextGlowmask = (short)VanillaGlowmaskCount;
        glowmasks.Clear();
        itemToGlowmask.Clear();
        npcToGlowmask.Clear();
        Array.Resize(ref TextureAssets.GlowMask, VanillaGlowmaskCount);
    }
}

internal class GlowmaskLoaderSystem : ModSystem
{
    public override void SetStaticDefaults()
    {
        GlowmaskLoader.Load();
        Mod.Logger.InfoFormat("Loaded {0} glowmasks", GlowmaskLoader.GlowmaskCount - GlowmaskLoader.VanillaGlowmaskCount);
    }

    public override void PostSetupContent()
    {
        GlowmaskLoader.ResizeAndFillArrays();
        Mod.Logger.InfoFormat("Glowmask texture array resized to {0} elements", TextureAssets.GlowMask.Length);
    }

    public override void Unload()
    {
        int loadedGlowmasks = GlowmaskLoader.GlowmaskCount;
        GlowmaskLoader.Unload();
        int unloadedGlowmasks = loadedGlowmasks - GlowmaskLoader.GlowmaskCount;
        int remainingGlowmasks = GlowmaskLoader.GlowmaskCount - GlowmaskLoader.VanillaGlowmaskCount;
        if (remainingGlowmasks > 0)
            Mod.Logger.WarnFormat("{0} glowmasks failed to unload", remainingGlowmasks);
        else
            Mod.Logger.InfoFormat("Successfully unloaded {0} glowmasks", unloadedGlowmasks);
        Mod.Logger.InfoFormat("Glowmask texture array resized to {0} elements", TextureAssets.GlowMask.Length);
    }
}
