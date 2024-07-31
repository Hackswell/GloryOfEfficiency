using System.IO;
using GloryOfEfficiency.Automation;
using GloryOfEfficiency.EventHandler;
using GloryOfEfficiency.Harmony;
using GloryOfEfficiency.Huds;
using GloryOfEfficiency.ModCheckers;
using GloryOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GloryOfEfficiency.Core
{
    using Player = Farmer;

    /// <summary>
    /// This class is a representation of the mod itself.
    /// </summary>
    internal class ModEntry : Mod
    {
        public static bool IsCoGOn { get; private set; }
        public static bool IsCaOn { get; private set; }
        private static Config Conf => InstanceHolder.Config;
        public static bool DebugMode { get; private set; }

        private static readonly Logger Logger = new Logger("Main");

        /// <summary>
        /// Called firstly when SMAPI finished loading of the mod.
        /// </summary>
        /// <param name="helper"></param>
        public override void Entry(IModHelper helper)
        {
            // Initialize Logger
            Logger.Init(Monitor);

            // Initialize InstanceHolder.
            InstanceHolder.Init(this);

            // Register events.
            EventHolder.RegisterEvents(Helper.Events);

            // Limit config values.
            ConfigLimitation.LimitConfigValues();

            // Registration commands.
            Helper.ConsoleCommands.Add("goedebug", "Debug command for GoE", OnDebugCommand);
            Helper.ConsoleCommands.Add("goerelcon", "Reloading config command for GoE", OnReloadConfigCommand);
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            // Check mod compatibilities.
            if(ModChecker.IsCoGLoaded(helper))
            {
                Logger.Log("CasksOnGround detected.");
                IsCoGOn = true;
            }

            if (ModChecker.IsCaLoaded(helper))
            {
                Logger.Log("CasksAnywhere detected.");
                IsCaOn = true;
            }

            // Do patching stuff
            if (!Conf.SafeMode)
            {
                HarmonyPatcher.DoPatching();
            }
            else
            {
                Logger.Log("Bypassing patching...");
            }

            helper.WriteConfig(Conf);

            if (Conf.DontEatThat)
            {
                Logger.Log($"Don't Eat That(tm) is enabled!");
                Helper.Events.Input.ButtonPressed += FoodAutomation.ButtonPressed;
            }

            MineIcons.Init(helper);
        }

        private static void OnReloadConfigCommand(string name, string[] args)
        {
            // Loads configuration from file.
            InstanceHolder.LoadConfig();
            if (Conf.DontEatThat)
            {
                Logger.Log($"Don't Eat That(tm) is enabled!");
                FoodAutomation.InitDontEat();
            }
            Logger.Log("Reloaded JoE's config.");
        }

        private static void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (Conf.DontEatThat)
            {
                Logger.Log($"Don't Eat That(tm) is enabled!");
                FoodAutomation.InitDontEat();
            }
        }

        private static void OnDebugCommand(string name, string[] args)
        {
            DebugMode = !DebugMode;
        }

        public string GetFilePath(string fileName)
        {
            return Path.Combine(Helper.DirectoryPath, fileName);
        }
    }
}
