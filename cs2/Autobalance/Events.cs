using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;


namespace AutoBalance;

partial class AutoBalance
{
    private void InitEvents()
    {
        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            if (teamsBalanced)
            {
                teamsBalanced = false;
            }

            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundEnd>((@event, info) =>
        {
            try
            {
                if (@event.Winner == 3) score.CT += 1;
                if (@event.Winner == 2) score.T += 1;
                Logger.LogInformation($"Score: CT: {score.CT} - T: {score.T}");

                if (teamSwitch)
                {
                    var ctScore = score.CT;
                    var tScore = score.T;
                    score.CT = tScore;
                    score.T = ctScore;
                    teamSwitch = false;
                }
                var roundsPlayed = score.CT + score.T;
                if (Utilities.GetPlayers().Count > 0 && roundsPlayed < _cfg.MaxRounds-2 && roundsPlayed > 2)
                {
                    var PlayerCount = CheckPlayerCount();
                    int PlayerCountDifference = Math.Abs(PlayerCount[0] - PlayerCount[1]);
                    if (forceBalance || PlayerCountDifference > 1)
                    {
                        Server.PrintToChatAll($" {ChatColors.Default}[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}] Balancing teams...");
                        AddTimer(_cfg.RoundRestartDelay - 0.15f, AutobalanceTeams, TimerFlags.STOP_ON_MAPCHANGE);
                        teamsBalanced = true;
                        forceBalance = false;
                    }
                    else
                    {

                        int ScoreDifference = Math.Abs(score.CT - score.T);
                        if ((ScoreDifference >= _cfg.ScoreDifference) && timeoutRounds < 1 && roundsPlayed < _cfg.MaxRounds - 2 && roundsPlayed > 2)
                        {
                            Server.PrintToChatAll($" {ChatColors.Default}[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}] Teams will be balanced at the end of next round!");
                            forceBalance = true;
                        }
                    }
                }
                if (timeoutRounds > 0)
                    timeoutRounds -= 1;
            }
            catch (Exception e)
            {
                Logger.LogError($"{e}");
            }

            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundAnnounceMatchStart>((@event, info) =>
        {
            ResetScoreCounter();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundAnnounceWarmup>((@event, info) =>
        {
            ResetScoreCounter();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundAnnounceLastRoundHalf>((@event, info) =>
        {
            teamSwitch = true;
            return HookResult.Continue;
        });
    }
}
