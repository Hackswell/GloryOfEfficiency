using System.Linq;
using GloryOfEfficiency.Core;
using GloryOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SVObject = StardewValley.Object;
using static StardewValley.Game1;

namespace GloryOfEfficiency.Automation
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
                    Logger.Log($"Trying to deposit {item.Name} into KEG: {obj.Name}. Not enough beans!");
                    return;
                }

                bool accepted = obj.Name == "Furnace" ? CanFurnaceAcceptItem(item, player) : Utility.isThereAnObjectHereWhichAcceptsThisItem(currentLocation, item, (int)loc.X * tileSize, (int)loc.Y * tileSize);
                Logger.Log($"Trying to deposit ({accepted}) {item.Name} into machine: {obj.Name}");
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
                        Logger.Log($"\tCrab Pot and {item.Name} are now ACCEPTED.");
                    }
                }
                else if (obj.Name == "Seed Maker" && InstanceHolder.Config.AutoDepositSeedMaker == false)
                {
                    continue;
                }

                if (!accepted)
                    continue;

                // performObjectDropInAction but only if it's currently empty
                if (obj.performObjectDropInAction(item, false, player, true))
                {
                    player.reduceActiveItemByOne();
                    Logger.Log($"Item {obj} MANUALLY consuming {item.Name}");
                } else {
                    Logger.Log($"Item {obj} should have already consumed {item.Name}");
                }
                Logger.Log($"DONE dropping {item.Name} into {obj}.");

                return;
            }
        }

        public static void PullMachineResult()
        {
            Farmer player = Game1.player;
            foreach (SVObject obj in Util.GetObjectsWithin<SVObject>(InstanceHolder.Config.MachineRadius).Where(IsObjectMachine))
            {
                // Nothing in the machine...
                if (obj.heldObject.Value == null)
                {
                    continue;
                }
                else if (!obj.readyForHarvest.Value)
                {
                    Logger.Log($"Time until {obj.Name} ready for collecting item {obj.heldObject.Value.Name}: {obj.MinutesUntilReady} game minutes.");
                    continue;
                }

                Item item = obj.heldObject.Value;
                if (player.couldInventoryAcceptThisItem(item))
                    obj.checkForAction(player);
            }
        }

        private static bool CanFurnaceAcceptItem(Item item, Farmer player)
        {
            Logger.Log($"{player.Items.ContainsId(Object.coalQID)} ** {item.Stack} ** {item.ParentSheetIndex}");

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
            if (InstanceHolder.Config.MachineTypes.Contains(obj.Name))
            {
                return true;
            }
            return false;
        }
    }
}
