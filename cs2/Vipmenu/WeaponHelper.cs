using static Vipmenu.Vipmenu;

namespace Vipmenu;

public partial class Vipmenu
{
    public enum WeaponType
    {
        Primary = 0,
        Secondary = 1,
        None= 2,
    }

    public class Weapon
    {
        public Weapon(string gameName, string giveName, WeaponType type = WeaponType.Primary)
        {
            Type = type;
            GameName = gameName;
            GiveName = giveName;
        }

        public WeaponType Type { get; set; }
        public string GameName { get; set; }
        public string GiveName { get; set; }
    }

    private class WeaponHelper
    {
        private List<Weapon> LoadAllWeaponList()
        {
            return new List<Weapon>()
            {
                new Weapon("weapon_ak47",           "AK-47"),
                new Weapon("weapon_aug",            "AUG"),
                new Weapon("weapon_awp",            "AWP"),
                new Weapon("weapon_bizon",          "PP-Bizon"),
                new Weapon("weapon_cz75a",          "CZ75-Auto",    WeaponType.Secondary),
                new Weapon("weapon_deagle",         "Desert Eagle", WeaponType.Secondary),
                new Weapon("weapon_elite",          "Dual Berettas",WeaponType.Secondary),
                new Weapon("weapon_famas",          "FAMAS"),
                new Weapon("weapon_fiveseven",      "Five-SeveN",   WeaponType.Secondary),
                new Weapon("weapon_g3sg1",          "G3SG1"),
                new Weapon("weapon_galilar",        "Galil AR"),
                new Weapon("weapon_glock",          "Glock-18",     WeaponType.Secondary),
                new Weapon("weapon_m249",           "M249"),
                new Weapon("weapon_m4a1",           "M4A4"),
                new Weapon("weapon_m4a1_silencer",  "M4A1-S"),
                new Weapon("weapon_mac10",          "MAC-10"),
                new Weapon("weapon_mag7",           "MAG-7"),
                new Weapon("weapon_mp5sd",          "MP5-SD"),
                new Weapon("weapon_mp7",            "MP7"),
                new Weapon("weapon_mp9",            "MP9"),
                new Weapon("weapon_negev",          "Negev"),
                new Weapon("weapon_nova",           "Nova"),
                new Weapon("weapon_p2000",          "P2000",        WeaponType.Secondary),
                new Weapon("weapon_p250",           "P250",         WeaponType.Secondary),
                new Weapon("weapon_p90",            "P90"),
                new Weapon("weapon_revolver",       "R8 Revolver",  WeaponType.Secondary),
                new Weapon("weapon_sawedoff",       "Sawed-Off"),
                new Weapon("weapon_scar20",         "SCAR-20"),
                new Weapon("weapon_sg553",          "SG 553"),
                new Weapon("weapon_ssg08",          "SSG 08"),
                new Weapon("weapon_taser",          "Taser",        WeaponType.Secondary),
                new Weapon("weapon_tec9",           "Tec-9",        WeaponType.Secondary),
                new Weapon("weapon_ump45",          "UMP-45"),
                new Weapon("weapon_usp_silencer",   "USP-S",        WeaponType.Secondary),
                new Weapon("weapon_hkp2000",        "P2000",        WeaponType.Secondary),
                new Weapon("weapon_xm1014",         "XM1014")
            };
        }
        private List<Weapon> LoadVipWeaponList()
        {
            return new List<Weapon>()
            {
                new Weapon("weapon_ak47",           "AK-47"),
                new Weapon("weapon_awp",            "AWP"),
                new Weapon("weapon_m4a1",           "M4A4"),
                new Weapon("weapon_m4a1_silencer",  "M4A1-S"),
                new Weapon("weapon_deagle",         "Desert Eagle", WeaponType.Secondary),
                new Weapon("weapon_tec9",           "Tec-9",        WeaponType.Secondary),
                new Weapon("weapon_p250",           "P250",         WeaponType.Secondary),
                new Weapon("weapon_fiveseven",      "Five-SeveN",   WeaponType.Secondary)
            };
        }
        private List<Weapon> LoadVipPistolList()
        {
            return new List<Weapon>()
            {
                new Weapon("weapon_deagle",         "Desert Eagle", WeaponType.Secondary),
                new Weapon("weapon_tec9",           "Tec-9",        WeaponType.Secondary),
                new Weapon("weapon_p250",           "P250",         WeaponType.Secondary),
                new Weapon("weapon_fiveseven",      "Five-SeveN",   WeaponType.Secondary),
                new Weapon("weapon_cz75a",          "CZ75-Auto",    WeaponType.Secondary),
                new Weapon("weapon_elite",          "Dual Berettas",WeaponType.Secondary),
                new Weapon("weapon_revolver",       "R8 Revolver",  WeaponType.Secondary)
            };
        }

        public string GetGameName(string weaponName)
        {
            var weapons = LoadAllWeaponList();
            foreach (var item in weapons)
            {
                if (weaponName == item.GiveName || weaponName == item.GameName)
                    return item.GameName;
            }
            return "";
        }

        public string GetGiveName(string weaponName)
        {
            var weapons = LoadAllWeaponList();
            foreach (var item in weapons)
            {
                if (weaponName == item.GiveName || weaponName == item.GameName)
                    return item.GiveName;
            }
            return "";
        }

        public WeaponType GetWeaponType(string weaponName)
        {
            var weapons = LoadAllWeaponList();
            foreach (var item in weapons)
            {
                if (weaponName == item.GiveName || weaponName == item.GameName)
                    return item.Type;
            }
            return WeaponType.None;
        }

        public List<string> GetPistolList()
        {
            var weapons = LoadVipPistolList();
            var weaponList = new List<string>();
            foreach (var item in weapons)
            {
                weaponList.Add(item.GiveName);
            }
            return weaponList;
        }

        public List<string> GetGunList()
        {
            var weapons = LoadVipWeaponList();
            var weaponList = new List<string>();
            foreach (var item in weapons)
            {
                weaponList.Add(item.GiveName);
            }
            return weaponList;
        }
    }
}