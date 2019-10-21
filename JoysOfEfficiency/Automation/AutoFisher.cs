﻿using JoysOfEfficiency.Core;
using JoysOfEfficiency.Utils;
using StardewModdingAPI;
using StardewValley.Menus;

namespace JoysOfEfficiency.Automation
{
    internal class AutoFisher
    {

        private static bool CatchingTreasure { get; set; }
        private static int AutoFishingCounter { get; set; }

        private static IReflectionHelper Reflection => InstanceHolder.Reflection;

        public static void AutoFishing(BobberBar bar)
        {
            AutoFishingCounter = (AutoFishingCounter + 1) % 3;
            if (AutoFishingCounter > 0)
            {
                return;
            }


            IReflectedField<float> bobberSpeed = Reflection.GetField<float>(bar, "bobberBarSpeed");

            float barPos = Reflection.GetField<float>(bar, "bobberBarPos").GetValue();
            int barHeight = Reflection.GetField<int>(bar, "bobberBarHeight").GetValue();
            float fishPos = Reflection.GetField<float>(bar, "bobberPosition").GetValue();
            float treasurePos = Reflection.GetField<float>(bar, "treasurePosition").GetValue();
            float distanceFromCatching = Reflection.GetField<float>(bar, "distanceFromCatching").GetValue();
            bool treasureCaught = Reflection.GetField<bool>(bar, "treasureCaught").GetValue();
            bool treasure = Reflection.GetField<bool>(bar, "treasure").GetValue();
            float treasureApeearTimer = Reflection.GetField<float>(bar, "treasureAppearTimer").GetValue();
            float bobberBarSpeed = bobberSpeed.GetValue();

            float top = barPos;

            if (treasure && treasureApeearTimer <= 0 && !treasureCaught)
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

            if (fishPos > barPos + (barHeight << 1))
            {
                return;
            }

            float strength = (fishPos - (barPos + (barHeight << 1))) / 16f;
            float distance = fishPos - top;

            float threshold = Util.Cap(InstanceHolder.Config.CpuThresholdFishing, 0, 0.5f);
            if (distance < threshold * barHeight || distance > (1 - threshold) * barHeight)
            {
                bobberBarSpeed = strength;
            }

            bobberSpeed.SetValue(bobberBarSpeed);
        }
    }
}
