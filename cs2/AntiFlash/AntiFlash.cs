using CounterStrikeSharp.API.Core;

namespace AntiFlash
{
    public class AntiFlash : BasePlugin
    {
        public override string ModuleName => "AntiFlash";

        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "Shmitz";

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerBlind>((@event, info) =>
            {
                var player = @event.Userid;

                if (player == null || !player.IsValid) return HookResult.Continue;

                var attacker = @event.Attacker;

                var playerPawn = player.PlayerPawn.Value;
                if (playerPawn == null || playerPawn.LifeState is not (byte)LifeState_t.LIFE_ALIVE) return HookResult.Continue;

                var sameTeam = attacker.Team == player.Team;
                if (sameTeam && player != attacker)
                    playerPawn.FlashDuration = 0.0f;

                return HookResult.Continue;
            });
        }
    }
}
