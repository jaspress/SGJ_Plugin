using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using HintServiceMeow.Core.Enum;
using MEC;
using SGJ_Plugin.SpecialContent.Base;
using SGJ_Plugin.UI.Elements;
using SGJ_Plugin.UI.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class SkillModule : ModuleBase
    {
        private const string SkillHudElementId = "skill_hud";

        private readonly Config _config;
        private readonly Dictionary<string, DateTime> _cooldowns = new Dictionary<string, DateTime>();
        private readonly List<SettingBase> _settings = new List<SettingBase>();
        private UIManager _uiManager;
        private CoroutineHandle _refreshCoroutine;
        private bool _refreshCoroutineStarted;

        public override string Name => "Skill Module";

        public SkillModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            if (!_config.SkillSystemConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            RegisterSettings();
            _uiManager = UIManager.Instance;
            _uiManager.Initialize();
            StartRefreshCoroutine();
            CustomPlayerEvents.Verified += OnVerified;
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Verified -= OnVerified;
            StopRefreshCoroutine();

            if (_settings.Count > 0)
                SettingBase.Unregister(player => true, _settings);

            foreach (Player player in Player.List)
                HideSkillHud(player);

            _settings.Clear();
            _cooldowns.Clear();
        }

        private void OnVerified(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
            if (ev?.Player == null || _settings.Count == 0)
                return;

            Timing.CallDelayed(1f, () => SettingBase.SendToPlayer(ev.Player, _settings));
        }

        private void RegisterSettings()
        {
            Config.SkillSystemConfigClass config = _config.SkillSystemConfig;
            HeaderSetting header = new HeaderSetting(config.HeaderSettingId, config.SettingsHeader, "设置特殊角色技能快捷键", false);

            KeybindSetting primary = new KeybindSetting(
                config.PrimarySkillSettingId,
                config.PrimarySkillLabel,
                ParseKey(config.PrimarySkillKey, KeyCode.F),
                config.PreventInteractionOnGui,
                config.AllowSpectatorTrigger,
                "按下后触发当前特殊角色主技能",
                config.CollectionId,
                header,
                (player, setting) => OnSkillKey(player, setting, true));

            KeybindSetting secondary = new KeybindSetting(
                config.SecondarySkillSettingId,
                config.SecondarySkillLabel,
                ParseKey(config.SecondarySkillKey, KeyCode.G),
                config.PreventInteractionOnGui,
                config.AllowSpectatorTrigger,
                "按下后触发当前特殊角色副技能",
                config.CollectionId,
                header,
                (player, setting) => OnSkillKey(player, setting, false));

            _settings.Clear();
            _settings.Add(header);
            _settings.Add(primary);
            _settings.Add(secondary);
            SettingBase.Register(_settings, player => true);
            SettingBase.SendToAll();
        }

        private void OnSkillKey(Player player, SettingBase setting, bool primary)
        {
            if (player == null || setting is not KeybindSetting keybind || !keybind.IsPressed)
                return;

            CustomRoleBase role = SpecialContentModule.Instance?.GetAssignedRole(player);
            if (role == null)
                return;

            if (!IsSkillEnabled(role, primary))
                return;

            if (IsOnCooldown(player, primary))
                return;

            bool handled = primary ? role.UsePrimarySkill(player) : role.UseSecondarySkill(player);
            if (handled)
                SetCooldown(player, role, primary);
        }

        private bool IsOnCooldown(Player player, bool primary)
        {
            string key = GetCooldownKey(player, primary);
            DateTime now = DateTime.UtcNow;
            return _cooldowns.TryGetValue(key, out DateTime readyAt) && readyAt > now;
        }

        private void SetCooldown(Player player, CustomRoleBase role, bool primary)
        {
            float cooldownSeconds = primary ? role?.PrimarySkillCooldownSeconds ?? 0f : role?.SecondarySkillCooldownSeconds ?? 0f;
            if (cooldownSeconds <= 0f)
                cooldownSeconds = _config.SkillSystemConfig.SkillCooldownSeconds;

            _cooldowns[GetCooldownKey(player, primary)] = DateTime.UtcNow.AddSeconds(Math.Max(0f, cooldownSeconds));
        }

        private void StartRefreshCoroutine()
        {
            if (_refreshCoroutineStarted)
                return;

            _refreshCoroutineStarted = true;
            _refreshCoroutine = Timing.RunCoroutine(RefreshLoop());
        }

        private void StopRefreshCoroutine()
        {
            if (!_refreshCoroutineStarted)
                return;

            Timing.KillCoroutines(_refreshCoroutine);
            _refreshCoroutineStarted = false;
        }

        private IEnumerator<float> RefreshLoop()
        {
            while (_refreshCoroutineStarted)
            {
                yield return Timing.WaitForSeconds(0.5f);

                foreach (Player player in Player.List)
                    RefreshSkillHud(player);
            }
        }

        private void RefreshSkillHud(Player player)
        {
            if (player == null || !_config.SkillSystemConfig.ShowSkillHud)
                return;

            CustomRoleBase role = SpecialContentModule.Instance?.GetAssignedRole(player);
            if (role == null || (!role.PrimarySkillEnabled && !role.SecondarySkillEnabled))
            {
                HideSkillHud(player);
                return;
            }

            string content = RenderSkillHud(player, role);
            if (string.IsNullOrWhiteSpace(content))
            {
                HideSkillHud(player);
                return;
            }

            string panelId = GetSkillHudPanelId(player);
            UIPanel panel = _uiManager.CreatePanel(panelId, "Skill Hud");
            TextHintElement element = panel.GetElement(SkillHudElementId) as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, SkillHudElementId, string.Empty);
                element.Alignment = HintAlignment.Right;
            }

            element.Alignment = HintAlignment.Right;
            element.XCoordinate = Clamp(_config.SkillSystemConfig.SkillHudXCoordinate, -1100f, 1100f);
            element.YCoordinate = Clamp(_config.SkillSystemConfig.SkillHudYCoordinate, 0f, 1030f);
            element.FontSize = Math.Max(8, Math.Min(60, _config.SkillSystemConfig.SkillHudFontSize));
            element.Content = content;
            element.IsVisible = true;
            element.Update();
            _uiManager.ShowPanel(player, panelId);
        }

        private string RenderSkillHud(Player player, CustomRoleBase role)
        {
            string primaryName = role.PrimarySkillEnabled ? role.PrimarySkillName : string.Empty;
            string secondaryName = role.SecondarySkillEnabled ? role.SecondarySkillName : string.Empty;
            string text = _config.SkillSystemConfig.SkillHudText ?? string.Empty;

            text = text
                .Replace("{primary_skill}", primaryName)
                .Replace("{primary_status}", role.PrimarySkillEnabled ? GetSkillStatus(player, true) : string.Empty)
                .Replace("{secondary_skill}", secondaryName)
                .Replace("{secondary_status}", role.SecondarySkillEnabled ? GetSkillStatus(player, false) : string.Empty);

            return RemoveEmptySkillLines(text);
        }

        private string GetSkillStatus(Player player, bool primary)
        {
            if (_cooldowns.TryGetValue(GetCooldownKey(player, primary), out DateTime readyAt) && readyAt > DateTime.UtcNow)
            {
                int seconds = Math.Max(1, (int)Math.Ceiling((readyAt - DateTime.UtcNow).TotalSeconds));
                return $"CD {seconds}秒";
            }

            return "已就绪";
        }

        private static bool IsSkillEnabled(CustomRoleBase role, bool primary)
        {
            return primary ? role?.PrimarySkillEnabled == true : role?.SecondarySkillEnabled == true;
        }

        private void HideSkillHud(Player player)
        {
            if (player == null || _uiManager == null)
                return;

            _uiManager.HidePanel(player, GetSkillHudPanelId(player));
        }

        private static string RemoveEmptySkillLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            List<string> lines = new List<string>();
            foreach (string line in value.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
            {
                if (line.Contains("{}") || line.Contains("[]"))
                    continue;

                string stripped = line.Replace("<color=#7FFFD4></color>", string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(stripped))
                    lines.Add(line);
            }

            return string.Join("\n", lines);
        }

        private static string GetSkillHudPanelId(Player player)
        {
            return "skill_hud_" + SanitizeKey(!string.IsNullOrWhiteSpace(player.RawUserId) ? player.RawUserId : player.UserId ?? player.Id.ToString());
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

        private static string GetCooldownKey(Player player, bool primary)
        {
            string id = !string.IsNullOrWhiteSpace(player.RawUserId)
                ? player.RawUserId
                : !string.IsNullOrWhiteSpace(player.UserId) ? player.UserId : player.Id.ToString();

            return id + ":" + (primary ? "primary" : "secondary");
        }

        private static KeyCode ParseKey(string value, KeyCode fallback)
        {
            return Enum.TryParse(value, true, out KeyCode keyCode) ? keyCode : fallback;
        }
    }
}
