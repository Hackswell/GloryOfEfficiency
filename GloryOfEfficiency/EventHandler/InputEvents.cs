using GloryOfEfficiency.Automation;
using GloryOfEfficiency.Core;
using GloryOfEfficiency.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GloryOfEfficiency.EventHandler
{
    internal class InputEvents
    {
        private static Config Conf => InstanceHolder.Config;
        public void OnButtonPressed(object sender, ButtonPressedEventArgs args)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }
            if (args.Button == Conf.ToggleAfkFishing)
            {
                AutoFisher.ToggleAfkFishing();
            }
            if (!Context.IsPlayerFree || Game1.activeClickableMenu != null)
            {
                return;
            }
            if (args.Button == Conf.ButtonShowMenu)
            {
                //Open Up Menu
                GoEMenu.OpenMenu();
            }
            else if (args.Button == Conf.ButtonToggleBlackList)
            {
                HarvestAutomation.ToggleBlacklistUnderCursor();
            }
            else if (args.Button == Conf.ButtonToggleFlowerColorUnification)
            {
                FlowerColorUnifier.ToggleFlowerColorUnification();
            }
        }
    }
}
