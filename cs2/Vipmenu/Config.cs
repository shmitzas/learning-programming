

using System.Text.Json;

namespace Vipmenu
{
    public partial class Vipmenu
    {
        private Config CreateConfig(string configPath)
        {
            Config config = new();

            File.WriteAllText(configPath,
                JsonSerializer.Serialize(new Config(), new JsonSerializerOptions { WriteIndented = true }));

            return config;
        }
        private Config LoadConfig()
        {
            var configPath = Path.Combine(ModuleDirectory, "config.json");
            if (!File.Exists(configPath)) return CreateConfig(configPath);

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

            return config;
        }

        public class Config
        {
            public int VipMenuMaxUse { get; set; } = 3;
            public int MaxRounds { get; set; } = 30;
            public int RoundRestartDelay { get; set; } = 10;
        }
    }
}
