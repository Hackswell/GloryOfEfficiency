using System.IO;
using System.Linq;
using GloryOfEfficiency.Automation;
using GloryOfEfficiency.EventHandler;
using GloryOfEfficiency.Harmony;
using GloryOfEfficiency.Huds;
using GloryOfEfficiency.Menus;
using GloryOfEfficiency.ModCheckers;
using GloryOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Integrations.GenericModConfigMenu;
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
        private static Config Config => InstanceHolder.Config;

        private static IModHelper Helper => InstanceHolder.Helper;
        public static bool DebugMode { get; private set; }

        private static readonly IManifest ModManifest;
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
                Logger.Debug("CasksOnGround detected.");
                IsCoGOn = true;
            }

            if (ModChecker.IsCaLoaded(helper))
            {
                Logger.Debug("CasksAnywhere detected.");
                IsCaOn = true;
            }

            // Do patching stuff
            if (!Config.SafeMode)
            {
                HarmonyPatcher.DoPatching();
            }
            else
            {
                Logger.Debug("Bypassing patching...");
            }

            helper.WriteConfig(Config);

            if (Config.DontEatThat)
            {
                Logger.Info($"Don't Eat That(tm) is enabled!");
                Helper.Events.Input.ButtonPressed += FoodAutomation.ButtonPressed;
            }

            MineIcons.Init(helper);
        }

        private static void OnReloadConfigCommand(string name, string[] args)
        {
            // Loads configuration from file.
            InstanceHolder.LoadConfig();
            if (Config.DontEatThat)
            {
                Logger.Info($"Don't Eat That(tm) is enabled!");
                FoodAutomation.InitDontEat();
            }
            Logger.Info("Reloaded GoE's config.");
        }

        private static void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (Config.DontEatThat)
            {
                Logger.Info($"Don't Eat That(tm) is enabled!");
                FoodAutomation.InitDontEat();
            }

            GoEMenu.setupGoEMenu();
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
