using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using HintServiceMeow.Core.Enum;
using MEC;
using PlayerRoles;
using SGJ_Plugin.UI.Elements;
using SGJ_Plugin.UI.Managers;
using System;
using System.Collections.Generic;
using ApiRespawn = Exiled.API.Features.Respawn;
using ApiTimedWave = Exiled.API.Features.Waves.TimedWave;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class SpectatorHudModule : ModuleBase
    {
        private const string ElementId = "spectator_hud";

        private readonly Config _config;
        private readonly Dictionary<string, string> _playerPanels = new Dictionary<string, string>();
        private UIManager _uiManager;
        private CoroutineHandle _refreshCoroutine;
        private bool _refreshCoroutineStarted;

        public override string Name => "Spectator HUD Module";

        public SpectatorHudModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            if (!_config.SpectatorHudConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            _uiManager = UIManager.Instance;
            if (!_uiManager.Initialize())
                throw new InvalidOperationException("UIManager failed to initialize.");

            CustomPlayerEvents.Verified += OnVerified;
            CustomPlayerEvents.Left += OnLeft;

            foreach (Player player in Player.List)
                CreateOrRefreshHud(player);

            StartRefreshCoroutine();
            Log.Info($"[{Name}] Enabled.");
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Verified -= OnVerified;
            CustomPlayerEvents.Left -= OnLeft;
            StopRefreshCoroutine();

            if (_uiManager != null)
            {
                foreach (Player player in Player.List)
                    RemoveHud(player);
            }

            _playerPanels.Clear();
            Log.Info($"[{Name}] Disabled.");
        }

        private void OnVerified(Exiled.Events.EventArgs.Player.VerifiedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            CreateOrRefreshHud(ev.Player);
        }

        private void OnLeft(Exiled.Events.EventArgs.Player.LeftEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            ForgetHud(ev.Player);
        }

        private void CreateOrRefreshHud(Player player)
        {
            if (player == null || !_config.SpectatorHudConfig.IsEnabled || _uiManager == null)
                return;

            string key = GetPlayerKey(player);
            string panelId = GetPanelId(key);
            _playerPanels[key] = panelId;

            UIPanel panel = _uiManager.CreatePanel(panelId, "Spectator HUD");
            TextHintElement element = panel.GetElement(ElementId) as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, ElementId, string.Empty);
                element.Alignment = HintAlignment.Right;
            }

            ConfigureElement(element);
            RefreshHud(player);
        }

        private void ConfigureElement(TextHintElement element)
        {
            if (element == null)
                return;

            element.Alignment = HintAlignment.Right;
            element.XCoordinate = Clamp(_config.SpectatorHudConfig.HudXCoordinate, -1100f, 1100f);
            element.YCoordinate = Clamp(_config.SpectatorHudConfig.HudYCoordinate, 0f, 1030f);
            element.FontSize = Math.Max(8, Math.Min(60, _config.SpectatorHudConfig.HudFontSize));
        }

        private void RefreshHud(Player player)
        {
            if (player == null || !_config.SpectatorHudConfig.IsEnabled || _uiManager == null)
                return;

            string key = GetPlayerKey(player);
            if (!_playerPanels.TryGetValue(key, out string panelId))
            {
                CreateOrRefreshHud(player);
                return;
            }

            if (player.Role.Type != RoleTypeId.Spectator)
            {
                _uiManager.HidePanel(player, panelId);
                return;
            }

            TextHintElement element = _uiManager.GetElement(panelId, ElementId) as TextHintElement;
            if (element == null)
            {
                CreateOrRefreshHud(player);
                return;
            }

            element.Content = TrimLines(BuildHudText(player), Math.Max(1, _config.SpectatorHudConfig.MaxVisibleLines));
            element.Update();
            _uiManager.ShowPanel(player, panelId);
        }

        private string BuildHudText(Player spectator)
        {
            RespawnWaveInfo respawnInfo = GetRespawnWaveInfo();

            string template = string.IsNullOrWhiteSpace(_config.SpectatorHudConfig.HudText)
                ? _config.SpectatorHudConfig.RespawnInfoText
                : _config.SpectatorHudConfig.HudText;

            string text = template
                .Replace("{observed_level_hud}", string.Empty)
                .Replace("{own_level_hud}", string.Empty)
                .Replace("{respawn_info}", ReplaceRespawnPlaceholders(_config.SpectatorHudConfig.RespawnInfoText, respawnInfo));

            return EnsureRespawnInfo(ReplaceRespawnPlaceholders(text, respawnInfo), respawnInfo);
        }

        private static Player GetObservedPlayer(Player spectator)
        {
            if (spectator == null || spectator.Role.Type != RoleTypeId.Spectator)
                return null;

            try
            {
                SpectatorRole spectatorRole = spectator.Role.As<SpectatorRole>();
                return spectatorRole?.SpectatedPlayer;
            }
            catch
            {
                return null;
            }
        }

        private static string ReplaceRespawnPlaceholders(string text, RespawnWaveInfo info)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("{respawn_wave}", info.WaveName)
                .Replace("{respawn_time}", info.TimeLeft)
                .Replace("{respawn_tickets}", info.Tickets)
                .Replace("{respawn_team}", info.TeamName)
                .Replace("{team_color}", info.TeamColor);
        }

        private string EnsureRespawnInfo(string text, RespawnWaveInfo info)
        {
            string respawnInfo = ReplaceRespawnPlaceholders(_config.SpectatorHudConfig.RespawnInfoText, info);
            if (string.IsNullOrWhiteSpace(respawnInfo))
                return text ?? string.Empty;

            string rendered = text ?? string.Empty;
            if (rendered.IndexOf("倒计时", StringComparison.OrdinalIgnoreCase) >= 0
                || rendered.IndexOf(info.TimeLeft, StringComparison.OrdinalIgnoreCase) >= 0)
                return rendered;

            return string.IsNullOrWhiteSpace(rendered) ? respawnInfo : rendered + "\n" + respawnInfo;
        }

        private RespawnWaveInfo GetRespawnWaveInfo()
        {
            TimeSpan timeLeft = TimeSpan.Zero;
            SpawnableFaction nextTeam = SpawnableFaction.None;

            try
            {
                nextTeam = ApiRespawn.NextKnownSpawnableFaction;
            }
            catch
            {
                nextTeam = SpawnableFaction.None;
            }

            if (TryGetSoonestTimedWave(out ApiTimedWave timedWave))
            {
                timeLeft = timedWave.Timer.TimeLeft;
                if (nextTeam == SpawnableFaction.None)
                    nextTeam = timedWave.SpawnableFaction;
            }

            int ntfTickets = GetRespawnTickets(true);
            int chaosTickets = GetRespawnTickets(false);
            bool isNtf = IsNtfWave(nextTeam);
            bool isChaos = IsChaosWave(nextTeam);

            return new RespawnWaveInfo
            {
                TeamName = GetRespawnTeamName(nextTeam),
                TeamColor = GetRespawnTeamColor(nextTeam),
                WaveName = GetRespawnWaveName(nextTeam),
                TimeLeft = FormatTimeLeft(timeLeft),
                Tickets = isNtf ? ntfTickets.ToString() : isChaos ? chaosTickets.ToString() : $"MTF:{ntfTickets} Chaos:{chaosTickets}",
            };
        }

        private static int GetRespawnTickets(bool ntf)
        {
            try
            {
                int tickets = 0;
                ApiRespawn.TryGetTokens(ntf ? SpawnableFaction.NtfWave : SpawnableFaction.ChaosWave, out int waveTickets);
                ApiRespawn.TryGetTokens(ntf ? SpawnableFaction.NtfMiniWave : SpawnableFaction.ChaosMiniWave, out int miniWaveTickets);
                tickets += waveTickets;
                tickets += miniWaveTickets;
                return tickets;
            }
            catch
            {
                return 0;
            }
        }

        private static bool TryGetSoonestTimedWave(out ApiTimedWave timedWave)
        {
            timedWave = null;

            try
            {
                List<ApiTimedWave> waves = ApiTimedWave.GetTimedWaves();
                if (waves == null || waves.Count == 0)
                    return false;

                foreach (ApiTimedWave wave in waves)
                {
                    if (wave == null)
                        continue;

                    if (timedWave == null || wave.Timer.TimeLeft < timedWave.Timer.TimeLeft)
                        timedWave = wave;
                }

                return timedWave != null;
            }
            catch
            {
                timedWave = null;
                return false;
            }
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
                yield return Timing.WaitForSeconds(1f);
                foreach (Player player in Player.List)
                    RefreshHud(player);
            }
        }

        private void RemoveHud(Player player)
        {
            if (player == null || _uiManager == null)
                return;

            string key = GetPlayerKey(player);
            if (_playerPanels.TryGetValue(key, out string panelId))
            {
                _uiManager.HidePanel(player, panelId);
                _uiManager.RemovePanel(panelId);
                _playerPanels.Remove(key);
            }
        }

        private void ForgetHud(Player player)
        {
            if (player == null || _uiManager == null)
                return;

            string key = GetPlayerKey(player);
            if (_playerPanels.TryGetValue(key, out string panelId))
            {
                _uiManager.ForgetPlayer(player);
                _uiManager.RemovePanel(panelId);
                _playerPanels.Remove(key);
            }
        }

        private static string TrimLines(string text, int maxLines)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string[] lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            if (lines.Length <= maxLines)
                return text;

            List<string> kept = new List<string>();
            for (int i = 0; i < maxLines && i < lines.Length; i++)
                kept.Add(lines[i]);

            return string.Join("\n", kept);
        }

        private static string FormatTimeLeft(TimeSpan timeLeft)
        {
            if (timeLeft < TimeSpan.Zero)
                timeLeft = TimeSpan.Zero;

            return $"{Math.Max(0, (int)timeLeft.TotalMinutes):00}:{timeLeft.Seconds:00}";
        }

        private static bool IsNtfWave(SpawnableFaction team)
        {
            string name = team.ToString();
            return name.IndexOf("Ntf", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Nine", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Mtf", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsChaosWave(SpawnableFaction team)
        {
            string name = team.ToString();
            return name.IndexOf("Chaos", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetRespawnWaveName(SpawnableFaction team)
        {
            if (IsNtfWave(team))
                return "MTF";

            if (IsChaosWave(team))
                return "Chaos";

            return "未知";
        }

        private static string GetRespawnTeamName(SpawnableFaction team)
        {
            if (IsNtfWave(team))
                return "九尾狐";

            if (IsChaosWave(team))
                return "混沌分裂者";

            return "未知";
        }

        private static string GetRespawnTeamColor(SpawnableFaction team)
        {
            if (IsNtfWave(team))
                return "#6699FF";

            if (IsChaosWave(team))
                return "#32CD32";

            return "#FFFFFF";
        }

        private static string GetPanelId(string playerKey)
        {
            return "spectator_hud_" + playerKey.Replace("@", "_").Replace(".", "_").Replace(":", "_");
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

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private class RespawnWaveInfo
        {
            public string TeamName { get; set; } = "未知";
            public string TeamColor { get; set; } = "#FFFFFF";
            public string WaveName { get; set; } = "未知";
            public string TimeLeft { get; set; } = "00:00";
            public string Tickets { get; set; } = "0";
        }
    }
}
