using System.Collections.Generic;
using System.Linq;
using JoysOfEfficiency.Core;
using JoysOfEfficiency.Utils;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using SVObject = StardewValley.Object;

namespace JoysOfEfficiency.Automation
{
    internal class FoodAutomation
    {
        private static Config Config => InstanceHolder.Config;
        private static readonly Logger Logger = new Logger("FoodAutomation");
        private static HashSet<string> _dontEat = new HashSet<string>();

        public static void TryToEatIfNeeded(Farmer player)
        {
            if (player.isEating || Game1.activeClickableMenu != null)
            {
                return;
            }

            if (player.CurrentTool != null && player.CurrentTool is FishingRod rod)
            {
                if (rod.inUse() && !player.UsingTool)
                {
                    return;
                }
            }

            if (!(player.Stamina <= player.MaxStamina * Config.StaminaToEatRatio) &&
                !(player.health <= player.maxHealth * Config.HealthToEatRatio))
            {
                return;
            }

            SVObject itemToEat = null;
            foreach (SVObject item in player.Items.OfType<SVObject>())
            {
                if (item.Edibility <= 0)
                    continue;

                //It's a edible item
                if (itemToEat == null ||
                    itemToEat.Edibility / itemToEat.salePrice() < item.Edibility / item.salePrice())
                {
                    //Found good edibility per price or just first food
                    itemToEat = item;
                }
            }

            if (itemToEat == null)
            {
                return;
            }

            player.eatObject(itemToEat);
            itemToEat.Stack--;
            if (itemToEat.Stack == 0)
            {
                player.removeItemFromInventory(itemToEat);
            }
        }


        // Don't Eat This mod copied and update here!  Liberated from Pyrohead37.
        public static void InitDontEat()
        {
            _dontEat.Clear();
            // 	public const int artisanGoodsCategory = -26;
            // 	public const int ingredientsCategory = -25;
            // 	public const int meatCategory = -14;
            //  public const int flowersCategory = -80;
            //	public const int FruitsCategory = -79;
            // 	public const int VegetableCategory = -75;
            //  public const int FishCategory = -4;
            //  public const int EggCategory = -5;
            //  public const int MilkCategory = -6;
            //  public const int CookingCategory = -7;
            // 	public const int inedible = -300;
            //  public const int GreensCategory = -81;

            // Go through all game objects and add any that are in our DontEatCategories first
            foreach (KeyValuePair<string, ObjectData> item in Game1.objectData)
            {
                string itemName = item.Value.Name;
                int itemEdibility = item.Value.Edibility;
                int itemCategory = item.Value.Category;
                string categoryName = Object.GetCategoryDisplayName(itemCategory);

                if (itemEdibility == -300) { continue; }
                if (Config.DontEatCategories.Contains(categoryName))
                {
                    _dontEat.Add(itemName);
                    Logger.Log($"Adding from Category [{item.Key}] {itemName} to the diet\t({categoryName})");
                }
            }

            // Add individual items from DontEat list
            foreach (var item in Config.DontEat)
            {
                _dontEat.Add(item);
                Logger.Log($"Adding from DontEat {item} to the diet\t");
            }

            // REMOVE any items in the DoEat list!   This must be last!
            foreach (var item in Config.DoEat)
            {
                _dontEat.Remove(item);
                Logger.Log($"REMOVING from DontEat {item}\t(DoEat exception)");
            }

            _dontEat.TrimExcess();
        }

        public static void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            SVObject activePlayerItem = Game1.player.ActiveObject;

            if (Context.IsWorldReady && e.Button.IsActionButton() && activePlayerItem != null)
            {
                string itemName = activePlayerItem.DisplayName;

                // Logger.Log($"Testing item {itemName} for dontEatability");
                if (_dontEat.Contains(itemName))
                {
                    activePlayerItem.Edibility = -300;
                    Logger.Log($"Don't eat that {activePlayerItem.DisplayName}! Spoofing inedibility.");
                }
            }
        }
    }


}
