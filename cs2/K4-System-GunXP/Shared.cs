using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using K4SharedApi;
using Microsoft.Extensions.Logging;

namespace K4SystemGunXP;
public partial class PluginK4SystemGunXP
{
    public static bool IsPlayerValid(CCSPlayerController? player)
    {
        return player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, TeamNum: 2 or 3, IsBot: false, IsHLTV: false };
    }

    public static bool IsPlayerConnected(CCSPlayerController? player)
    {
        return player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, IsBot: false, IsHLTV: false };
    }

    private IPlayerAPI? GetPlayerDetails(CCSPlayerController player)
    {
        if (player == null || !IsPlayerValid(player))
            return null;


        IPlayerAPI? h = Capability_SharedAPI.Get(player);

        if (h == null || !h.IsPlayer)
            return null;
        return h;
    }

    private Weapon WeaponNameByXP(int xp)
    {
        Weapon? weapon = null;
        foreach (var item in this.Config.Weapons)
        {
            if (xp >= item.MinimumXP)
                weapon = item;
        }
        return weapon!;
    }

    private int GetNextLevelPoints(int xp)
    {
        int nextLevel = 0;
        foreach (var item in this.Config.Weapons)
        {
            if (xp < item.MinimumXP)
            {
                nextLevel = item.MinimumXP;
                break;
            }
        }
        return nextLevel;
    }

    private List<Weapon> GetAvailableWeapons(int xp)
    {
        List<Weapon> weapons = new();
        foreach (var item in this.Config.Weapons)
        {
            if (xp >= item.MinimumXP)
                weapons.Add(item);
        }
        return weapons;
    }

    private string GetWeaponGameName(string weaponName)
    {
        var weapon = this.Config.Weapons.FirstOrDefault(x => x.GiveName == weaponName);
        return weapon?.GameName ?? "";
    }

    private string GetWeaponGiveName(string weaponName)
    {
        var weapon = this.Config.Weapons.FirstOrDefault(x => x.GameName == weaponName);
        return weapon?.GiveName ?? "";
    }
}
