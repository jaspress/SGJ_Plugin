using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.Modules
{
    public class ScpIdleRegenModule : ModuleBase
    {
        private readonly Config _config;
        private readonly Dictionary<string, IdleState> _states = new Dictionary<string, IdleState>();
        private CoroutineHandle _regenCoroutine;

        public override string Name => "SCP Idle Regen Module";

        public ScpIdleRegenModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            if (_config.ScpIdleRegenConfig == null || !_config.ScpIdleRegenConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            CustomPlayerEvents.Spawned += OnSpawned;
            _regenCoroutine = Timing.RunCoroutine(RegenLoop());
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Spawned -= OnSpawned;
            Timing.KillCoroutines(_regenCoroutine);
            _states.Clear();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            _states.Remove(GetPlayerKey(ev.Player));
            ApplyHealthOverride(ev.Player);
        }

        private IEnumerator<float> RegenLoop()
        {
            while (true)
            {
                float interval = Math.Max(0.25f, _config.ScpIdleRegenConfig.HealIntervalSeconds);
                yield return Timing.WaitForSeconds(interval);

                try
                {
                    Tick(interval);
                }
                catch (Exception ex)
                {
                    Log.Warn($"[{Name}] Regen tick failed: {ex}");
                }
            }
        }

        private void Tick(float interval)
        {
            HashSet<string> activeKeys = new HashSet<string>();
            foreach (Player player in Player.List)
            {
                if (!IsEligibleScp(player))
                    continue;

                string key = GetPlayerKey(player);
                activeKeys.Add(key);

                if (!_states.TryGetValue(key, out IdleState state))
                {
                    state = new IdleState
                    {
                        LastPosition = player.Position,
                        StillSeconds = 0f,
                    };
                    _states[key] = state;
                    continue;
                }

                float moved = Vector3.Distance(player.Position, state.LastPosition);
                if (moved <= Math.Max(0f, _config.ScpIdleRegenConfig.StillDistanceThreshold))
                    state.StillSeconds += interval;
                else
                    state.StillSeconds = 0f;

                state.LastPosition = player.Position;

                if (state.StillSeconds < Math.Max(0f, _config.ScpIdleRegenConfig.IdleSecondsRequired))
                    continue;

                Heal(player);
            }

            foreach (string key in _states.Keys.ToList())
            {
                if (!activeKeys.Contains(key))
                    _states.Remove(key);
            }
        }

        private void Heal(Player player)
        {
            float amount = GetConfiguredAmount(_config.ScpIdleRegenConfig.HealAmountsByRole, player.Role.Type, 0f);
            if (amount <= 0f || player.Health >= player.MaxHealth)
                return;

            float before = player.Health;
            player.Health = Math.Min(player.MaxHealth, player.Health + amount);
            float healed = player.Health - before;
            if (healed <= 0f || !_config.ScpIdleRegenConfig.ShowHealHint)
                return;

            string text = (_config.ScpIdleRegenConfig.HealHintText ?? string.Empty)
                .Replace("{role}", PluginHelper.GetChineseRoleName(player.Role.Type))
                .Replace("{amount}", healed.ToString("0.#"))
                .Replace("{health}", player.Health.ToString("0.#"))
                .Replace("{max_health}", player.MaxHealth.ToString("0.#"));

            PluginHelper.ShowTopRightHint(player, text, Math.Max(0.5f, _config.ScpIdleRegenConfig.HealHintDuration));
        }

        private void ApplyHealthOverride(Player player)
        {
            if (!IsEligibleScp(player))
                return;

            float health = GetConfiguredAmount(_config.ScpIdleRegenConfig.HealthOverridesByRole, player.Role.Type, -1f);
            if (health <= 0f)
                return;

            player.MaxHealth = health;
            player.Health = Math.Min(Math.Max(player.Health, health), health);
        }

        private static float GetConfiguredAmount(List<Config.ScpRoleAmount> values, RoleTypeId role, float fallback)
        {
            Config.ScpRoleAmount match = values?.FirstOrDefault(value =>
                value != null && string.Equals(value.Role, role.ToString(), StringComparison.OrdinalIgnoreCase));

            return match == null ? fallback : match.Amount;
        }

        private static bool IsEligibleScp(Player player)
        {
            return player != null
                && player.IsAlive
                && player.Role.Team == Team.SCPs
                && player.Role.Type != RoleTypeId.Scp079;
        }

        private static string GetPlayerKey(Player player)
        {
            if (player == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(player.RawUserId))
                return player.RawUserId;

            if (!string.IsNullOrWhiteSpace(player.UserId))
                return player.UserId;

            return player.Id.ToString();
        }

        private class IdleState
        {
            public Vector3 LastPosition { get; set; }
            public float StillSeconds { get; set; }
        }
    }
}
