using StardewModdingAPI;

namespace GloryOfEfficiency.Utils
{
    public class Logger
    {
        public static IMonitor Monitor
        {
            get; private set;
        }

        public static void Init(IMonitor monitor)
        {
            Monitor = monitor;
        }

        private string Name { get; }
        public Logger(string loggerName)
        {
            Name = loggerName;
        }

        public void Log(string text, LogLevel level = LogLevel.Trace)
        {
            Monitor.Log($"[{Name}]{text}", level);
        }
        public void Trace(string text, LogLevel level = LogLevel.Trace)
        {
            Monitor.Log($"[{Name}]{text}", level);
        }
        public void Debug(string text, LogLevel level = LogLevel.Debug)
        {
            Monitor.Log($"[{Name}{text}", LogLevel.Debug);
        }

        public void Info(string text, LogLevel level = LogLevel.Info)
        {
            Monitor.Log($"[{Name}]{text}", level);
        }
        public void Warn(string text, LogLevel level = LogLevel.Warn)
        {
            Monitor.Log($"[{Name}]{text}", level);
        }

        public void Error(string text, LogLevel level = LogLevel.Error)
        {
            Monitor.Log($"[{Name}]{text}", level);
        }
    }
}
