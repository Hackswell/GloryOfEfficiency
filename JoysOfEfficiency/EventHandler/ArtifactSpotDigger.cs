using JoysOfEfficiency.Core;
using JoysOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace JoysOfEfficiency.EventHandler
{
    internal class ArtifactSpotDigger
    {
        private static Config Config => InstanceHolder.Config;
        private static readonly Logger Logger = new Logger("ArtifactSpotDigger");

        public static void DigNearbyArtifactSpots()
        {
            Farmer player = Game1.player;
            int radius = Config.AutoDigRadius;
            Hoe hoe = Util.FindToolFromInventory<Hoe>(player, InstanceHolder.Config.FindHoeFromInventory);
            GameLocation location = player.currentLocation;
            if (hoe == null)
            {
                return;
            }

            bool flag = false;
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    Point currPos = player.TilePoint;
                    int x = currPos.X + i;
                    int y = currPos.Y + j;
                    Vector2 loc = new Vector2(x, y);
                    if (!location.Objects.ContainsKey(loc) || location.isTileHoeDirt(loc))
                    {
                        continue;
                    }

                    if (location.Objects[loc].name == "Artifact Spot")
                    {
                        Logger.Log($"ArtifactSpot: {location.Objects[loc].name} at [{loc.X},{loc.Y}]");
                        location.digUpArtifactSpot(x, y, player);
                        location.Objects.Remove(loc);
                        location.terrainFeatures.Add(loc, new HoeDirt());
                        flag = true;
                    }
                    else if (location.Objects[loc].name == "Seed Spot")
                    {
                        Logger.Log($"SeedSpot: {location.Objects[loc].name} at [{loc.X},{loc.Y}]");
//                        location.performUseAction(location);
//                        location.performToolAction(hoe, x, y);
//                        hoe.DoFunction(location, x, y, 1, player);
//                        location.Objects.Remove(loc);
//                        location.terrainFeatures.Add(loc, new HoeDirt());
                    }
                }
            }

            if (flag)
            {
                Game1.playSound("hoeHit");
            }
        }

    }
}
