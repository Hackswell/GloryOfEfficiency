using HarmonyLib;

namespace GloryOfEfficiency.Harmony
{
    internal class HarmonyPatcher
    {
        private static HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.pome.joe");

        public static void DoPatching()
        {
            harmony.PatchAll();
        }

    }
}
