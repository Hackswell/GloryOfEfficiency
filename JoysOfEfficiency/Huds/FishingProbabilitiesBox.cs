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

namespace JoysOfEfficiency.Huds
{
    public class FishingProbabilitiesBox
    {
        private static readonly Logger Logger = new Logger("FishingProbabilitiesInfo");

        private static Dictionary<String, double> _fishingDictionary;

        private static bool _isFirstTimeOfFishing = true;

        public static void UpdateProbabilities(FishingRod rod)
        {
            if (rod.isFishing)
            {
                if (_isFirstTimeOfFishing)
                {

                    _isFirstTimeOfFishing = false;
                    GameLocation location = Game1.currentLocation;

                    Rectangle rectangle = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
                    Rectangle value = new Rectangle((int)rod.bobber.X - 80, (int)rod.bobber.Y - 80, 64, 64);
                    bool flag = rectangle.Intersects(value);
                    int clearWaterDistance = rod.clearWaterDistance;

                    _fishingDictionary = GetFishes(location, rod.attachments[0]?.ParentSheetIndex ?? -1, clearWaterDistance + (flag ? 1 : 0), Game1.player, InstanceHolder.Config.MorePreciseProbabilities ? InstanceHolder.Config.TrialOfExamine : 1);
                }
            }
            else
            {
                _isFirstTimeOfFishing = true;
                _fishingDictionary = null;
            }
        }

        public static void PrintFishingInfo()
        {
            if (_fishingDictionary == null)
            {
                return;
            }
            DrawProbBox(_fishingDictionary);
        }

        private static Dictionary<String, double> GetFishes(GameLocation location, int bait, int waterDepth, Farmer who, int trial = 1)
        {
            List<Dictionary<String, double>> dictList = new List<Dictionary<String, double>>();
            for (int i = 0; i < trial; i++)
            {
                switch (location)
                {
                    case Farm _:
                        dictList.Add(GetFishesFarm(waterDepth, who));
                        break;
                    case MineShaft shaft:
//                        dictList.Add(GetFishes(waterDepth, who));
                        dictList.Add(GetFishesMine(shaft, bait, waterDepth, who));
                        break;
                    case Submarine _:
                        dictList.Add(GetFishesSubmarine());
                        break;
                    default:
                        dictList.Add(GetFishes(waterDepth, who));
                        break;
                }
            }

            Dictionary<String, double> dict = ShuffleAndAverageFishingDictionary(dictList);

            Dictionary<String, double> dict2 =
                dict.OrderByDescending(x => x.Value)
                    .Where(kv => !IsGarbage(kv.Key)).ToDictionary(x => x.Key, x => x.Value);
            double sum = dict2.Sum(kv => kv.Value);
            if (1 - sum >= 0.0001)
            {
                dict2.Add("168", 1 - sum);
            }
            return dict2;
        }

        private static Dictionary<String, double> GetFishes(int waterDepth, Farmer who, string locationName = null)
        {
            Dictionary<String, double> dictFish = new Dictionary<String, double>();
            GameLocation currLocation = Game1.currentLocation;
            Season season = Game1.GetSeasonForLocation(currLocation);

            LocationData thisLocData = Game1.currentLocation.GetData();
            String jThisLocData = JsonSerializer.Serialize(thisLocData, new JsonSerializerOptions { WriteIndented = true });
            //Logger.Info($"This LocationData: {jThisLocData}");

            Dictionary<string, string> allFishData = DataLoader.Fish(Game1.content);

            string locName = locationName ?? currLocation.Name;
            //Logger.Info($"GetFishes: Loc: {locName} ** Season: {season}");

            if (locName.Equals("WitchSwamp")
                && !Game1.MasterPlayer.mailReceived.Contains("henchmanGone")
                && !Game1.player.Items.ContainsId("Void Mayonnaise", 1))
            {
                return new Dictionary<String, double>
                {
                    { "308", 0.25 } // If the Henchman is gone, and the user has no void mayo, 25% chance to fish it up
                };
            }

            String JSONFishes = JsonSerializer.Serialize(allFishData, new JsonSerializerOptions { WriteIndented = true });
            //Logger.Info($"All Fishes: {JSONFishes}");
            Dictionary<string, LocationData> dictionary = DataLoader.Locations(Game1.content);
            String jDict = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions { WriteIndented = true });
            //Logger.Info($"All Location Data: {jDict}");

            IEnumerable<SpawnFishData> possibleFish = dictionary["Default"].Fish;
            if (thisLocData != null && thisLocData.Fish?.Count > 0)
            {
                possibleFish = possibleFish.Concat(thisLocData.Fish);
            }


            if (!(who.CurrentTool is FishingRod rod))
            {
                return dictFish;
            }

            //Logger.Info($"Finding fish area for farmer: {who.Tile.X},{who.Tile.Y} [bobber] {rod.bobber.X},{rod.bobber.Y} ** {currLocation.locationContextId} ** ");
            LocationContextData bubba = currLocation.GetLocationContext();
            String jsonBubba = JsonSerializer.Serialize(bubba, new JsonSerializerOptions { WriteIndented = true });
            //Logger.Info($"GameLocationContext: {jsonBubba}");

            currLocation.TryGetFishAreaForTile(who.Tile, out var fishAreaID, out var fishAreaData);
            //Logger.Info($"FishAreaID: {fishAreaID}");
            String jsonFishAreaData = JsonSerializer.Serialize(fishAreaData, new JsonSerializerOptions { WriteIndented = true });
            //Logger.Info($"FishAreaData: {jsonFishAreaData}");

            possibleFish =  from p in possibleFish
                            orderby p.Precedence, Game1.random.Next()
                            where    (p.FishAreaId == fishAreaID || p.FishAreaId == null) && (p.Season == season || p.Season == null)
                            select p;

            String JSON = JsonSerializer.Serialize(possibleFish, new JsonSerializerOptions{WriteIndented = true});
            //Logger.Info($"Possibru Fishu:\n{JSON}");

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
                String id = f.ItemId;
                if (id == null) { id = f.Id; }
                id = Regex.Replace(id, "\\(O\\)", "");
                //Logger.Info($"Testing Fish ID: {id}");

                // Any list item like "(O)167|(O)168|(O)169|(O)170|(O)171|(O)172" is a list of garbage. Handle it.
                if (id.Contains('|'))
                {
                    dictFish["168"] = f.Chance;
                    continue;
                }

                // Furniture, wrong conditions or the item doesnt exist. Skip it
                if (id.Contains("(F)") || !GameStateQuery.CheckConditions(f.Condition) || !ItemRegistry.Exists(id))
                {
                    continue;
                }

                // If it's not found in the allFishData, assign the chance from possibleFish instead
                if (!allFishData.ContainsKey(id))
                {
                    //Logger.Info($"\tdictFish: Adding {id} :: {f.Chance}");
                    dictFish[id] = f.Chance;
                    continue;
                }

                //Logger.Info($"Parsing through allFishData for {f.Id} now...");
                String[] fishData = allFishData[id].Split('/');
                String jFishData = JsonSerializer.Serialize(fishData, new JsonSerializerOptions{WriteIndented = true});
                //Logger.Info($"\tID: {id} *** {jFishData}");

                // If the time isn't in the fish's catch times, go on to next fish.
                String[] catchTimes = fishData[5].Split(' ');
                if (Game1.timeOfDay < Convert.ToInt32(catchTimes[0]) ||
                    Game1.timeOfDay >= Convert.ToInt32(catchTimes[1]))
                {
                    continue;
                }

                // Make sure weather matches the fish pool.  [Hackswell: pun intended]
                if (! fishData[7].Equals("both"))
                {
                    if (fishData[7].Equals("rainy") && !Game1.isRaining)
                    {
                        continue;
                    }
                    else if (fishData[7].Equals("sunny") && Game1.isRaining)
                    {
                        continue;
                    }
                }

                if (who.FishingLevel < Convert.ToInt32(fishData[12]))
                {
                   continue;
                }

                int maxDepth = Convert.ToInt32(fishData[9]);
                double spawnMultiplier = Convert.ToDouble(fishData[10]);
                double depthMultiplier = Convert.ToDouble(fishData[11]) * spawnMultiplier;
                spawnMultiplier -= Math.Max(0, maxDepth - waterDepth) * depthMultiplier;
                spawnMultiplier += who.FishingLevel / 50f;
                chance = Math.Min(spawnMultiplier, 0.89999997615814209);

                //Logger.Info($"dictFish: Adding {id} :: {chance}");
                dictFish[id] = chance;
            }
            String jDictFish = JsonSerializer.Serialize(dictFish, new JsonSerializerOptions{WriteIndented = true});
            //Logger.Info($"Possible Fish:\n{jDictFish}");

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

        private static Dictionary<String, double> GetFishesMine(MineShaft shaft, int bait, int waterDepth, Farmer who)
        {
            Dictionary<String, double> dict = new Dictionary<String, double>();
            double num2 = 1.0;
            num2 += 0.4 * who.FishingLevel;
            num2 += waterDepth * 0.1;
            double p;
            int level = shaft.getMineArea();
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
            {
                return ConcatDictionary(dict,
                    MagnifyProbabilities(
                        GetFishes(waterDepth, who, "UndergroundMine")
                            .Where(kv => !IsGarbage(kv.Key)).ToDictionary(x => x.Key, x => x.Value),
                        1 - p));
            }

            return dict;
        }

        private static Dictionary<String, double> GetFishesFarm(int waterDepth, Farmer who)
        {
            switch (Game1.whichFarm)
            {
                case 1:
                    return ConcatDictionary(MagnifyProbabilities(GetFishes(waterDepth, who, "Forest"), 0.3), MagnifyProbabilities(GetFishes(waterDepth, who, "Town"), 0.7));
                case 3:
                    return MagnifyProbabilities(GetFishes(waterDepth, who, "Forest"), 0.5);
                case 2:
                    {
                        double p = 0.05 + Game1.player.DailyLuck;
                        return ConcatDictionary(
                            new Dictionary<String, double> { { "734", p } },
                            MagnifyProbabilities(
                                GetFishes(waterDepth, who, "Forest"),
                                (1 - p) * 0.45)
                            );
                    }
                case 4:
                    {
                        return MagnifyProbabilities(
                            GetFishes(waterDepth, who, "Mountain"),
                            0.35);
                    }
                default:
                    return GetFishes(waterDepth, who);
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
                int index = int.Parse(item);
                if (index is >= 167 and <= 172)
                {
                    return true;
                }
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

        private static void DrawProbBox(Dictionary<String, double> probabilities)
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

            IClickableMenu.drawTextureBox(Game1.spriteBatch,
                                          InstanceHolder.Config.ProbBoxCoordinates.X,
                                          yOffset,
                                          size.Width, size.Height, Color.White);
            const int square = (int)(Game1.tileSize / 1.5);
            int x = InstanceHolder.Config.ProbBoxCoordinates.X + 8;
            int y = yOffset + 16;
            SpriteFont font = Game1.smallFont;
            {
                foreach (KeyValuePair<String, double> kv in probabilities)
                {
                    if (kv.Key.Contains("SECRET_NOTE"))
                        continue;

                    string text = $"{kv.Value * 100:f1}%";
                    Object fish = new Object(kv.Key, 1);
                    fish.drawInMenu(b, new Vector2(x + 8, y), 0.70f);
                    Utility.drawTextWithShadow(b, text, font, new Vector2(x + 32 + square, y + 16), Color.Black);

                    y += square + 16;
                }
            }
        }

        private static Size GetProbBoxSize(Dictionary<String, double> probabilities)
        {
            int width = 16, height = 48;
            int square = (int)(Game1.tileSize / 1.5);
            SpriteFont font = Game1.smallFont;
            {
                foreach (KeyValuePair<String, double> kv in probabilities)
                {
                    if (kv.Key.Contains("SECRET_NOTE"))
                        continue;
                    string text = $"{kv.Value * 100:f1}%";
                    Vector2 textSize = font.MeasureString(text);
                    int w = square + (int)textSize.X + 64;
                    if (w > width)
                    {
                        width = w;
                    }
                    height += square + 16;
                }
            }
            return new Size(width, height);
        }
    }
}
