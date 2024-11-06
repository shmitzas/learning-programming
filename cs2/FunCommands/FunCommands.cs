using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;

namespace FunCommands;
public class FunCommands : BasePlugin
{
    public override string ModuleName => "FunCommands";
    public override string ModuleAuthor => "Shmitz";
    public override string ModuleDescription => "Fun commands for noisy players ;)";
    public override string ModuleVersion => "0.0.1";

    private string _giveawayPrize {  get; set; }
    private string _prefix = $" {ChatColors.Default}[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}]";

    public override void Load(bool hotReload)
    {
        _giveawayPrize = "";
    }

    [ConsoleCommand("css_pasunkinti")]
    public void OnPasunkinti(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null && IsPlayerValid(player) && IsPlayerAlive(player) && player.PlayerPawn.Value != null)
        {
            player.PlayerPawn.Value.Health = 1;
            player.PlayerPawn.Value.MaxHealth = 1;
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
            player.PrintToChat($"{_prefix} Hard mode activated!");
            Server.PrintToChatAll($"{_prefix} {ChatColors.Lime}{player.PlayerName} {ChatColors.Default}increased their difficulty for this round!");
        }
    }

    [ConsoleCommand("css_perlengva")]
    public void OnPerlengva(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null && IsPlayerValid(player) && IsPlayerAlive(player) && player.PlayerPawn.Value != null)
        {
            player.PlayerPawn.Value.Health = 1;
            player.PlayerPawn.Value.MaxHealth = 1;
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
            player.PrintToChat($"{_prefix} Hard mode activated!");
            Server.PrintToChatAll($"{_prefix} {ChatColors.Lime}{player.PlayerName} {ChatColors.Default}increased their difficulty for this round!");
        }
    }
    [RequiresPermissions("@css/rcon")]
    [ConsoleCommand("css_giveaway")]
    public void OnGyveaway(CCSPlayerController? player, CommandInfo command)
    {
        _giveawayPrize = "";
        for (int i = 1; i < command.ArgCount; i++)
        {
            _giveawayPrize += command.GetArg(i) + " ";
        }
        if (player != null)
        {
            Server.PrintToChatAll($"{_prefix} Starting giveaway for {ChatColors.LightRed}{_giveawayPrize}");
            AddTimer(5, GiveawayWinner, TimerFlags.STOP_ON_MAPCHANGE);
        }
    }
    [RequiresPermissions("@css/rcon")]
    [ConsoleCommand("css_reroll_giveaway")]
    public void OnRerollGyveaway(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null)
        {
            Server.PrintToChatAll($"{_prefix} Rerolling giveaway for {ChatColors.LightRed}{_giveawayPrize}");
            AddTimer(5, GiveawayWinner, TimerFlags.STOP_ON_MAPCHANGE);
        }
    }

    private void GiveawayWinner()
    {
        if (!string.IsNullOrEmpty(_giveawayPrize)) {
            var players = Utilities.GetPlayers().Where(IsPlayerConnected).OrderByDescending(p => p.Score).ToList();
            int randomInt = new Random().Next(0, players.Count);
            Server.PrintToChatAll($"{_prefix} {ChatColors.Lime}{players[randomInt].PlayerName} {ChatColors.Default}won {ChatColors.LightRed}{_giveawayPrize}");
        }
    }

    private bool IsPlayerAlive(CCSPlayerController? player)
    {
        if (player != null && !player.IsBot && player.PawnIsAlive)
        {
            return true;
        }
        player.PrintToChat($"{_prefix} Only alive players can call this command");
        return false;
    }

    public static bool IsPlayerValid(CCSPlayerController? player)
    {
        return player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, TeamNum: 2 or 3 };
    }
    public static bool IsPlayerConnected(CCSPlayerController? player)
    {
        return player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected };
    }
}