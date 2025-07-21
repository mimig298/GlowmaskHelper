using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace GlowmaskHelper.Content;

public class Configs : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Header("Debug")]
    [DefaultValue(false)]
    [ReloadRequired]
    public bool LoadDebugTests;
}
