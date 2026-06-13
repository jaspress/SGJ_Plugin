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

        private static readonly Dictionary<string, List<TimedMessage>> TopRightHints = new Dictionary<string, List<TimedMessage>>();
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
                }
            }

            TopRightHints.Clear();
            BroadcastMessages.Clear();
            Map.ClearBroadcasts();
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
            messages.Insert(0, new TimedMessage
            {
                Text = text,
                ExpireAt = now.AddSeconds(System.Math.Max(0.5f, duration)),
            });

            RefreshTopRightHint(player);
        }

        public static void ShowBroadcast(string text, float duration)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            System.DateTime now = System.DateTime.UtcNow;
            BroadcastMessages.RemoveAll(message => message.ExpireAt <= now);
            BroadcastMessages.Insert(0, new TimedMessage
            {
                Text = text,
                ExpireAt = now.AddSeconds(System.Math.Max(0.5f, duration)),
            });

            RefreshBroadcast();
        }

        public static string FormatTemplate(string template, Player player, Config config)
        {
            return (template ?? string.Empty)
                .Replace("{ev.Player.Nickname}", player?.Nickname ?? string.Empty)
                .Replace("{player_name}", player?.Nickname ?? string.Empty)
                .Replace("{name}", player?.Nickname ?? string.Empty)
                .Replace("{Config.ShowServerName}", config?.ShowServerName ?? string.Empty)
                .Replace("{server_name}", config?.ShowServerName ?? string.Empty);
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
                    return "MTF列兵";
                case RoleTypeId.NtfSergeant:
                    return "MTF中士";
                case RoleTypeId.NtfCaptain:
                    return "MTF指挥官";
                case RoleTypeId.NtfSpecialist:
                    return "MTF收容专家";
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
                yield return Timing.WaitForSeconds(1f);

                foreach (Player player in Player.List)
                    RefreshTopRightHint(player);

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
            element.Content = BuildTimedText(messages, now);
            element.IsVisible = !string.IsNullOrWhiteSpace(element.Content);
            element.Update();
            _uiManager.ShowPanel(player, panelId);
        }

        private static void RefreshBroadcast()
        {
            System.DateTime now = System.DateTime.UtcNow;
            BroadcastMessages.RemoveAll(message => message.ExpireAt <= now);

            string content = BuildTimedText(BroadcastMessages, now);
            if (string.IsNullOrWhiteSpace(content))
            {
                Map.ClearBroadcasts();
                return;
            }

            int remainingSeconds = System.Math.Max(1, (int)System.Math.Ceiling(BroadcastMessages.Max(message => (message.ExpireAt - now).TotalSeconds)));
            Map.Broadcast((ushort)remainingSeconds, content, Broadcast.BroadcastFlags.Normal, true);
        }

        private static string BuildTimedText(List<TimedMessage> messages, System.DateTime now)
        {
            if (messages == null || messages.Count == 0)
                return string.Empty;

            List<string> lines = new List<string>();
            foreach (TimedMessage message in messages)
            {
                int seconds = System.Math.Max(0, (int)System.Math.Ceiling((message.ExpireAt - now).TotalSeconds));
                lines.Add($"[{seconds}s] {message.Text}");
            }

            return string.Join("\n", lines);
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

        private class TimedMessage
        {
            public string Text { get; set; } = string.Empty;
            public System.DateTime ExpireAt { get; set; }
        }
    }
}
