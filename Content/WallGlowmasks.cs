using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace GlowmaskHelper.Content;

internal class WallGlowmasks : GlobalWall
{
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        short glowmaskSlot = GlowmaskLoader.GetGlowmaskSlot_Wall(type);
        if (glowmaskSlot == -1)
            return;

        // Most of this is copied from GameContent.Drawing.WallDrawing.DrawWalls()

        VertexColors vertices = new(Color.White);
        Rectangle rectangle = new(0, 0, 32, 32);

        Tile tile = Framing.GetTileSafely(i, j);

        rectangle.X = tile.WallFrameX;
        rectangle.Y = tile.WallFrameY + Main.wallFrame[type] * 180;
        Vector2 drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X - 8, j * 16 - (int)Main.screenPosition.Y - 8) + new Vector2(Main.offScreenRange);

        Asset<Texture2D> glowmaskTexture = TextureAssets.GlowMask[glowmaskSlot];

        if (Lighting.NotRetro && !Main.wallLight[type] && !WorldGen.SolidTile(tile))
        {
            Main.tileBatch.Draw(glowmaskTexture.Value, drawPos, rectangle, vertices, Vector2.Zero, 1f, SpriteEffects.None);
        }
        else
        {
            spriteBatch.Draw(glowmaskTexture.Value, drawPos, rectangle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}
