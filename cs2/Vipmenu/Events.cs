using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Vipmenu;
public partial class Vipmenu
{
    private int _winner = 0;
    public void InitEvents()
    {
        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            VipmenuUsed = [];
            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundAnnounceLastRoundHalf>((@event, info) =>
        {
            LastRoundHalf = true;
            VipmenuUsed = [];
            return HookResult.Continue;
        });

        RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            RoundsPlayed = 0;
            VipmenuUsed = new Dictionary<int, int>();
        });

        RegisterEventHandler<EventRoundAnnounceWarmup>((@event, info) =>
        {
            RoundsPlayed = 0;
            VipmenuUsed = new Dictionary<int, int>();
            return HookResult.Continue;
        });

        RegisterEventHandler<EventWarmupEnd>((@event, info) =>
        {
            RoundsPlayed = 0;
            VipmenuUsed = new Dictionary<int, int>();
            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundEnd>((@event, info) =>
        { 
            _winner = @event.Winner;
            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundPrestart>((@event, info) =>
        {
            if (_winner == 3 || _winner == 2)
            {
                RoundsPlayed += 1;
                if (RoundsPlayed > 0)
                    PistolRound = false;
                if (LastRoundHalf)
                    PistolRound = true;
                if (RoundsPlayed == _config.MaxRounds / 2 + 1)
                {
                    LastRoundHalf = false;
                    PistolRound = false;
                }
            }
            CloseActiveVipmenus();
            return HookResult.Continue;
        });
    }
}

