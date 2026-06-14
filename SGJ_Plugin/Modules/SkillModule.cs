using Exiled.API.Features;
using Exiled.API.Features.Core.UserSettings;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class SkillModule : ModuleBase
    {
        private readonly Config _config;
        private readonly Dictionary<string, DateTime> _cooldowns = new Dictionary<string, DateTime>();
        private readonly List<SettingBase> _settings = new List<SettingBase>();

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
            CustomPlayerEvents.Verified += OnVerified;
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Verified -= OnVerified;

            if (_settings.Count > 0)
                SettingBase.Unregister(player => true, _settings);

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

            Config.SpecialRoleDefinition role = SpecialContentModule.Instance?.GetAssignedRole(player);
            if (role == null)
            {
                Helper.Helper.ShowTopRightHint(player, _config.SkillSystemConfig.NoSkillText, 3f);
                return;
            }

            if (!TryConsumeCooldown(player, role, primary))
                return;

            bool handled = primary ? role.UsePrimarySkill(player) : role.UseSecondarySkill(player);
            if (!handled)
                Helper.Helper.ShowTopRightHint(player, _config.SkillSystemConfig.NoSkillText, 3f);
        }

        private bool TryConsumeCooldown(Player player, Config.SpecialRoleDefinition role, bool primary)
        {
            string key = GetCooldownKey(player, primary);
            DateTime now = DateTime.UtcNow;
            if (_cooldowns.TryGetValue(key, out DateTime readyAt) && readyAt > now)
            {
                int seconds = Math.Max(1, (int)Math.Ceiling((readyAt - now).TotalSeconds));
                string text = (_config.SkillSystemConfig.CooldownText ?? string.Empty).Replace("{seconds}", seconds.ToString());
                Helper.Helper.ShowTopRightHint(player, text, 2f);
                return false;
            }

            float cooldownSeconds = primary ? role?.PrimarySkillCooldownSeconds ?? 0f : role?.SecondarySkillCooldownSeconds ?? 0f;
            if (cooldownSeconds <= 0f)
                cooldownSeconds = _config.SkillSystemConfig.SkillCooldownSeconds;

            _cooldowns[key] = now.AddSeconds(Math.Max(0f, cooldownSeconds));
            return true;
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
