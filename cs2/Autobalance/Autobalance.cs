using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AutoBalance;
public partial class AutoBalance : BasePlugin
{
    public override string ModuleName => "AutoBalance";
    public override string ModuleAuthor => "Shmitz";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleDescription => "Autobalances teams";

    private class Score 
    {
        public int CT {  get; set; }
        public int T { get; set; }
    }
    private Config _cfg;
    private Score score;
    private bool forceBalance;
    private bool teamsBalanced;
    private bool teamSwitch;
    private int timeoutRounds;

    public override void Load(bool hotReload)
    {
        Init();
        score = new Score
        {
            CT = 0,
            T = 0
        };
        InitEvents();
    }

    public void Init()
    {
        _cfg = LoadConfig();
        forceBalance = false;
        teamsBalanced = false;
        teamSwitch = false;
        timeoutRounds = _cfg.BalanceTimeoutRounds;
    }

    private void BalanceByScore()
    {
        var players = Utilities.GetPlayers().Where(IsPlayerValid).OrderByDescending(p => p.Score).ToList();
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (score.CT > score.T)
            {
                if (i % 2 == 0)
                {
                    player.SwitchTeam(CsTeam.Terrorist);
                }
                else
                {
                    player.SwitchTeam(CsTeam.CounterTerrorist);
                }
            }
            else
            {
                if (i % 2 == 0)
                {
                    player.SwitchTeam(CsTeam.CounterTerrorist);
                }
                else
                {
                    player.SwitchTeam(CsTeam.Terrorist);
                }
            }
        }
        players.Clear();
    }
    private List<int> CheckPlayerCount()
    {
        var ctCount = 0;
        var tCount = 0;
        var players = Utilities.GetPlayers().Where(IsPlayerValid).ToList();
        foreach (var player in players)
        {
            if (player.TeamNum == 2)
            {
                tCount +=1;
            }
            else
            {
                ctCount +=1;
            }
        }
        return new List<int> { ctCount, tCount };
    }
    private void BalanceByKills()
    {
        var players = Utilities.GetPlayers().Where(IsPlayerValid).OrderByDescending(p => p.Kills.Count).ToList();
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            if (score.CT > score.T)
            {
                if (i % 2 == 0)
                {
                    player.SwitchTeam(CsTeam.Terrorist);
                }
                else
                {
                    player.SwitchTeam(CsTeam.CounterTerrorist);
                }
            }
            else
            {
                if (i % 2 == 0)
                {
                    player.SwitchTeam(CsTeam.CounterTerrorist);
                }
                else
                {
                    player.SwitchTeam(CsTeam.Terrorist);
                }
            }
        }
        players.Clear();
    }

    private void ResetScoreCounter()
    {
        score.CT = 0;
        score.T = 0;
        timeoutRounds = _cfg.BalanceTimeoutRounds;
    }
    public static bool IsPlayerValid(CCSPlayerController? player)
    {
        return player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, TeamNum: 2 or 3, IsBot: false, IsHLTV: false };
    }
}
