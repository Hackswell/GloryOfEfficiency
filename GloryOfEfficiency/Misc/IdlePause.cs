using GloryOfEfficiency.Core;
using GloryOfEfficiency.Huds;
using GloryOfEfficiency.Utils;
using StardewValley;
using static StardewValley.Game1;

namespace GloryOfEfficiency.Misc
{
    internal class IdlePause
    {
        private static Config Conf => InstanceHolder.Config;
        private static double TimeoutCounter { get; set; }
        private static bool Paused { get; set; }
        private static int LastTimeOfDay { get; set; }

        private static int _lastItemIndex;

        private static readonly Logger Logger = new Logger("IdlePause");

        public static void OnTickUpdate()
        {
            if (Conf.PauseWhenIdle)
            {
                if (IsPlayerIdle())
                {
                    TimeoutCounter += Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
                    if (TimeoutCounter > Conf.IdleTimeout * 1000)
                    {
                        if (!Paused)
                        {
                            Logger.Log("Paused game");
                            Paused = true;
                        }

                        Game1.timeOfDay = LastTimeOfDay;
                    }
                }
                else
                {
                    if (Paused)
                    {
                        Paused = false;
                        Logger.Log("Resumed game");
                    }

                    TimeoutCounter = 0;
                    LastTimeOfDay = Game1.timeOfDay;
                }
            }
            else
            {
                Paused = false;
            }
        }

        public static void OnDataLoaded()
        {
            LastTimeOfDay = Game1.timeOfDay;
        }

        public static void DrawHud()
        {
            if (Paused)
            {
                PausedHud.DrawPausedHud();
            }
        }

        private static bool IsPlayerIdle()
        {
            if (paused || !shouldTimePass())
            {
                //When game is paused or time is stopped already. it's not idle.
                return false;
            }

            if (player.CurrentToolIndex != _lastItemIndex)
            {
                //When tool index changed, it's not idle.
                _lastItemIndex = player.CurrentToolIndex;
                return false;
            }

            if (player.isMoving() || player.UsingTool)
            {
                //When player is moving or is using tools, it's not idle of cause.
                return false;
            }

            return true;
        }

    }
}
