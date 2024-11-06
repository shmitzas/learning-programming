using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace PistolNoHelmet;
public partial class PistolNoHelmet : BasePlugin
{
    public override string ModuleName => "Pistol No Helmet";
    public override string ModuleDescription => "Disables helmet on pistol rounds";

    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Shmitz";

    private int _round;
    private Config _cfg;
    private bool _printAnnouncementToChat;
    private string _prefix = $"[ {ChatColors.DarkRed}Rampage.lt {ChatColors.Default}]";

    public override void Load(bool hotReload)
    {
        _round = 0;
        _printAnnouncementToChat = false;
        _cfg = LoadConfig();
        InitEvents();
    }

    private void InitEvents()
    {
        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            if (_printAnnouncementToChat)
            {
                Server.PrintToChatAll($"{_prefix} Helmet is {ChatColors.LightRed}disabled {ChatColors.Default}for this round!");
                _printAnnouncementToChat = false;
            }
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundEnd>((@event, info) =>
        {
            if (@event.Winner == 3 || @event.Winner == 2) _round++;
            if (_round == 0 || _round == _cfg.MaxRounds / 2)
            {
                EnableHelmet(false);
            }
            else
            {
                EnableHelmet();
            }
            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundAnnounceMatchStart>((@event, info) =>
        {
            _round = 0;
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundAnnounceWarmup>((@event, info) =>
        {
            _round = 0;
            return HookResult.Continue;
        });

        RegisterEventHandler<EventWarmupEnd>((@event, info) =>
        {
            EnableHelmet(false);
            return HookResult.Continue;
        });
    }

    private void EnableHelmet(bool enabled = true)
    {
        var cvar1 = ConVar.Find("mp_free_armor");
        var cvar2 = ConVar.Find("mp_max_armor");
        if (!enabled) 
        {
            cvar1?.SetValue(1);
            cvar2?.SetValue(1);
            if (_cfg.BroadcastMessage)
            {
                _printAnnouncementToChat = true;
            }
        }
        else
        {
            cvar1?.SetValue(2);
            cvar2?.SetValue(2);
        }
    }
}

