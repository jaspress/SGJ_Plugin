using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using SGJ_Plugin.SpecialContent.Base;
using SGJ_Plugin.SpecialContent.CustomItems;
using SGJ_Plugin.SpecialContent.CustomRoles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SGJ_Plugin
{
    public class Config : IConfig
    {
        [Description("Enable the plugin.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enable debug logs.")]
        public bool Debug { get; set; } = false;

        [Description("Show Server name(title)")]
        public string ShowServerName { get; set; } = "诗歌剧服务器";

        [Description("Misc helper and welcome settings.")]
        public MiscConfigClass MiscConfig { get; set; } = new MiscConfigClass();

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

        [Description("SCP idle regeneration and health balance settings.")]
        public ScpIdleRegenConfigClass ScpIdleRegenConfig { get; set; } = new ScpIdleRegenConfigClass();

        [Description("Player surrender module settings.")]
        public SurrenderConfigClass SurrenderModuleConfig { get; set; } = new SurrenderConfigClass();

        [Description("Special RXSEND-style camps, roles, and items settings.")]
        public SpecialContentConfigClass SpecialContentConfig { get; set; } = new SpecialContentConfigClass();

        [Description("Skill hotkey and server-specific settings page.")]
        public SkillSystemConfigClass SkillSystemConfig { get; set; } = new SkillSystemConfigClass();

        [Description("Simple camp and special role task system settings.")]
        public TaskSystemConfigClass TaskSystemConfig { get; set; } = new TaskSystemConfigClass();

        [Description("SCP role swap request settings.")]
        public ScpSwapConfigClass ScpSwapConfig { get; set; } = new ScpSwapConfigClass();

        [Description("Custom RXBreach-style respawn wave settings.")]
        public CustomRespawnConfigClass CustomRespawnConfig { get; set; } = new CustomRespawnConfigClass();

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

        public class MiscConfigClass
        {
            [Description("Enable miscellaneous helper features in Main.")]
            public bool IsEnabled { get; set; } = true;

            [Description("Show welcome messages when a player joins.")]
            public bool WelcomeEnabled { get; set; } = true;

            [Description("Send the private welcome top-right hint to the joining player.")]
            public bool PrivateWelcomeEnabled { get; set; } = true;

            [Description("Send the public welcome broadcast to all players.")]
            public bool PublicWelcomeBroadcastEnabled { get; set; } = true;

            [Description("Private welcome text. Placeholders: {ev.Player.Nickname}, {player_name}, {name}, {Config.ShowServerName}, {server_name}.")]
            public string PrivateWelcomeText { get; set; } = "<b><size=20><color=#7FFFD4>[私人信息]欢迎<color=#90EE90>{ev.Player.Nickname}</color>!</color></size></b>";

            [Description("Public welcome broadcast text. Placeholders: {ev.Player.Nickname}, {player_name}, {name}, {Config.ShowServerName}, {server_name}.")]
            public string PublicWelcomeBroadcastText { get; set; } = "<b><size=25><color=#7FFFD4>[公告📢]欢迎<color=#90EE90>{ev.Player.Nickname}</color>加入{Config.ShowServerName}!</color></size></b>";

            [Description("Private welcome hint duration in seconds.")]
            public float PrivateWelcomeDuration { get; set; } = 5f;

            [Description("Public welcome broadcast duration in seconds.")]
            public float PublicWelcomeBroadcastDuration { get; set; } = 5f;

            [Description("Shared helper UI refresh interval in seconds. Lower values make queued UI countdowns smoother.")]
            public float UiRefreshIntervalSeconds { get; set; } = 0.5f;

            [Description("Delay before queued server broadcasts are shown, in seconds.")]
            public float BroadcastDelaySeconds { get; set; } = 3f;

            [Description("Show a persistent top status bar.")]
            public bool TopStatusEnabled { get; set; } = true;

            [Description("Top status text. Placeholders: {Config.ShowServerName}, {server_name}, {plugin_version}, {server_tps}, {server_max_tps}, {player_count}, {max_player_count}")]
            public string TopStatusText { get; set; } = "<size=22.5>| {Config.ShowServerName} | TPS: {server_tps}/{server_max_tps} | Plugin {plugin_version} |</size>";

            [Description("Top status X coordinate.")]
            public float TopStatusXCoordinate { get; set; } = 0f;

            [Description("Top status Y coordinate.")]
            public float TopStatusYCoordinate { get; set; } = 20f;

            [Description("Top status font size.")]
            public int TopStatusFontSize { get; set; } = 18;

            [Description("Line-height percent used by queued top-right UI hints to avoid rich-text lines sticking together.")]
            public int TopRightHintLineHeightPercent { get; set; } = 140;

            [Description("Blank lines inserted between queued top-right UI messages.")]
            public int TopRightHintMessageSpacingLines { get; set; } = 1;

            [Description("Maximum visible messages in the top-right queued UI. Older messages are dropped first.")]
            public int TopRightHintMaxVisibleMessages { get; set; } = 4;

            [Description("Maximum estimated text lines in the top-right queued UI.")]
            public int TopRightHintMaxVisibleLines { get; set; } = 10;

            [Description("Center-top queued hint X coordinate.")]
            public float CenterTopHintXCoordinate { get; set; } = 0f;

            [Description("Center-top queued hint Y coordinate.")]
            public float CenterTopHintYCoordinate { get; set; } = 150f;

            [Description("Center-top queued hint font size.")]
            public int CenterTopHintFontSize { get; set; } = 22;

            [Description("Line-height percent used by queued center-top UI hints.")]
            public int CenterTopHintLineHeightPercent { get; set; } = 140;

            [Description("Blank lines inserted between queued center-top UI messages.")]
            public int CenterTopHintMessageSpacingLines { get; set; } = 1;

            [Description("Maximum visible messages in the center-top queued UI. Older messages are dropped first.")]
            public int CenterTopHintMaxVisibleMessages { get; set; } = 3;

            [Description("Maximum estimated text lines in the center-top queued UI.")]
            public int CenterTopHintMaxVisibleLines { get; set; } = 8;

            [Description("Maximum visible queued server broadcast messages.")]
            public int BroadcastMaxVisibleMessages { get; set; } = 3;

            [Description("Maximum estimated lines in queued server broadcasts.")]
            public int BroadcastMaxVisibleLines { get; set; } = 7;

            [Description("Exclusive center info UI X coordinate. This UI is replaced by the newest role/item introduction.")]
            public float CenterInfoXCoordinate { get; set; } = 0f;

            [Description("Exclusive center info UI Y coordinate.")]
            public float CenterInfoYCoordinate { get; set; } = 260f;

            [Description("Exclusive center info UI font size.")]
            public int CenterInfoFontSize { get; set; } = 21;

            [Description("Line-height percent used by exclusive center info UI.")]
            public int CenterInfoLineHeightPercent { get; set; } = 160;
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
            public string LevelUpText { get; set; } = "<b><size=20><color=#7FFFD4>[📢]你已升级到<color=#90EE90>{level}</color>!</color></size></b>";

            [Description("Experience gain hint template. Placeholders are the same as HUD text, plus {gained_xp} and {reason}.")]
            public string ExperienceGainText { get; set; } = "<b><size=20><color=#7FFFD4>[📢]你已增加经验值<color=#90EE90>{gained_xp}</color>!</color></size></b>";

            [Description("Experience notification X coordinate.")]
            public float ExperienceHintXCoordinate { get; set; } = 700f;

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

            [Description("Text added above the level HUD while spectating a player. Placeholders: {observed_name}.")]
            public string ObservedPlayerText { get; set; } = "<align=center><size=23><b>你正在观察：{observed_name}</b></size></align>";

            [Description("Spectator HUD text template. Placeholders: {server_name}, {player_count}, {max_player_count}, {respawn_wave}, {respawn_time}, {respawn_tickets}, {respawn_team}, {team_color}.")]
            public string HudText { get; set; } = "<align=right><b><size=21.5>{server_name}\n玩家：{player_count}/{max_player_count}\n\n{respawn_wave}\n\n倒计时：{respawn_time}\n\n票数：{respawn_tickets}\n\n阵营：<color={team_color}>{respawn_team}</color></size></b></align>";

            [Description("Template used when no spectated player is found. Placeholders: {server_name}, {player_count}, {max_player_count}, {respawn_wave}, {respawn_time}, {respawn_tickets}, {respawn_team}, {team_color}.")]
            public string NoObservedPlayerText { get; set; } = "<align=right><b><size=21.5>{server_name}\n玩家：{player_count}/{max_player_count}\n\n{respawn_wave}\n\n倒计时：{respawn_time}\n\n票数：{respawn_tickets}\n\n阵营：<color={team_color}>{respawn_team}</color></size></b></align>";

            [Description("Respawn wave info template. Placeholders: {server_name}, {player_count}, {max_player_count}, {respawn_wave}, {respawn_time}, {respawn_tickets}, {respawn_team}, {team_color}.")]
            public string RespawnInfoText { get; set; } = "<align=right><b><size=21.5>{server_name} - 玩家：{player_count}/{max_player_count}\n\n{respawn_wave}\n\n倒计时：{respawn_time}\n\n票数：{respawn_tickets}\n\n阵营：<color={team_color}>{respawn_team}</color></size></b></align>";

            [Description("Maximum visible lines. Extra lines are trimmed to avoid going off screen.")]
            public int MaxVisibleLines { get; set; } = 12;

            [Description("Spectator HUD X coordinate.")]
            public float HudXCoordinate { get; set; } = 870f;

            [Description("Spectator HUD Y coordinate.")]
            public float HudYCoordinate { get; set; } = 865f;

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

            [Description("Chat log JSON file name stored in %AppData%/EXILED/Configs/.")]
            public string DataFileName { get; set; } = "ChatLog.json";

            [Description("Blocked words JSON file name stored in %AppData%/EXILED/Configs/. The file contains a JSON string array, for example [\"word1\", \"word2\"].")]
            public string BlockedWordsDataFileName { get; set; } = "ChatBlockedWords.json";

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

            [Description("Default blocked words written when the blocked words JSON file is first created.")]
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
                DamageType.Scp207
            };
        }

        public class ScpIdleRegenConfigClass
        {
            [Description("Enable SCP idle regeneration. SCP-079 is always ignored.")]
            public bool IsEnabled { get; set; } = true;

            [Description("Seconds an SCP must stay still before regeneration starts.")]
            public float IdleSecondsRequired { get; set; } = 15f;

            [Description("How often SCP idle regeneration is checked and applied, in seconds.")]
            public float HealIntervalSeconds { get; set; } = 1f;

            [Description("Maximum movement distance between checks still counted as standing still.")]
            public float StillDistanceThreshold { get; set; } = 0.08f;

            [Description("Healing amount per tick by SCP role. SCP-079 is intentionally omitted.")]
            public List<ScpRoleAmount> HealAmountsByRole { get; set; } = new List<ScpRoleAmount>
            {
                new ScpRoleAmount { Role = RoleTypeId.Scp049.ToString(), Amount = 5f },
                new ScpRoleAmount { Role = RoleTypeId.Scp0492.ToString(), Amount = 5f },
                new ScpRoleAmount { Role = RoleTypeId.Scp096.ToString(), Amount = 5f },
                new ScpRoleAmount { Role = RoleTypeId.Scp106.ToString(), Amount = 5f },
                new ScpRoleAmount { Role = RoleTypeId.Scp173.ToString(), Amount = 5f },
                new ScpRoleAmount { Role = RoleTypeId.Scp939.ToString(), Amount = 5f },
                new ScpRoleAmount { Role = RoleTypeId.Scp3114.ToString(), Amount = 5f },
            };

            [Description("Optional SCP max-health overrides. Empty means do not change SCP health.")]
            public List<ScpRoleAmount> HealthOverridesByRole { get; set; } = new List<ScpRoleAmount>();

            [Description("Show a rich-text UI hint when idle regeneration heals an SCP.")]
            public bool ShowHealHint { get; set; } = true;

            [Description("Idle regeneration hint text. Placeholders: {role}, {amount}, {health}, {max_health}.")]
            public string HealHintText { get; set; } = "<b><size=20><color=#FF7777>[SCP恢复]</color>\n静止恢复 <color=#90EE90>+{amount} HP</color>\n生命值：<color=#FFFFFF>{health}/{max_health}</color></size></b>";

            [Description("Idle regeneration hint duration in seconds.")]
            public float HealHintDuration { get; set; } = 2f;
        }

        public class ScpRoleAmount
        {
            public string Role { get; set; } = RoleTypeId.Scp173.ToString();
            public float Amount { get; set; } = 5f;
        }

        public class SurrenderConfigClass
        {
            [Description("Enable surrender module.")]
            public bool IsEnabled { get; set; } = true;

        }

        public class SpecialContentConfigClass
        {
            [Description("Enable special camps, roles, and items module.")]
            public bool IsEnabled { get; set; } = false;

            [Description("Enable applying special role profiles to players after they spawn.")]
            public bool ApplySpecialRolesOnSpawn { get; set; } = false;

            [Description("Enable giving configured loadout items for special roles.")]
            public bool GiveRoleLoadouts { get; set; } = false;

            [Description("Clear vanilla role inventory before giving special role loadout items.")]
            public bool ClearInventoryBeforeLoadout { get; set; } = true;

            [Description("Enable special item definitions. Unknown custom items stay configurable but are not spawned unless mapped to an existing ItemType.")]
            public bool EnableSpecialItems { get; set; } = false;

            [Description("Show a private hint when a special role is applied.")]
            public bool ShowAssignedRoleHint { get; set; } = false;

            [Description("Special role hint text. Placeholders: {role_name}, {camp_name}, {base_role}.")]
            public string AssignedRoleHintText { get; set; } = "<b><size=20><color=#7FFFD4>[特殊角色]</color> 你正在扮演：<color=#90EE90>{role_name}</color>\n阵营：{camp_name}</size></b>";

            [Description("Special role hint duration in seconds.")]
            public float AssignedRoleHintDuration { get; set; } = 20f;

            [Description("Show role introduction in the exclusive center info UI when a special role is assigned.")]
            public bool ShowRoleIntroduction { get; set; } = false;

            [Description("Role introduction template. Placeholders: {role_name}, {camp_name}, {description}, {health}, {stamina}, {speed}, {resistance_head}, {resistance_body}, {resistance_arm}, {resistance_leg}, {primary_skill}, {secondary_skill}, {primary_skill_description}, {secondary_skill_description}, {primary_cooldown}, {secondary_cooldown}, {loadout}.")]
            public string RoleIntroductionText { get; set; } = "<b><size=21><color={role_color}>[{role_name}]</color></size></b>\n<line-height=150%><size=18>{description}\n阵营：<color={role_color}>{camp_name}</color>\n血量：{health} | 耐力：{stamina} | 速度：{speed}\n子弹抗性：头 {resistance_head}% / 身 {resistance_body}% / 手 {resistance_arm}% / 腿 {resistance_leg}%\n主技能：<color=#90EE90>{primary_skill}</color> [{primary_cooldown}s] - {primary_skill_description}\n副技能：<color=#90EE90>{secondary_skill}</color> [{secondary_cooldown}s] - {secondary_skill_description}\n背包：{loadout}</size></line-height>";

            [Description("Exclusive center role introduction duration in seconds.")]
            public float RoleIntroductionDuration { get; set; } = 20f;

            [Description("Show item introduction in the exclusive center info UI when switching held item.")]
            public bool ShowItemIntroductionOnSwitch { get; set; } = false;

            [Description("Special item introduction template. Placeholders: {item_name}, {item_type}, {item_tag}, {description}.")]
            public string SpecialItemIntroductionText { get; set; } = "<b><size=21><color=#7FFFD4>[特殊物品]</color> <color=#90EE90>{item_name}</color></size></b>\n<line-height=160%><size=19>{description}</size></line-height>";

            [Description("Normal item introduction template. Placeholders: {item_name}, {item_type}, {item_tag}, {description}.")]
            public string NormalItemIntroductionText { get; set; } = "<b><size=21><color=#DCDCDC>[普通物品]</color> <color=#FFFFFF>{item_name}</color></size></b>\n<line-height=160%><size=19>{description}</size></line-height>";

            [Description("Held item introduction duration in seconds.")]
            public float ItemIntroductionDuration { get; set; } = 5f;

            [Description("Add special role prefix to DisplayNickname.")]
            public bool UpdateDisplayNickname { get; set; } = false;

            [Description("Special role display name template. Placeholders: {role_name}, {camp_name}, {base_name}.")]
            public string DisplayNicknameTemplate { get; set; } = "[{role_name}] {base_name}";

            [Description("Role enable switches by special role name. Defaults include RXSEND wiki role names and are all enabled.")]
            public Dictionary<string, bool> EnabledRoles { get; set; } = new Dictionary<string, bool>();

            [Description("Camp enable switches by camp name. Defaults include RXSEND wiki camps and are all enabled.")]
            public Dictionary<string, bool> EnabledCamps { get; set; } = new Dictionary<string, bool>();

            [Description("Special item enable switches by item name. Defaults include RXSEND wiki item categories/items and are all enabled.")]
            public Dictionary<string, bool> EnabledItems { get; set; } = new Dictionary<string, bool>();

            [Description("Special role definitions. BaseRole uses existing RoleTypeId. Use Tutorial for camps without matching vanilla role, such as Serpent's Hand.")]
            public List<SpecialRoleDefinition> Roles { get; set; } = new List<SpecialRoleDefinition>();

            [Description("Special item definitions mapped to existing ItemType names.")]
            public List<SpecialItemDefinition> Items { get; set; } = new List<SpecialItemDefinition>();
        }

        public class SkillSystemConfigClass
        {
            [Description("Enable custom role skill hotkeys.")]
            public bool IsEnabled { get; set; } = false;

            [Description("Server-specific settings page header.")]
            public string SettingsHeader { get; set; } = "SGJ 技能系统";

            [Description("Collection id used by server-specific settings.")]
            public byte CollectionId { get; set; } = 20;

            [Description("Header setting id.")]
            public int HeaderSettingId { get; set; } = 61000;

            [Description("Primary skill setting id.")]
            public int PrimarySkillSettingId { get; set; } = 61001;

            [Description("Secondary skill setting id.")]
            public int SecondarySkillSettingId { get; set; } = 61002;

            [Description("Primary skill label.")]
            public string PrimarySkillLabel { get; set; } = "主技能";

            [Description("Secondary skill label.")]
            public string SecondarySkillLabel { get; set; } = "副技能";

            [Description("Suggested UnityEngine.KeyCode for primary skill, for example F, G, H, Alpha1.")]
            public string PrimarySkillKey { get; set; } = "F";

            [Description("Suggested UnityEngine.KeyCode for secondary skill, for example G, H, Alpha2.")]
            public string SecondarySkillKey { get; set; } = "G";

            [Description("Prevent interaction while GUI is open.")]
            public bool PreventInteractionOnGui { get; set; } = true;

            [Description("Allow spectators to trigger skills.")]
            public bool AllowSpectatorTrigger { get; set; } = false;

            [Description("Skill cooldown in seconds.")]
            public float SkillCooldownSeconds { get; set; } = 5f;

            [Description("Text shown when skill is on cooldown. Placeholders: {seconds}.")]
            public string CooldownText { get; set; } = "<b><size=20><color=#FF9999>[技能]</color> 冷却中：{seconds}s</size></b>";

            [Description("Text shown when no special role skill is available.")]
            public string NoSkillText { get; set; } = "<b><size=20><color=#FF9999>[技能]</color> 当前角色没有可用技能</size></b>";

            [Description("Show fixed skill cooldown HUD at the bottom-right of the screen.")]
            public bool ShowSkillHud { get; set; } = false;

            [Description("Skill HUD X coordinate.")]
            public float SkillHudXCoordinate { get; set; } = 850f;

            [Description("Skill HUD Y coordinate. Default matches the level HUD height.")]
            public float SkillHudYCoordinate { get; set; } = 1015f;

            [Description("Skill HUD font size.")]
            public int SkillHudFontSize { get; set; } = 18;

            [Description("Skill HUD line-height percent.")]
            public int SkillHudLineHeightPercent { get; set; } = 130;

            [Description("Skill HUD template. Placeholders: {primary_skill}, {primary_status}, {secondary_skill}, {secondary_status}.")]
            public string SkillHudText { get; set; } = "<align=right><line-height=130%><b><size=18><color=#7FFFD4>{primary_skill}</color> <color=#FFFFFF>[{primary_status}]</color>\n<color=#7FFFD4>{secondary_skill}</color> <color=#FFFFFF>[{secondary_status}]</color></size></b></line-height></align>";
        }

        public class TaskSystemConfigClass
        {
            [Description("Enable simple task system.")]
            public bool IsEnabled { get; set; } = false;

            [Description("Assign a task after a player spawns.")]
            public bool AssignOnSpawn { get; set; } = false;

            [Description("Delay before assigning task after spawn, in seconds.")]
            public float AssignDelaySeconds { get; set; } = 1.2f;

            [Description("Show task assigned hint.")]
            public bool ShowAssignedHint { get; set; } = true;

            [Description("Show task progress hint.")]
            public bool ShowProgressHint { get; set; } = true;

            [Description("Show task completed hint.")]
            public bool ShowCompletedHint { get; set; } = true;

            [Description("Task assigned hint text. Placeholders: {task_name}, {description}, {progress}, {target}, {reward_xp}.")]
            public string AssignedHintText { get; set; } = "<b><size=20><color=#7FFFD4>[任务]</color> {task_name}\n{description}\n奖励：{reward_xp} EXP</size></b>";

            [Description("Task progress hint text. Placeholders: {task_name}, {description}, {progress}, {target}, {reward_xp}.")]
            public string ProgressHintText { get; set; } = "<b><size=20><color=#7FFFD4>[任务]</color> {task_name}: {progress}/{target}</size></b>";

            [Description("Task completed hint text. Placeholders: {task_name}, {description}, {progress}, {target}, {reward_xp}.")]
            public string CompletedHintText { get; set; } = "<b><size=20><color=#90EE90>[任务完成]</color> {task_name}\n获得 {reward_xp} EXP</size></b>";

            [Description("Task definitions. MatchRoleName has priority over MatchCamp. TaskType: KillPlayers, KillScps, KillHumans, Escape.")]
            public List<TaskDefinition> Tasks { get; set; } = new List<TaskDefinition>();
        }

        public class TaskDefinition
        {
            public string Name { get; set; } = "基础任务";
            public string Description { get; set; } = "完成一个简单目标。";
            public string MatchCamp { get; set; } = string.Empty;
            public string MatchRoleName { get; set; } = string.Empty;
            public string TaskType { get; set; } = "KillPlayers";
            public int TargetCount { get; set; } = 1;
            public int RewardExperience { get; set; } = 25;
            public bool IsEnabled { get; set; } = true;
        }

        public class ScpSwapConfigClass
        {
            [Description("Enable .swap command for harmful SCP role swaps.")]
            public bool IsEnabled { get; set; } = true;

            [Description("Request timeout in seconds.")]
            public float RequestTimeoutSeconds { get; set; } = 20f;

            [Description("Harmful SCP roles allowed to swap.")]
            public List<string> AllowedScpRoles { get; set; } = new List<string>
            {
                RoleTypeId.Scp049.ToString(),
                RoleTypeId.Scp079.ToString(),
                RoleTypeId.Scp096.ToString(),
                RoleTypeId.Scp106.ToString(),
                RoleTypeId.Scp173.ToString(),
                RoleTypeId.Scp939.ToString(),
                RoleTypeId.Scp3114.ToString(),
            };

            [Description("Hint shown to harmful SCPs after spawn.")]
            public string SpawnHintText { get; set; } = "<b><size=20><color=#FF9999>[SCP交换]</color>\n\n可使用：<color=#90EE90><b>.swap [SCP名字]</b></color>\n请求和其他SCP交换角色\n\n同意：<color=#90EE90><b>.swap al</b></color>\n拒绝：<color=#FF9999><b>.swap nal</b></color></size></b>";

            [Description("Request sent hint. Placeholders: {target}, {role}, {seconds}.")]
            public string RequestSentText { get; set; } = "<b><size=20><color=#7FFFD4>[SCP交换]</color> 已向 <color=#90EE90>{target}</color> 请求交换 {role}，{seconds}s 后超时</size></b>";

            [Description("Request received hint. Placeholders: {requester}, {requester_role}, {your_role}, {seconds}.")]
            public string RequestReceivedText { get; set; } = "<b><size=20><color=#7FFFD4>[SCP交换]</color>\n\n<color=#90EE90>{requester}</color> 想用 {requester_role}\n和你的 {your_role} 交换\n\n同意：<color=#90EE90><b>.swap al</b></color>\n拒绝：<color=#FF9999><b>.swap nal</b></color>\n\n剩余 {seconds}s</size></b>";

            public string AcceptedText { get; set; } = "<b><size=20><color=#90EE90>[SCP交换]</color> 交换成功</size></b>";
            public string DeniedText { get; set; } = "<b><size=20><color=#FF9999>[SCP交换]</color> 对方拒绝了交换请求</size></b>";
            public string TimeoutText { get; set; } = "<b><size=20><color=#FF9999>[SCP交换]</color> 交换请求已超时</size></b>";
            public string NoRequestText { get; set; } = "<b><size=20><color=#FF9999>[SCP交换]</color> 当前没有待处理请求</size></b>";
        }

        public class CustomRespawnConfigClass
        {
            [Description("Enable custom respawn wave role/faction replacement.")]
            public bool IsEnabled { get; set; } = false;

            [Description("Delay after vanilla respawn before applying special role profiles.")]
            public float ApplySpecialRoleDelaySeconds { get; set; } = 0.35f;

            [Description("Show spawned wave hint to respawned players.")]
            public bool ShowWaveHint { get; set; } = true;

            [Description("Wave hint text. Placeholders: {wave_name}, {camp_name}.")]
            public string WaveHintText { get; set; } = "<b><size=21><color=#7FFFD4>[支援刷新]</color>\n阵营：<color=#90EE90>{camp_name}</color>\n波次：{wave_name}</size></b>";

            [Description("Wave definitions. BaseWave: Ntf, Chaos, Any.")]
            public List<RespawnWaveDefinition> Waves { get; set; } = new List<RespawnWaveDefinition>();
        }

        public class RespawnWaveDefinition
        {
            public bool IsEnabled { get; set; } = true;
            public string Name { get; set; } = "支援波次";
            public string Camp { get; set; } = "特殊阵营";
            public string BaseWave { get; set; } = "Ntf";
            public int Weight { get; set; } = 1;
            public int MaxRespawnAmount { get; set; } = 0;
            public string Color { get; set; } = "#FFFFFF";
            public List<string> RoleNames { get; set; } = new List<string>();
            public List<string> RoleQueue { get; set; } = new List<string>();
        }

        public class SpecialRoleDefinition : CustomRoleBase
        {
        }

        public class SpecialItemDefinition : CustomItemBase
        {
        }

        private static Dictionary<string, bool> DefaultEnabledRoles()
        {
            return ToEnabledDictionary(ToDerivativeNames(new[]
            {
                "九尾狐 指挥官", "九尾狐 狙击手", "九尾狐 战斗专家", "九尾狐 士兵",
                "快速反应部队 指挥官", "快速反应部队 机枪手", "快速反应部队 精准射手", "快速反应部队 医疗兵", "快速反应部队 突击队员", "快速反应部队 盾牌手", "快速反应部队 士兵",
                "精锐快反 指挥官", "精锐快反 工程师", "精锐快反 机枪手", "精锐快反 医疗兵", "精锐快反 士兵",
                "战术应对二部 指挥官", "战术应对二部 机枪手", "战术应对二部 工程师", "战术应对二部 医疗兵", "战术应对二部 士兵",
                "落锤特战A连 指挥官", "落锤特战A连 无畏战士", "落锤特战A连 医疗专家", "落锤特战A连 支援兵", "落锤特战A连 作战专家", "落锤特战A连 先锋", "落锤特战A连 士兵",
                "落锤特战B连 指挥官", "落锤特战B连 副指挥", "落锤特战B连 机枪手", "落锤特战B连 技术员", "落锤特战B连 毒气专家", "落锤特战B连 医疗兵", "落锤特战B连 士兵",
                "落锤特战B连3队 组长", "落锤特战B连3队 维修专家", "落锤特战B连3队 组员",
                "律法左手调查小队 执法官", "律法左手调查小队 助手", "律法左手调查小队 抓捕手", "律法左手调查小队 征召人员"
            }));
        }

        private static Dictionary<string, bool> DefaultEnabledCamps()
        {
            return ToEnabledDictionary(new[]
            {
                "基金会阵营", "九尾狐小队", "快速反应部队", "精锐快速反应部队", "战术应对二部",
                "落锤特战A连", "落锤特战B连", "落锤特战B连3队", "律法左手调查小队"
            });
        }

        private static Dictionary<string, bool> DefaultEnabledItems()
        {
            return ToEnabledDictionary(ToDerivativeNames(new[]
            {
                "ID卡", "通用权限卡", "警卫权限卡", "安保权限卡", "科研权限卡", "特殊权限卡", "O5权限卡",
                "缴械与手铐", "对讲机", "九尾狐权限卡", "P90-NTF冲锋枪", "沙漠之鹰手枪", "m82a1狙击枪",
                "特殊护理医疗包", "肾上腺素注射器", "苏打水", "绿色夜视仪", "防毒面具",
                "近战武器", "远程武器", "CI对讲机", "断手", "大G钥匙", "cheems", "小说", "“重要”情报文件",
                "医疗用品", "食品", "SCP-005 万能钥匙", "SCP-009 红冰", "SCP-109 无限水壶", "SCP-127 活体枪", "SCP-207 一箱可乐",
                "SCP-215 拟人眼镜", "SCP-268 疏忽帽", "SCP-294 咖啡机", "SCP-303 门后男", "SCP-330 只能拿两个",
                "SCP-409 晶蔓", "SCP-427 洛夫克拉夫特吊坠", "SCP-500 万能药", "SCP-1025 疾病百科大全",
                "SCP-1033-RU 防御手镯", "SCP-1499 防毒面具", "SCP-3238 dado汁", "伪装服饰", "防化服", "防护装备", "视觉辅助", "背包扩容", "工具箱", "硬币，眼药水", "电池"
            }));
        }

        private static List<SpecialRoleDefinition> DefaultSpecialRoles()
        {
            List<SpecialRoleDefinition> roles = new List<SpecialRoleDefinition>();
            AddRole(roles, new NtfCommanderRole());
            AddRole(roles, new NtfSniperRole());
            AddRole(roles, new NtfCombatSpecialistRole());

            foreach (string name in DefaultEnabledRoles().Keys)
            {
                if (roles.Exists(role => role.Name == name))
                    continue;

                roles.Add(new SpecialRoleDefinition
                {
                    Name = name,
                    Camp = GuessCamp(name),
                    BaseRole = GuessBaseRole(name).ToString(),
                    Health = GuessHealth(name),
                    ArtificialHealth = GuessArtificialHealth(name),
                    Stamina = 100,
                    Speed = GuessSpeed(name),
                    BulletResistanceHead = GuessBulletResistanceHead(name),
                    BulletResistanceBody = GuessBulletResistanceBody(name),
                    BulletResistanceArm = GuessBulletResistanceArm(name),
                    BulletResistanceLeg = GuessBulletResistanceLeg(name),
                    BadgeColor = GuessBadgeColor(name),
                    RoleColor = GuessRoleColor(name),
                    KillExperience = GuessSpecialKillExperience(name),
                    Description = GuessRoleDescription(name),
                    PrimarySkillEnabled = GuessHasSkill(name, true),
                    PrimarySkillName = "角色能力",
                    PrimarySkillDescription = $"触发 {name} 的主能力。",
                    PrimarySkillCooldownSeconds = GuessSkillCooldown(name, true),
                    SecondarySkillEnabled = GuessHasSkill(name, false),
                    SecondarySkillName = "辅助能力",
                    SecondarySkillDescription = $"触发 {name} 的辅助能力。",
                    SecondarySkillCooldownSeconds = GuessSkillCooldown(name, false),
                    LoadoutItems = ToDerivativeNames(GuessLoadout(name)),
                });
            }

            return roles;
        }

        private static List<SpecialItemDefinition> DefaultSpecialItems()
        {
            List<SpecialItemDefinition> items = new List<SpecialItemDefinition>();
            AddItem(items, new IdCardCustomItem());
            AddItem(items, new ScientistKeycardCustomItem());
            AddItem(items, new SecurityKeycardCustomItem());
            AddItem(items, new RangedWeaponCustomItem());
            AddItem(items, new MeleeWeaponCustomItem());
            AddItem(items, new CiRadioCustomItem());
            AddItem(items, new MedicalCustomItem());
            AddItem(items, new FoodCustomItem());
            AddItem(items, new Scp500CustomItem());
            AddItem(items, new Scp207CustomItem());
            AddItem(items, new Scp268CustomItem());
            AddItem(items, new ArmorCustomItem());

            foreach (SpecialItemDefinition item in new List<SpecialItemDefinition>
            {
                new SpecialItemDefinition { Name = "ID卡", GameItem = ItemType.KeycardJanitor.ToString() },
                new SpecialItemDefinition { Name = "通用权限卡", GameItem = ItemType.KeycardZoneManager.ToString() },
                new SpecialItemDefinition { Name = "警卫权限卡", GameItem = ItemType.KeycardGuard.ToString() },
                new SpecialItemDefinition { Name = "安保权限卡", GameItem = ItemType.KeycardMTFPrivate.ToString() },
                new SpecialItemDefinition { Name = "缴械与手铐", GameItem = ItemType.None.ToString(), GiveByDefault = false, IsSpecialItem = false, Description = "左键缴械武器；右键佩戴手铐。" },
                new SpecialItemDefinition { Name = "对讲机", GameItem = ItemType.Radio.ToString(), IsSpecialItem = false, Description = "对讲机。" },
                new SpecialItemDefinition { Name = "九尾狐权限卡", GameItem = ItemType.KeycardMTFCaptain.ToString(), IsSpecialItem = false, Description = "九尾狐权限卡。" },
                new SpecialItemDefinition { Name = "P90-NTF冲锋枪", GameItem = ItemType.GunFSP9.ToString(), IsSpecialItem = false, Description = "P90-NTF冲锋枪；当前版本使用FSP-9冲锋枪替代。" },
                new SpecialItemDefinition { Name = "沙漠之鹰手枪", GameItem = ItemType.GunRevolver.ToString(), IsSpecialItem = false, Description = "沙漠之鹰手枪；当前版本映射为左轮手枪。" },
                new SpecialItemDefinition { Name = "m82a1狙击枪", GameItem = ItemType.GunE11SR.ToString(), IsSpecialItem = false, Description = "m82a1狙击枪；当前版本使用E-11-SR替代，并加装高倍镜。", AttachmentNames = new List<string> { "ScopeSight" } },
                new SpecialItemDefinition { Name = "特殊护理医疗包", GameItem = ItemType.Medkit.ToString(), Description = "特殊护理医疗包：可在移动时使用，基础行为使用当前游戏医疗包。" },
                new SpecialItemDefinition { Name = "肾上腺素注射器", GameItem = ItemType.Adrenaline.ToString(), IsSpecialItem = false, Description = "肾上腺素注射器。" },
                new SpecialItemDefinition { Name = "苏打水", GameItem = ItemType.SCP207.ToString(), IsSpecialItem = false, Description = "苏打水；当前版本映射为SCP-207。" },
                new SpecialItemDefinition { Name = "绿色夜视仪", GameItem = ItemType.SCP1344.ToString(), IsSpecialItem = false, Description = "绿色夜视仪；当前版本映射为SCP-1344视觉辅助物品。" },
                new SpecialItemDefinition { Name = "防毒面具", GameItem = ItemType.AntiSCP207.ToString(), IsSpecialItem = false, Description = "防毒面具；当前版本映射为Anti-SCP-207。" },
                new SpecialItemDefinition { Name = "科研权限卡", GameItem = ItemType.KeycardScientist.ToString() },
                new SpecialItemDefinition { Name = "特殊权限卡", GameItem = ItemType.KeycardFacilityManager.ToString() },
                new SpecialItemDefinition { Name = "O5权限卡", GameItem = ItemType.KeycardO5.ToString() },
                new SpecialItemDefinition { Name = "近战武器", GameItem = ItemType.Jailbird.ToString() },
                new SpecialItemDefinition { Name = "远程武器", GameItem = ItemType.GunCOM15.ToString() },
                new SpecialItemDefinition { Name = "CI对讲机", GameItem = ItemType.Radio.ToString() },
                new SpecialItemDefinition { Name = "医疗用品", GameItem = ItemType.Medkit.ToString() },
                new SpecialItemDefinition { Name = "食品", GameItem = ItemType.Painkillers.ToString() },
                new SpecialItemDefinition { Name = "SCP-207 一箱可乐", GameItem = ItemType.SCP207.ToString() },
                new SpecialItemDefinition { Name = "SCP-268 疏忽帽", GameItem = ItemType.SCP268.ToString() },
                new SpecialItemDefinition { Name = "SCP-330 只能拿两个", GameItem = ItemType.SCP330.ToString() },
                new SpecialItemDefinition { Name = "SCP-500 万能药", GameItem = ItemType.SCP500.ToString() },
                new SpecialItemDefinition { Name = "防护装备", GameItem = ItemType.ArmorCombat.ToString() },
                new SpecialItemDefinition { Name = "硬币，眼药水", GameItem = ItemType.Coin.ToString() },
                new SpecialItemDefinition { Name = "电池", GameItem = ItemType.Flashlight.ToString() },
            })
            {
                item.Name = DerivativeNames.ToDerivativeName(item.Name);
                item.IsSpecialItem = false;
                item.Description = item.Name;
                if (!items.Exists(existing => existing.Name == item.Name))
                    items.Add(item);
            }

            return items;
        }

        private static List<RespawnWaveDefinition> DefaultRespawnWaves()
        {
            return new List<RespawnWaveDefinition>
            {
                CreateRespawnWave("九尾狐支援", "九尾狐小队", "Ntf", "#6699FF", 4,
                    new[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.NtfPrivate, RoleTypeId.NtfPrivate },
                    new[] { "九尾狐 指挥官", "九尾狐 狙击手", "九尾狐 战斗专家", "九尾狐 士兵" }),

                CreateRespawnWave("快速反应支援", "快速反应部队", "Ntf", "#66CCFF", 3,
                    new[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.NtfPrivate, RoleTypeId.NtfPrivate },
                    new[] { "快速反应部队 指挥官", "快速反应部队 机枪手", "快速反应部队 精准射手", "快速反应部队 医疗兵", "快速反应部队 突击队员", "快速反应部队 盾牌手", "快速反应部队 士兵" }),

                CreateRespawnWave("精锐快反支援", "精锐快速反应部队", "Ntf", "#66CCFF", 2,
                    new[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfSpecialist, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.NtfPrivate },
                    new[] { "精锐快反 指挥官", "精锐快反 工程师", "精锐快反 机枪手", "精锐快反 医疗兵", "精锐快反 士兵" }),

                CreateRespawnWave("战术应对二部支援", "战术应对二部", "Ntf", "#5DADE2", 2,
                    new[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.NtfSpecialist, RoleTypeId.NtfPrivate },
                    new[] { "战术应对二部 指挥官", "战术应对二部 机枪手", "战术应对二部 工程师", "战术应对二部 医疗兵", "战术应对二部 士兵" }),

                CreateRespawnWave("落锤特战A连支援", "落锤特战A连", "Ntf", "#4DA6FF", 2,
                    new[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.NtfPrivate, RoleTypeId.NtfPrivate },
                    new[] { "落锤特战A连 指挥官", "落锤特战A连 无畏战士", "落锤特战A连 医疗专家", "落锤特战A连 支援兵", "落锤特战A连 作战专家", "落锤特战A连 先锋", "落锤特战A连 士兵" }),

                CreateRespawnWave("落锤特战B连支援", "落锤特战B连", "Ntf", "#4DA6FF", 2,
                    new[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.NtfPrivate, RoleTypeId.NtfPrivate },
                    new[] { "落锤特战B连 指挥官", "落锤特战B连 副指挥", "落锤特战B连 机枪手", "落锤特战B连 技术员", "落锤特战B连 毒气专家", "落锤特战B连 医疗兵", "落锤特战B连 士兵" }),

                CreateRespawnWave("落锤特战B连3队支援", "落锤特战B连3队", "Ntf", "#4DA6FF", 1,
                    new[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfSpecialist, RoleTypeId.NtfPrivate, RoleTypeId.NtfPrivate },
                    new[] { "落锤特战B连3队 组长", "落锤特战B连3队 维修专家", "落锤特战B连3队 组员" }),

                CreateRespawnWave("律法左手支援", "律法左手调查小队", "Ntf", "#9BB7FF", 1,
                    new[] { RoleTypeId.NtfCaptain, RoleTypeId.NtfSpecialist, RoleTypeId.NtfSergeant, RoleTypeId.NtfPrivate },
                    new[] { "律法左手调查小队 执法官", "律法左手调查小队 助手", "律法左手调查小队 抓捕手", "律法左手调查小队 征召人员" }),
            };
        }

        private static RespawnWaveDefinition CreateRespawnWave(string name, string camp, string baseWave, string color, int weight, IEnumerable<RoleTypeId> roleQueue, IEnumerable<string> roleNames)
        {
            return new RespawnWaveDefinition
            {
                Name = DerivativeNames.ToDerivativeName(name),
                Camp = camp,
                BaseWave = baseWave,
                Color = color,
                Weight = weight,
                RoleQueue = roleQueue.Select(role => role.ToString()).ToList(),
                RoleNames = ToDerivativeNames(roleNames),
            };
        }

        private static void AddRole(List<SpecialRoleDefinition> roles, CustomRoleBase role)
        {
            roles.Add(new SpecialRoleDefinition
            {
                Name = DerivativeNames.ToDerivativeName(role.Name),
                Camp = role.Camp,
                BaseRole = role.BaseRole,
                Health = role.Health,
                ArtificialHealth = role.ArtificialHealth,
                Stamina = role.Stamina,
                Speed = role.Speed,
                BulletResistanceHead = role.BulletResistanceHead,
                BulletResistanceBody = role.BulletResistanceBody,
                BulletResistanceArm = role.BulletResistanceArm,
                BulletResistanceLeg = role.BulletResistanceLeg,
                BadgeColor = role.BadgeColor,
                RoleColor = role.RoleColor,
                KillExperience = role.KillExperience,
                Description = role.Description,
                PrimarySkillEnabled = role.PrimarySkillEnabled,
                PrimarySkillName = role.PrimarySkillName,
                PrimarySkillDescription = role.PrimarySkillDescription,
                PrimarySkillCooldownSeconds = role.PrimarySkillCooldownSeconds,
                SecondarySkillEnabled = role.SecondarySkillEnabled,
                SecondarySkillName = role.SecondarySkillName,
                SecondarySkillDescription = role.SecondarySkillDescription,
                SecondarySkillCooldownSeconds = role.SecondarySkillCooldownSeconds,
                LoadoutItems = ToDerivativeNames(role.LoadoutItems),
            });
        }

        private static void AddItem(List<SpecialItemDefinition> items, CustomItemBase item)
        {
            items.Add(new SpecialItemDefinition
            {
                Name = DerivativeNames.ToDerivativeName(item.Name),
                GameItem = item.GameItem,
                GiveByDefault = item.GiveByDefault,
                IsSpecialItem = item.IsSpecialItem,
                Description = item.Description,
                PickupHintText = item.PickupHintText,
                ConsumeOnUse = item.ConsumeOnUse,
                HealOnUse = item.HealOnUse,
                ArtificialHealthOnUse = item.ArtificialHealthOnUse,
                EffectOnUse = item.EffectOnUse,
                EffectOnUseIntensity = item.EffectOnUseIntensity,
                EffectOnUseDuration = item.EffectOnUseDuration,
                ExtraItemsOnUse = item.ExtraItemsOnUse,
                HealOnSelect = item.HealOnSelect,
                ArtificialHealthOnSelect = item.ArtificialHealthOnSelect,
                EffectOnSelect = item.EffectOnSelect,
                EffectOnSelectIntensity = item.EffectOnSelectIntensity,
                EffectOnSelectDuration = item.EffectOnSelectDuration,
                AmmoOnSelect = item.AmmoOnSelect,
                AttachmentNames = item.AttachmentNames,
            });
        }

        private static Dictionary<string, bool> ToEnabledDictionary(IEnumerable<string> names)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            foreach (string name in names)
            {
                if (!string.IsNullOrWhiteSpace(name) && !result.ContainsKey(name))
                    result[name] = true;
            }

            return result;
        }

        private static List<string> ToDerivativeNames(IEnumerable<string> names)
        {
            return (names ?? new List<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(DerivativeNames.ToDerivativeName)
                .ToList();
        }

        private static string GuessCamp(string name)
        {
            if (name.StartsWith("SCP-")) return "SCP";
            if (name.StartsWith("D级人员")) return "D级人员";
            if (name.Contains("科研") || name == "医生" || name == "清洁工" || name.Contains("特科")) return name.Contains("特科") ? "特殊科研" : "科研人员";
            if (name.StartsWith("安保部门")) return "安保人员";
            if (name.StartsWith("战术应对一部")) return "战术应对一部";
            if (name.StartsWith("战术应对二部")) return "战术应对二部";
            if (name.StartsWith("MEG")) return "MEG专家组";
            if (name.StartsWith("九尾狐")) return "九尾狐小队";
            if (name.StartsWith("快速反应部队")) return "快速反应部队";
            if (name.StartsWith("精锐快反")) return "精锐快速反应部队";
            if (name.StartsWith("落锤特战A连")) return "落锤特战A连";
            if (name.StartsWith("落锤特战B连3队")) return "落锤特战B连3队";
            if (name.StartsWith("落锤特战B连")) return "落锤特战B连";
            if (name.StartsWith("律法左手")) return "律法左手调查小队";
            if (name.StartsWith("GOC")) return "全球超自然联盟 攻击小组";
            if (name.StartsWith("蛇之手")) return "蛇之手";
            if (name.StartsWith("混沌分裂者")) return "混沌分裂者";
            if (name.StartsWith("GRU")) return "格鲁乌P（后时代）侵入部队";
            if (name.StartsWith("UIU") || name.StartsWith("特异事故处")) return "特异事故处特工小组";
            if (name.StartsWith("深红王之子")) return "深红王之子";
            if (name.StartsWith("异界特遣队")) return "异界特遣队C组";
            if (name.Contains("间谍")) return "间谍人员";
            return "特殊人员";
        }

        private static RoleTypeId GuessBaseRole(string name)
        {
            if (name.Contains("049-2")) return RoleTypeId.Scp0492;
            if (name.Contains("049")) return RoleTypeId.Scp049;
            if (name.Contains("096")) return RoleTypeId.Scp096;
            if (name.Contains("106")) return RoleTypeId.Scp106;
            if (name.Contains("173")) return RoleTypeId.Scp173;
            if (name.Contains("939")) return RoleTypeId.Scp939;
            if (name.Contains("3114")) return RoleTypeId.Scp3114;
            if (name.StartsWith("SCP-")) return RoleTypeId.Tutorial;
            if (name.StartsWith("D级人")) return RoleTypeId.ClassD;
            if (name.Contains("科研") || name == "医生" || name == "清洁工" || name.Contains("特科")) return RoleTypeId.Scientist;
            if (name.StartsWith("安保部门")) return RoleTypeId.FacilityGuard;
            if (name.StartsWith("混沌分裂者")) return RoleTypeId.ChaosRifleman;
            if (name.StartsWith("蛇之手") || name.StartsWith("深红王之子") || name.StartsWith("异界特遣队") || name.StartsWith("GRU")) return RoleTypeId.Tutorial;
            if (name.Contains("指挥官") || name.Contains("主管") || name.Contains("队长")) return RoleTypeId.NtfCaptain;
            if (name.Contains("专家") || name.Contains("工程师") || name.Contains("医疗")) return RoleTypeId.NtfSpecialist;
            if (name.Contains("中士") || name.Contains("机枪手") || name.Contains("重装")) return RoleTypeId.NtfSergeant;
            if (name.StartsWith("观察者")) return RoleTypeId.Spectator;
            return RoleTypeId.NtfPrivate;
        }

        private static int GuessHealth(string name)
        {
            if (name == "混沌分裂者 指挥官") return 150;
            if (name == "混沌分裂者 恶魔") return 170;
            if (name.Contains("重装") || name.Contains("无畏") || name.Contains("深红铁骑")) return 150;
            if (name.Contains("指挥官") || name.Contains("主管")) return 125;
            if (name.StartsWith("SCP-")) return 500;
            return 100;
        }

        private static int GuessArtificialHealth(string name)
        {
            if (name.Contains("重装") || name.Contains("无畏")) return 50;
            if (name.StartsWith("SCP-")) return 200;
            return 0;
        }

        private static float GuessSpeed(string name)
        {
            if (name == "混沌分裂者 指挥官") return 203.28f;
            if (name == "混沌分裂者 恶魔") return 210.21f;
            return 0f;
        }

        private static int GuessBulletResistanceHead(string name)
        {
            if (name.StartsWith("混沌分裂者")) return 0;
            return 0;
        }

        private static int GuessBulletResistanceBody(string name)
        {
            if (name.StartsWith("混沌分裂者")) return 30;
            if (name.Contains("重装") || name.Contains("无畏")) return 25;
            return 0;
        }

        private static int GuessBulletResistanceArm(string name)
        {
            if (name.StartsWith("混沌分裂者")) return 20;
            if (name.Contains("重装") || name.Contains("无畏")) return 15;
            return 0;
        }

        private static int GuessBulletResistanceLeg(string name)
        {
            if (name.StartsWith("混沌分裂者")) return 20;
            if (name.Contains("重装") || name.Contains("无畏")) return 15;
            return 0;
        }

        private static string GuessBadgeColor(string name)
        {
            string camp = GuessCamp(name);
            if (camp.Contains("混沌")) return "green";
            if (camp.Contains("蛇之手")) return "lime";
            if (camp.Contains("GOC")) return "cyan";
            if (camp.Contains("SCP")) return "red";
            if (camp.Contains("D级")) return "orange";
            return "blue_green";
        }

        private static string GuessRoleColor(string name)
        {
            string camp = GuessCamp(name);
            if (camp.Contains("SCP")) return "#FF4040";
            if (camp.Contains("D级")) return "#FF8C00";
            if (camp.Contains("科研")) return "#FFD700";
            if (camp.Contains("混沌")) return "#32CD32";
            if (camp.Contains("蛇之手")) return "#7FFF00";
            if (camp.Contains("GOC")) return "#00FFFF";
            if (camp.Contains("深红")) return "#DC143C";
            return "#6699FF";
        }

        private static int GuessSpecialKillExperience(string name)
        {
            RoleTypeId baseRole = GuessBaseRole(name);
            if (name.StartsWith("SCP-")) return 90;
            if (baseRole == RoleTypeId.Tutorial) return 45;
            if (name.Contains("指挥官") || name.Contains("主管")) return 50;
            if (name.Contains("重装") || name.Contains("无畏")) return 45;
            return 30;
        }

        private static string GuessRoleDescription(string name)
        {
            if (name == "混沌分裂者 指挥官")
                return "混沌分裂者常规部队的领导者，持有未知权限卡，配备有SCAR-H自动步枪。";

            if (name == "混沌分裂者 恶魔")
                return "混沌分裂者的重火力支援，持有?权限卡，配备Saiga-12 Spike霰弹枪。";

            return $"{name}，隶属于{GuessCamp(name)}。";
        }

        private static float GuessSkillCooldown(string name, bool primary)
        {
            if (name == "混沌分裂者 指挥官" && primary)
                return 3f;

            return 5f;
        }

        private static bool GuessHasSkill(string name, bool primary)
        {
            return name == "混沌分裂者 指挥官" && primary;
        }

        private static List<string> GuessLoadout(string name)
        {
            if (name.StartsWith("SCP-") || name.StartsWith("观察者"))
                return new List<string>();

            if (name.StartsWith("D级人"))
                return new List<string> { "ID卡", "医疗用品" };

            if (name.Contains("科研") || name == "医生" || name == "清洁工" || name.Contains("特科"))
                return new List<string> { "科研权限卡", "医疗用品", "SCP-500 万能药" };

            if (name.StartsWith("混沌分裂者"))
                return new List<string> { "CI对讲机", "远程武器", "防护装备", "医疗用品" };

            if (name.StartsWith("蛇之手") || name.StartsWith("GRU") || name.StartsWith("深红王之子") || name.StartsWith("异界特遣队"))
                return new List<string> { "远程武器", "防护装备", "医疗用品" };

            return new List<string> { "安保权限卡", "远程武器", "防护装备", "医疗用品" };
        }

        private static List<TaskDefinition> DefaultTasks()
        {
            return new List<TaskDefinition>
            {
                new TaskDefinition { Name = "九尾狐收容", MatchCamp = "九尾狐小队", TaskType = "KillScps", TargetCount = 1, RewardExperience = 80, Description = "击杀 1 个 SCP。" },
                new TaskDefinition { Name = "快速反应", MatchCamp = "快速反应部队", TaskType = "KillPlayers", TargetCount = 2, RewardExperience = 60, Description = "击杀 2 名敌对目标。" },
                new TaskDefinition { Name = "精锐快反推进", MatchCamp = "精锐快速反应部队", TaskType = "KillPlayers", TargetCount = 2, RewardExperience = 65, Description = "击杀 2 名敌对目标。" },
                new TaskDefinition { Name = "战术二部推进", MatchCamp = "战术应对二部", TaskType = "KillPlayers", TargetCount = 2, RewardExperience = 65, Description = "击杀 2 名敌对目标。" },
                new TaskDefinition { Name = "落锤A连推进", MatchCamp = "落锤特战A连", TaskType = "KillPlayers", TargetCount = 2, RewardExperience = 65, Description = "击杀 2 名敌对目标。" },
                new TaskDefinition { Name = "落锤B连推进", MatchCamp = "落锤特战B连", TaskType = "KillPlayers", TargetCount = 2, RewardExperience = 65, Description = "击杀 2 名敌对目标。" },
                new TaskDefinition { Name = "落锤B连3队维修", MatchCamp = "落锤特战B连3队", TaskType = "KillPlayers", TargetCount = 1, RewardExperience = 45, Description = "击杀 1 名敌对目标。" },
                new TaskDefinition { Name = "律法左手抓捕", MatchCamp = "律法左手调查小队", TaskType = "KillPlayers", TargetCount = 1, RewardExperience = 45, Description = "击杀 1 名敌对目标。" },
            };
        }
    }
}
