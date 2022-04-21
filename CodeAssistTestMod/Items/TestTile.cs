using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CodeAssistTestMod.Items
{
    class TestTile : ModTile
    {
        public override void SetDefaults()
        {
            dustType = 13;
        }
    }
}
