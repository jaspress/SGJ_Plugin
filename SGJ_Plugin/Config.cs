using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;

namespace SGJ_Plugin
{
    public class Config : IConfig
    {
        [Description("Enable the plugin.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enable debug logs.")]
        public bool Debug { get; set; } = false;

        [Description("Gun infinite ammo module settings.")]
        public InfiniteAmmoConfigClass InfiniteAmmoConfig { get; set; } = new InfiniteAmmoConfigClass();

        [Description("Facility Guard escape role conversion settings.")]
        public GuardOffDutyConfigClass GuardOffDutyConfig { get; set; } = new GuardOffDutyConfigClass();

        [Description("Player level system settings.")]
        public LevelSystemConfigClass LevelSystemConfig { get; set; } = new LevelSystemConfigClass();

        [Description("Player title override system settings.")]
        public TitleSystemConfigClass TitleSystemConfig { get; set; } = new TitleSystemConfigClass();

        public class InfiniteAmmoConfigClass
        {
            [Description("Enable infinite ammo for firearms.")]
            public bool IsEnabled { get; set; } = true;

            [Description("Restore magazine ammo when reloading.")]
            public bool RestoreOnReload { get; set; } = true;

            [Description("Provide infinite reserve ammo.")]
            public bool InfiniteReserveAmmo { get; set; } = true;

            [Description("User IDs ignored by this module.")]
            public List<string> IgnorePlayers { get; set; } = new List<string>();

            [Description("Allowed gun item type names. Empty means all firearms.")]
            public List<string> AllowedGuns { get; set; } = new List<string>();

            [Description("Excluded gun item type names.")]
            public List<string> ExcludedGuns { get; set; } = new List<string>();
        }

        public class GuardOffDutyConfigClass
        {
            [Description("Enable Facility Guard off-duty escape conversion.")]
            public bool IsEnabled { get; set; } = true;

            [Description("Role assigned when a Facility Guard escapes.")]
            public RoleTypeId EscapeRole { get; set; } = RoleTypeId.NtfPrivate;
        }

        public class LevelSystemConfigClass
        {
            [Description("Enable the level system.")]
            public bool IsEnabled { get; set; } = true;

            [Description("Maximum reachable level.")]
            public int MaxLevel { get; set; } = 10000;

            [Description("Experience awarded for killing another player.")]
            public int KillExperience { get; set; } = 25;

            [Description("Experience awarded after escaping.")]
            public int EscapeExperience { get; set; } = 75;

            [Description("Experience awarded when joining the server.")]
            public int JoinExperience { get; set; } = 0;

            [Description("Experience awarded when dying.")]
            public int DeathExperience { get; set; } = 0;

            [Description("Experience required by level ranges. The matching range for current level decides XP needed for next level.")]
            public List<LevelExperienceRange> ExperienceRanges { get; set; } = new List<LevelExperienceRange>
            {
                new LevelExperienceRange { MinLevel = 1, MaxLevel = 10, RequiredExperience = 50 },
                new LevelExperienceRange { MinLevel = 11, MaxLevel = 25, RequiredExperience = 100 },
                new LevelExperienceRange { MinLevel = 26, MaxLevel = 50, RequiredExperience = 200 },
                new LevelExperienceRange { MinLevel = 51, MaxLevel = 100, RequiredExperience = 400 },
                new LevelExperienceRange { MinLevel = 101, MaxLevel = 200, RequiredExperience = 800 },
                new LevelExperienceRange { MinLevel = 201, MaxLevel = 350, RequiredExperience = 1500 },
                new LevelExperienceRange { MinLevel = 351, MaxLevel = 500, RequiredExperience = 2500 },
                new LevelExperienceRange { MinLevel = 501, MaxLevel = 750, RequiredExperience = 4000 },
                new LevelExperienceRange { MinLevel = 751, MaxLevel = 1000, RequiredExperience = 6500 },
                new LevelExperienceRange { MinLevel = 1001, MaxLevel = 1500, RequiredExperience = 9000 },
                new LevelExperienceRange { MinLevel = 1501, MaxLevel = 2000, RequiredExperience = 12000 },
                new LevelExperienceRange { MinLevel = 2001, MaxLevel = 3000, RequiredExperience = 16000 },
                new LevelExperienceRange { MinLevel = 3001, MaxLevel = 4000, RequiredExperience = 22000 },
                new LevelExperienceRange { MinLevel = 4001, MaxLevel = 5000, RequiredExperience = 30000 },
                new LevelExperienceRange { MinLevel = 5001, MaxLevel = 6500, RequiredExperience = 40000 },
                new LevelExperienceRange { MinLevel = 6501, MaxLevel = 8000, RequiredExperience = 55000 },
                new LevelExperienceRange { MinLevel = 8001, MaxLevel = 9000, RequiredExperience = 70000 },
                new LevelExperienceRange { MinLevel = 9001, MaxLevel = 10000, RequiredExperience = 90000 },
            };

            [Description("Show the level HUD.")]
            public bool ShowHud { get; set; } = true;

            [Description("JSON file name stored in %AppData%/EXILED/Config/.")]
            public string DataFileName { get; set; } = "SGJ_LevelSystem.json";

            [Description("HUD text template. Placeholders: {name}, {steamid}, {level}, {xp}, {required_xp}, {total_xp}, {title}, {rankname}, {level_rankname}, {title_rankname}, {title_color}, {progress_bar}, {progress_percent}, {kills}, {deaths}, {escapes}.")]
            public string HudText { get; set; } = "<size=22><b><color=#00FFFF>名字:</color> {name} | <color=#FFD700>等级:</color> {level} | <color=#7FFF00>EXP:</color> {xp}/{required_xp} | <color=#DA70D6>称号:</color> {title}</size>\n    <align=center><size=20>[{level_rankname}]</size></align></b>";

            [Description("Progress bar filled character.")]
            public string ProgressBarFilled { get; set; } = "|";

            [Description("Progress bar empty character.")]
            public string ProgressBarEmpty { get; set; } = ".";

            [Description("Progress bar length.")]
            public int ProgressBarLength { get; set; } = 12;

            [Description("Default player rank name saved in JSON.")]
            public string DefaultRankName { get; set; } = "白银 I";

            [Description("Default level rank color used when no rank range matches.")]
            public string DefaultRankColor { get; set; } = "#C0C0C0";

            [Description("Automatically update rankname by level ranges. Disable this if you want to edit rankname manually in JSON.")]
            public bool AutoUpdateRankNameByLevel { get; set; } = true;

            [Description("Rank names by level ranges.")]
            public List<LevelRankNameRange> RankNameRanges { get; set; } = new List<LevelRankNameRange>
            {
                new LevelRankNameRange { MinLevel = 1, MaxLevel = 199, RankName = "白银 I", Color = "#C0C0C0" },
                new LevelRankNameRange { MinLevel = 200, MaxLevel = 399, RankName = "白银 II", Color = "#C8C8C8" },
                new LevelRankNameRange { MinLevel = 400, MaxLevel = 599, RankName = "白银 III", Color = "#D0D0D0" },
                new LevelRankNameRange { MinLevel = 600, MaxLevel = 799, RankName = "白银 IV", Color = "#D8D8D8" },
                new LevelRankNameRange { MinLevel = 800, MaxLevel = 999, RankName = "白银精英", Color = "#E0E0E0" },
                new LevelRankNameRange { MinLevel = 1000, MaxLevel = 1299, RankName = "大师级白银精英", Color = "#F0F0F0" },
                new LevelRankNameRange { MinLevel = 1300, MaxLevel = 1699, RankName = "黄金新星 I", Color = "#FFD700" },
                new LevelRankNameRange { MinLevel = 1700, MaxLevel = 2099, RankName = "黄金新星 II", Color = "#FFC400" },
                new LevelRankNameRange { MinLevel = 2100, MaxLevel = 2499, RankName = "黄金新星 III", Color = "#FFB000" },
                new LevelRankNameRange { MinLevel = 2500, MaxLevel = 2999, RankName = "大师级黄金新星", Color = "#FFA000" },
                new LevelRankNameRange { MinLevel = 3000, MaxLevel = 3599, RankName = "大师级守护者 I", Color = "#7FFF00" },
                new LevelRankNameRange { MinLevel = 3600, MaxLevel = 4199, RankName = "大师级守护者 II", Color = "#50C878" },
                new LevelRankNameRange { MinLevel = 4200, MaxLevel = 4899, RankName = "大师级守护者精英", Color = "#32CD32" },
                new LevelRankNameRange { MinLevel = 4900, MaxLevel = 5699, RankName = "杰出大师级守护者", Color = "#00FF7F" },
                new LevelRankNameRange { MinLevel = 5700, MaxLevel = 6599, RankName = "传奇之鹰", Color = "#00B7EB" },
                new LevelRankNameRange { MinLevel = 6600, MaxLevel = 7499, RankName = "大师级传奇之鹰", Color = "#00FFFF" },
                new LevelRankNameRange { MinLevel = 7500, MaxLevel = 8499, RankName = "无上之首席大师", Color = "#DA70D6" },
                new LevelRankNameRange { MinLevel = 8500, MaxLevel = 10000, RankName = "全球精英", Color = "#FF69B4" },
            };

            [Description("Level-up hint template. Placeholders are the same as HUD text, plus {gained_xp}.")]
            public string LevelUpText { get; set; } = "<color=#ffd966>等级提升!</color> 当前等级: Lv.{level}";

            [Description("Experience gain hint template. Placeholders are the same as HUD text, plus {gained_xp} and {reason}.")]
            public string ExperienceGainText { get; set; } = "<color=#9be7ff>+{gained_xp} XP</color> {reason}";

            [Description("Automatically add the level prefix before the player's displayed name.")]
            public bool UpdateDisplayNickname { get; set; } = true;

            [Description("Displayed name template. Placeholders are the same as HUD text.")]
            public string DisplayNameText { get; set; } = "[Lv.{level}][{level_rankname}] {name}";

            [Description("Level HUD X coordinate.")]
            public float HudXCoordinate { get; set; } = 0f;

            [Description("Level HUD Y coordinate.")]
            public float HudYCoordinate { get; set; } = 1015f;

            [Description("Level HUD font size.")]
            public int HudFontSize { get; set; } = 20;
        }

        public class LevelExperienceRange
        {
            public int MinLevel { get; set; } = 1;
            public int MaxLevel { get; set; } = 10;
            public int RequiredExperience { get; set; } = 50;
        }

        public class LevelRankNameRange
        {
            public int MinLevel { get; set; } = 1;
            public int MaxLevel { get; set; } = 4;
            public string RankName { get; set; } = "新手";
            public string Color { get; set; } = "#C0C0C0";
        }

        public class TitleSystemConfigClass
        {
            [Description("Enable title override system.")]
            public bool IsEnabled { get; set; } = true;

            [Description("JSON file name stored in %AppData%/EXILED/Config/.")]
            public string DataFileName { get; set; } = "SGJ_TitleSystem.json";

            [Description("Default title color when title system rankcolor is empty. Use color names like pink, red, green, cyan, or rainbow.")]
            public string DefaultRankColor { get; set; } = "green";

            [Description("Rainbow color sequence used when rankcolor is rainbow.")]
            public List<string> RainbowColors { get; set; } = new List<string>
            {
                "pink",
                "red",
                "brown",
                "silver",
                "light_green",
                "crimson",
                "cyan",
                "aqua",
                "deep_pink",
                "tomato",
                "yellow",
                "magenta",
                "blue_green",
                "orange",
                "lime",
                "green",
                "emerald",
                "carmine",
                "nickel",
                "mint",
                "army_green",
                "pumpkin",
            };
        }
    }
}
