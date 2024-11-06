using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace Reklamos;
public class Reklamos : BasePlugin
{
    public override string ModuleName => "Reklama";
    public override string ModuleAuthor => "Shmitz";
    public override string ModuleVersion => "1.0.0";

    // Kad nereikėtų nuolat kartoti ilgo prefixo stringo rašant žinutę, išsisaugome prefixą su "readonly" atributu (nes jis nesikeis)
    private readonly string _prefix = $"[ {ChatColors.DarkRed}ShmitzHakeriai {ChatColors.Default}]";

    // Sukuriame bazinį reklamos tekstų sąrašą
    private List<string> _reklamos { get; set; }

    // Šis metodas suveikia užsikrovus pluginui (bet ne pasikeitus mapui)
    public override void Load(bool hotReload)
    {
        _reklamos =
        [
            $"{_prefix} Užsuk į mūsų discord! {ChatColors.LightBlue}discord.shmitz.xyz",
            $"{_prefix} Visas komandas gali rasti parašius komandą {ChatColors.Grey}!help"
        ];

        // Trigerinus šį eventą (žaidėjui prisijungus prie serverio) bus įvygdomas parašytas kodas
        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            var player = @event.Userid;

            // BŪTINAI prieš naudojant objekto kintamąjį (object refference) privalome patikrinti ar jis egzistuoja, ypač jei negalime to užtikrinti prieš tai
            // To nepadarius serveris arba pluginas gali užcrashinti jei bandysime prieiti jame esančią informaciją (kuri gali neegzistuoti) 
            if (player == null || !player.IsValid)
                return HookResult.Continue;

            player.PrintToChat($"{_prefix} Labas {ChatColors.Lime}{player.PlayerName}{ChatColors.Default}!");
            player.PrintToChat($"Serverio komandos: {ChatColors.BlueGrey}!ws, !rank, !vip");

            return HookResult.Continue;
        });
    }

    // Čia aprašome komandą kurią galime naudoti serveryje
    [ConsoleCommand("css_hello")]
    public void OnHello(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid)
            return;

        player.PrintToChat($"{_prefix} Labas ir tau {ChatColors.Lime}{player.PlayerName}{ChatColors.Default}!");
    }
}