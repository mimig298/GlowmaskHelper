using System;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

/// <summary>
/// This attribute annotates a ModType to indicate that the game should autoload a glowmask texture for that entity.<br/>
/// The glowmask texture will be autoloaded from the regular texture + "_Glow". An error will be thrown if this texture is not found.
/// <para/> Currently supports adding glowmasks to <see cref="ModItem"/>s, <see cref="ModNPC"/>s, <see cref="ModTile"/>s, <see cref="ModWall"/>s, and <see cref="EquipTexture"/>s.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoloadGlowmask : Attribute
{
}
