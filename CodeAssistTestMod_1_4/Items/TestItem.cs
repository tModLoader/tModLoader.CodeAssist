using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CodeAssistTestMod_1_4.Items
{
	public class TestItem : ModItem
	{
		public override void SetDefaults()
		{
            // Each of these fields should have a suggestion
            Item.createTile = 3;
            if (Item.type == 6) { }
            if (Item.type == 4) { }
            Item.shoot = 266;
            Item.useStyle = 5; // Shoot, not HoldingOut
            Item.rare = 5;

            // Each of these method arguments should have a suggestion
            Item.CloneDefaults(1);
        }

        public override bool? UseItem(Player player)
        {
            if (Main.netMode == 1) { }
            if (Main.netMode != 1) { }
            if (Item.shoot > 13) { }
            NetMessage.SendData(31, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
            NetMessage.SendData(12, -1, -1, null, Main.player[Main.myPlayer].chest, 0f, 0f, 0f, 0, 0, 0);
            NetMessage.SendData(120, -1, -1, null, 0, 0f, 0f, 0f, 0, 0, 0); // Does exist in 1.4, fix expected

            Terraria.Audio.SoundEngine.PlaySound(1, 2, 3);
            Terraria.Audio.SoundEngine.PlaySound(3, Vector2.Zero, 3);

            Projectile.NewProjectile(null, 1, 2, 3, 4, 13, 14, 1, Main.myPlayer);
            Projectile.NewProjectile(null, Vector2.Zero, Vector2.Zero, 13, 14, 1, Main.myPlayer);
            Projectile.NewProjectileDirect(null, Vector2.Zero, Vector2.Zero, 13, 14, 1, Main.myPlayer);

            Dust.NewDust(Vector2.Zero, 1, 2, 3, 4, 5, 6, Color.Red, 7);
            Dust.NewDustDirect(Vector2.Zero, 1, 2, 3, 4, 5, 6, Color.Red, 7);
            Dust.NewDustPerfect(Vector2.Zero, 1, null, 6, Color.Red, 7);
            Dust.NewDust(Vector2.Zero, 1, 2, 6, 4, 5, 6, Color.Red, 7); // Torch, not Fire

            Dust.NewDust(Vector2.Zero, 1, 2, DustID.Adamantite, 4, 5, 6, Color.Red, 7);

            if (Main.rand.Next(5) == 6) { }
            if (Main.rand.Next(6) == 11) { }
            var r = Main.rand; if (r.Next(5 + 1) == 10) { }
            bool a = false;
            if (r.Next(a ? 1 : 4) == 34) { }
            if (r.Next(a ? 1 : 6) != 22) { }

            return base.UseItem(player);
        }

        public override void AddRecipes()
		{
			Recipe recipe = Mod.CreateRecipe(12);
			recipe.AddTile(32);
			recipe.AddIngredient(12);
			recipe.Register();
		}
	}
}