using System.IO;
using JoysOfEfficiency.Automation;
using JoysOfEfficiency.EventHandler;
using JoysOfEfficiency.Harmony;
using JoysOfEfficiency.Huds;
using JoysOfEfficiency.ModCheckers;
using JoysOfEfficiency.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JoysOfEfficiency.Core
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
            Helper.ConsoleCommands.Add("joedebug", "Debug command for JoE", OnDebugCommand);
            Helper.ConsoleCommands.Add("joerelcon", "Reloading config command for JoE", OnReloadConfigCommand);
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

            Game1.graphics.PreferredBackBufferWidth = Conf.WindowWidth;
            Game1.graphics.PreferredBackBufferHeight = Conf.WindowHeight;
            var viewport = Game1.graphics.GraphicsDevice.Viewport;
            viewport.Width = Conf.WindowWidth;
            viewport.Height = Conf.WindowHeight;
//            viewport.X = Conf.WindowX;             // Hackswell: seems to do nothing
//            viewport.Y = Conf.WindowY;             // Hackswell: seems to do nothing
//            viewport.Bounds = new Rectangle(new Point(Conf.WindowX, Conf.WindowY), new Point(Conf.WindowWidth, Conf.WindowHeight)); // Hackswell: seems to do nothing
            GameRunner.instance.Window.Position = new Point(Conf.WindowX, Conf.WindowY);        // Hackswell: seems to do nothing
            Game1.graphics.ApplyChanges();
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
