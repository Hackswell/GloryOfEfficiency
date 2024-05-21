using GloryOfEfficiency.Core;
using GloryOfEfficiency.Utils;
using StardewValley;
using StardewValley.Tools;

namespace GloryOfEfficiency.Automation
{
    internal class WateringCanRefiller
    {
        private static Config Config => InstanceHolder.Config;
        public static void RefillWateringCan()
        {
            WateringCan can = Util.FindToolFromInventory<WateringCan>(Config.FindCanFromInventory);
            if (can == null || can.WaterLeft >= Util.GetMaxCan(can) ||
                !Util.IsThereAnyWaterNear(Game1.player.currentLocation, Game1.player.Tile))
            {
                return;
            }
            can.WaterLeft = can.waterCanMax;
            Game1.playSound("slosh");
            DelayedAction.playSoundAfterDelay("glug", 250);
        }
    }
}
