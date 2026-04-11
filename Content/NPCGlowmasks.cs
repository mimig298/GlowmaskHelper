using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class NPCGlowmasks : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return GlowmaskLoader.GetGlowmaskSlot_NPC(entity.type) > 0;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Asset<Texture2D> originalTexture = TextureAssets.Npc[npc.type];
        Asset<Texture2D> glowmask = TextureAssets.GlowMask[GlowmaskLoader.GetGlowmaskSlot_NPC(npc.type)];
        Vector2 origin = new(originalTexture.Width() / 2, originalTexture.Height() / Main.npcFrameCount[npc.type] / 2);
        SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 drawPos = new(npc.position.X - screenPos.X + (npc.width / 2) - originalTexture.Width() * npc.scale / 2f + origin.X * npc.scale, npc.position.Y - screenPos.Y + npc.height - originalTexture.Height() * npc.scale / Main.npcFrameCount[npc.type] + origin.Y * npc.scale);

        drawPos.Y += Main.NPCAddHeight(npc) + 4 + npc.gfxOffY;

        spriteBatch.Draw(glowmask.Value, drawPos, npc.frame, Color.White, npc.rotation, origin, npc.scale, spriteEffects, 0f);
    }
}
