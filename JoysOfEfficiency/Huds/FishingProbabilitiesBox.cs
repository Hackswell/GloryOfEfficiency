using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Text.RegularExpressions;
using JoysOfEfficiency.Core;
using JoysOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Dimensions;
using Game1 = StardewValley.Game1;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using System.Xml;
using StardewValley.Buildings;
using Microsoft.VisualBasic;

namespace JoysOfEfficiency.Huds
{
    public class FishingProbabilitiesBox
    {
        private static readonly Logger Logger = new Logger("FishingProbabilitiesInfo");

        private static Dictionary<String, double> _fishingDictionary;
        private static Dictionary<String, int> _attachmentDictionary;

        private static bool _isFirstTimeOfFishing = true;

        public static void UpdateProbabilities(FishingRod rod)
        {
            if (rod.isFishing)
            {
                if (_isFirstTimeOfFishing)
                {
                    // Only run the probablies once when line is cast
                    _isFirstTimeOfFishing = false;

                    // Get water depth
                    GameLocation location = Game1.currentLocation;
                    Rectangle rectangle = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
                    Rectangle value = new Rectangle((int)rod.bobber.X - 80, (int)rod.bobber.Y - 80, 64, 64);
                    bool flag = rectangle.Intersects(value);
                    int clearWaterDistance = rod.clearWaterDistance;

                    // Populate dictionaries
                    _fishingDictionary = GetFishes(location, rod.HasMagicBait() ,rod.attachments[0]?.ParentSheetIndex ?? -1, clearWaterDistance + (flag ? 1 : 0), Game1.player, InstanceHolder.Config.MorePreciseProbabilities ? InstanceHolder.Config.TrialOfExamine : 1);
                    _attachmentDictionary = GetAttachments(rod);
                }
            }
            else
            {
                _isFirstTimeOfFishing = true;
                _fishingDictionary = null;
                _attachmentDictionary = null;
            }
        }

        public static void PrintFishingInfo()
        {
            int x_pos_add = 0;
            if (InstanceHolder.Config.FishingProbabilitiesInfo && _fishingDictionary != null)
                x_pos_add = DrawProbBox(_fishingDictionary);

            if (InstanceHolder.Config.FishingTackleInfo && _attachmentDictionary != null && _attachmentDictionary.Count > 0)
                DrawTackleBox(_attachmentDictionary, x_pos_add);

        }

        private static Dictionary<String, int> GetAttachments(FishingRod rod)
        {
            Dictionary<String, int> dictList = new Dictionary<String, int>();

            // If no attachments, return null set
            if (rod.attachments.Count == 0) return null;

            // Loop through attachments
            for (int i = 0; i < rod.attachments.Count; i++)
            {
                int rodIndex = rod.attachments[i]?.ParentSheetIndex ?? -1;
                if (rodIndex > -1)
                {
                    // Bait vs Tackle
                    if (i == 0)
                        dictList[rod.attachments[i].QualifiedItemId] = rod.attachments[i].Stack;
                    else
                        dictList[rod.attachments[i].QualifiedItemId] = FishingRod.maxTackleUses - rod.attachments[i].uses.Value;
                }
            }

            return dictList;
        }

        private static Dictionary<String, double> GetFishes(GameLocation location, bool magicBait, int bait, int waterDepth, Farmer who, int trial = 1)
        {
            List<Dictionary<String, double>> dictList = new List<Dictionary<String, double>>();
            for (int i = 0; i < trial; i++)
            {
                switch (location)
                {
                    case Farm _:
                        dictList.Add(GetFishesFarm(waterDepth, who, magicBait));
                        break;
                    case MineShaft shaft:
                        dictList.Add(GetFishesMine(shaft, magicBait, bait, waterDepth, who));
                        break;
                    case Submarine _:
                        dictList.Add(GetFishesSubmarine());
                        break;
                    default:
                        dictList.Add(GetFishesGeneric(waterDepth, who, magicBait));
                        break;
                }
            }

            Dictionary<String, double> dict = ShuffleAndAverageFishingDictionary(dictList);
            Dictionary<String, double> dict2 = dict.OrderByDescending(x => x.Value).Where(kv => !IsGarbage(kv.Key)).ToDictionary(x => x.Key, x => x.Value);
            double sum = dict2.Sum(kv => kv.Value);
            if (1 - sum >= 0.0001)
                dict2.Add("168", 1 - sum);
            
            return dict2;
        }

        private static Dictionary<String, double> GetFishesGeneric(int waterDepth, Farmer who, bool magicBait, string locationName = null)
        {
            Dictionary<String, double> dictFish = new Dictionary<String, double>();
            GameLocation currLocation = Game1.currentLocation;
            Season season = Game1.GetSeasonForLocation(currLocation);

            // Load all Fish and Locations
            Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);
            Dictionary<string, LocationData> dictionary = DataLoader.Locations(Game1.content);

            // Get Location Data
            LocationData thisLocData = Game1.currentLocation.GetData();
            string locName = locationName ?? currLocation.Name;
            Logger.Log($"GetFishes: Loc: {locName} ** Season: {season}");

            // If the Henchman is gone, and the user has no void mayo, 25% chance to fish it up
            if (locName.Equals("WitchSwamp") && !Game1.MasterPlayer.mailReceived.Contains("henchmanGone") && !Game1.player.Items.ContainsId("Void Mayonnaise", 1))
                return new Dictionary<String, double> { { "308", 0.25 } };

            // Show all available fishes
            if (Game1.debugMode)
            {
                String JSONFishes = JsonSerializer.Serialize(allFishData, new JsonSerializerOptions { WriteIndented = true });
                Logger.Info($"All Fishes: {JSONFishes}");
            }
            
            // Get all possible fish in the area
            IEnumerable<SpawnFishData> possibleFish = dictionary["Default"].Fish;
            if (thisLocData != null && thisLocData.Fish?.Count > 0)
                possibleFish = possibleFish.Concat(thisLocData.Fish);

            // Debug Location Data
            if (Game1.debugMode) {
                Logger.Info($"Finding fish area for farmer: {who.Tile.X},{who.Tile.Y} ** {currLocation.locationContextId} ** ");
                LocationContextData bubba = currLocation.GetLocationContext();
                String jsonBubba = JsonSerializer.Serialize(bubba, new JsonSerializerOptions { WriteIndented = true });
                Logger.Info($"GameLocationContext: {jsonBubba}");
            }

            // Get all fish for the current location
            currLocation.TryGetFishAreaForTile(who.Tile, out var fishAreaID, out var fishAreaData);
            if (Game1.debugMode)
            {
                String jsonFishAreaData = JsonSerializer.Serialize(fishAreaData, new JsonSerializerOptions { WriteIndented = true });
                Logger.Info($"FishAreaData: {jsonFishAreaData}");
            }

            // If we are using magic bait, include all seasons
            if (magicBait)
                possibleFish = from p in possibleFish
                               orderby p.Precedence, Game1.random.Next()
                               where (p.FishAreaId == fishAreaID || p.FishAreaId == null)
                               select p;
            else
                possibleFish = from p in possibleFish
                               orderby p.Precedence, Game1.random.Next()
                               where (p.FishAreaId == fishAreaID || p.FishAreaId == null) && (p.Season == season || p.Season == null)
                               select p;

            // Show all possible fish
            if (Game1.debugMode)
            {
                String JSON = JsonSerializer.Serialize(possibleFish, new JsonSerializerOptions { WriteIndented = true });
                Logger.Info($"Possibru Fishu:\n{JSON}");
            }
            // Reference: https://stardewvalleywiki.com/Modding:Fish_data#Fish_data_and_spawn_criteria
            //              0          1   2      3  4    5            6                   7     8           9  10 11 12  13
            //  "143": "Catfish/       75/mixed/ 12/72/600 2400/spring fall winter/      rainy/689 .4 680 .1/4/.40/.1/0/true",
            //  "800": "Blobfish/      75/floater/8/25/600 2600/spring summer fall winter/both/685 .35/      3/.40/.1/0/false"
            //  "701": "Tilapia/       50/mixed/ 11/30/600 1400/summer fall/              both/683 .35/      3/.4/.2/0/false",
            //  "838": "Blue Discus/   60/dart/   2/ 9/600 2600/spring summer fall winter/both/685 .35/      1/.25/.1/0/false",
            //  "162": "Lava Eel/      90/mixed/ 32/32/600 2600/spring summer fall winter/both/684 .1/       2/.05/.1/7/false",

            /* 0   Name
               5   startTime  endTime
               6   season1 season2 ...
               7   rainy|sunny|both
               9   maxDepth
               10  spawnMultiplier
               11  depthMultiplier
               12  fishingLevel            */

            double chance = 0.0d;
            foreach (SpawnFishData f in possibleFish)
            {
                // Use ItemId unless its null. Account for mods with non-standard IDs for vanilla records
                String id = f.ItemId ?? f.Id;
                String qualifiedID = id;
                id = Regex.Replace(id, "\\(O\\)", "");

                // Debug
                if (Game1.debugMode) Logger.Info($"Testing Fish ID: {id}");

                // Skip Garbage, Furniture and items that dont exist
                if (id.Contains('|') || id.Contains("(F)") || !ItemRegistry.Exists(id)) continue;

                // Remove season check when using magic bait
                String newCondition = f.Condition;
                if (magicBait && f.Condition != null)
                {
                    List<string> conditions = f.Condition.Split(',').ToList();
                    conditions.RemoveAll(u => u.ToUpper().Contains("LOCATION_SEASON"));
                    newCondition = String.Join(",", conditions);
                } 
                if (!GameStateQuery.CheckConditions(newCondition)) continue;

                // If it's not found in the allFishData, assign the chance from possibleFish instead
                if (!allFishData.ContainsKey(id))
                {
                    if (Game1.debugMode) Logger.Info($"\tdictFish: Adding {id} :: {f.Chance}");
                    dictFish[qualifiedID] = f.Chance;
                    continue;
                }
                
                // Get the data for the fish
                String[] fishData = allFishData[id].Split('/');

                // Debug code for fish data
                if (Game1.debugMode)
                {
                    Logger.Info($"Parsing through allFishData for {f.Id} now...");
                    String jFishData = JsonSerializer.Serialize(fishData, new JsonSerializerOptions{WriteIndented = true});
                    Logger.Info($"\tID: {id} *** {jFishData}");
                }

                // Check the time window of the fish
                String[] catchTimes = fishData[5].Split(' ');
                if (!magicBait && (Game1.timeOfDay < Convert.ToInt32(catchTimes[0]) || Game1.timeOfDay >= Convert.ToInt32(catchTimes[1]))) continue;

                // Make sure weather matches the fish pool.  [Hackswell: pun intended]
                if (!fishData[7].Equals("both") && !magicBait)
                {
                    if (fishData[7].Equals("rainy") && !Game1.isRaining) continue;
                    else if (fishData[7].Equals("sunny") && Game1.isRaining) continue;
                }

                // Below the required level
                if (who.FishingLevel < Convert.ToInt32(fishData[12])) continue;

                // Calculate chance
                int maxDepth = Convert.ToInt32(fishData[9]);
                double spawnMultiplier = Convert.ToDouble(fishData[10]);
                double depthMultiplier = Convert.ToDouble(fishData[11]) * spawnMultiplier;
                spawnMultiplier -= Math.Max(0, maxDepth - waterDepth) * depthMultiplier;
                spawnMultiplier += who.FishingLevel / 50f;
                chance = Math.Min(spawnMultiplier, 0.89999997615814209);

                if (Game1.debugMode) Logger.Info($"dictFish: Adding {id} :: {chance}");
                dictFish[qualifiedID] = chance;
            }

            // Return fish list
            if (Game1.debugMode)
            {
                String jDictFish = JsonSerializer.Serialize(dictFish, new JsonSerializerOptions { WriteIndented = true });
                Logger.Info($"Possible Fish:\n{jDictFish}");
            }
            return dictFish;
        }

        private static Dictionary<String, double> GetFishesSubmarine()
        {
            return new Dictionary<String, double>
            {
                { "800", 0.1 },
                { "799", 0.18 },
                { "798", 0.28 },
                { "154", 0.1 },
                { "155", 0.08 },
                { "149", 0.05 },
                { "797", 0.01 }
            };
        }

        private static Dictionary<String, double> GetFishesMine(MineShaft shaft, bool magicBait, int bait, int waterDepth, Farmer who)
        {
            Dictionary<String, double> dict = new Dictionary<String, double>();
            double num2 = 1.0;
            num2 += 0.4 * who.FishingLevel;
            num2 += waterDepth * 0.1;
            double p;
            int level = shaft.getMineArea();

            // Add fish based on mine level
            switch (level)
            {
                case 0:
                case 10:
                    num2 += bait == 689 ? 3 : 0;
                    p = 0.02 + 0.01 * num2;
                    dict.Add("158", p);             // Stonefish
                    break;
                case 40:
                    num2 += bait == 682 ? 3 : 0;
                    p = 0.015 + 0.009 * num2;
                    dict.Add("161", p);             // Ice Pip
                    break;
                case 80:
                    num2 += bait == 684 ? 3 : 0;
                    p = 0.01 + 0.008 * num2;
                    dict.Add("162", p);             // Lava Eel
                    break;
                default:
                    return dict;
            }

            if (level == 10 || level == 40)
                return ConcatDictionary(dict,MagnifyProbabilities(GetFishesGeneric(waterDepth, who, magicBait, "UndergroundMine").Where(kv => !IsGarbage(kv.Key)).ToDictionary(x => x.Key, x => x.Value),1 - p));

            return dict;
        }

        private static Dictionary<String, double> GetFishesFarm(int waterDepth, Farmer who, bool magicBait)
        {
            switch (Game1.whichFarm)
            {
                case 1:
                    return ConcatDictionary(MagnifyProbabilities(GetFishesGeneric(waterDepth, who, magicBait, "Forest"), 0.3), MagnifyProbabilities(GetFishesGeneric(waterDepth, who, magicBait, "Town"), 0.7));
                case 3:
                    return MagnifyProbabilities(GetFishesGeneric(waterDepth, who, magicBait, "Forest"), 0.5);
                case 2:
                    {
                        double p = 0.05 + Game1.player.DailyLuck;
                        return ConcatDictionary(
                            new Dictionary<String, double> { { "734", p } },
                            MagnifyProbabilities(
                                GetFishesGeneric(waterDepth, who, magicBait, "Forest"),
                                (1 - p) * 0.45)
                            );
                    }
                case 4:
                    {
                        return MagnifyProbabilities(
                            GetFishesGeneric(waterDepth, who, magicBait, "Mountain"),
                            0.35);
                    }
                default:
                    return GetFishesGeneric(waterDepth, who, magicBait);
            }
        }

        private static Dictionary<String, double> GetFinalProbabilities(Dictionary<String, double> dict)
        {
            Dictionary<String, double> result = new Dictionary<String, double>();
            double ratio = 1.0;
            foreach (KeyValuePair<String, double> kv in dict)
            {
                result.Add(kv.Key, kv.Value * ratio);
                ratio *= (1 - kv.Value);
            }

            return result;
        }

        private static bool IsGarbage(String item)
        {
            try
            {
                // Account for qualified item names
                int index = int.Parse(Regex.Replace(item, "\\(O\\)", ""));
                if (index is >= 167 and <= 172) return true;
                switch (index)
                {
                    case 152:
                    case 153:
                    case 157:
                        return true;
                }
            }
            catch { }

            return false;
        }

        private static Dictionary<String, double> ShuffleAndAverageFishingDictionary(IEnumerable<Dictionary<String, double>> list)
        {
            List<Dictionary<String, double>> dicts = list.Select(dict => GetFinalProbabilities(ShuffleDictionary(dict))).ToList();

            return AverageDictionary(dicts);
        }

        private static Dictionary<String, double> ShuffleDictionary(Dictionary<String, double> dict)
        {
            KeyValuePair<String, double>[] pairs = dict.ToArray();
            Utility.Shuffle(Game1.random, pairs);
            return pairs.ToDictionary(x => x.Key, x => x.Value);
        }

        private static Dictionary<String, double> AverageDictionary(List<Dictionary<String, double>> list)
        {
            Dictionary<String, double> sum = new Dictionary<String, double>();
            foreach (Dictionary<String, double> elem in list)
            {
                foreach (KeyValuePair<String, double> pair in elem)
                {
                    if (sum.ContainsKey(pair.Key))
                    {
                        sum[pair.Key] += pair.Value;
                    }
                    else
                    {
                        sum.Add(pair.Key, pair.Value);
                    }
                }
            }

            return MagnifyProbabilities(sum, 1.0 / list.Count);
        }

        private static Dictionary<String, double> MagnifyProbabilities(Dictionary<String, double> dict, double ratio)
        {
            return dict.ToDictionary(kv => kv.Key, kv => kv.Value * ratio);
        }

        private static Dictionary<TK, TV> ConcatDictionary<TK, TV>(IDictionary<TK, TV> a, Dictionary<TK, TV> b)
        {
            Dictionary<TK, TV> dict = new Dictionary<TK, TV>(a);

            foreach (KeyValuePair<TK, TV> kv in b.Where(kv => !dict.ContainsKey(kv.Key)))
            {
                dict.Add(kv.Key, kv.Value);
            }

            return dict;
        }

        private static int DrawProbBox(Dictionary<String, double> probabilities)
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
            foreach (KeyValuePair<String, double> kv in probabilities)
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

        private static Size GetProbBoxSize(Dictionary<String, double> probabilities)
        {
            int width = 16, height = 48;
            int square = (int)(Game1.tileSize / 1.5);
            int fishCount = 1;
            int x_space = 0;
            bool y_max = false;
            SpriteFont font = Game1.smallFont;
            foreach (KeyValuePair<String, double> kv in probabilities)
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

        private static void DrawTackleBox(Dictionary<String, int> attachments, int x_add)
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
            foreach (KeyValuePair<String, int> kv in attachments)
            {
                string text = $"{kv.Value}";
                Item tackle = ItemRegistry.Create(kv.Key, 1);
                tackle.drawInMenu(b, new Vector2(x + 8, y), 0.70f);
                Utility.drawTextWithShadow(b, text, font, new Vector2(x + 32 + square, y + 18), Color.Black);

                y += square + 16;
            }
        }

        private static Size GetTackleBoxSize(Dictionary<String, int> attachments)
        {
            int width = 16, height = 48;
            int square = (int)(Game1.tileSize / 1.5);
            SpriteFont font = Game1.smallFont;
            foreach (KeyValuePair<String, int> kv in attachments)
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
