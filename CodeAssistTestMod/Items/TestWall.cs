using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CodeAssistTestMod.Items
{
    class TestWall : ModWall
    {
        public override void SetDefaults()
        {
            dustType = 35;
        }
    }
}
