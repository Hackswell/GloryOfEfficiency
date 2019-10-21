﻿using JoysOfEfficiency.Core;
using JoysOfEfficiency.Utils;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace JoysOfEfficiency.Automation
{
    internal class InventoryAutomation
    {
        private static IMonitor Monitor => InstanceHolder.Monitor;
        private static IReflectionHelper Reflection => InstanceHolder.Reflection;

        public static void LootAllAcceptableItems(ItemGrabMenu menu, bool skipCheck = false)
        {
            if (!skipCheck)
            {
                if (menu.shippingBin || Util.IsCaShippingBinMenu(menu))
                {
                    Monitor.Log("Don't do anything with shipping bin");
                    return;
                }

                if (menu.reverseGrab)
                {
                    Monitor.Log("You can't get item from this menu.");
                    return;
                }

                if (menu.source == ItemGrabMenu.source_chest)
                {
                    Monitor.Log("Don't do anything with chest player placed");
                    return;
                }

                if (menu.showReceivingMenu && menu.source == ItemGrabMenu.source_none)
                {
                    Monitor.Log("showReceivingMenu true but is not gift or fishing chest.");
                    return;
                }
            }

            for (int i = menu.ItemsToGrabMenu.actualInventory.Count - 1; i >= 0; i--)
            {
                if (i >= menu.ItemsToGrabMenu.actualInventory.Count)
                {
                    continue;
                }

                Item item = menu.ItemsToGrabMenu.actualInventory[i];
                int oldStack = item.Stack;
                int remain = Util.AddItemIntoInventory(item);
                int taken = oldStack - remain;
                if (taken > 0)
                {
                    Monitor.Log($"You looted {item.DisplayName}{(taken == 1 ? "" : " x" + taken)}.");
                }

                if (remain == 0)
                {
                    menu.ItemsToGrabMenu.actualInventory.Remove(item);
                    continue;
                }

                menu.ItemsToGrabMenu.actualInventory[i].Stack = remain;
            }
        }

        public static void TryCloseItemGrabMenu(ItemGrabMenu menu)
        {
            if (!menu.areAllItemsTaken() || menu.heldItem != null)
            {
                return;
            }

            if (menu.shippingBin || Util.IsCaShippingBinMenu(menu))
            {
                //It's a shipping bin.
                return;
            }

            if (menu.context is Event && GetEssential(menu))
            {
                // You should not emergency close in events (it may stop the dialogue).
                return;
            }

            switch (menu.source)
            {
                case ItemGrabMenu.source_chest:
                case ItemGrabMenu.source_none when menu.context == null:
                    return; // It's a chest.
            }

            menu.exitThisMenu();
        }

        private static bool GetEssential(ItemGrabMenu menu)
        {
            return Reflection.GetField<bool>(menu, "essential").GetValue();
        }
    }
}
