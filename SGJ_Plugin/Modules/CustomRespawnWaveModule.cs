using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomServerEvents = Exiled.Events.Handlers.Server;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.Modules
{
    public class CustomRespawnWaveModule : ModuleBase
    {
        private readonly Config _config;
        private readonly Random _random = new Random();
        private readonly Dictionary<string, Config.RespawnWaveDefinition> _previewWaves = new Dictionary<string, Config.RespawnWaveDefinition>();
        private PendingWave _pendingWave;

        public static CustomRespawnWaveModule Instance { get; private set; }

        public override string Name => "Custom Respawn Wave Module";

        public CustomRespawnWaveModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            if (_config.CustomRespawnConfig == null || !_config.CustomRespawnConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            Instance = this;
            CustomServerEvents.RespawningTeam += OnRespawningTeam;
            CustomServerEvents.RespawnedTeam += OnRespawnedTeam;
            Log.Info($"[{Name}] Enabled.");
        }

        protected override void OnDisable()
        {
            CustomServerEvents.RespawningTeam -= OnRespawningTeam;
            CustomServerEvents.RespawnedTeam -= OnRespawnedTeam;
            _pendingWave = null;
            _previewWaves.Clear();
            if (Instance == this)
                Instance = null;

            Log.Info($"[{Name}] Disabled.");
        }

        public bool TryGetPreviewForSpawnableFaction(SpawnableFaction faction, out string waveName, out string campName, out string color)
        {
            Config.RespawnWaveDefinition wave = GetOrCreatePreview(GetBaseWave(faction));
            waveName = wave?.Name;
            campName = wave?.Camp;
            color = wave?.Color;
            return wave != null;
        }

        private void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (ev == null || !ev.IsAllowed)
                return;

            string baseWave = GetBaseWave(ev.NextKnownTeam);
            Config.RespawnWaveDefinition wave = GetOrCreatePreview(baseWave);
            if (wave == null)
                return;

            _previewWaves.Remove(baseWave);

            int playerCount = Math.Max(0, ev.Players?.Count ?? 0);
            if (wave.MaxRespawnAmount > 0)
                ev.MaximumRespawnAmount = Math.Min(ev.MaximumRespawnAmount, wave.MaxRespawnAmount);

            int queueCount = wave.MaxRespawnAmount > 0
                ? Math.Min(playerCount, wave.MaxRespawnAmount)
                : playerCount;

            List<RoleTypeId> baseRoles = BuildRoleQueue(wave, Math.Max(1, queueCount));
            ReplaceSpawnQueue(ev, baseRoles);

            _pendingWave = new PendingWave
            {
                Wave = wave,
                BaseRoles = baseRoles,
                RoleNames = BuildSpecialRoleQueue(wave, Math.Max(1, queueCount)),
                CreatedAt = DateTime.UtcNow,
            };

            Log.Debug($"[{Name}] Selected custom wave {wave.Name} ({wave.Camp}) for {baseWave}.");
        }

        private void OnRespawnedTeam(RespawnedTeamEventArgs ev)
        {
            if (ev?.Players == null || _pendingWave == null)
                return;

            PendingWave pending = _pendingWave;
            _pendingWave = null;

            List<Player> players = ev.Players.Where(player => player != null).ToList();
            if (players.Count == 0)
                return;

            Timing.CallDelayed(Math.Max(0f, _config.CustomRespawnConfig.ApplySpecialRoleDelaySeconds), () =>
            {
                for (int i = 0; i < players.Count; i++)
                {
                    Player player = players[i];
                    if (player == null)
                        continue;

                    RoleTypeId baseRole = pending.BaseRoles.Count == 0
                        ? player.Role.Type
                        : pending.BaseRoles[i % pending.BaseRoles.Count];

                    if (baseRole != RoleTypeId.None && player.Role.Type != baseRole)
                        player.Role.Set(baseRole, SpawnReason.Respawn);

                    string roleName = pending.RoleNames.Count == 0
                        ? string.Empty
                        : pending.RoleNames[i % pending.RoleNames.Count];

                    if (!string.IsNullOrWhiteSpace(roleName))
                        SpecialContentModule.Instance?.TrySetAssignedRole(player, roleName, out _);

                    if (_config.CustomRespawnConfig.ShowWaveHint)
                    {
                        string text = (_config.CustomRespawnConfig.WaveHintText ?? string.Empty)
                            .Replace("{wave_name}", pending.Wave.Name ?? string.Empty)
                            .Replace("{camp_name}", pending.Wave.Camp ?? string.Empty)
                            .Replace("{color}", pending.Wave.Color ?? "#FFFFFF");

                        PluginHelper.ShowCenterTopHint(player, text, 5f);
                    }
                }
            });
        }

        private Config.RespawnWaveDefinition PickWave(string baseWave)
        {
            List<Config.RespawnWaveDefinition> waves = (_config.CustomRespawnConfig.Waves ?? new List<Config.RespawnWaveDefinition>())
                .Where(wave => wave != null && wave.IsEnabled)
                .Where(wave => MatchesBaseWave(wave.BaseWave, baseWave))
                .ToList();

            if (waves.Count == 0)
                return null;

            int totalWeight = waves.Sum(wave => Math.Max(1, wave.Weight));
            int roll = _random.Next(totalWeight);
            foreach (Config.RespawnWaveDefinition wave in waves)
            {
                roll -= Math.Max(1, wave.Weight);
                if (roll < 0)
                    return wave;
            }

            return waves[waves.Count - 1];
        }

        private void ReplaceSpawnQueue(RespawningTeamEventArgs ev, List<RoleTypeId> roles)
        {
            if (ev == null || roles == null || roles.Count == 0)
                return;

            PropertyInfo property = ev.GetType().GetProperty("SpawnQueue", BindingFlags.Instance | BindingFlags.Public);
            object queue = property?.GetValue(ev, null);
            if (queue == null)
            {
                Log.Warn($"[{Name}] SpawnQueue is unavailable.");
                return;
            }

            MethodInfo clear = queue.GetType().GetMethod("Clear", Type.EmptyTypes);
            MethodInfo enqueue = queue.GetType().GetMethod("Enqueue", new[] { typeof(RoleTypeId) });
            if (clear == null || enqueue == null)
            {
                Log.Warn($"[{Name}] SpawnQueue does not expose Clear/Enqueue.");
                return;
            }

            clear.Invoke(queue, null);
            foreach (RoleTypeId role in roles)
                enqueue.Invoke(queue, new object[] { role });
        }

        private Config.RespawnWaveDefinition GetOrCreatePreview(string baseWave)
        {
            if (string.IsNullOrWhiteSpace(baseWave))
                baseWave = "Ntf";

            if (_previewWaves.TryGetValue(baseWave, out Config.RespawnWaveDefinition wave) && wave != null)
                return wave;

            wave = PickWave(baseWave);
            if (wave != null)
                _previewWaves[baseWave] = wave;

            return wave;
        }

        private static bool MatchesBaseWave(string configured, string actual)
        {
            if (string.IsNullOrWhiteSpace(configured) || configured.Equals("Any", StringComparison.OrdinalIgnoreCase))
                return true;

            return configured.Equals(actual, StringComparison.OrdinalIgnoreCase);
        }

        private static List<RoleTypeId> BuildRoleQueue(Config.RespawnWaveDefinition wave, int count)
        {
            List<RoleTypeId> source = new List<RoleTypeId>();
            foreach (string roleName in wave.RoleQueue ?? new List<string>())
            {
                if (Enum.TryParse(roleName, true, out RoleTypeId role) && role != RoleTypeId.None)
                    source.Add(role);
            }

            if (source.Count == 0)
                source.Add(wave.BaseWave != null && wave.BaseWave.Equals("Chaos", StringComparison.OrdinalIgnoreCase) ? RoleTypeId.ChaosConscript : RoleTypeId.NtfPrivate);

            List<RoleTypeId> result = new List<RoleTypeId>();
            for (int i = 0; i < count; i++)
                result.Add(source[i % source.Count]);

            return result;
        }

        private static List<string> BuildSpecialRoleQueue(Config.RespawnWaveDefinition wave, int count)
        {
            List<string> source = (wave.RoleNames ?? new List<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (source.Count == 0)
                return source;

            List<string> result = new List<string>();
            for (int i = 0; i < count; i++)
                result.Add(source[i % source.Count]);

            return result;
        }

        private static string GetBaseWave(Faction faction)
        {
            string name = faction.ToString();
            if (name.IndexOf("Chaos", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Chaos";

            return "Ntf";
        }

        private static string GetBaseWave(SpawnableFaction faction)
        {
            string name = faction.ToString();
            if (name.IndexOf("Chaos", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Chaos";

            return "Ntf";
        }

        private class PendingWave
        {
            public Config.RespawnWaveDefinition Wave { get; set; }
            public List<RoleTypeId> BaseRoles { get; set; } = new List<RoleTypeId>();
            public List<string> RoleNames { get; set; } = new List<string>();
            public DateTime CreatedAt { get; set; }
        }
    }
}
