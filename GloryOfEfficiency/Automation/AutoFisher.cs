using System;
using System.Collections.Generic;
using GloryOfEfficiency.Core;
using GloryOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.SpecialOrders;
using StardewValley.Tools;
using SVObject = StardewValley.Object;

namespace GloryOfEfficiency.Automation
{
    internal class AutoFisher
    {
        private static Config Config => InstanceHolder.Config;

        private static readonly Logger Logger = new Logger("AutoFisher");

        public static bool AfkMode { get; private set; }

        public static int FishQuality { get; set; }
        public static bool Treasure { get; set; }

        private static bool CatchingTreasure { get; set; }
        private static int AutoFishingCounter { get; set; }
        private static int AfkCooltimeCounter { get; set; }

        public static void AfkFishing()
        {
            Farmer player = Game1.player;

            if (AfkMode && player.passedOut)
            {
                AfkMode = false;
                Util.ShowHudMessageTranslated("hud.afk.passedout");
                return;
            }

            if (!AfkMode || !(player.CurrentTool is FishingRod rod) || Game1.activeClickableMenu != null)
            {
                return;
            }

            if (!rod.inUse() && !rod.castedButBobberStillInAir)
            {

                if (player.Stamina <= (player.MaxStamina * Config.ThresholdStaminaPercentage) / 100.0f)
                {
                    AfkMode = false;
                    Util.ShowHudMessageTranslated("hud.afk.tired");
                    return;
                }
                AfkCooltimeCounter++;
                if (AfkCooltimeCounter < 10)
                {
                    return;
                }
                AfkCooltimeCounter = 0;
                rod.beginUsing(player.currentLocation, 0, 0, player);
            }
            if (rod.isTimingCast)
            {
                rod.castingPower = Config.ThrowPower;
            }
            if (rod.fishCaught)
            {
                CollectFish(player, rod);
            }
        }

        public static void AutoReelRod()
        {
            Farmer player = Game1.player;
            if (!(player.CurrentTool is FishingRod rod) || Game1.activeClickableMenu != null)
            {
                return;
            }

            if (!rod.isNibbling || !rod.isFishing || rod.whichFish != null || rod.isReeling || rod.hit ||
                rod.isTimingCast || rod.pullingOutOfWater || rod.fishCaught || rod.castedButBobberStillInAir)
            {
                return;
            }

            rod.DoFunction(player.currentLocation, 1, 1, 1, player);
        }

        private static void CollectFish(Farmer who, FishingRod rod)
        {
            ItemMetadata whichFish = rod.whichFish;
            String whichFishName = rod.whichFish.QualifiedItemId;
            FishQuality = rod.fishQuality;
            string fishID = whichFish.GetParsedData().ItemId;
            int itemCategory = whichFish.GetParsedData().Category;

            if (!Game1.isFestival())
            {
                who.faceDirection(2);
                who.FarmerSprite.setCurrentFrame(84);
            }

            if (!who.IsLocalPlayer)
            {
                return;
            }

            who.currentLocation.localSound("coin");
            if (!rod.treasureCaught)
            {
                SVObject @object = null;
                switch (itemCategory)
                {
                    case SVObject.furnitureCategory:
                        {
                            Logger.Log($"\tFurniture? {fishID}\tItemCategory: {itemCategory}");
                            @object = new Furniture(fishID, Vector2.Zero);
                            break;
                        }
                    case SVObject.junkCategory:
                    case SVObject.litterCategory:
                    case SVObject.FishCategory:
                    default:
                        {
                            Logger.Log($"\tFishy, Litter, or Junky!  {fishID}");
                            @object = new SVObject(fishID, 1, false, -1, FishQuality);
                            if (fishID == GameLocation.CAROLINES_NECKLACE_ITEM_QID)
                            {
                                @object.questItem.Value = true;
                                break;
                            }

                            if (fishID == "79" || fishID == "842") // Secret Note (79) or Journal Scrap (842)
                            {
                                @object = who.currentLocation.tryToCreateUnseenSecretNote(who);
                                if (@object == null) return;
                            }

                            if (rod.numberOfFishCaught > 1)
                            {
                                @object.Stack = rod.numberOfFishCaught;
                            }

                            break;
                        }
                }

                bool fromFishPond = rod.fromFishPond;
                who.completelyStopAnimatingOrDoingAction();
                rod.doneFishing(who, !fromFishPond);
                if (!Game1.isFestival() && !fromFishPond && (itemCategory == SVObject.FishCategory && Game1.player.team.specialOrders.Count > 0))
                {
                    foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
                    {
                        specialOrder.onFishCaught?.Invoke(Game1.player, @object);
                    }
                }

                if (Game1.isFestival() || who.addItemToInventoryBool(@object))
                {
                    return;
                }

                Game1.activeClickableMenu = new ItemGrabMenu(new List<Item> { @object }, rod).setEssential(true);
            }
            else
            {
                Logger.Log($"Treazhure Cot!");
                rod.fishCaught = false;
                rod.showingTreasure = true;
                who.UsingTool = true;
                int initialStack = 1;
                if (rod.numberOfFishCaught > 1)
                {
                    initialStack = rod.numberOfFishCaught;
                }

                SVObject @object = new SVObject(fishID, initialStack, false, -1, FishQuality);
                if (Game1.player.team.specialOrders.Count > 0)
                {
                    Logger.Log($"\tSpechul Treazhure!");
                    foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
                    {
                        specialOrder.onFishCaught?.Invoke(Game1.player, @object);
                    }
                }
                bool inventoryBool = who.addItemToInventoryBool(@object);
                rod.openChestEndFunction(inventoryBool ? 0 : 1);
            }
        }

        public static void AutoFishing(BobberBar bar)
        {
            AutoFishingCounter = (AutoFishingCounter + 1) % 3;
            if (AutoFishingCounter > 0)
            {
                return;
            }

            float barPos = bar.bobberBarPos;
            int barHeight = bar.bobberBarHeight;
            float fishPos = bar.bobberPosition;
            float treasurePos = bar.treasurePosition;
            float distanceFromCatching = bar.distanceFromCatching;
            bool treasureCaught = bar.treasureCaught;
            bool Treasure = bar.treasure;
            float treasureAppearTimer = bar.treasureAppearTimer;
            float bobberBarSpeed = bar.bobberBarSpeed;
            float top = barPos;

            if (Treasure && treasureAppearTimer <= 0 && !treasureCaught)
            {
                if (!CatchingTreasure && distanceFromCatching > 0.7f)
                {
                    CatchingTreasure = true;
                }
                if (CatchingTreasure && distanceFromCatching < 0.3f)
                {
                    CatchingTreasure = false;
                }
                if (CatchingTreasure)
                {
                    fishPos = treasurePos;
                }
            }

            if (fishPos > barPos + (barHeight / 2f))
            {
                return;
            }

            float strength = (fishPos - (barPos + barHeight / 2f)) / 16f;
            float distance = fishPos - top;

            float threshold = Util.Cap(InstanceHolder.Config.CpuThresholdFishing, 0, 0.5f);
            if (distance < threshold * barHeight || distance > (1 - threshold) * barHeight)
            {
                bobberBarSpeed = strength;
            }

            bar.bobberBarSpeed = bobberBarSpeed;
        }

        public static void ToggleAfkFishing()
        {
            AfkMode = !AfkMode;
            Util.ShowHudMessageTranslated(AfkMode ? "hud.afk.on" : "hud.afk.off");
            Logger.Log($"AFK Mode is {(AfkMode ? "enabled" : "disabled")}.");
        }
    }
}
