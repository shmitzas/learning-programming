using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Vipmenu;

public partial class Vipmenu : BasePlugin
{
    public override string ModuleName => "VipMenu";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "Shmitz";
    public override string ModuleDescription => "Gun menu for VIPs";

    private readonly string _prefix = $"[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}]";

    public int RoundsPlayed = 0;
    public Dictionary<int, int> VipmenuUsed = [];
    private Config _config = new();
    private WeaponHelper _weapons = new();
    private bool LastRoundHalf = false;
    private bool PistolRound = true;
    private List<CCSPlayerController> _playersWithOpenVipmenu = [];

    public override void Load(bool hotReload)
    {
        Init();
        InitEvents();
        _config = LoadConfig();
    }

    public void Init()
    {
        _config = LoadConfig();
        LastRoundHalf = false;
        PistolRound = true;
        VipmenuUsed = [];
    }

    [RequiresPermissions("@css/reservation")]
    [ConsoleCommand("css_guns")]
    [ConsoleCommand("css_gunmenu")]
    [ConsoleCommand("css_vipmenu")]
    [ConsoleCommand("css_vipmeniu")]
    public void CallVipmenu(CCSPlayerController? player, CommandInfo info)
    {
        try
        {
            if (player == null)
            {
                Logger.LogError("Player is null");
                return;
            }
            if (player.AuthorizedSteamID == null)
            {
                Logger.LogError("Player SteamID is null");
                return;
            }

            if (!ValidatePlayer(player))
                return;

            try
            {
                if (!VipmenuUsed.ContainsKey(player.AuthorizedSteamID.AccountId))
                    VipmenuUsed.Add(player.AuthorizedSteamID.AccountId, 0);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                VipmenuUsed.Add(player.AuthorizedSteamID.AccountId, 0);
            }

            int callCount = VipmenuUsed[player.AuthorizedSteamID.AccountId];

            if (callCount < _config.VipMenuMaxUse)
            {
                if (!_playersWithOpenVipmenu.Contains(player))
                    _playersWithOpenVipmenu.Add(player);
                else
                {
                    Logger.LogError("Player already has open vipmenu or _playersWithOpen");
                }
                var menu = MenuManager.GetActiveMenu(player);
                menu?.Close();

                var gunMenu = new CenterHtmlMenu("Vipmenu", this);
                if (RoundsPlayed == 0 || RoundsPlayed == _config.MaxRounds / 2)
                {
                    if (GetPistols(gunMenu))
                        MenuManager.OpenCenterHtmlMenu(this, player, gunMenu);
                }
                else
                {
                    if (GetGuns(gunMenu))
                        MenuManager.OpenCenterHtmlMenu(this, player, gunMenu);
                }
                VipmenuUsed[player.AuthorizedSteamID.AccountId] = callCount + 1;
            }
            else
            {
                player.PrintToChat($"{_prefix} Wait for next round to use {ChatColors.Gold}!vipmenu");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
        }
    }

    [RequiresPermissions("@css/rcon")]
    [ConsoleCommand("css_vipmenu_reload")]
    public void OnCmdReload(CCSPlayerController? player, CommandInfo info)
    {
        Init();

        if (player == null)
            Logger.LogInformation("Configuration successfully rebooted");
        else
            player?.PrintToChat($"{_prefix} Configuration successfully rebooted");
    }

    private bool ValidatePlayer(CCSPlayerController player)
    {
        if (player == null || player.IsBot || !player.IsValid)
        {
            return false;
        }

        return IsPlayerAlive(player);
    }

    private bool IsPlayerAlive(CCSPlayerController player)
    {
        if (player != null && !player.IsBot && player.PawnIsAlive)
        {
            return true;
        }
        player?.PrintToChat($"{_prefix} You must be {ChatColors.Lime}alive {ChatColors.Default}to use {ChatColors.Gold}!vipmenu");
        return false;
    }

    private bool GetGuns(CenterHtmlMenu gunMenu, WeaponType? type = null)
    {
        var weaponsList = _weapons.GetGunList();
        foreach (var item in weaponsList)
        {
            gunMenu.AddMenuOption(item, GiveSelectedItem);
        }
        return true;
    }

    private bool GetPistols(CenterHtmlMenu gunMenu, WeaponType? type = null)
    {
        var weaponsList = _weapons.GetPistolList();
        foreach (var item in weaponsList)
        {
            gunMenu.AddMenuOption(item, GiveSelectedItem);
        }
        return true;
    }

    private void GiveSelectedItem(CCSPlayerController player, ChatMenuOption option)
    {
        try
        {
            if (_playersWithOpenVipmenu == null || _weapons == null)
            {
                Logger.LogError("Dependencies are not initialized");
                return;
            }

            _playersWithOpenVipmenu.Remove(player);
            MenuManager.CloseActiveMenu(player);
            var selectedWeapon = _weapons.GetGameName(option.Text);

            if (player == null || player.PlayerPawn == null || player.PlayerPawn.Value == null || player.PlayerPawn.Value.WeaponServices == null) return;
            try { RemoveCurrentWeapon(player, selectedWeapon); }
            catch (Exception ex)
            {
                Logger.LogError($"While removing weapon: {ex}");
            }
            player.GiveNamedItem(selectedWeapon);
        }
        catch (Exception ex)
        {
            Logger.LogError($"GiveSelectedItem: {ex}");
        }
    }

    private bool RemoveCurrentWeapon(CCSPlayerController player, string selectedWeapon)
    {
        try
        {
            if (player == null || player.PlayerPawn == null || player.PlayerPawn.Value == null || player.PlayerPawn.Value.WeaponServices == null)
            {
                Logger.LogError("Player or PlayerPawn is null");
                return false;
            }
            if (_weapons == null)
            {
                Logger.LogError("Weapons is null");
                return false;
            }
            var selectedWeaponType = _weapons.GetWeaponType(selectedWeapon);
            foreach (var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
            {
                if (weapon.Value == null || string.IsNullOrEmpty(weapon.Value.DesignerName)) continue;

                var weaponName = weapon.Value.DesignerName;

                if (!string.IsNullOrWhiteSpace(weaponName) && !string.IsNullOrEmpty(weaponName) && weaponName != "[null]")
                {
                    var currentWeaponType = _weapons.GetWeaponType(weaponName);

                    if(currentWeaponType == WeaponType.None) continue;

                    if (selectedWeaponType == WeaponType.Primary && currentWeaponType == WeaponType.Primary)
                        return player.RemoveItemByDesignerName(weaponName);
                    
                    if (selectedWeaponType == WeaponType.Secondary && currentWeaponType == WeaponType.Secondary)
                        return player.RemoveItemByDesignerName(weaponName);
                }
            }
        }
        catch (Exception ex)
        {
            player.PrintToChat($"{_prefix} Failed to give {ChatColors.Grey}{selectedWeapon}");
            Logger.LogError(ex.ToString());
        }
        return false;
    }

    private void CloseActiveVipmenus()
    {
        Logger.LogInformation("CloseActiveVipmenus");
        try
        {
            foreach (var player in _playersWithOpenVipmenu)
            {
                if (player == null) continue;
                Logger.LogInformation($"Closed active menu by force for {player!.PlayerName!}");
                MenuManager.CloseActiveMenu(player!);
            }
            _playersWithOpenVipmenu.Clear();
        }
        catch (Exception ex)
        {
            if (ex is ArgumentNullException)
            {
                return;
            }
            Logger.LogError($"Error while removing active menu: {ex}");
        }
    }

    private void IncreaseRoundCounter(int? winner)
    {
        Logger.LogInformation("IncreaseRoundCounter");
        if (winner != null && winner == 3 || winner == 2)
        {
            RoundsPlayed += 1;
            if (RoundsPlayed == 0 || RoundsPlayed == _config.MaxRounds / 2 + 1)
                PistolRound = true;
            else
                PistolRound = false;
        }
    }
}
