using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Core.Capabilities;
using K4SharedApi;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using System.Numerics;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;

namespace K4SystemGunXP;

public sealed class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("Weapons")]
    public List<Weapon> Weapons { get; set; } =
    [
        new ("weapon_hkp2000",        "P2000",         minimumXP: 0),
        new ("weapon_glock",          "Glock-18",      minimumXP: 500),
        new ("weapon_usp_silencer",   "USP-S",         minimumXP: 1300),
        new ("weapon_p250",           "P250",          minimumXP: 2100),
        new ("weapon_elite",          "Dual Berettas", minimumXP: 3000),
        new ("weapon_cz75a",          "CZ75-Auto",     minimumXP: 4800),
        new ("weapon_fiveseven",      "Five-SeveN",    minimumXP: 3900),
        new ("weapon_tec9",           "Tec-9",         minimumXP: 5700),
        new ("weapon_revolver",       "R8 Revolver",   minimumXP: 6600),
        new ("weapon_deagle",         "Desert Eagle",  minimumXP: 7500),
        new ("weapon_mac10",          "MAC-10",        minimumXP: 8400),
        new ("weapon_mp5sd",          "MP5-SD",        minimumXP: 9300),
        new ("weapon_ump45",          "UMP-45",        minimumXP: 10200),
        new ("weapon_bizon",          "PP-Bizon",      minimumXP: 11100),
        new ("weapon_mp7",            "MP7",           minimumXP: 12000),
        new ("weapon_mp9",            "MP9",           minimumXP: 12900),
        new ("weapon_p90",            "P90",           minimumXP: 13800),
        new ("weapon_sawedoff",       "Sawed-Off",     minimumXP: 14700),
        new ("weapon_mag7",           "MAG-7",         minimumXP: 15600),
        new ("weapon_nova",           "Nova",          minimumXP: 16500),
        new ("weapon_xm1014",         "XM1014",        minimumXP: 17400),
        new ("weapon_m249",           "M249",          minimumXP: 18300),
        new ("weapon_negev",          "Negev",         minimumXP: 19200),
        new ("weapon_galilar",        "Galil AR",      minimumXP: 20100),
        new ("weapon_famas",          "FAMAS",         minimumXP: 21000),
        new ("weapon_aug",            "AUG",           minimumXP: 21900),
        new ("weapon_sg553",          "SG 553",        minimumXP: 22800),
        new ("weapon_ak47",           "AK-47",         minimumXP: 23700),
        new ("weapon_m4a1",           "M4A4",          minimumXP: 24600),
        new ("weapon_m4a1_silencer",  "M4A1-S",        minimumXP: 25500),
        new ("weapon_ssg08",          "SSG 08",        minimumXP: 26400),
        new ("weapon_awp",            "AWP",           minimumXP: 27300),
        new ("weapon_g3sg1",          "G3SG1",         minimumXP: 28200),
        new ("weapon_scar20",         "SCAR-20",       minimumXP: 29100),
    ];

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
}

public sealed class Weapon
{
    public string GameName { get; set; }
    public string GiveName { get; set; }
    public int MinimumXP { get; set; }

    public Weapon(string gameName, string giveName, int minimumXP)
    {
        GameName = gameName;
        GiveName = giveName;
        MinimumXP = minimumXP;
    }
}

public class RankInfo()
{
    public int CurrentPoints { get; set; }
    public string CurrentWeapon { get; set; }
    public int NextLevelPoints { get; set; }
    public string NextWeapon { get; set; }
    public string ActiveWeapon { get; set; }
}

public sealed partial class PluginK4SystemGunXP : BasePlugin, IPluginConfig<PluginConfig>
{

    public override string ModuleName => "K4-System GunXP";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Shmitz";

    public bool AllowUpdate = false;

    public readonly string _prefix = $"[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}]";

    public required PluginConfig Config { get; set; } = new PluginConfig();
    public Dictionary<CCSPlayerController, RankInfo> GunLevel { get; set; } = new();
    public List<Dictionary<CCSPlayerController, RankInfo>> DisplayRanksList { get; set; } = new();

    public Dictionary<int, string> ActiveWeapon { get; set; } = new();
    public List<Dictionary<int, string>> ActiveWeapons { get; set; } = new();
    public List<int> PlayersChangedActiveWeapon { get; set; } = new();

    public void OnConfigParsed(PluginConfig config)
    {
        if (config.Version < Config.Version)
            base.Logger.LogWarning("Configuration version mismatch (Expected: {0} | Current: {1})", this.Config.Version, config.Version);

        this.Config = config;
    }

    public static PlayerCapability<IPlayerAPI> Capability_SharedAPI { get; } = new("k4-system:sharedapi-player");

    public override void Load(bool hotReload)
    {        
        InitEvents();
    }

    [ConsoleCommand("css_guns")]
    [ConsoleCommand("css_gun")]
    public void OnGuns(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        if (!player.PawnIsAlive)
        {
            player.PrintToChat($"{_prefix} {ChatColors.Red}You must be alive to use this command");
            return;
        }

            var h = GetPlayerDetails(player);
        if (h == null)
        {
            Logger.LogError("Failed to get player details");
            player.PrintToChat($"{_prefix} {ChatColors.Red}Failed to get player details");
            return;
        }

        if (player.AuthorizedSteamID == null)
        {
            Logger.LogError("Failed to get player steam ID");
            player.PrintToChat($"{_prefix} {ChatColors.Red}Failed to get player steam ID");
            return;
        }

        var weapons = GetAvailableWeapons(h!.Points);
        if (weapons == null)
        {
            Logger.LogError("Failed to get available weapons");
            player.PrintToChat($"{_prefix} {ChatColors.Red}Failed to get available weapons");
            return;
        }
        if (weapons.Count == 0)
        {
            player.PrintToChat($"{_prefix} {ChatColors.Red}No weapons available");
            return;
        }

        var gunMenu = new CenterHtmlMenu("Available weapons", this);
        var currentWeapon = string.Empty;

        foreach (var weapon in weapons)
            gunMenu.AddMenuOption(weapon.GiveName, SetActiveWeapon);
        
        MenuManager.OpenCenterHtmlMenu(this, player, gunMenu);
        
    }

    public void SetActiveWeapon(CCSPlayerController player, ChatMenuOption option)
    {
        try
        {
            var weapon_name = GetWeaponGameName(option.Text);
            if (string.IsNullOrEmpty(weapon_name))
            {
                Logger.LogError("Failed to get selected weapon details");
                player.PrintToChat($"{_prefix} {ChatColors.Red}Failed to get selected weapon details");
                return;
            }

            if (player.AuthorizedSteamID == null)
            {
                Logger.LogError("Failed to get player steam ID");
                player.PrintToChat($"{_prefix} {ChatColors.Red}Failed to get player steam ID");
                return;
            }

            int playerId = player.AuthorizedSteamID.AccountId;

            // Remove existing active weapon for the player if it exists
            ActiveWeapons.RemoveAll(ActiveWeapon => ActiveWeapon.ContainsKey(playerId));

            // Add new active weapon for the player
            ActiveWeapons.Add(new Dictionary<int, string> { { playerId, weapon_name } });
            player.PrintToChat($"{_prefix} Changed active weapon to {ChatColors.Gold}{option.Text}");
            
            if (!PlayersChangedActiveWeapon.Contains(playerId)) PlayersChangedActiveWeapon.Add(playerId);
            MenuManager.CloseActiveMenu(player);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to set active weapon");
        }
    }
}
