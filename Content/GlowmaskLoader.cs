using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

/// <summary>
/// This class serves as the central place to store and register custom glowmasks, and can be used to access information about them.<br/>
/// Using an <see cref="AutoloadGlowmask"/> attribute in your classes will do all the special logic for you, so you usually don't need to use this class if that's the case.
/// <para/> Currently supports adding glowmasks to <see cref="ModItem"/>s, <see cref="ModNPC"/>s, <see cref="ModTile"/>s, <see cref="ModWall"/>s, and <see cref="EquipTexture"/>s.
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
    internal static IDictionary<int, short> wallToGlowmask = new Dictionary<int, short>();

    public static int GlowmaskCount => nextGlowmask;

    internal static void Load()
    {
        VanillaGlowmaskCount = TextureAssets.GlowMask.Length;
        nextGlowmask = (short)VanillaGlowmaskCount;

        // Register glowmasks queued by mods
        foreach (string queuedGlowmask in glowmasks.Keys)
        {
            RegisterGlowmaskTexture(queuedGlowmask);
        }

        // Register autoloaded glowmasks using the AutoloadGlowmask attribute.
        foreach (ModItem modItem in ModContent.GetContent<ModItem>())
        {
            if (modItem.GetType().GetAttribute<AutoloadGlowmask>() == null)
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

        foreach (ModWall modWall in ModContent.GetContent<ModWall>())
        {
            if (modWall.GetType().GetAttribute<AutoloadGlowmask>() != null)
            {
                AssignGlowmaskTexture_Wall(RegisterGlowmaskTexture(modWall.Texture + "_Glow"), modWall.Type);
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
                    short glowmaskSlot = RegisterGlowmaskTexture(equipTexture.Texture + "_Glow");
                    AssignGlowmaskTexture_Equip(glowmaskSlot, equipType, equipSlotTexturePair.Key);
                    if (equipType == EquipType.Body)
                        AssignGlowmaskTexture_Equip_Arms(glowmaskSlot, equipSlotTexturePair.Key);
                }
            }
        }

        ResizeAndFillArrays();
    }

    // Hello person reading this! Are you interested in understanding why RegisterGlowmaskTexture is private and modders have to queue the glowmask?
    // It's a bit complicated but I am here to help!
    // Glowmask Helper was designed to work as closely as possible to how regular TML autoloading works.
    // The main inspiration were the AutoloadEquip and AutoloadNPCHead attributes, hence the AutoloadGlowmask attribute.
    // Now there is just one issue! TML's system is, well, TML's. It's integrated very deeply into the loading code for items and NPCs, in a sealed Register method and not calling any hook.
    // There isn't even the option of detouring or IL edits since TML was made to mod Terraria, not itself, and as such doesn't provide any hooks for its own loading code.
    // Because of that, there's the big block of code above in Load() that iterates every single ModItem, ModNPC and so on.
    // Now that isn't that bad. HOWEVER! Modders should be able to register and assign glowmasks on their own, without having to use the AutoloadGlowmask attribute.
    // Versatility! Usability! Accessibility! This is a library mod! This is what Glowmask Helper is all about! What kind of library mod has hardcoded, unadaptable logic??
    // For that there's the RegisterGlowmaskTexture and AssignGlowmaskTexture methods. They are* public and can be used by modders to register glowmasks and assign them to entities wherever they want.
    // But... uhhh... *when* do modders call them? Load()? Too early, glowmasks aren't registered yet! SetStaticDefaults()? Too late, array already resized!
    // The original solution I worked on was to resize the arrays in PostSetupContent() instead, but that raises a few issues:
    // 1. Mods should already have access to every correct value in SetStaticDefaults(), and this forces them to wait until PostSetupContent() and rewrite any crucial logic they may have.
    // 2. If another mod resizes the TextureAssets.GlowMask array between Glowmask Helper reading its lenght and resizing it, everything will break.
    // Ok... so resize the arrays in SetStaticDefaults() and have modders call RegisterGlowmaskTexture in Load()?
    // That doesn't work either, since that opens up an interval between Load() and SetStaticDefaults() where the glowmask array could be resized by another mod.
    // This is tricky... Reading the lenght of the glowmasks array and resizing it *must* happen in the same method, but doing so makes modders unable to register glowmasks manually...
    // That's where the queueing system comes in. In Load(), modders tell Glowmask Helper that they want to register a glowmask texture.
    // Unfortunately, they won't be able to get the glowmask slot right away since we can't read the lenght of the glowmask array yet, but they can always call GetGlowmaskSlot_Texture() later to get it.
    // Then, in SetStaticDefaults(), Glowmask Helper will process the queue and register all glowmasks that were queued in Load().
    // This way, by the start of any dependent mod's SetStaticDefaults(), all glowmasks will be registered and the glowmask array will be resized to the correct lenght.

    /// <summary>
    /// Queues a glowmask texture for registration. This doesn't assign it to any entity type or give you the glowmask slot immediately.<br/>
    /// The registration will be processed before your mod's <c>SetStaticDefaults()</c> hooks.<br/>
    /// To get the glowmask slot after registration, call <see cref="GetGlowmaskSlot_Texture(string)"/> with the same texture you provided here.
    /// <para/><strong>Only call this during a <c>Load()</c> hook.</strong>
    /// </summary>
    /// <param name="glowmaskTexture">The path to the glowmask texture.</param>
    public static void QueueGlowmaskRegistration(string glowmaskTexture)
    {
        if (!((GlowmaskHelper)ModLoader.GetMod("GlowmaskHelper")).IsLoading)
        {
            throw new Exception("QueueGlowmaskRegistration must be called during the Mod.Load stage.");
        }

        glowmasks[glowmaskTexture] = -1;
    }

    /// <summary>
    /// Registers a glowmask texture but doesn't assign it to any entity type.
    /// </summary>
    /// <param name="glowmaskTexture">The path to the glowmask texture.</param>
    /// <returns>The slot corresponding to the glowmask.</returns>
    private static short RegisterGlowmaskTexture(string glowmaskTexture)
    {
        if (glowmasks.TryGetValue(glowmaskTexture, out short value) && value >= 0)
        {
            return value; 
            // Already registered, return the existing slot
            // If the glowmask is queued but not registered yet, we need to update it
        }

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
    /// Assigns a glowmask slot to the given wall type.
    /// </summary>
    /// <param name="glowmaskSlot">Slot of the glowmask texture to assign.</param>
    /// <param name="wallType">Type of the wall.</param>
    public static void AssignGlowmaskTexture_Wall(short glowmaskSlot, int wallType) => wallToGlowmask[wallType] = glowmaskSlot;

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

    /// <summary>
    /// Gets the index of the glowmask texture corresponding to the given wall type.
    /// </summary>
    /// <param name="wallType">The wall type.</param>
    /// <returns>The slot of the glowmask texture or -1 if not found.</returns>
    public static short GetGlowmaskSlot_Wall(int wallType) => wallToGlowmask.TryGetValue(wallType, out short slot) ? slot : (short)-1;

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
        ((GlowmaskHelper)Mod).IsLoading = false;
        GlowmaskLoader.Load();
        Mod.Logger.InfoFormat("Loaded {0} glowmasks", GlowmaskLoader.GlowmaskCount - GlowmaskLoader.VanillaGlowmaskCount);
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
