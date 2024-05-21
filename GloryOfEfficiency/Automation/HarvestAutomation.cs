using System;
using System.Collections.Generic;
using System.Linq;
using GloryOfEfficiency.Core;
using GloryOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SVObject = StardewValley.Object;

namespace GloryOfEfficiency.Automation
{
    internal class HarvestAutomation
    {
        private static Config Config => InstanceHolder.Config;

        private static readonly Logger Logger = new Logger("HarvestAutomation");

        private static readonly List<Vector2> FlowerLocationProducingNectar = new List<Vector2>();
        public static void UpdateNectarInfo()
        {
            FlowerLocationProducingNectar.Clear();
            foreach (KeyValuePair<Vector2, SVObject> kv in
                Game1.currentLocation.Objects.Pairs.Where(
                    pair => pair.Value.Name == "Bee House")
                )
            {
                Vector2 houseLoc = kv.Key;
                foreach (Vector2 flowerLoc in GetAreaOfCollectingNectar(houseLoc))
                {
                    if ((int)flowerLoc.X >= 0 && (int)flowerLoc.Y >= 0 && !FlowerLocationProducingNectar.Contains(flowerLoc))
                    {
                        FlowerLocationProducingNectar.Add(flowerLoc);
                    }
                }
            }
        }

        public static void HarvestNearbyCrops(Farmer player)
        {
            GameLocation location = player.currentLocation;
            int radius = Config.AutoHarvestRadius;

            if (Config.ProtectNectarProducingFlower)
            {
                UpdateNectarInfo();
            }

            foreach (KeyValuePair<Vector2, HoeDirt> kv in Util.GetFeaturesWithin<HoeDirt>(radius))
            {
                Vector2 loc = kv.Key;
                HoeDirt dirt = kv.Value;
                if (dirt.crop != null)   Logger.Log($"Crop: {dirt.crop.indexOfHarvest} ** {dirt.crop.whichForageCrop}");
                if (dirt.crop == null || !dirt.readyForHarvest())
                {
                    continue;
                }

                if (IsBlackListed(dirt.crop) || Config.ProtectNectarProducingFlower && IsProducingNectar(loc))
                {
                    continue;
                }

                if (!dirt.crop.harvest((int) loc.X, (int) loc.Y, dirt))
                {
                    continue;
                }

                if (dirt.crop.RegrowsAfterHarvest() == false || dirt.crop.forageCrop.Value)
                {
                    dirt.destroyCrop(true);
                }
            }
            foreach (IndoorPot pot in Util.GetObjectsWithin<IndoorPot>(radius))
            {
                HoeDirt dirt = pot.hoeDirt.Value;
                if (dirt?.crop == null || !dirt.readyForHarvest())
                {
                    continue;
                }

                if (!dirt.crop.harvest((int)pot.TileLocation.X, (int)pot.TileLocation.Y, dirt))
                {
                    continue;
                }

                if (dirt.crop.RegrowsAfterHarvest() == false || dirt.crop.forageCrop.Value)
                {
                    dirt.destroyCrop(true);
                }
            }
        }

        public static void HarvestNearbySlimeBalls(Farmer player)
        {
            int radius = Config.AutoHarvestRadius;

            foreach (SVObject obj in Util.GetObjectsWithin<SVObject>(radius).Where(obj => obj.QualifiedItemId == "(BC)56"))
            {
                obj.checkForAction(player);
            }
        }

        public static void WaterNearbyCrops()
        {
            WateringCan can = Util.FindToolFromInventory<WateringCan>(Game1.player, InstanceHolder.Config.FindCanFromInventory);
            if (can == null)
                return;

            bool watered = false;
            foreach (KeyValuePair<Vector2, HoeDirt> kv in Util.GetFeaturesWithin<HoeDirt>(InstanceHolder.Config.AutoWaterRadius))
            {
                HoeDirt dirt = kv.Value;
                float consume = 2 * (1.0f / (can.UpgradeLevel / 2.0f + 1));
                if (dirt.crop == null || dirt.crop.dead.Value || dirt.state.Value != 0 ||
                    !(Game1.player.Stamina >= consume)
                    || can.WaterLeft <= 0)
                {
                    continue;
                }

                dirt.state.Value = 1;
                Game1.player.Stamina -= consume;
                if (!can.IsBottomless) can.WaterLeft--;
                watered = true;
            }
            foreach (IndoorPot pot in Util.GetObjectsWithin<IndoorPot>(InstanceHolder.Config.AutoWaterRadius))
            {
                if (pot.hoeDirt.Value == null)
                    continue;

                HoeDirt dirt = pot.hoeDirt.Value;
                float consume = 2 * (1.0f / (can.UpgradeLevel / 2.0f + 1));
                if (dirt.crop != null && !dirt.crop.dead.Value && dirt.state.Value != 1 && Game1.player.Stamina >= consume && can.WaterLeft > 0)
                {
                    dirt.state.Value = 1;
                    pot.showNextIndex.Value = true;
                    Game1.player.Stamina -= consume;
                    if (!can.IsBottomless) can.WaterLeft--;
                    watered = true;
                }
            }
            if (watered)
            {
                Game1.playSound("slosh");
            }
        }

        public static void ToggleBlacklistUnderCursor()
        {
            GameLocation location = Game1.currentLocation;
            Vector2 tile = Game1.currentCursorTile;
            if (!location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain))
                return;
            if (!(terrain is HoeDirt dirt))
                return;

            if (dirt.crop == null)
                Util.ShowHudMessage("There is no crop under the cursor");
            else
            {
                string cropID = dirt.crop.indexOfHarvest.Value;
                string name = ItemRegistry.ResolveMetadata(cropID)?.GetParsedData().DisplayName;
                if (name == "")
                    return;

                string text = ToggleBlackList(dirt.crop)
                    ? $"{name} has been added to AutoHarvest exception"
                    : $"{name} has been removed from AutoHarvest exception";
                Util.ShowHudMessage(text, 1000);
                Logger.Log(text);
            }
        }

        public static void DestroyNearDeadCrops(Farmer player)
        {
            GameLocation location = player.currentLocation;
            foreach (KeyValuePair<Vector2, HoeDirt> kv in Util.GetFeaturesWithin<HoeDirt>(1))
            {
                Vector2 loc = kv.Key;
                HoeDirt dirt = kv.Value;
                if (dirt.crop != null && dirt.crop.dead.Value)
                {
                    dirt.destroyCrop(true);
                }

            }
            foreach (IndoorPot pot in Util.GetObjectsWithin<IndoorPot>(1))
            {
                Vector2 loc = Util.GetLocationOf(location, pot);
                HoeDirt dirt = pot.hoeDirt.Value;
                if (dirt?.crop != null && dirt.crop.dead.Value)
                {
                    dirt.destroyCrop(true);
                }
            }
        }

        public static void ShakeNearbyFruitedBush()
        {
            int radius = InstanceHolder.Config.AutoShakeRadius;
            foreach (Bush bush in Game1.currentLocation.largeTerrainFeatures.OfType<Bush>())
            {
                Vector2 loc = bush.Tile;
                Vector2 diff = loc - Game1.player.Tile;
                if (Math.Abs(diff.X) > radius || Math.Abs(diff.Y) > radius)
                    continue;

                if (IsBushFruited(bush))
                    bush.performUseAction(loc);
            }
        }

        public static void ShakeNearbyFruitedTree()     // RCB TODO... Broken for coconut trees [fine with bananas]
        {
            foreach (KeyValuePair<Vector2, TerrainFeature> kv in Util.GetFeaturesWithin<TerrainFeature>(InstanceHolder.Config.AutoShakeRadius))
            {
                Vector2 loc = kv.Key;
                TerrainFeature feature = kv.Value;
                switch (feature)
                {
                    case Tree tree:
                        if (tree.hasSeed.Value && !tree.stump.Value)
                        {
                            if (!Game1.IsMultiplayer && Game1.player.ForagingLevel < 1)
                            {
                                break;
                            }

                            int num2;
                            switch (tree.treeType.Value)    // treeType.Value is now a string!  -RCB
                            {
                                case "3":
                                    num2 = 311;     // Pine Cone
                                    break;
                                case "1":
                                    num2 = 309;     // Acorn
                                    break;
                                case "2":
                                    num2 = 310;     // Maple Seed
                                    break;
                                case "6":
                                case "9":
                                    num2 = 88;      // Coconut
                                    break;
                                default:
                                    num2 = -1;
                                    break;
                            }

                            if (Game1.currentSeason.Equals("fall") && tree.treeType.Value == "2" &&
                                Game1.dayOfMonth >= 14)
                            {
                                num2 = 408;         // Hazelnut
                            }

                            if (num2 != -1)
                            {
                                tree.shake(loc, false);
                                Logger.Log($@"Shook fruited tree @{loc}");
                            }
                        }

                        break;
                    case FruitTree fruitTree:
                        if (fruitTree.growthStage.Value >= 4 && fruitTree.fruit.Count > 0 && !fruitTree.stump.Value)
                        {
                            fruitTree.shake(loc, false);
                            Logger.Log($@"Shook fruited tree @{loc}");
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Is the dirt's crop is a flower and producing nectar
        /// </summary>
        /// <param name="location">HoeDirt location to evaluate</param>
        /// <returns>Result</returns>
        private static bool IsProducingNectar(Vector2 location) => FlowerLocationProducingNectar.Contains(location);

        private static bool IsBlackListed(Crop crop)
        {
            String cropName = crop.forageCrop.Value ? crop.whichForageCrop.Value : crop.indexOfHarvest.Value;
            return InstanceHolder.Config.HarvestException.Contains(cropName);
        }

        private static bool ToggleBlackList(Crop crop)
        {
            String cropName = crop.forageCrop.Value ? crop.whichForageCrop.Value : crop.indexOfHarvest.Value;
            if (IsBlackListed(crop))
            {
                InstanceHolder.Config.HarvestException.Remove(cropName);
            }
            else
            {
                InstanceHolder.Config.HarvestException.Add(cropName);
            }

            InstanceHolder.WriteConfig();
            return IsBlackListed(crop);
        }

        private static IEnumerable<Vector2> GetAreaOfCollectingNectar(Vector2 homePoint)
        {
            List<Vector2> cropLocations = new List<Vector2>();
            Queue<Vector2> vector2Queue = new Queue<Vector2>();
            HashSet<Vector2> vector2Set = new HashSet<Vector2>();
            vector2Queue.Enqueue(homePoint);
            for (int index1 = 0; index1 <= 150 && vector2Queue.Count > 0; ++index1)
            {
                Vector2 index2 = vector2Queue.Dequeue();
                if (Game1.currentLocation.terrainFeatures.ContainsKey(index2) &&
                    Game1.currentLocation.terrainFeatures[index2] is HoeDirt dirt && dirt.crop != null &&
                    dirt.crop.programColored.Value && !dirt.crop.dead.Value && dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1)
                {
                    cropLocations.Add(index2);
                }
                foreach (Vector2 adjacentTileLocation in Utility.getAdjacentTileLocations(index2))
                {
                    if (!vector2Set.Contains(adjacentTileLocation))
                        vector2Queue.Enqueue(adjacentTileLocation);
                }
                vector2Set.Add(index2);
            }

            return cropLocations;
        }

        private static bool IsTeaBush(Bush bush)
        {
            return bush.size.Value == Bush.greenTeaBush;
        }

        private static bool IsBerryBush(Bush bush)
        {
            return bush.size.Value == Bush.mediumBush && !bush.townBush.Value;
        }

        private static bool IsBushFruited(Bush bush)
        {
            if (IsBerryBush(bush) || IsTeaBush(bush))
            {
                return bush.tileSheetOffset.Value == 1 && bush.inBloom();
            }

            return false;
        }
    }
}
