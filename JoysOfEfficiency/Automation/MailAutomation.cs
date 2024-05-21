﻿using System;
using System.Linq;
using JoysOfEfficiency.Utils;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;

namespace JoysOfEfficiency.Automation
{
    internal class MailAutomation
    {
        private static readonly Logger Logger = new Logger("MailAutomation");

        public static void CollectMailAttachmentsAndQuests(LetterViewerMenu menu)
        {
            String questId = menu.questID;

            if (menu.itemsLeftToGrab())
            {
                foreach (ClickableComponent component in menu.itemsToGrab.ToArray())
                {
                    if (component.item == null || !CanPlayerAcceptsItemPartially(component.item))
                    {
                        continue;
                    }

                    int stack = component.item.Stack;
                    Game1.playSound("coin");
                    int remain = Util.AddItemIntoInventory(component.item);

                    Logger.Log($"You collected {component.item.DisplayName}{(stack - remain > 1 ? " x" + (stack - remain) : "")}.");
                    if (remain == 0)
                    {
                        component.item = null;
                    }
                    else
                    {
                        component.item.Stack = remain;
                    }
                }
            }

            if (questId == null)
            {
                return;
            }

            Logger.Log($"You started Quest: '{Quest.getQuestFromId(questId).questTitle}'.");
            Game1.player.addQuest(questId);
            Game1.playSound("newArtifact");
            menu.questID = null;
        }

        private static bool CanPlayerAcceptsItemPartially(Item item)
        {
            if (Game1.player.Items.Contains(null) || Game1.player.Items.Count < Game1.player.MaxItems)
            {
                // Inventory includes at least one free space.
                return true;
            }

            return Game1.player.Items.Any(stack => stack.canStackWith(item) && stack.Stack < stack.maximumStackSize());
        }
    }
}
