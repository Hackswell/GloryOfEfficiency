using GloryOfEfficiency.Core;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using xTile.Layers;

namespace GloryOfEfficiency.Automation
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
            GameLocation currLocation = Game1.currentLocation;
            int radius = InstanceHolder.Config.BalancedMode ? 1 : InstanceHolder.Config.ScavengingRadius;
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = player.TilePoint.X + dx;
                    int y = player.TilePoint.Y + dy;
                    if (currLocation.doesTileHaveProperty(x, y, "Action", "Buildings").StartsWith("Garbage"))
                    {
                        string whichGarbage = currLocation.doesTileHaveProperty(x, y, "Action", "Buildings").Split(' ')[1];
                        town.CheckGarbage(whichGarbage, new Vector2(x, y), Game1.player, true, Config.GarbageDisgustsNPCs);
                    }
                }
            }
        }
    }
}
