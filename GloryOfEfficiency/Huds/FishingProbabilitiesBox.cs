using GloryOfEfficiency.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using xTile.Dimensions;
using Game1 = StardewValley.Game1;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace GloryOfEfficiency.Huds
{
    public class FishingProbabilitiesBox
    {
        private static Dictionary<string, double> _fishingDictionary;
        private static Dictionary<string, int> _attachmentDictionary;

        private static bool _isFirstTimeOfFishing = true;

        public static void UpdateProbabilities(FishingRod rod)
        {
            if (rod.isFishing)
            {
                if (_isFirstTimeOfFishing)
                {
                    // Only run the probablies once when line is cast
                    _isFirstTimeOfFishing = false;

                    // Game Location
                    GameLocation location = Game1.currentLocation;

                    // Water Depth
                    Rectangle rectangle = new(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
                    Rectangle value = new((int)rod.bobber.X - 80, (int)rod.bobber.Y - 80, 64, 64);
                    bool flag = rectangle.Intersects(value);
                    int clearWaterDistance = rod.clearWaterDistance;

                    // Bobber Location
                    Vector2 vector = new(rod.bobber.X / 64f, rod.bobber.Y / 64f);

                    // Populate Fish and Tackle
                    _fishingDictionary = GetFishes(location, vector, clearWaterDistance + (flag ? 1 : 0), Game1.player, rod, InstanceHolder.Config.MorePreciseProbabilities ? InstanceHolder.Config.TrialOfExamine : 1);
                    _attachmentDictionary = GetAttachments(rod);
                }
            }

            // Clear out lists when not fishing
            else
            {
                _isFirstTimeOfFishing = true;
                _fishingDictionary = null;
                _attachmentDictionary = null;
            }
        }

        public static void PrintFishingInfo()
        {
            // Draw Fishing Window
            int x_pos_add = 0;
            if (InstanceHolder.Config.FishingProbabilitiesInfo && _fishingDictionary != null)
                x_pos_add = DrawProbBox(_fishingDictionary);

            // Draw Tackle Window
            if (InstanceHolder.Config.FishingTackleInfo && _attachmentDictionary != null && _attachmentDictionary.Count > 0)
                DrawTackleBox(_attachmentDictionary, x_pos_add);
        }

        private static Dictionary<string, int> GetAttachments(FishingRod rod)
        {
            Dictionary<string, int> dictList = new();

            // Get Bait and Tackle from Rod
            Object baitObject = rod.GetBait();
            List<Object> tackleObjects = rod.GetTackle();

            // Add Bait
            if (baitObject != null)
                dictList[baitObject.QualifiedItemId] = baitObject.Stack;

            // Add Tackle
            foreach (Object tackle in tackleObjects)
                if (tackle != null)
                    dictList[tackle.QualifiedItemId] = FishingRod.maxTackleUses - tackle.uses.Value;

            return dictList;
        }

        private static Dictionary<string, double> GetFishes(GameLocation location, Vector2 bobbleTile, int waterDepth, Farmer player, FishingRod rod, int trial = 1)
        {
            // Run X amount of trials and get the results
            List<Dictionary<string, double>> dictList = new();
            for (int i = 0; i < trial; i++)
            {
                // Mineshaft have to be handled slightly differently
                if (location is MineShaft)
                    dictList.Add(GetFishesMine((MineShaft)location, bobbleTile, waterDepth, player, rod));
                else
                    dictList.Add(GetFishesGeneric(bobbleTile, waterDepth, player, rod));
            }

            // Shuffle and get a list of valid items
            Dictionary<string, double> shuffledDict = ShuffleAndAverageFishingDictionary(dictList);
            Dictionary<string, double> fishDict = shuffledDict.OrderByDescending(x => x.Value).Where(kv => !IsGarbage(kv.Key)).ToDictionary(x => x.Key, x => x.Value);

            // Add in sum of Garbage items if they have a probability
            Dictionary<string, double> garbageDict = shuffledDict.OrderByDescending(x => x.Value).Where(kv => IsGarbage(kv.Key)).ToDictionary(x => x.Key, x => x.Value);
            double garbageSum = garbageDict.Sum(kv => kv.Value);
            if (garbageSum > 0.0001)
                fishDict.Add("(O)168", garbageSum);

            // Get probability to 100%
            double totalSum = fishDict.Sum(kv => kv.Value);
            Dictionary<string, double> finalDict = fishDict.ToDictionary(x => x.Key, x => x.Value / totalSum);

            return finalDict;
        }

        private static Dictionary<string, double> GetFishesMine(MineShaft shaft, Vector2 bobbleTile, int waterDepth, Farmer player, FishingRod rod)
        {
            Dictionary<String, double> dict = new();
            double p;

            // If player is using training rod in mines, return trash
            if (rod.QualifiedItemId.Contains("TrainingRod"))
            {
                dict["(O)168"] = 1;
                return dict;
            }

            // Setup initial chance
            double num = 1.0;
            num += 0.4 * player.FishingLevel;
            num += waterDepth * 0.1;

            // Lure Check
            if (rod.HasCuriosityLure())
                num += 5.0;

            // Target Bait Check
            string textTarget = rod.GetBait()?.Name ?? "";

            // Add specific fish based on mine level
            int level = shaft.getMineArea();
            switch (level)
            {
                case 0:
                case 10:
                    num += textTarget.Contains("Stonefish") ? 10 : 0;
                    p = 0.02 + 0.01 * num;
                    dict.Add("(O)158", p);
                    break;
                case 40:
                    num += textTarget.Contains("Ice Pip") ? 10 : 0;
                    p = 0.015 + 0.009 * num;
                    dict.Add("(O)161", p);
                    break;
                case 80:
                    num += textTarget.Contains("Lava Eel") ? 10 : 0;
                    p = 0.01 + 0.008 * num;
                    dict.Add("(O)162", p);
                    break;
                default:
                    return dict;
            }

            // Get general fish data on level 10/40, and add Cave Jelly and trash to level 80
            if (level == 10 || level == 40)
                return ConcatDictionary(dict, GetFishesGeneric(bobbleTile, waterDepth, player, rod, "UndergroundMine"));
            else if (level == 80)
            {
                p = 0.05 + player.LuckLevel * 0.05;
                dict.Add("(O)CaveJelly", p);
                dict.Add("(O)168", 1);
            }

            return dict;
        }

        private static Dictionary<string, double> GetFishesGeneric(Vector2 bobberTile, int waterDepth, Farmer player, FishingRod rod, string locationName = null)
        {
            Dictionary<string, double> dictFish = new();

            // GameData
            GameLocation location = Game1.currentLocation;
            Dictionary<string, LocationData> dictionary = DataLoader.Locations(Game1.content);
            LocationData locationData = ((location != null) ? location.GetData() : GameLocation.GetData(locationName));
            Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
            Season seasonForLocation = Game1.GetSeasonForLocation(location);

            // Get Fish Area for Tile
            if (location == null || !location.TryGetFishAreaForTile(bobberTile, out var id, out var _))
                id = null;

            // Magic Bait
            bool flagMagicBait = false; // flag
            bool hasCuriosityLure = false;
            string textTarget = null;
            flagMagicBait = rod.HasMagicBait();

            // Curiosity Lure
            hasCuriosityLure = rod.HasCuriosityLure();

            // Targeted Bait
            StardewValley.Object bait = rod.GetBait();
            if (bait?.QualifiedItemId == "(O)SpecificBait" && bait.preservedParentSheetIndex.Value != null)
                textTarget = "(O)" + bait.preservedParentSheetIndex.Value;

            // Keys
            HashSet<string> ignoreQueryKeys = (flagMagicBait ? GameStateQuery.MagicBaitIgnoreQueryKeys : null);
            Point tilePoint = player.TilePoint;

            // Fish Data
            IEnumerable<SpawnFishData> enumerable = dictionary["Default"].Fish;
            if (locationData != null && locationData.Fish?.Count > 0)
                enumerable = enumerable.Concat(locationData.Fish);

            // Add Inherited Locations if found in spawn data
            IEnumerable<SpawnFishData> inherited = enumerable.Where(x => x.Id.Contains("LOCATION_FISH"));
            foreach (SpawnFishData spawn in inherited)
            {
                string[] inheritedLocation = spawn.Id.Split(' ');
                enumerable = enumerable.Concat(dictionary[inheritedLocation[1]].Fish.Where(x => x.CanBeInherited));
            }

            // Gather all fish and order them by presidence
            enumerable = from p in enumerable
                         orderby p.Precedence, Game1.random.Next()
                         select p;

            // Loop through spawn data
            foreach (SpawnFishData spawn in enumerable)
            {
                string fishIDQualified = spawn.ItemId ?? spawn.Id;
                string fishID = Regex.Replace(fishIDQualified, "\\(O\\)", "");

                // If we get multiple options, select the first
                if (fishIDQualified.Contains('|'))
                {
                    fishIDQualified = fishIDQualified.Split('|')[0];
                    fishID = Regex.Replace(fishIDQualified, "\\(O\\)", "");
                }

                // Secret Notes / Inherited Fish Types
                if (fishIDQualified.Contains("SECRET_NOTE_OR_ITEM") || fishIDQualified.Contains("LOCATION_FISH")) continue;

                // Not the correct area or season
                if ((spawn.FishAreaId != null && id != spawn.FishAreaId) || (spawn.Season.HasValue && !flagMagicBait) && spawn.Season != seasonForLocation)
                    continue;

                // Player Position
                Rectangle? playerPosition = spawn.PlayerPosition;
                if (playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains(tilePoint.X, tilePoint.Y))
                    continue;

                // Bobber Position
                playerPosition = spawn.BobberPosition;
                if ((playerPosition.HasValue && !playerPosition.GetValueOrDefault().Contains((int)bobberTile.X, (int)bobberTile.Y)) || player.FishingLevel < spawn.MinFishingLevel || waterDepth < spawn.MinDistanceFromShore || (spawn.MaxDistanceFromShore > -1 && waterDepth > spawn.MaxDistanceFromShore) || (spawn.RequireMagicBait && !flagMagicBait))
                    continue;

                // Spawn Conditions
                if (spawn.Condition != null && !GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, null, ignoreQueryKeys))
                    continue;

                // We have caught the limit of the fish
                if (spawn.CatchLimit > 0 && player.fishCaught.TryGetValue(fishIDQualified, out var value2) && value2[0] >= spawn.CatchLimit)
                    continue;

                // If fish it not actually a fish
                if (!allFishData.TryGetValue(fishID, out var fishArray))
                {
                    float chance = spawn.GetChance(hasCuriosityLure, player.DailyLuck, player.LuckLevel, (float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location), spawn.ItemId == textTarget);
                    chance = Math.Min(chance, 1f);
                    dictFish[fishIDQualified] = chance;

                    // If this item has a 100% chance, we stop
                    if (chance >= 1f)
                        break;
                    else
                        continue;
                }

                // Split Fish Data to Array
                string[] fishData = fishArray.Split('/');

                // Training Rod Check
                bool flagTrainingRod = rod.QualifiedItemId == "(T)TrainingRod";
                if (flagTrainingRod)
                {
                    bool? canUseTrainingRod = spawn.CanUseTrainingRod;
                    if (canUseTrainingRod.HasValue)
                    {
                        if (!canUseTrainingRod.GetValueOrDefault())
                            continue;
                        else if (ArgUtility.TryGetInt(fishData, 1, out var value, out var error) && value >= 50)
                            continue;
                    }
                }

                // If we do not ingore requirements, check and get spawn chance
                if (!spawn.IgnoreFishDataRequirements)
                {
                    // Time and Weather Check if not using Magic Bait
                    if (!flagMagicBait)
                    {
                        // Time of Day Check
                        if (!ArgUtility.TryGet(fishData, 5, out var timeData, out var errorTime))
                            continue;

                        string[] timeArray = ArgUtility.SplitBySpace(timeData);
                        bool flagTime = false;
                        for (int i = 0; i < timeArray.Length; i += 2)
                        {
                            if (!ArgUtility.TryGetInt(timeArray, i, out var startTime, out errorTime) || !ArgUtility.TryGetInt(timeArray, i + 1, out var stopTime, out errorTime))
                                continue;

                            if (Game1.timeOfDay >= startTime && Game1.timeOfDay < stopTime)
                            {
                                flagTime = true;
                                break;
                            }
                        }

                        if (!flagTime)
                            continue;

                        // Weather Check
                        if (!ArgUtility.TryGet(fishData, 7, out var value, out var errorWeather))
                            continue;

                        if (!value.Equals("both"))
                        {
                            if (value.Equals("sunny") && location.IsRainingHere())
                                continue;
                            if (value.Equals("rainy") && !location.IsRainingHere())
                                continue;
                        }
                    }

                    // Fishing Level Check
                    if (ArgUtility.TryGetInt(fishData, 12, out var value8, out var error) && player.FishingLevel < value8)
                        continue;

                    // Max Depth, Spawn Chance, Spawn Depth Check
                    if (!ArgUtility.TryGetInt(fishData, 9, out var maxDepth, out error) || !ArgUtility.TryGetFloat(fishData, 10, out var spawnMultiplier, out error) || !ArgUtility.TryGetFloat(fishData, 11, out var depthMultiplier, out error))
                        continue;

                    // Chance to Catch
                    float num = depthMultiplier * spawnMultiplier;
                    spawnMultiplier -= (float)Math.Max(0, maxDepth - waterDepth) * num;
                    spawnMultiplier += (float)player.FishingLevel / 50f;
                    if (flagTrainingRod)
                        spawnMultiplier *= 1.1f;

                    spawnMultiplier = Math.Min(spawnMultiplier, 0.9f);
                    if ((double)spawnMultiplier < 0.25 && hasCuriosityLure)
                        if (spawn.CuriosityLureBuff > -1f)
                            spawnMultiplier += spawn.CuriosityLureBuff;
                        else
                        {
                            float num2 = 0.25f;
                            float num3 = 0.08f;
                            spawnMultiplier = (num2 - num3) / num2 * spawnMultiplier + (num2 - num3) / 2f;
                        }

                    if (spawn.ItemId == textTarget)
                        spawnMultiplier *= 1.66f;

                    if (spawn.ApplyDailyLuck)
                        spawnMultiplier += (float)player.DailyLuck;

                    List<QuantityModifier> chanceModifiers = spawn.ChanceModifiers;
                    if (chanceModifiers != null && chanceModifiers.Count > 0)
                        spawnMultiplier = Utility.ApplyQuantityModifiers(spawnMultiplier, spawn.ChanceModifiers, spawn.ChanceModifierMode, location);

                    // Only if fish has a chance to sapwn
                    if (spawnMultiplier > 0)
                    {
                        spawnMultiplier = Math.Min(spawnMultiplier, 1f);
                        dictFish[fishIDQualified] = spawnMultiplier;
                    }

                    // If this item has a 100% chance, we stop
                    if (spawnMultiplier >= 1f)
                        break;
                }

                // If we ignore requirements, get chance from spawn
                else
                {
                    float chance = spawn.GetChance(hasCuriosityLure, player.DailyLuck, player.LuckLevel, (float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location), spawn.ItemId == textTarget);
                    chance = Math.Min(chance, 1f);
                    dictFish[fishIDQualified] = chance;

                    // If this item has a 100% chance, we stop
                    if (chance >= 1f)
                        break;
                }
            }

            return dictFish;
        }

        private static bool IsGarbage(string item)
        {
            // Return false for any Non-Objects
            if (!item.Contains("(O)"))
                return false;

            // Get object information
            string itemID = Regex.Replace(item, "\\(O\\)", "");
            var objectData = Game1.objectData[itemID];

            // Fish items that are not fish
            if (objectData != null && objectData.ContextTags.Contains("fish_nonfish") && !objectData.ContextTags.Contains("counts_as_fish_catch"))
                return true;

            // JoJa Garbage
            if (item == "(O)167" || item == "(O)168" || item == "(O)169" || item == "(O)170" || item == "(O)171" || item == "(O)172")
                return true;

            return false;
        }

        private static Dictionary<string, double> GetFinalProbabilities(Dictionary<string, double> dict)
        {
            Dictionary<string, double> result = new();
            double ratio = 1.0;

            // Get the result of each fish while moving down from 100%
            foreach (KeyValuePair<string, double> kv in dict)
            {
                result.Add(kv.Key, kv.Value * ratio);
                ratio *= (1 - kv.Value);
            }

            return result;
        }

        private static Dictionary<string, double> ShuffleAndAverageFishingDictionary(IEnumerable<Dictionary<string, double>> list)
        {
            List<Dictionary<string, double>> dicts = list.Select(dict => GetFinalProbabilities(ShuffleDictionary(dict))).ToList();
            return AverageDictionary(dicts);
        }

        private static Dictionary<string, double> ShuffleDictionary(Dictionary<string, double> dict)
        {
            // Get the last value of the dictionary if higher than 100%
            double? probability = null;
            string LastKey = "";
            if (dict[dict.Keys.Last()] >= 1f)
            {
                LastKey = dict.Keys.Last();
                dict.Remove(LastKey);

                // Get the probabilities of the remaining fish
                foreach (KeyValuePair<string, double> kv in dict)
                    if (!probability.HasValue)
                        probability = 1 - kv.Value;
                    else
                        probability *= 1 - kv.Value;
            }

            // Shuffle the remaining items
            KeyValuePair<string, double>[] pairs = dict.ToArray();
            Utility.Shuffle(Game1.random, pairs);
            Dictionary<string, double> itemsShuffled = pairs.ToDictionary(x => x.Key, x => x.Value);

            // Insert the probability of the 100%
            if (LastKey != "")
                itemsShuffled[LastKey] = probability ?? 1;


            return itemsShuffled;
        }

        private static Dictionary<string, double> AverageDictionary(List<Dictionary<string, double>> list)
        {
            Dictionary<string, double> sum = new();
            foreach (Dictionary<string, double> elem in list)
            {
                foreach (KeyValuePair<string, double> pair in elem)
                {
                    if (sum.ContainsKey(pair.Key))
                        sum[pair.Key] += pair.Value;
                    else
                        sum.Add(pair.Key, pair.Value);
                }
            }

            return MagnifyProbabilities(sum, 1.0 / list.Count);
        }

        private static Dictionary<string, double> MagnifyProbabilities(Dictionary<string, double> dict, double ratio)
        {
            return dict.ToDictionary(kv => kv.Key, kv => kv.Value * ratio);
        }

        private static Dictionary<TK, TV> ConcatDictionary<TK, TV>(IDictionary<TK, TV> a, Dictionary<TK, TV> b)
        {
            Dictionary<TK, TV> dict = new(a);

            foreach (KeyValuePair<TK, TV> kv in b.Where(kv => !dict.ContainsKey(kv.Key)))
                dict.Add(kv.Key, kv.Value);

            return dict;
        }

        private static int DrawProbBox(Dictionary<string, double> probabilities)
        {
            Rectangle window = Game1.game1.Window.ClientBounds;
            SpriteBatch b = Game1.spriteBatch;
            Size size = GetProbBoxSize(probabilities);

            int yOffset = InstanceHolder.Config.ProbBoxCoordinates.Y;
            if (size.Height > window.Height - yOffset)
            {
                yOffset = window.Height - size.Height;
                if (yOffset < 0)
                    yOffset = 0;
            }

            // Draw menu without shadow
            IClickableMenu.drawTextureBox(b,Game1.menuTexture,new Rectangle(0, 256, 60, 60),InstanceHolder.Config.ProbBoxCoordinates.X,yOffset,size.Width,size.Height,Color.White,1f,false,-1f);
            const int square = (int)(Game1.tileSize / 1.5);
            int width = 16;
            int x_space = 0;
            int x = InstanceHolder.Config.ProbBoxCoordinates.X + 8;
            int y = yOffset + 16;
            int fishCount = 1;
            SpriteFont font = Game1.smallFont;

            // Draw Fish
            foreach (KeyValuePair<string, double> kv in probabilities)
            {
                if (kv.Key.Contains("SECRET_NOTE")) continue;
                string text = $"{kv.Value * 100:f1}%";
                Item fish = ItemRegistry.Create(kv.Key, 1);
                fish.drawInMenu(b, new Vector2(x + 8 + x_space, y), 0.70f);
                Utility.drawTextWithShadow(b, text, font, new Vector2(x + 32 + square + x_space, y + 18), Color.Black);

                Vector2 textSize = font.MeasureString(text);
                int w = square + (int)textSize.X + 64 + x_space;
                if (w > width) width = w;


                // Move cursor based on number of fish
                if (fishCount % InstanceHolder.Config.ProbBoxMaxFish == 0)
                {
                    x_space = 8 + w;
                    y = yOffset + 16;
                }
                else
                    y += square + 16;

                // Track Count
                fishCount++;
            }

            return size.Width;
        }

        private static Size GetProbBoxSize(Dictionary<string, double> probabilities)
        {
            int width = 16, height = 48;
            int square = (int)(Game1.tileSize / 1.5);
            int fishCount = 1;
            int x_space = 0;
            bool y_max = false;
            SpriteFont font = Game1.smallFont;
            foreach (KeyValuePair<string, double> kv in probabilities)
            {
                if (kv.Key.Contains("SECRET_NOTE")) continue;
                string text = $"{kv.Value * 100:f1}%";
                Vector2 textSize = font.MeasureString(text);
                int w = square + (int)textSize.X + 64 + x_space;
                if (w > width) width = w;

                // Increase Height if applicable
                if (!y_max)
                    height += square + 16;

                // Start a new column?
                if (fishCount % InstanceHolder.Config.ProbBoxMaxFish == 0)
                {
                    x_space = 8 + w;
                    y_max = true;
                }

                // Index
                fishCount++;
            }
            return new Size(width, height);
        }

        private static void DrawTackleBox(Dictionary<string, int> attachments, int x_add)
        {
            Rectangle window = Game1.game1.Window.ClientBounds;
            SpriteBatch b = Game1.spriteBatch;
            Size size = GetTackleBoxSize(attachments);

            // Determine location based on Probability box or defined coordinates
            int xOffset, yOffset;
            if (InstanceHolder.Config.TackleBoxAttach) {
                xOffset = InstanceHolder.Config.ProbBoxCoordinates.X + x_add;
                yOffset = InstanceHolder.Config.ProbBoxCoordinates.Y;
            }
            else
            {
                xOffset = InstanceHolder.Config.TackleBoxCoordinates.X;
                yOffset = InstanceHolder.Config.TackleBoxCoordinates.Y;
            }

            // Set the initial yOffset
            if (size.Height > window.Height - yOffset)
            {
                yOffset = window.Height - size.Height;
                if (yOffset < 0)
                    yOffset = 0;
            }

            //Draw Window for Bait and Bobbers
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xOffset, yOffset, size.Width, size.Height, Color.White, 1f, false, -1f);
            const int square = (int)(Game1.tileSize / 1.5);
            int x = xOffset + 8;
            int y = yOffset + 16;
            SpriteFont font = Game1.smallFont;

            // Add Bait and Tackle
            foreach (KeyValuePair<string, int> kv in attachments)
            {
                string text = $"{kv.Value}";
                Item tackle = ItemRegistry.Create(kv.Key, 1);
                tackle.drawInMenu(b, new Vector2(x + 8, y), 0.70f);
                Utility.drawTextWithShadow(b, text, font, new Vector2(x + 32 + square, y + 18), Color.Black);

                y += square + 16;
            }
        }

        private static Size GetTackleBoxSize(Dictionary<string, int> attachments)
        {
            int width = 16, height = 48;
            int square = (int)(Game1.tileSize / 1.5);
            SpriteFont font = Game1.smallFont;
            foreach (KeyValuePair<string, int> kv in attachments)
            {
                string text = $"{kv.Value}";
                Vector2 textSize = font.MeasureString(text);
                int w = square + (int)textSize.X + 64;
                if (w > width) width = w;
                height += square + 16;
            }
            return new Size(width, height);
        }
    }
}
