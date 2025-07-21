using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

/// <summary>
/// This class serves as the central place to store and register custom glowmasks, and can be used to access information about them.<br/>
/// Using an <see cref="AutoloadGlowmask"/> attribute in your classes will do all the special logic for you, so you usually don't need to use this class if that's the case.
/// <para/> Currently only supports adding glowmasks to <see cref="Item"/>s<see/> and <see cref="NPC"/>s<see/>.
/// </summary>
public static class GlowmaskLoader
{
    // Do NOT ever change this value unless you're inside GlowmaskLoader.Load()
    public static int VanillaGlowmaskCount { get; private set; }

    private static short nextGlowmask = (short)TextureAssets.GlowMask.Length;

    internal static IDictionary<string, short> glowmasks = new Dictionary<string, short>();
    internal static IDictionary<int, short> itemToGlowmask = new Dictionary<int, short>();
    internal static IDictionary<int, short> npcToGlowmask = new Dictionary<int, short>();

    public static int GlowmaskCount => nextGlowmask;

    internal static void Load()
    {
        VanillaGlowmaskCount = TextureAssets.GlowMask.Length;
        nextGlowmask = (short)VanillaGlowmaskCount;

        foreach (ModItem modItem in ModContent.GetContent<ModItem>())
        {
            if (modItem.GetType().GetAttribute<AutoloadGlowmask>() != null)
            {
                modItem.Item.glowMask = AddGlowmaskTexture(modItem.Item.type, modItem.Texture + "_Glow", typeof(Item));
            }
        }
        foreach (ModNPC modNPC in ModContent.GetContent<ModNPC>())
        {
            if (modNPC.GetType().GetAttribute<AutoloadGlowmask>() != null)
            {
                AddGlowmaskTexture(modNPC.NPC.type, modNPC.Texture + "_Glow", typeof(NPC));
            }
        }

        ResizeAndFillArrays();
    }

    private static short RegisterAndCheckGlowmaskTexture(string glowmaskTexture)
    {
        if (glowmasks.TryGetValue(glowmaskTexture, out short value))
            return value;

        short slot = nextGlowmask++;
        glowmasks[glowmaskTexture] = slot;

        if (!Main.dedServ)
            ModContent.Request<Texture2D>(glowmaskTexture);

        return slot;
    }

    /// <summary>
    /// Assigns a glowmask texture to the given entity type if possible.
    /// </summary>
    /// <param name="type">Type of the entity.</param>
    /// <param name="glowmaskTexture">The glowmask texture path.</param>
    /// <param name="entityClass">The class the entity belongs to.</param>
    /// <returns>The glowmask texture slot or -1 if adding the glowmask texture failed.</returns>
    public static short AddGlowmaskTexture(int type, string glowmaskTexture, Type entityClass)
    {
        short slot = RegisterAndCheckGlowmaskTexture(glowmaskTexture);
        if (entityClass == typeof(Item))
            itemToGlowmask[type] = slot;
        else if (entityClass == typeof(NPC))
            npcToGlowmask[type] = slot;
        else
            slot = -1;
        return slot;
    }

    /// <summary>
    /// Assigns a glowmask texture to the given entity if possible.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="glowmaskTexture">The glowmask texture path.</param>
    /// <returns>The glowmask texture slot. Returns -1 if the entity isn't supported by the mod.</returns>
    public static short AddGlowmaskTexture(Entity entity, string glowmaskTexture)
    {
        if (entity is Item item)
            return AddGlowmaskTexture(item.type, glowmaskTexture, typeof(Item));
        else if (entity is NPC npc)
            return AddGlowmaskTexture(npc.type, glowmaskTexture, typeof(NPC));
        return -1;
    }

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given texture path.
    /// </summary>
    /// <param name="texture">The path to the glowmask texture.</param>
    /// <returns>The slot of the glowmask texture, -1 if not found.</returns>
    public static short GetGlowmaskSlot(string texture) => glowmasks.TryGetValue(texture, out short slot) ? slot : (short)-1;

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given entity's type.
    /// </summary>
    /// <param name="type">The type of the entity.</param>
    /// <param name="entityClass">The class the entity belongs to.</param>
    /// <returns>The slot of the glowmask texture, -1 if not found.</returns>
    public static short GetGlowmaskSlot(int type, Type entityClass)
    {
        short slot = -1;
        if (entityClass == typeof(Item))
            itemToGlowmask.TryGetValue(type, out slot);
        else if (entityClass == typeof(NPC))
            npcToGlowmask.TryGetValue(type, out slot);
        return slot;
    }

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given entity's type.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The slot of the glowmask texture, -1 if not found.</returns>
    public static short GetGlowmaskSlot(Entity entity)
    {
        if (entity is Item item)
            return GetGlowmaskSlot(item.type, typeof(Item));
        if (entity is NPC npc)
            return GetGlowmaskSlot(npc.type, typeof(NPC));
        return -1; // Not found
    }

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
    }
}
