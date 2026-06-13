using Exiled.API.Enums;
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

        [Description("Spectator HUD module settings.")]
        public SpectatorHudConfigClass SpectatorHudConfig { get; set; } = new SpectatorHudConfigClass();

        [Description("Player title override system settings.")]
        public TitleSystemConfigClass TitleSystemConfig { get; set; } = new TitleSystemConfigClass();

        [Description("Chat UI module settings.")]
        public ChatConfigClass ChatConfig { get; set; } = new ChatConfigClass();

        [Description("Player damage manager module settings.")]
        public DamageManagerConfigClass DamageManagerConfig { get; set; } = new DamageManagerConfigClass();

        [Description("Player surrender module settings.")]
        public SurrenderConfigClass SurrenderModuleConfig { get; set; } = new SurrenderConfigClass();

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

            [Description("Kill experience by killed role. The role name must match RoleTypeId, for example Scp173 or ClassD. A matching entry overrides KillExperience.")]
            public List<RoleExperienceReward> KillExperienceByRole { get; set; } = new List<RoleExperienceReward>
            {
                new RoleExperienceReward { Role = RoleTypeId.Scp049.ToString(), Experience = 60 },
                new RoleExperienceReward { Role = RoleTypeId.Scp0492.ToString(), Experience = 35 },
                new RoleExperienceReward { Role = RoleTypeId.Scp096.ToString(), Experience = 90 },
                new RoleExperienceReward { Role = RoleTypeId.Scp106.ToString(), Experience = 90 },
                new RoleExperienceReward { Role = RoleTypeId.Scp173.ToString(), Experience = 80 },
                new RoleExperienceReward { Role = RoleTypeId.Scp939.ToString(), Experience = 70 },
                new RoleExperienceReward { Role = RoleTypeId.Scp079.ToString(), Experience = 100 },
            };

            [Description("Enable assist experience. When disabled, the full kill experience goes to the killer only.")]
            public bool AssistExperienceEnabled { get; set; } = true;

            [Description("Percent of kill experience awarded to each valid assister.")]
            public float AssistExperiencePercent { get; set; } = 0.35f;

            [Description("Minimum damage required to qualify for assist experience.")]
            public float AssistMinimumDamage { get; set; } = 15f;

            [Description("How long damage records remain valid for assists, in seconds.")]
            public float AssistDamageExpireSeconds { get; set; } = 20f;

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

            [Description("JSON file name stored in %AppData%/EXILED/Configs/.")]
            public string DataFileName { get; set; } = "LevelModule_Config.json";

            [Description("HUD text template. Placeholders: {name}, {steamid}, {level}, {xp}, {required_xp}, {total_xp}, {title}, {rankname}, {level_rankname}, {title_rankname}, {title_color}, {progress_bar}, {progress_percent}, {kills}, {deaths}, {escapes}, {role_name}, {rolecolor}, {role_color}.")]
            public string HudText { get; set; } = "<size=22><b><color=#00FFFF>名字:</color> {name} | <color=#FFD700>等级:</color> {level} | <color=#7FFF00>EXP:</color> {xp}/{required_xp} | <color=#DA70D6>称号:</color> {title}</size>\n<align=center><size=20>[{level_rankname}] \n你正在扮演：[<color={rolecolor}>{role_name}</color>]</size></align></b>";

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
            public string LevelUpText { get; set; } = "<b><size=20>[📢]恭喜升级到{level}!</size></b>";

            [Description("Experience gain hint template. Placeholders are the same as HUD text, plus {gained_xp} and {reason}.")]
            public string ExperienceGainText { get; set; } = "<b><size=20>[📢]增加经验值{gained_xp}!</size></b>";

            [Description("Experience notification X coordinate.")]
            public float ExperienceHintXCoordinate { get; set; } = 820f;

            [Description("Experience notification Y coordinate.")]
            public float ExperienceHintYCoordinate { get; set; } = 120f;

            [Description("Experience notification font size.")]
            public int ExperienceHintFontSize { get; set; } = 20;

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

        public class RoleExperienceReward
        {
            public string Role { get; set; } = RoleTypeId.None.ToString();
            public int Experience { get; set; } = 0;
        }

        public class SpectatorHudConfigClass
        {
            [Description("Enable spectator HUD module.")]
            public bool IsEnabled { get; set; } = true;

            [Description("Show observed player's level HUD while spectating.")]
            public bool ShowObservedPlayerLevelHud { get; set; } = true;

            [Description("Spectator HUD text template. Placeholders: {observed_name}, {respawn_info}, {respawn_wave}, {respawn_time}, {respawn_tickets}, {respawn_team}.")]
            public string HudText { get; set; } = "<align=center>你正在观察：{observed_name}</align>\n{respawn_info}";

            [Description("Template used when no spectated player is found. Placeholders: {respawn_wave}, {respawn_time}, {respawn_tickets}, {respawn_team}.")]
            public string NoObservedPlayerText { get; set; } = "<align=center>你正在观察：无</align>\n<align=center>下一次刷新：{respawn_wave} | 倒计时：{respawn_time} | 票数：{respawn_tickets} | 阵营：{respawn_team}</align>";

            [Description("Respawn wave info template. Placeholders: {respawn_wave}, {respawn_time}, {respawn_tickets}, {respawn_team}.")]
            public string RespawnInfoText { get; set; } = "<align=center>下一次刷新：{respawn_wave} | 倒计时：{respawn_time} | 票数：{respawn_tickets} | 阵营：{respawn_team}</align>";

            [Description("Maximum visible lines. Extra lines are trimmed to avoid going off screen.")]
            public int MaxVisibleLines { get; set; } = 8;

            [Description("Spectator HUD X coordinate.")]
            public float HudXCoordinate { get; set; } = 0f;

            [Description("Spectator HUD Y coordinate.")]
            public float HudYCoordinate { get; set; } = 760f;

            [Description("Spectator HUD font size.")]
            public int HudFontSize { get; set; } = 18;
        }

        public class TitleSystemConfigClass
        {
            [Description("Enable title override system.")]
            public bool IsEnabled { get; set; } = true;

            [Description("JSON file name stored in %AppData%/EXILED/Configs/.")]
            public string DataFileName { get; set; } = "TitleModule_Config.json";

            [Description("Default title color when title system rankcolor is empty. Use color names like pink, red, green, cyan, or rainbow.")]
            public string DefaultRankColor { get; set; } = string.Empty;

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

        public class ChatConfigClass
        {
            [Description("Enable chat UI module.")]
            public bool IsEnabled { get; set; } = true;

            [Description("JSON file name stored in %AppData%/EXILED/Configs/.")]
            public string DataFileName { get; set; } = "ChatModule_Config.json";

            [Description("Global chat template. Placeholders: {channel}, {team_color}, {role_color}, {rolecolor}, {role_name}, {role}, {name}, {content}.")]
            public string GlobalChatTemplate { get; set; } = "<b>[{channel}][<color={rolecolor}>{role_name}</color>] {name}: {content}</b>";

            [Description("Team chat template. Placeholders: {channel}, {team_color}, {role_color}, {rolecolor}, {role_name}, {role}, {name}, {content}.")]
            public string TeamChatTemplate { get; set; } = "<b>[{channel}][<color={rolecolor}>{role_name}</color>] {name}: {content}</b>";

            [Description("Seconds global chat remains visible.")]
            public float GlobalChatVisibleSeconds { get; set; } = 5f;

            [Description("Seconds team chat remains visible.")]
            public float TeamChatVisibleSeconds { get; set; } = 5f;

            [Description("Maximum global chat lines shown in the UI.")]
            public int GlobalMaxVisibleMessages { get; set; } = 6;

            [Description("Maximum team chat lines shown in the UI.")]
            public int TeamMaxVisibleMessages { get; set; } = 6;

            [Description("Maximum messages saved in the JSON database.")]
            public int MaxStoredMessages { get; set; } = 500;

            [Description("Global chat UI X coordinate.")]
            public float GlobalXCoordinate { get; set; } = 120f;

            [Description("Global chat UI Y coordinate.")]
            public float GlobalYCoordinate { get; set; } = 120f;

            [Description("Team chat UI X coordinate.")]
            public float TeamXCoordinate { get; set; } = 120f;

            [Description("Team chat UI Y coordinate.")]
            public float TeamYCoordinate { get; set; } = 230f;

            [Description("Blocked words. Messages containing these words will be rejected.")]
            public List<string> BlockedWords { get; set; } = new List<string>();

            [Description("Log accepted chat messages to server console.")]
            public bool LogChatMessages { get; set; } = true;

            [Description("Log messages rejected by the blocked-word system to server console.")]
            public bool LogBlockedMessages { get; set; } = true;

            [Description("Log chat UI refresh diagnostics to server console.")]
            public bool LogUiDebug { get; set; } = false;

            [Description("Team colors used by chat templates.")]
            public List<TeamChatColor> TeamColors { get; set; } = new List<TeamChatColor>
            {
                new TeamChatColor { Team = "SCPs", Color = "#FF4040" },
                new TeamChatColor { Team = "FoundationForces", Color = "#6699FF" },
                new TeamChatColor { Team = "ChaosInsurgency", Color = "#32CD32" },
                new TeamChatColor { Team = "Scientists", Color = "#FFD700" },
                new TeamChatColor { Team = "ClassD", Color = "#FF8C00" },
                new TeamChatColor { Team = "Dead", Color = "#B0B0B0" },
                new TeamChatColor { Team = "OtherAlive", Color = "#FFFFFF" },
            };
        }

        public class TeamChatColor
        {
            public string Team { get; set; } = "OtherAlive";
            public string Color { get; set; } = "#FFFFFF";
        }

        public class DamageManagerConfigClass
        {
            public List<DamageType> DisabledDamageTypes { get; set; } = new List<DamageType>
            {
                DamageType.Scp207,
                DamageType.Scp1509
            };
        }

        public class SurrenderConfigClass
        {
            [Description("Enable surrender module.")]
            public bool IsEnabled { get; set; } = true;

        }
    }
}
