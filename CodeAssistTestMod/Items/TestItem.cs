using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CodeAssistTestMod.Items
{
    // Each statement in this class should have a code fix.
    public class TestItem : ModItem
    {
        public override void SetDefaults()
        {
            // Each of these fields should have a suggestion
            item.createTile = 3;
            if (item.type == 6) { }
            if (item.type == 4) { }
            item.shoot = 266;
            item.useStyle = 5; // HoldingOut, not Shoot
            item.rare = 5;

            // Each of these method arguments should have a suggestion
            item.CloneDefaults(1);

            // These values are higher than the 1.3 max,
            // so these shouldn't have a suggestion, if they do the wrong ID dictionary is being queried
            // These should also possibly be shown as a separate warning, to be implemented
            item.createTile = 470;
            if (item.type == 3930) { }
            item.shoot = 714;
            item.useStyle = 6;
        }

        public override bool UseItem(Player player)
        {
            if (Main.netMode == 1) { }
            if (Main.netMode != 1) { }
            if (item.shoot > 13) { }
            NetMessage.SendData(31, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
            NetMessage.SendData(12, -1, -1, null, Main.player[Main.myPlayer].chest, 0f, 0f, 0f, 0, 0, 0);
            NetMessage.SendData(120, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0); // Does not exist in 1.3, no fix expected

            Main.PlaySound(1, 2, 3);
            Main.PlaySound(3, Vector2.Zero, 3);

            Projectile.NewProjectile(1, 2, 3, 4, 13, 14, 1, Main.myPlayer);
            Projectile.NewProjectile(Vector2.Zero, Vector2.Zero, 13, 14, 1, Main.myPlayer);
            Projectile.NewProjectileDirect(Vector2.Zero, Vector2.Zero, 13, 14, 1, Main.myPlayer);

            Dust.NewDust(Vector2.Zero, 1, 2, 3, 4, 5, 6, Color.Red, 7);
            Dust.NewDustDirect(Vector2.Zero, 1, 2, 3, 4, 5, 6, Color.Red, 7);
            Dust.NewDustPerfect(Vector2.Zero, 1, null, 6, Color.Red, 7);
            Dust.NewDust(Vector2.Zero, 1, 2, 6, 4, 5, 6, Color.Red, 7); // Fire, not Torch

            Dust.NewDust(Vector2.Zero, 1, 2, 50, 4, 5, 6, Color.Red, 7);

            if (Main.rand.Next(5) == 6) { }
            if (Main.rand.Next(6) == 11) { }
            var r = Main.rand; if(r.Next(5) == 10) { }

            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddTile(32);
            recipe.AddIngredient(12);
            recipe.SetResult(12);
            recipe.AddRecipe();
        }
    }
}