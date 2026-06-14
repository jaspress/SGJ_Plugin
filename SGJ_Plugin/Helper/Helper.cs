using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using HintServiceMeow.Core.Enum;
using MEC;
using PlayerRoles;
using SGJ_Plugin.UI.Elements;
using SGJ_Plugin.UI.Managers;

namespace SGJ_Plugin.Helper
{
    public static class Helper
    {
        private const string TopRightHintElementId = "helper_top_right_hint";
        private const string CenterTopHintElementId = "helper_center_top_hint";
        private const string CenterInfoElementId = "helper_center_info";
        private const string TopStatusElementId = "helper_top_status";

        private static readonly List<TimedMessage> BroadcastQueue = new();
        private static List<string> _lastRendered = new();
        private static readonly object _lock = new();
        private static readonly Dictionary<string, List<TimedMessage>> TopRightHints = new Dictionary<string, List<TimedMessage>>();
        private static readonly Dictionary<string, List<TimedMessage>> CenterTopHints = new Dictionary<string, List<TimedMessage>>();
        private static readonly Dictionary<string, TimedMessage> CenterInfoHints = new Dictionary<string, TimedMessage>();
        private static readonly List<TimedMessage> BroadcastMessages = new List<TimedMessage>();
        private static UIManager _uiManager;
        private static Config _config;
        private static CoroutineHandle _refreshCoroutine;
        private static bool _refreshCoroutineStarted;

        public static void Initialize(Config config)
        {
            _config = config;
            _uiManager = UIManager.Instance;
            _uiManager.Initialize();
            StartRefreshCoroutine();
        }

        public static void Shutdown()
        {
            StopRefreshCoroutine();

            if (_uiManager != null)
            {
                foreach (Player player in Player.List)
                {
                    string panelId = GetTopRightPanelId(GetPlayerKey(player));
                    _uiManager.HidePanel(player, panelId);
                    _uiManager.RemovePanel(panelId);

                    string centerTopPanelId = GetCenterTopPanelId(GetPlayerKey(player));
                    _uiManager.HidePanel(player, centerTopPanelId);
                    _uiManager.RemovePanel(centerTopPanelId);

                    string centerInfoPanelId = GetCenterInfoPanelId(GetPlayerKey(player));
                    _uiManager.HidePanel(player, centerInfoPanelId);
                    _uiManager.RemovePanel(centerInfoPanelId);

                    string topStatusPanelId = GetTopStatusPanelId(GetPlayerKey(player));
                    _uiManager.HidePanel(player, topStatusPanelId);
                    _uiManager.RemovePanel(topStatusPanelId);
                }
            }

            TopRightHints.Clear();
            CenterTopHints.Clear();
            CenterInfoHints.Clear();
            BroadcastMessages.Clear();
            ClearBroadcasts();
            _uiManager = null;
            _config = null;
        }

        public static void ShowTopRightHint(Player player, string text, float duration)
        {
            if (player == null || string.IsNullOrWhiteSpace(text))
                return;

            EnsureUiManager();

            string key = GetPlayerKey(player);
            if (!TopRightHints.TryGetValue(key, out List<TimedMessage> messages))
            {
                messages = new List<TimedMessage>();
                TopRightHints[key] = messages;
            }

            System.DateTime now = System.DateTime.UtcNow;
            messages.RemoveAll(message => message.ExpireAt <= now);
            messages.Add(new TimedMessage
            {
                Text = text,
                ExpireAt = now.AddSeconds(System.Math.Max(0.5f, duration)),
            });
            TrimMessageCount(messages, _config?.MiscConfig?.TopRightHintMaxVisibleMessages ?? 4);

            RefreshTopRightHint(player);
        }

        public static void ShowCenterTopHint(Player player, string text, float duration)
        {
            if (player == null || string.IsNullOrWhiteSpace(text))
                return;

            EnsureUiManager();

            string key = GetPlayerKey(player);
            if (!CenterTopHints.TryGetValue(key, out List<TimedMessage> messages))
            {
                messages = new List<TimedMessage>();
                CenterTopHints[key] = messages;
            }

            System.DateTime now = System.DateTime.UtcNow;
            messages.RemoveAll(message => message.ExpireAt <= now);
            BroadcastMessages.RemoveAll(m => m.Text == text);
            messages.Add(new TimedMessage
            {
                Text = text,
                ExpireAt = now.AddSeconds(System.Math.Max(0.5f, duration)),
            });
            TrimMessageCount(messages, _config?.MiscConfig?.CenterTopHintMaxVisibleMessages ?? 3);

            RefreshCenterTopHint(player);
        }

        public static void ShowCenterInfoHint(Player player, string text, float duration)
        {
            if (player == null || string.IsNullOrWhiteSpace(text))
                return;

            EnsureUiManager();

            CenterInfoHints[GetPlayerKey(player)] = new TimedMessage
            {
                Text = text,
                ExpireAt = System.DateTime.UtcNow.AddSeconds(System.Math.Max(0.5f, duration)),
            };

            RefreshCenterInfoHint(player);
        }

        public static void ShowBroadcast(string id, string text, float duration)
        {
            var now = DateTime.UtcNow;

            lock (_lock)
            {
                BroadcastQueue.RemoveAll(m => m.Id == id);

                BroadcastQueue.Add(new TimedMessage
                {
                    Id = id,
                    Text = text,
                    NotBefore = now,
                    ExpireAt = now.AddSeconds(duration)
                });
            }
        }

        public static string FormatTemplate(string template, Player player, Config config)
        {
            return (template ?? string.Empty)
                .Replace("{ev.Player.Nickname}", player?.Nickname ?? string.Empty)
                .Replace("{player_name}", player?.Nickname ?? string.Empty)
                .Replace("{name}", player?.Nickname ?? string.Empty)
                .Replace("{Config.ShowServerName}", config?.ShowServerName ?? string.Empty)
                .Replace("{server_name}", config?.ShowServerName ?? string.Empty)
                .Replace("{server_tps}", FormatNumber(Server.Tps))
                .Replace("{server_max_tps}", Server.MaxTps.ToString())
                .Replace("{player_count}", Server.PlayerCount.ToString())
                .Replace("{max_player_count}", Server.MaxPlayerCount.ToString())
                .Replace("{plugin_version}", Main.Instance?.Version?.ToString() ?? "1.0.0");
        }

        public static string GetChineseRoleName(RoleTypeId role)
        {
            switch (role)
            {
                case RoleTypeId.ClassD:
                    return "D级人员";
                case RoleTypeId.Scientist:
                    return "科学家";
                case RoleTypeId.FacilityGuard:
                    return "设施保安";
                case RoleTypeId.NtfPrivate:
                    return "九尾狐列兵";
                case RoleTypeId.NtfSergeant:
                    return "九尾狐中士";
                case RoleTypeId.NtfCaptain:
                    return "九尾狐指挥官";
                case RoleTypeId.NtfSpecialist:
                    return "九尾狐收容专家";
                case RoleTypeId.ChaosConscript:
                    return "混沌分裂者征召兵";
                case RoleTypeId.ChaosRifleman:
                    return "混沌分裂者步枪手";
                case RoleTypeId.ChaosRepressor:
                    return "混沌分裂者压制者";
                case RoleTypeId.ChaosMarauder:
                    return "混沌分裂者掠夺者";
                case RoleTypeId.Flamingo:
                    return "火烈鸟";
                case RoleTypeId.ZombieFlamingo:
                    return "僵尸火烈鸟";
                case RoleTypeId.Scp3114:
                    return "SCP-3114";
                case RoleTypeId.Scp049:
                    return "SCP-049";
                case RoleTypeId.Scp0492:
                    return "SCP-049-2";
                case RoleTypeId.Scp079:
                    return "SCP-079";
                case RoleTypeId.Scp096:
                    return "SCP-096";
                case RoleTypeId.Scp106:
                    return "SCP-106";
                case RoleTypeId.Scp173:
                    return "SCP-173";
                case RoleTypeId.Scp939:
                    return "SCP-939";
                case RoleTypeId.Destroyed:
                    return "已销毁";
                case RoleTypeId.CustomRole:
                    return "自定义角色";
                case RoleTypeId.Tutorial:
                    return "教程角色";
                case RoleTypeId.Spectator:
                    return "观察者";
                case RoleTypeId.Overwatch:
                    return "监督者";
                case RoleTypeId.None:
                    return "无";
                default:
                    return role.ToString();
            }
        }

        public static string GetChineseItemName(ItemType item)
        {
            switch (item)
            {
                case ItemType.KeycardJanitor:
                    return "清洁工权限卡";
                case ItemType.KeycardScientist:
                    return "科学家权限卡";
                case ItemType.KeycardResearchCoordinator:
                    return "研究主管权限卡";
                case ItemType.KeycardZoneManager:
                    return "区域经理权限卡";
                case ItemType.KeycardGuard:
                    return "警卫权限卡";
                case ItemType.KeycardMTFPrivate:
                    return "MTF列兵权限卡";
                case ItemType.KeycardMTFOperative:
                    return "MTF权限卡";
                case ItemType.KeycardMTFCaptain:
                    return "MTF指挥官权限卡";
                case ItemType.KeycardFacilityManager:
                    return "设施主管权限卡";
                case ItemType.KeycardChaosInsurgency:
                    return "混沌分裂者权限卡";
                case ItemType.KeycardO5:
                    return "O5权限卡";
                case ItemType.Radio:
                    return "对讲机";
                case ItemType.GunCOM15:
                    return "COM-15手枪";
                case ItemType.GunCOM18:
                    return "COM-18手枪";
                case ItemType.GunRevolver:
                    return "左轮手枪";
                case ItemType.GunFSP9:
                    return "FSP-9冲锋枪";
                case ItemType.GunCrossvec:
                    return "Crossvec冲锋枪";
                case ItemType.GunE11SR:
                    return "E-11-SR步枪";
                case ItemType.GunAK:
                    return "AK步枪";
                case ItemType.GunShotgun:
                    return "霰弹枪";
                case ItemType.GunLogicer:
                    return "Logicer机枪";
                case ItemType.GrenadeHE:
                    return "高爆手雷";
                case ItemType.GrenadeFlash:
                    return "闪光弹";
                case ItemType.Medkit:
                    return "医疗包";
                case ItemType.Adrenaline:
                    return "肾上腺素";
                case ItemType.Painkillers:
                    return "止痛药";
                case ItemType.ArmorLight:
                    return "轻型护甲";
                case ItemType.ArmorCombat:
                    return "战斗护甲";
                case ItemType.ArmorHeavy:
                    return "重型护甲";
                case ItemType.Flashlight:
                    return "手电筒";
                case ItemType.Coin:
                    return "硬币";
                case ItemType.Jailbird:
                    return "囚鸟";
                case ItemType.SCP207:
                    return "SCP-207";
                case ItemType.SCP268:
                    return "SCP-268";
                case ItemType.SCP330:
                    return "SCP-330";
                case ItemType.SCP500:
                    return "SCP-500";
                case ItemType.None:
                    return "无";
                default:
                    return item.ToString();
            }
        }

        private static void StartRefreshCoroutine()
        {
            if (_refreshCoroutineStarted)
                return;

            _refreshCoroutineStarted = true;
            _refreshCoroutine = Timing.RunCoroutine(RefreshLoop());
        }

        private static void StopRefreshCoroutine()
        {
            if (!_refreshCoroutineStarted)
                return;

            Timing.KillCoroutines(_refreshCoroutine);
            _refreshCoroutineStarted = false;
        }

        private static IEnumerator<float> RefreshLoop()
        {
            while (_refreshCoroutineStarted)
            {
                yield return Timing.WaitForSeconds(GetUiRefreshInterval());

                foreach (Player player in Player.List)
                {
                    RefreshTopRightHint(player);
                    RefreshCenterTopHint(player);
                    RefreshCenterInfoHint(player);
                    RefreshTopStatus(player);
                }

                RefreshBroadcast();
            }
        }

        private static void RefreshTopRightHint(Player player)
        {
            if (player == null)
                return;

            EnsureUiManager();

            string key = GetPlayerKey(player);
            if (!TopRightHints.TryGetValue(key, out List<TimedMessage> messages))
                messages = new List<TimedMessage>();

            System.DateTime now = System.DateTime.UtcNow;
            messages.RemoveAll(message => message.ExpireAt <= now);
            TrimMessageCount(messages, _config?.MiscConfig?.TopRightHintMaxVisibleMessages ?? 4);

            if (messages.Count == 0)
            {
                if (TopRightHints.ContainsKey(key))
                    TopRightHints.Remove(key);

                _uiManager.HidePanel(player, GetTopRightPanelId(key));
                return;
            }

            string panelId = GetTopRightPanelId(key);
            UIPanel panel = _uiManager.CreatePanel(panelId, "Helper Top Right Hint");
            TextHintElement element = panel.GetElement(TopRightHintElementId) as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, TopRightHintElementId, string.Empty);
                element.Alignment = HintAlignment.Right;
            }

            element.Alignment = HintAlignment.Right;
            element.XCoordinate = Clamp(_config?.LevelSystemConfig?.ExperienceHintXCoordinate ?? 820f, -1100f, 1100f);
            element.YCoordinate = Clamp(_config?.LevelSystemConfig?.ExperienceHintYCoordinate ?? 120f, 0f, 1030f);
            element.FontSize = System.Math.Max(8, System.Math.Min(60, _config?.LevelSystemConfig?.ExperienceHintFontSize ?? 20));
            element.Content = FormatHintContent(BuildTimedText(messages, now, _config?.MiscConfig?.TopRightHintMessageSpacingLines ?? 1, _config?.MiscConfig?.TopRightHintMaxVisibleMessages ?? 4, _config?.MiscConfig?.TopRightHintMaxVisibleLines ?? 10), _config?.MiscConfig?.TopRightHintLineHeightPercent ?? 140);
            element.IsVisible = !string.IsNullOrWhiteSpace(element.Content);
            element.Update();
            _uiManager.ShowPanel(player, panelId);
        }

        private static void RefreshCenterTopHint(Player player)
        {
            if (player == null)
                return;

            EnsureUiManager();

            string key = GetPlayerKey(player);
            if (!CenterTopHints.TryGetValue(key, out List<TimedMessage> messages))
                messages = new List<TimedMessage>();

            System.DateTime now = System.DateTime.UtcNow;
            messages.RemoveAll(message => message.ExpireAt <= now);
            TrimMessageCount(messages, _config?.MiscConfig?.CenterTopHintMaxVisibleMessages ?? 3);

            if (messages.Count == 0)
            {
                if (CenterTopHints.ContainsKey(key))
                    CenterTopHints.Remove(key);

                _uiManager.HidePanel(player, GetCenterTopPanelId(key));
                return;
            }

            Config.MiscConfigClass misc = _config?.MiscConfig;
            string panelId = GetCenterTopPanelId(key);
            UIPanel panel = _uiManager.CreatePanel(panelId, "Helper Center Top Hint");
            TextHintElement element = panel.GetElement(CenterTopHintElementId) as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, CenterTopHintElementId, string.Empty);
                element.Alignment = HintAlignment.Center;
            }

            element.Alignment = HintAlignment.Center;
            element.XCoordinate = Clamp(misc?.CenterTopHintXCoordinate ?? 0f, -1100f, 1100f);
            element.YCoordinate = Clamp(misc?.CenterTopHintYCoordinate ?? 120f, 0f, 1030f);
            element.FontSize = System.Math.Max(8, System.Math.Min(60, misc?.CenterTopHintFontSize ?? 22));
            element.Content = FormatHintContent(BuildTimedText(messages, now, misc?.CenterTopHintMessageSpacingLines ?? 1, misc?.CenterTopHintMaxVisibleMessages ?? 3, misc?.CenterTopHintMaxVisibleLines ?? 8), misc?.CenterTopHintLineHeightPercent ?? 140);
            element.IsVisible = !string.IsNullOrWhiteSpace(element.Content);
            element.Update();
            _uiManager.ShowPanel(player, panelId);
        }

        private static void RefreshCenterInfoHint(Player player)
        {
            if (player == null)
                return;

            EnsureUiManager();

            string key = GetPlayerKey(player);
            string panelId = GetCenterInfoPanelId(key);
            if (!CenterInfoHints.TryGetValue(key, out TimedMessage message) || message.ExpireAt <= System.DateTime.UtcNow)
            {
                CenterInfoHints.Remove(key);
                _uiManager.HidePanel(player, panelId);
                return;
            }

            Config.MiscConfigClass misc = _config?.MiscConfig;
            UIPanel panel = _uiManager.CreatePanel(panelId, "Helper Center Info");
            TextHintElement element = panel.GetElement(CenterInfoElementId) as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, CenterInfoElementId, string.Empty);
                element.Alignment = HintAlignment.Center;
            }

            element.Alignment = HintAlignment.Center;
            element.XCoordinate = Clamp(misc?.CenterInfoXCoordinate ?? 0f, -1100f, 1100f);
            element.YCoordinate = Clamp(misc?.CenterInfoYCoordinate ?? 260f, 0f, 1030f);
            element.FontSize = System.Math.Max(8, System.Math.Min(60, misc?.CenterInfoFontSize ?? 21));
            element.Content = FormatHintContent(message.Text, misc?.CenterInfoLineHeightPercent ?? 160);
            element.IsVisible = !string.IsNullOrWhiteSpace(element.Content);
            element.Update();
            _uiManager.ShowPanel(player, panelId);
        }

        private static void RefreshTopStatus(Player player)
        {
            if (player == null)
                return;

            EnsureUiManager();

            string key = GetPlayerKey(player);
            string panelId = GetTopStatusPanelId(key);
            Config.MiscConfigClass misc = _config?.MiscConfig;
            if (misc == null || !misc.IsEnabled || !misc.TopStatusEnabled)
            {
                _uiManager.HidePanel(player, panelId);
                return;
            }

            UIPanel panel = _uiManager.CreatePanel(panelId, "Helper Top Status");
            TextHintElement element = panel.GetElement(TopStatusElementId) as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, TopStatusElementId, string.Empty);
                element.Alignment = HintAlignment.Center;
            }

            element.Alignment = HintAlignment.Center;
            element.XCoordinate = Clamp(misc.TopStatusXCoordinate, -1100f, 1100f);
            element.YCoordinate = Clamp(misc.TopStatusYCoordinate, 0f, 1030f);
            element.FontSize = System.Math.Max(8, System.Math.Min(60, misc.TopStatusFontSize));
            element.Content = FormatTemplate(misc.TopStatusText, player, _config);
            element.IsVisible = !string.IsNullOrWhiteSpace(element.Content);
            element.Update();
            _uiManager.ShowPanel(player, panelId);
        }

        private static void RefreshBroadcast()
        {
            var now = DateTime.UtcNow;

            List<string> snapshot;

            lock (_lock)
            {
                BroadcastQueue.RemoveAll(m => m.ExpireAt <= now);

                snapshot = BroadcastQueue
                    .Where(m => m.NotBefore <= now)
                    .OrderBy(m => m.ExpireAt)
                    .Select(m =>
                    {
                        int s = Math.Max(0,
                            (int)Math.Ceiling((m.ExpireAt - now).TotalSeconds));

                        return $"[{s}s] {m.Text}";
                    })
                    .ToList();
            }

            if (_lastRendered.SequenceEqual(snapshot))
                return;

            _lastRendered = snapshot;

            string content = string.Join("\n", snapshot);

            SendStableBroadcast(content);
        }

        private static void SendStableBroadcast(string content)
        {
            ushort duration = 1;

            foreach (var p in Player.List)
            {
                p.ClearBroadcasts();
                p.Broadcast(duration, content);
            }
        }

        private static void ClearBroadcasts(Player p) => p.ClearBroadcasts();
        private static void ClearBroadcasts()
        {
            try
            {
                foreach (var p in Player.List)
                {
                    p.ClearBroadcasts();
                }
            }
            catch (System.Exception ex)
            {
                if (_config?.Debug == true)
                    Log.Debug($"[Helper] Broadcast clear skipped: {ex.Message}");
            }
        }

        private static string BuildTimedText(List<TimedMessage> messages, System.DateTime now, int spacingLines, int maxMessages, int maxEstimatedLines)
        {
            if (messages == null || messages.Count == 0)
                return string.Empty;

            maxMessages = System.Math.Max(1, maxMessages);
            maxEstimatedLines = System.Math.Max(1, maxEstimatedLines);
            var visibleMessages = messages
                .OrderBy(m => m.ExpireAt)
                .Take(maxMessages)
                .ToList();
            List<string> lines = new List<string>();
            int usedLines = 0;
            foreach (TimedMessage message in visibleMessages)
            {
                int seconds = System.Math.Max(0, (int)System.Math.Ceiling((message.ExpireAt - now).TotalSeconds));
                string line = $"[{seconds}s] {message.Text}";
                int estimatedLines = EstimateLineCount(line);
                if (lines.Count > 0 && usedLines + estimatedLines > maxEstimatedLines)
                    break;

                lines.Add(line);
                usedLines += estimatedLines + System.Math.Max(0, spacingLines);
            }

            spacingLines = System.Math.Max(0, spacingLines);
            string separator = "\n" + new string('\n', spacingLines);
            return string.Join(separator, lines);
        }

        private static void TrimMessageCount(List<TimedMessage> messages, int maxMessages)
        {
            if (messages == null)
                return;

            maxMessages = System.Math.Max(1, maxMessages);
            if (messages.Count > maxMessages)
                messages.RemoveRange(0, messages.Count - maxMessages);
        }

        private static int EstimateLineCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 1;

            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            int explicitLines = normalized.Count(ch => ch == '\n') + 1;
            int wrappedLines = System.Math.Max(1, normalized.Length / 42);
            return System.Math.Max(explicitLines, wrappedLines);
        }

        private static string FormatHintContent(string text, int lineHeightPercent)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            int lineHeight = System.Math.Max(100, System.Math.Min(260, lineHeightPercent));

            if (normalized.IndexOf("<line-height=", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return normalized;

            return $"<line-height={lineHeight}%>{normalized}</line-height>";
        }

        private static void EnsureUiManager()
        {
            if (_uiManager != null)
                return;

            _uiManager = UIManager.Instance;
            _uiManager.Initialize();
        }

        private static string GetTopRightPanelId(string playerKey)
        {
            return "helper_top_right_" + SanitizeKey(playerKey);
        }

        private static string GetCenterTopPanelId(string playerKey)
        {
            return "helper_center_top_" + SanitizeKey(playerKey);
        }

        private static string GetCenterInfoPanelId(string playerKey)
        {
            return "helper_center_info_" + SanitizeKey(playerKey);
        }

        private static string GetTopStatusPanelId(string playerKey)
        {
            return "helper_top_status_" + SanitizeKey(playerKey);
        }

        private static string GetPlayerKey(Player player)
        {
            if (player == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(player.RawUserId))
                return player.RawUserId;

            if (!string.IsNullOrWhiteSpace(player.UserId))
                return player.UserId.Replace("@steam", string.Empty).Replace("@northwood", string.Empty);

            return player.Id.ToString();
        }

        private static string SanitizeKey(string key)
        {
            return (key ?? string.Empty).Replace("@", "_").Replace(".", "_").Replace(":", "_");
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private static float GetUiRefreshInterval()
        {
            float interval = _config?.MiscConfig?.UiRefreshIntervalSeconds ?? 0.5f;
            return Clamp(interval, 0.1f, 2f);
        }

        private static string FormatNumber(double value)
        {
            return System.Math.Round(value).ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        }

        private class TimedMessage
        {
            public string Id;
            public string Text;
            public DateTime ExpireAt;
            public DateTime NotBefore;
        }
    }
}
