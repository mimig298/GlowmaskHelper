using System;

namespace GlowmaskHelper.Content;

/// <summary>
/// This attribute annotates a <see cref="Terraria.ModLoader.ModItem"/> or <see cref="Terraria.ModLoader.ModNPC"/> class to indicate that the game should autoload a glowmask texture for this entity.
/// <para/> The glowmask texture will be autoloaded from the regular texture + "_Glow". An error will be thrown if this texture is not found.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoloadGlowmask : Attribute
{
}
