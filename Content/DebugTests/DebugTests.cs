using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content.DebugTests;

[AutoloadGlowmask]
public class TestItem : ModItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return ModContent.GetInstance<Configs>().LoadDebugTests;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
    }
}

[AutoloadGlowmask]
public class TestNPC : ModNPC
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return ModContent.GetInstance<Configs>().LoadDebugTests;
    }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults()
    {
        NPC.height = 20;
        NPC.width = 20;
        NPC.lifeMax = 100;
        NPC.aiStyle = NPCAIStyleID.Passive;
    }

    public override void FindFrame(int frameHeight)
    {
        if (++NPC.frameCounter >= 5)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[Type] * frameHeight);
        }
    }
}

public class TestProjectile : ModProjectile
{
    public override string GlowTexture => base.GlowTexture;

    public override bool IsLoadingEnabled(Mod mod)
    {
        return ModContent.GetInstance<Configs>().LoadDebugTests;
    }

    public override void SetDefaults()
    {
        Projectile.height = 20;
        Projectile.width = 20;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
    }
}

public class SpawnDebugs : ModCommand
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return ModContent.GetInstance<Configs>().LoadDebugTests;
    }

    public override CommandType Type => CommandType.World;

    public override string Command => "SpawnDebugs";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        Player player = caller.Player;
        NPC.NewNPC(Entity.GetSource_None(), (int)player.position.X, (int)player.position.Y, ModContent.NPCType<TestNPC>());
        Item.NewItem(Entity.GetSource_None(), player.position, ModContent.ItemType<TestItem>());
        caller.Reply("Here you go!", Microsoft.Xna.Framework.Color.LightGreen);
    }
}
