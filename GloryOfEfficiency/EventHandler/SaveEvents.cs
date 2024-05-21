using System;
using GloryOfEfficiency.Automation;
using GloryOfEfficiency.Core;
using GloryOfEfficiency.Misc;
using StardewModdingAPI;

namespace GloryOfEfficiency.EventHandler
{
    internal class SaveEvents
    {
        private static Config Conf => InstanceHolder.Config;

        public void OnBeforeSave(object sender, EventArgs args)
        {
            if (!Context.IsWorldReady || !Conf.AutoAnimalDoor)
            {
                return;
            }
            AnimalAutomation.LetAnimalsInHome();
            AnimalAutomation.AutoCloseAnimalDoor();
        }

        public void OnDayStarted(object sender, EventArgs args)
        {
            if (!Context.IsWorldReady || !Conf.AutoAnimalDoor)
            {
                return;
            }
            AnimalAutomation.AutoOpenAnimalDoor();
            UpdateEvents.DayEnded = false;
            IdlePause.OnDataLoaded();
        }
    }
}
