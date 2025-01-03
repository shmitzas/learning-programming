
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;

namespace K4SystemGunXP;

public partial class PluginK4SystemGunXP
{
    private void InitEvents()
    {
        RegisterEventHandler<EventPlayerDeath>((@event, info) => {
            var player = @event.Userid;
            if (player != null)
                if (DisplayRanksList.Any(x => x.ContainsKey(player)))
                {
                    DisplayRanksList.RemoveAll(x => x.ContainsKey(player));
                }

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerSpawn>((@event, info) => {
            var player = @event.Userid;
            if (player == null)
                return HookResult.Continue;

            var h = GetPlayerDetails(player);
            if (h == null)
                return HookResult.Continue;

            var weapon = WeaponNameByXP(h!.Points);
            if (weapon == null)
            {
                player.PrintToChat($"[ Rampage.lt {ChatColors.Default}] {ChatColors.Red}Failed to give a weapon");
                player.PrintToChat($"[ Rampage.lt {ChatColors.Default}] You need to earn more XP to get a weapon");

                return HookResult.Continue;
            }
            try
            {
                if (player.AuthorizedSteamID == null)
                {
                    player.PrintToChat($"[ Rampage.lt {ChatColors.Default}] {ChatColors.Red}Failed to get player steam ID");
                    return HookResult.Continue;
                }

                var playerId = player.AuthorizedSteamID.AccountId;

                // Check if the player already has an active weapon
                var existingWeapon = ActiveWeapons.FirstOrDefault(x => x.ContainsKey(playerId));

                if (existingWeapon == null)
                {
                    // If no active weapon exists, add the new active weapon for the player
                    ActiveWeapons.Add(new Dictionary<int, string> { { playerId, weapon.GameName } });
                    existingWeapon = ActiveWeapons.First(x => x.ContainsKey(playerId));
                }

                // Retrieve the weapon to give
                existingWeapon.TryGetValue(playerId, out var weaponToGive);

                if (string.IsNullOrEmpty(weaponToGive))
                    weaponToGive = weapon.GameName;

                if (!PlayersChangedActiveWeapon.Contains(playerId))
                    weaponToGive = weapon.GameName;
                Server.NextFrame(() => player.GiveNamedItem(weaponToGive));
            

                int nextLevel = GetNextLevelPoints(h.Points);
                if (nextLevel == 0)
                    return HookResult.Continue;
                DisplayRanksList.Add(new Dictionary<CCSPlayerController, RankInfo>
                {
                    { player, new RankInfo(){
                        CurrentPoints = h.Points,
                        CurrentWeapon = weapon.GiveName,
                        NextLevelPoints = nextLevel,
                        NextWeapon = WeaponNameByXP(nextLevel).GiveName,
                        ActiveWeapon = GetWeaponGiveName(weaponToGive)
                    }}
                });
            }
            catch (Exception e)
            {
                player.PrintToChat($"[ Rampage.lt {ChatColors.Default}] Failed to give a weapon");
                Logger.LogError(e, "Failed to give a weapon to {0}. Player XP: {1}", player.PlayerName, h.Points);
                return HookResult.Continue;
            }

            return HookResult.Continue;
        });

        RegisterListener<Listeners.OnTick>(() =>
        {
            foreach (var item in DisplayRanksList)
            {
                foreach (var player in item.Keys)
                {
                    var rank = item[player];
                    player.PrintToCenterHtml($"<font class='fontSize-ml'>Gun Level</font>" +
                        $"<br><font class='fontSize-s'>▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬</font>" +
                        $"<br><font class='fontSize-m' color='#FFD700'>Current XP: {rank.CurrentPoints} <span color='#FFFFFF'>|</span> {rank.CurrentWeapon}</font>" +
                        $"<br><font class='fontSize-m' color='#E62C2C'>Next Level: {rank.NextLevelPoints} <span color='#FFFFFF'>|</span> {rank.NextWeapon}</font>" +
                        $"<br><font class='fontSize-s'>▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬</font>" +
                        $"<br><font class='fontSize-m' color='#FFFFFF'>Active weapon: <span color='#5AF54C'>{rank.ActiveWeapon}</span></font><br>");
                }
            }
        });

        RegisterEventHandler<EventPlayerConnectFull>((@event, info) => {      

            if (Utilities.GetPlayers().Where(IsPlayerConnected).ToList().Count == 1)
            {
                Server.ExecuteCommand("mp_restartgame 1");
            }
            return HookResult.Continue;
        });
    }
}
