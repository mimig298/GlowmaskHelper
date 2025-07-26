using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class NPCGlowmasks : GlobalNPC
{
    public override void Load()
    {
        On_Main.DrawNPCDirect_Inner += DrawGlowmask;
    }

    private static void DrawGlowmask(On_Main.orig_DrawNPCDirect_Inner orig, Main self, SpriteBatch mySpriteBatch, NPC rCurrentNPC, bool behindTiles, Vector2 screenPos, ref Color npcColor)
    {
        orig(self, mySpriteBatch, rCurrentNPC, behindTiles, screenPos, ref npcColor);

        if (rCurrentNPC.ModNPC?.GetType().GetAttribute<AutoloadGlowmask>() == null)
            return;

        Asset<Texture2D> originalTexture = TextureAssets.Npc[rCurrentNPC.type];
        Asset<Texture2D> glowmask = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_NPC(rCurrentNPC.type)];
        Vector2 halfSize = rCurrentNPC.frame.Center();
        SpriteEffects spriteEffects = rCurrentNPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 drawPos = new(rCurrentNPC.position.X - screenPos.X + (rCurrentNPC.width / 2) - originalTexture.Width() * rCurrentNPC.scale / 2f + halfSize.X * rCurrentNPC.scale, rCurrentNPC.position.Y - screenPos.Y + rCurrentNPC.height - originalTexture.Height() * rCurrentNPC.scale / (float)Main.npcFrameCount[rCurrentNPC.type] + halfSize.Y * rCurrentNPC.scale);
        drawPos.Y += Main.NPCAddHeight(rCurrentNPC) + 4 + rCurrentNPC.gfxOffY;

        mySpriteBatch.Draw(glowmask.Value, drawPos, rCurrentNPC.frame, Color.White, rCurrentNPC.rotation, halfSize, rCurrentNPC.scale, spriteEffects, 0f);
    }
}
