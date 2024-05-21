using JoysOfEfficiency.Core;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using xTile.Layers;

namespace JoysOfEfficiency.Automation
{
    internal class TrashCanScavenger
    {
        private static Config Config => InstanceHolder.Config;

        public static void ScavengeTrashCan()
        {
            if (!(Game1.currentLocation is Town town))
            {
                return;
            }

            Farmer player = Game1.player;
            int radius = InstanceHolder.Config.BalancedMode ? 1 : InstanceHolder.Config.ScavengingRadius;
            Layer layer = Game1.currentLocation.Map.GetLayer("Buildings");
            Point currPos = player.TilePoint;
            int ox = currPos.X;
            int oy = currPos.Y;
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = ox + dx, y = oy + dy;

                    if (layer.Tiles[x, y]?.TileIndex == 78)
                    {
                        string whichGarbage = Game1.currentLocation.doesTileHaveProperty(x, y, "Action", "Buildings");
                        town.CheckGarbage(whichGarbage, new Vector2(x, y), Game1.player, true, Config.GarbageDisgustsNPCs);
                    }
                }
            }
        }
    }
}
