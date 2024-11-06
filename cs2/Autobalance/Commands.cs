using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;

namespace AutoBalance;

partial class AutoBalance
{
    [RequiresPermissions("@css/balance")]
    [ConsoleCommand("css_balance")]
    public void ForceBalance(CCSPlayerController? player, CommandInfo info)
    {
        forceBalance = true;
        Server.PrintToChatAll($" {ChatColors.Default}[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}] Teams will be balanced at the end of the round!");
    }
    public void AutobalanceTeams()
    {
        try
        {
            if (_cfg.BalanceByScore) BalanceByScore();
            if (_cfg.BalanceByKills) BalanceByKills();
            Server.PrintToChatAll($" {ChatColors.Default}[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}] Teams have been balanced!");
            timeoutRounds = _cfg.BalanceTimeoutRounds;
        }
        catch (Exception e) { Logger.LogError($"{e}"); }
    }

    [RequiresPermissions("@css/rcon")]
    [ConsoleCommand("css_balance_reload")]
    public void OnCmdReload(CCSPlayerController? player, CommandInfo info)
    {
        Init();

        if (player == null)
            Logger.LogInformation("Configuration successfully rebooted");
        else
            player.PrintToChat($" {ChatColors.Default}[ {ChatColors.DarkRed}AutoBalance {ChatColors.Default}] Configuration successfully rebooted");
    }
}
