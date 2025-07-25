using System;
using System.Collections.Generic;
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
                if (TryAddGlowmaskTexture(modItem.Texture + "_Glow", modItem.Item.type, typeof(Item), out short glowmaskSlot))
                    modItem.Item.glowMask = glowmaskSlot;
            }
        }
        foreach (ModNPC modNPC in ModContent.GetContent<ModNPC>())
        {
            if (modNPC.GetType().GetAttribute<AutoloadGlowmask>() != null)
            {
                TryAddGlowmaskTexture(modNPC.Texture + "_Glow", modNPC.NPC.type, typeof(NPC), out _);
            }
        }
        foreach (ModTile modTile in ModContent.GetContent<ModTile>())
        {
            if (modTile.GetType().GetAttribute<AutoloadGlowmask>() != null)
            {
                TryAddGlowmaskTexture(modTile.Texture + "_Glow", modTile.Type, typeof(Tile), out _);
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

        ModContent.Request<Texture2D>(glowmaskTexture, AssetRequestMode.DoNotLoad);

        return slot;
    }

    /// <summary>
    /// Assigns a glowmask slot to the given entity type if possible.
    /// </summary>
    /// <param name="glowmaskSlot">Slot of the glowmask texture to assign.</param>
    /// <param name="type">Type of the entity.</param>
    /// <param name="entityClass">The class the entity belongs to.</param>
    /// <returns><see langword="true"/> the glowmask was assigned successfully.</returns>
    public static bool AssignGlowmaskTexture(short glowmaskSlot, int type, Type entityClass)
    {
        if (entityClass == typeof(Item) || entityClass == typeof(ModItem))
            itemToGlowmask[type] = glowmaskSlot;
        else if (entityClass == typeof(NPC) || entityClass == typeof(ModNPC))
            npcToGlowmask[type] = glowmaskSlot;
        else if (entityClass == typeof(Tile) || entityClass == typeof(ModTile))
            Main.tileGlowMask[type] = glowmaskSlot;
        else
            return false; // Not supported
        return true;
    }

    /// <summary>
    /// Assigns a glowmask slot to the given entity if possible.
    /// </summary>
    /// <param name="glowmaskSlot">Slot of the glowmask texture to assign.</param>
    /// <param name="entity">The entity.</param>
    /// <returns><see langword="true"/> the glowmask was assigned successfully.</returns>
    public static bool AssignGlowmaskTexture(short glowmaskSlot, Entity entity)
    {
        if (entity is Item item)
            return AssignGlowmaskTexture(glowmaskSlot, item.type, typeof(Item));
        else if (entity is NPC npc)
            return AssignGlowmaskTexture(glowmaskSlot, npc.type, typeof(NPC));
        return false; // Not supported
    }

    /// <summary>
    /// Assigns a glowmask slot to the given mod type if possible.
    /// </summary>
    /// <param name="glowmaskSlot">Slot of the glowmask texture to assign.</param>
    /// <param name="modType">The mod type.</param>
    /// <returns><see langword="true"/> the glowmask was assigned successfully.</returns>
    public static bool AssignGlowmaskTexture(short glowmaskSlot, ModType modType)
    {
        if (modType is ModItem modItem)
            return AssignGlowmaskTexture(glowmaskSlot, modItem.Type, typeof(Item));
        else if (modType is ModNPC modNPC)
            return AssignGlowmaskTexture(glowmaskSlot, modNPC.Type, typeof(NPC));
        else if (modType is ModTile modTile)
            return AssignGlowmaskTexture(glowmaskSlot, modTile.Type, typeof(Tile));
        return false; // Not supported
    }

    /// <summary>
    /// Registers and assigns a glowmask texture to the given entity type if possible.
    /// </summary>
    /// <param name="type">The type of the entity.</param>
    /// <param name="glowmaskTexture">The path to the glowmask texture.</param>
    /// <param name="entityClass">The type the entity belongs to.</param>
    /// <param name="glowmaskSlot">The slot of the glowmask texture.</param>
    /// <returns><see langword="true"/> the glowmask was assigned successfully.</returns>
    public static bool TryAddGlowmaskTexture(string glowmaskTexture, int type, Type entityClass, out short glowmaskSlot)
    {
        glowmaskSlot = RegisterGlowmaskTexture(glowmaskTexture);
        if (AssignGlowmaskTexture(glowmaskSlot, type, entityClass))
            return true;
        else
            return false; // Not supported
    }

    public static bool TryAddGlowmaskTexture(string glowmaskTexture, Entity entity, out short glowmaskSlot)
    {
        glowmaskSlot = RegisterGlowmaskTexture(glowmaskTexture);
        if (AssignGlowmaskTexture(glowmaskSlot, entity))
            return true;
        else
            return false; // Not supported
    }

    public static bool TryAddGlowmaskTexture(string glowmaskTexture, ModType modType, out short glowmaskSlot)
    {
        glowmaskSlot = RegisterGlowmaskTexture(glowmaskTexture);
        if (AssignGlowmaskTexture(glowmaskSlot, modType))
            return true;
        else
            return false; // Not supported
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
        if (entityClass == typeof(Item) || entityClass == typeof(ModItem))
            itemToGlowmask.TryGetValue(type, out slot);
        else if (entityClass == typeof(NPC) || entityClass == typeof(ModNPC))
            npcToGlowmask.TryGetValue(type, out slot);
        else if (entityClass == typeof(Tile) || entityClass == typeof(ModTile))
            slot = Main.tileGlowMask[type];
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
        return -1; // Not supported
    }

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the mod type's entity.
    /// </summary>
    /// <param name="modType">The mod type.</param>
    /// <returns>The slot of the glowmask texture, -1 if not found.</returns>
    public static short GetGlowmaskSlot(ModType modType)
    {
        if (modType is ModItem modItem)
            return GetGlowmaskSlot(modItem.Type, typeof(Item));
        if (modType is ModNPC modNPC)
            return GetGlowmaskSlot(modNPC.Type, typeof(NPC));
        if (modType is ModTile modTile)
            return GetGlowmaskSlot(modTile.Type, typeof(Tile));
        return -1; // Not supported
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
