using System.IO;
using System.Text.Json;

namespace GloryOfEfficiency.Utils
{
    public abstract class ConfigHolder<T>
    {
        private readonly string _configFileName;

        public T Entry { get; private set; }

        private static Logger Logger = new Logger("ConfigHolder");

        protected ConfigHolder(string filePath)
        {
            _configFileName = filePath;
            Load();
            Save();
        }

        protected void Load()
        {
            if (!File.Exists(_configFileName))
            {
                Entry = GetNewInstance();
                return;
            }

            Logger.Log("Loaded "+ _configFileName);

            string jsonContent = File.ReadAllText(_configFileName);
            Entry = JsonSerializer.Deserialize<T>(jsonContent);
        }

        public void Save()
        {
            string jsonContent = JsonSerializer.Serialize(Entry);
            File.WriteAllText(_configFileName, jsonContent);
            Logger.Log("Saved " + Path.GetFullPath(_configFileName));
        }

        protected abstract T GetNewInstance();
    }
}
