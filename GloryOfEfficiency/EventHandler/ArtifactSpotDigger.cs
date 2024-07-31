using GloryOfEfficiency.Core;
using GloryOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace GloryOfEfficiency.EventHandler
{
    internal class ArtifactSpotDigger
    {
        private static Config Config => InstanceHolder.Config;
        private static readonly Logger Logger = new Logger("ArtifactSpotDigger");
        private static Farmer player = Game1.player;

        public static void DigNearbyArtifactSpots()
        {
            int radius = Config.AutoDigRadius;
            Hoe hoe = Util.FindToolFromInventory<Hoe>(player, InstanceHolder.Config.FindHoeFromInventory);
            if (hoe == null)
            {
                return;
            }

            bool flag = false;
            GameLocation location = player.currentLocation;
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

                    string spotName = location.Objects[loc].name;
                    if (spotName == "Seed Spot" || spotName == "Artifact Spot")
                    {
                        Logger.Log($"{spotName}: {location.Objects[loc].name} at [{loc.X},{loc.Y}]");

                        // Hackswell: location.digUpArtifactSpot() works fine... except with Farm Type Manager Mod.
                        //      Just easier to do everything the following way... =shurg=
                        //
                        // Liberated this from Pathoschild's "TractorMod"
                        player.lastClick = (loc * Game1.tileSize) + new Vector2(Game1.tileSize / 2f);
                        hoe.swingTicker++;
                        hoe.DoFunction(location, (int)player.lastClick.X, (int)player.lastClick.Y, 0, player);
                        flag = true;
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
