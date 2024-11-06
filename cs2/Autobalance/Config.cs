using System.Text.Json;

namespace AutoBalance;

partial class AutoBalance
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
            ScoreDifference = 5,
            BalanceTimeoutRounds = 5,
            RoundRestartDelay = 10,
            BalanceByKills = false,
            BalanceByScore = true,
            MaxRounds = 30
        };

        File.WriteAllText(configPath,
        JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

        return config;

    }

    public class Config
    {
        public int ScoreDifference { get; set; }
        public int BalanceTimeoutRounds { get; set; }
        public bool BalanceByKills { get; set; }
        public bool BalanceByScore { get; set; }
        public int RoundRestartDelay { get; set; }
        public int MaxRounds { get; set; }
    }
}
