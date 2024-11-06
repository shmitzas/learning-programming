using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace WeaponRestrict;
public class WeaponRestrict : BasePlugin
{
    public override string ModuleName => "WeaponRestrict";
    public override string ModuleAuthor => "Shmitz";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleDescription => "Restricts weapon buying/pickup in certain conditions";

    private Config _cfg;
    private List<Weapon> _weapons;
    private List<CCSPlayerController> _players;
    private string _prefix = $" {ChatColors.Default}[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}]";
    //private List<string> _playersWithGun;

    public override void Load(bool hotReload)
    {
        _cfg = new Config();
        _players = new List<CCSPlayerController>();
        //_playersWithGun = new List<string>();
        Init();
    }

    public void Init()
    {
        InitEvents();
        _cfg = LoadConfig();
        _weapons = _cfg.Weapons;
    }

    [RequiresPermissions("@css/rcon")]
    [ConsoleCommand("css_wpr_reload")]
    public void RefreshConfig(CCSPlayerController? player, CommandInfo command)
    {
        try
        {
            Init();
            if (player != null)
            {
                player.PrintToChat($"{ChatColors.DarkRed}[WeaponRestrict] {ChatColors.Default}Config reloaded");
            }
            else
            {
                Logger.LogInformation($"Config reloaded");
            }
        }
        catch (Exception ex) 
        {
            if (player != null)
            {
                player.PrintToChat($"{ChatColors.DarkRed}[WeaponRestrict] {ChatColors.Default}Config failed to reload");
            }
            else
            {
                Logger.LogError($"Config failed to reload: {ex}");
            }
        }
        
    }

    private void RefreshPlayersList()
    {
        _players = Utilities.GetPlayers().Where(IsPlayerValid).ToList();
    }

    private int CountWeaponsOnTeam(string weaponName, CsTeam team)
    {
        int count = 0;
        try
        {
            RefreshPlayersList();
            weaponName = "weapon_" + weaponName;
            var players = _players.Where(p => p.Team == team).ToList();
            //_playersWithGun.Clear();
            foreach (CCSPlayerController player in players)
            {
                // Get all weapons (ignore null dereference because we already null checked these)
                foreach (CHandle<CBasePlayerWeapon> weapon in player.PlayerPawn.Value!.WeaponServices!.MyWeapons)
                {
                    //Get the item DesignerName and compare it to the counted name
                    if (weapon.Value is null || weapon.Value.DesignerName != weaponName) continue;
                    // Increment count if weapon is found
                    count++;
                    //_playersWithGun.Add(player.PlayerName);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"CountWeaponsOnTeam: {ex}");
        }

        return count;
    }

    public static bool IsPlayerValid(CCSPlayerController? player)
    {
        return player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, TeamNum: 2 or 3 };
    }

    private bool IsItemAllowed(CCSPlayerController player, string weaponName, bool broadcastMessage = false)
    {
        
        var weaponToEquip = weaponName.ToLower();
        try
        {
            Weapon weaponData = new();
            try
            {
                weaponData = _weapons.First(w => w.Name == weaponToEquip);
            }
            catch (Exception ex)
            {
                return true;
            }
            if (weaponData != null)
            {
                var existingSelectedWeapons = CountWeaponsOnTeam(weaponToEquip, player.Team);
                //if(_playersWithGun.Count > 0)
                //{
                //    Logger.LogInformation($"Weapon to check: {weaponToEquip}, Team: {player.Team}\nPlayers with weapon: {_playersWithGun.Count}\n{_playersWithGun.ToArray()}");
                //}
                var weaponLimit = weaponData.Limits.OrderByDescending(l => l.TotalPlayers).First(l => l.TotalPlayers <= _players.Count());
                //Console.WriteLine($"Player Count: {_players.Count()} | Weapon: {weaponData.Name} | Limit: {weaponLimit.LimitPerTeam} | Existing: {existingSelectedWeapons}");
                if (existingSelectedWeapons > weaponLimit.LimitPerTeam)
                {
                    if (broadcastMessage)
                        player.PrintToChat($"{_prefix} {ChatColors.Purple}{weaponName} {ChatColors.Default}is restricted to {ChatColors.LightRed}{weaponLimit.LimitPerTeam} {ChatColors.Default}players per team");

                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"IsItemAllowed: {ex}");
        }
        return true;
    }

    private void RoundStartWeaponCheck()
    {
        RefreshPlayersList();
        try
        {
            var weaponName = "";
            foreach (var player in _players)
            {
                foreach (var weapon in player!.PlayerPawn.Value!.WeaponServices!.MyWeapons)
                {
                    try
                    {
                        weaponName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(weapon.Value.DesignerName.Replace("weapon_", ""));
                    }
                    catch 
                    {
                        continue;
                    }

                    if (!IsItemAllowed(player, weaponName))
                    {
                        var refundValue = _weapons.First(w => w.Name == weaponName.ToLower()).Price;
                        player.ExecuteClientCommand("slot2");
                        player.RemoveItemByDesignerName("weapon_" + weaponName.ToLower(), false);
                        
                        player.InGameMoneyServices!.Account += refundValue;
                        player.PrintToChat($"{_prefix}Refunded {ChatColors.Purple}{ weaponName} {ChatColors.Default}for {ChatColors.Lime}${refundValue}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"RoundStartWeaponCheck: {ex}");
        }
    }

    public void InitEvents()
    {
        RegisterEventHandler<EventItemPurchase>((@event, info) =>
        {
            try
            {
                var player = @event.Userid;
                var weaponName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(@event.Weapon.Replace("weapon_", ""));

                if (player == null)
                    return HookResult.Continue;

                if (!IsItemAllowed(player, weaponName))
                {

                    var refundValue = _weapons.First(w => w.Name == weaponName.ToLower()).Price;
                    player.ExecuteClientCommand("slot2");
                    player.RemoveItemByDesignerName("weapon_" + weaponName.ToLower(), false);
                    player.InGameMoneyServices!.Account += refundValue;
                    player.PrintToChat($"{_prefix} Refunded {ChatColors.Purple}{weaponName} {ChatColors.Default}for {ChatColors.Lime}${refundValue}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"EventItemPurchase: {ex}");
            }

            return HookResult.Continue;
        }, HookMode.Pre);

        RegisterEventHandler<EventItemPickup>((@event, info) =>
        {
            try
            {
                var player = @event.Userid;
                var weaponName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(@event.Item.Replace("weapon_", ""));

                if (player == null) 
                    return HookResult.Continue;

                if (!IsItemAllowed(player, weaponName, true))
                {
                    player.ExecuteClientCommand("slot2");
                    player.RemoveItemByDesignerName("weapon_" + weaponName.ToLower(), false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"EventItemPickup: {ex}");
            }

            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            RoundStartWeaponCheck();
            return HookResult.Continue;
        });
    }

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
            Weapons = new List<Weapon>
            {
                new()
                {
                    Name = "awp",
                    Price = 4750,
                    Limits = new List<WeaponLimit>
                    {
                        new()
                        {
                            TotalPlayers = 0,
                            LimitPerTeam = 0
                        },
                        new()
                        {
                            TotalPlayers = 10,
                            LimitPerTeam = 2
                        },
                        new()
                        {
                            TotalPlayers = 16,
                            LimitPerTeam = 3
                        },
                        new()
                        {
                            TotalPlayers = 20,
                            LimitPerTeam = 4
                        }
                    },
                },
                new()
                {
                    Name = "scar20",
                    Price = 5000,
                    Limits = new List<WeaponLimit>
                    {
                        new()
                        {
                            TotalPlayers = 0,
                            LimitPerTeam = 0
                        },
                        new()
                        {
                            TotalPlayers = 16,
                            LimitPerTeam = 1
                        },
                        new()
                        {
                            TotalPlayers = 21,
                            LimitPerTeam = 2
                        }
                    },
                },
                new()
                {
                    Name = "g3sg1",
                    Price = 5000,
                    Limits = new List<WeaponLimit>
                    {
                        new()
                        {
                            TotalPlayers = 0,
                            LimitPerTeam = 0
                        },
                        new()
                        {
                            TotalPlayers = 16,
                            LimitPerTeam = 1
                        },
                        new()
                        {
                            TotalPlayers = 21,
                            LimitPerTeam = 2
                        }
                    },
                },
                new()
                {
                    Name = "negev",
                    Price = 1700,
                    Limits = new List<WeaponLimit>
                    {
                        new()
                        {
                            TotalPlayers = 0,
                            LimitPerTeam = 0
                        },
                        new()
                        {
                            TotalPlayers = 16,
                            LimitPerTeam = 1
                        },
                        new()
                        {
                            TotalPlayers = 21,
                            LimitPerTeam = 2
                        }
                    },
                }
            }
        };

        File.WriteAllText(configPath,
        JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

        return config;

    }

    public class Config
    {
        public List<Weapon> Weapons {  get; set; }
    }
    public class Weapon
    {
        public string Name { get; set; }
        public int Price {  get; set; }
        public List<WeaponLimit> Limits { get; set; }
    }
    public class WeaponLimit
    {
        public int TotalPlayers { get; set; }
        public int LimitPerTeam { get; set; }
    }
}
