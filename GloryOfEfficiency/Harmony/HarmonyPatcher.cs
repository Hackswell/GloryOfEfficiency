#nullable enable
using System;
using System.Linq.Expressions;
using System.Reflection;
using GloryOfEfficiency.Utils;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using SVObject = StardewValley.Object;

namespace GloryOfEfficiency.Harmony
{
    internal class HarmonyPatcher
    {
        private static HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.hackswell.goe");

        private static readonly Logger Logger = new Logger("GoE HarmonyPatcher");

        public static void DoPatching()
        {
            harmony.PatchAll();
        }
/*
        /// <summary>
        /// Patches for FTM's burial spot.
        /// </summary>
        internal static class FTMArtifactSpotPatch
        {
            #region delegates
            private static readonly Lazy<Func<SVObject, bool>?> isBuriedItem = new(() =>
            {
                Type? buriedItem = AccessTools.TypeByName("FarmTypeManager.ModEntry+BuriedItems");
                if (buriedItem is null)
                {
                    return null;
                }

                ParameterExpression? obj = Expression.Parameter(typeof(SVObject), "obj");
                TypeBinaryExpression? express = Expression.TypeIs(obj, buriedItem);
                return Expression.Lambda<Func<SVObject, bool>>(express, obj).Compile();
            });

            /// <summary>
            /// Gets whether an item is a MoreGrassStarter grass starter.
            /// </summary>
            internal static Func<SVObject, bool>? IsBuriedItem => isBuriedItem.Value;
            #endregion

            /// <summary>
            /// Applies the patches for this class.
            /// </summary>
            /// <param name="harmony">My harmony instance.</param>
            internal static void ApplyPatch(HarmonyLib.Harmony harmony)
            {
                Type? buriedItem = AccessTools.TypeByName("FarmTypeManager.ModEntry+BuriedItems");
                if (buriedItem is null)
                {
                    Logger.Log($"Farm Type Manager's buried items may not behave correctly if dug up with the tool.", LogLevel.Error);
                    return;
                }

                try
                {
                    harmony.Patch(
                        original: buriedItem.GetMethod("performToolAction", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy),
                        prefix: new HarmonyMethod(typeof(FTMArtifactSpotPatch).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)));
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to patch FTM to support artifact spots.\n\n{ex}", LogLevel.Error);
                }
            }

            private static bool Prefix(SVObject __instance, Tool t, GameLocation location, ref bool __result)
            {
                // Until I know if we need the Prefix() method or not...
                // We don't have a ShovelTool in this mod...
                //if (t is not ShovelTool shovel)
                //return true;

                try
                {
                    __result = true;
                    MethodInfo method = __instance.GetType().GetMethod("releaseContents", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                    method.Invoke(__instance, new[] { location });

                    if (!location.terrainFeatures.ContainsKey(__instance.TileLocation))
                    {
                        location.makeHoeDirt(__instance.TileLocation);
                    }
                    location.playSound("hoeHit");
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed while trying to use tool on FTM artifact spot:\n\n{ex}", LogLevel.Error);
                }

                return true;


            }
        }
*/
    }
}
