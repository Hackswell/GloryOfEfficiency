﻿using GloryOfEfficiency.Huds;
using StardewModdingAPI.Events;

namespace GloryOfEfficiency.EventHandler
{
    internal class EventHolder
    {
        public static UpdateEvents Update { get;} = new UpdateEvents();
        public static GraphicsEvents Graphics { get; } = new GraphicsEvents();
        public static SaveEvents Save { get; } = new SaveEvents();
        public static MenuEvents Menu { get; } = new MenuEvents();
        public static InputEvents Input { get; } = new InputEvents();

        public static void RegisterEvents(IModEvents events)
        {
            events.Input.ButtonPressed += Input.OnButtonPressed;

            events.GameLoop.UpdateTicked += Update.OnGameUpdateEvent;
            events.GameLoop.Saving += Save.OnBeforeSave;
            events.GameLoop.DayStarted += Save.OnDayStarted;

            events.Display.RenderingHud += Graphics.OnRenderHud;
            events.Display.RenderedActiveMenu += Graphics.OnPostRenderGui;
            events.Display.MenuChanged += Menu.OnMenuChanged;
            events.Display.RenderingHud += FpsCounter.OnHudDraw;
            events.Display.RenderedHud += FpsCounter.PostHudDraw;
        }
    }
}
