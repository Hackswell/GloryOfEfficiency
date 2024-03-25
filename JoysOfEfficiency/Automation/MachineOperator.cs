using System.Linq;
using JoysOfEfficiency.Core;
using JoysOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SVObject = StardewValley.Object;
using static StardewValley.Game1;

namespace JoysOfEfficiency.Automation
{
    internal class MachineOperator
    {
        private static readonly Logger Logger = new Logger("MachineOperator");

        public static void DepositIngredientsToMachines()
        {
            Farmer player = Game1.player;
            if (player.CurrentItem == null || !(player.CurrentItem is SVObject item))
            {
                return;
            }
            foreach (SVObject obj in Util.GetObjectsWithin<SVObject>(InstanceHolder.Config.MachineRadius).Where(IsObjectMachine))
            {
                Vector2 loc = Util.GetLocationOf(currentLocation, obj);
                if (obj.heldObject.Value != null)
                    continue;

                if (obj.Name == "Keg" && item.ParentSheetIndex == 433 && item.Stack < 5)
                {
                    // You don't have enough beans.
                    Logger.Info($"Trying to deposit {item.Name} into KEG: {obj.Name}. Not enough beans!");
                    return;
                }

                bool accepted = obj.Name == "Furnace" ? CanFurnaceAcceptItem(item, player) : Utility.isThereAnObjectHereWhichAcceptsThisItem(currentLocation, item, (int)loc.X * tileSize, (int)loc.Y * tileSize);
                Logger.Info($"Trying to deposit ({accepted}) {item.Name} into machine: {obj.Name}");
                if (obj is Cask)
                {
                    if (ModEntry.IsCoGOn || ModEntry.IsCaOn)
                    {
                        if (obj.performObjectDropInAction(item, true, player))
                        {
                            obj.heldObject.Value = null;
                            accepted = true;
                        }
                    }
                    else if (currentLocation is not Cellar && accepted)
                    {
                        accepted = false;
                    }
                }
                else if (obj.Name == "Crab Pot")
                {
                    if (item.Name == "Bait" || item.Name == "Magic Bait")
                    {
                        accepted = true;
                        Logger.Info($"\tCrab Pot and {item.Name} are now ACCEPTED.");
                    }
                }
                else if (obj.Name == "Seed Maker")
                {
                    accepted = InstanceHolder.Config.AutoDepositSeedMaker == false ? false : true;
                }

                if (!accepted)
                    continue;

                Logger.Info($"Trying to drop {item.Name} into {obj}.");
                // performObjectDropInAction but only if it's currently empty
                obj.performObjectDropInAction(item, false, player);
                if (!(obj.Name == "Furnace" || obj.Name == "Charcoal Kiln") || item.Stack == 0)
                {
                    player.reduceActiveItemByOne();
                }
                Logger.Info($"DONE dropping {item.Name} into {obj}.");

                return;
            }
        }

        public static void PullMachineResult()
        {
            Farmer player = Game1.player;
            foreach (SVObject obj in Util.GetObjectsWithin<SVObject>(InstanceHolder.Config.MachineRadius).Where(IsObjectMachine))
            {
                if (!obj.readyForHarvest.Value || obj.heldObject.Value == null)
                    continue;

                Item item = obj.heldObject.Value;
                if (player.couldInventoryAcceptThisItem(item))
                    obj.checkForAction(player);
            }
        }

        private static bool CanFurnaceAcceptItem(Item item, Farmer player)
        {
            Logger.Info($"{player.Items.ContainsId(Object.coalQID)} ** {item.Stack} ** {item.ParentSheetIndex}");

            // Minimum of one coal in inventory
            if (! player.Items.ContainsId(Object.coalQID, 1))
                return false;

            switch (item.Name)
            {
                // One item per coal...
                case "Clay":
                case "Quartz":
                case "Fire Quartz":
                    break;

                // Five items per coal...
                case "Copper Ore":
                case "Iron Ore":
                case "Gold Ore":
                case "Iridium Ore":
                case "Radioactive Ore":
                    if (item.Stack < 5)
                        return false;
                    break;

                default:
                    return false;
            }
            return true;
        }

        private static bool IsObjectMachine(SVObject obj)
        {
            Logger.Log($"IsObjectMachine: Name: {obj.Name}");
            if (InstanceHolder.Config.MachineTypes.Contains(obj.Name))
            {
                return true;
            }
            return false;
        }
    }
}
