using System.Text.Json;

namespace PistolNoHelmet;

partial class PistolNoHelmet
{
    public Config LoadConfig()
    {
        var configPath = Path.Combine(ModuleDirectory, "config.json");
        if (!File.Exists(configPath)) return CreateConfig(configPath);

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

        return config;
    }

    private Config CreateConfig(string configPath)
    {
        var config = new Config
        {
            MaxRounds = 30,
            BroadcastMessage = false
        };

        File.WriteAllText(configPath,
        JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

        return config;

    }

    public class Config
    {
        public int MaxRounds { get; set; }
        public bool BroadcastMessage { get; set; }
    }
}