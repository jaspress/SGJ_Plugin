using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HintServiceMeow.Core.Enum;
using MEC;
using Newtonsoft.Json;
using SGJ_Plugin.UI.Elements;
using SGJ_Plugin.UI.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class LevelModule : ModuleBase
    {
        private readonly Config _config;
        private readonly Dictionary<string, PlayerLevelData> _levelData = new Dictionary<string, PlayerLevelData>();
        private readonly Dictionary<string, string> _playerPanels = new Dictionary<string, string>();
        private UIManager _uiManager;
        private string _dataFilePath;
        private CoroutineHandle _reloadCoroutine;
        private bool _reloadCoroutineStarted;

        public override string Name => "Level System Module";

        public LevelModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            if (!_config.LevelSystemConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            _dataFilePath = GetDataFilePath();
            LoadData();

            _uiManager = UIManager.Instance;
            if (!_uiManager.Initialize())
                throw new InvalidOperationException("UIManager failed to initialize.");

            CustomPlayerEvents.Verified += OnVerified;
            CustomPlayerEvents.Left += OnLeft;
            CustomPlayerEvents.Died += OnDied;
            CustomPlayerEvents.Escaped += OnEscaped;
            StartReloadCoroutine();

            foreach (Player player in Player.List)
            {
                EnsurePlayerData(player);
                ApplyPlayerVisuals(player);
            }

            Log.Info($"[{Name}] Enabled. Data file: {_dataFilePath}");
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Verified -= OnVerified;
            CustomPlayerEvents.Left -= OnLeft;
            CustomPlayerEvents.Died -= OnDied;
            CustomPlayerEvents.Escaped -= OnEscaped;
            StopReloadCoroutine();

            if (_uiManager != null)
            {
                foreach (Player player in Player.List)
                {
                    ResetPlayerVisuals(player);
                    RemoveHud(player);
                }
            }

            _playerPanels.Clear();
            Log.Info($"[{Name}] Disabled.");
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            LoadData();
            TitleModule.Instance?.EnsurePlayerData(ev.Player);
            EnsurePlayerData(ev.Player);

            if (_config.LevelSystemConfig.JoinExperience > 0)
                AddExperience(ev.Player, _config.LevelSystemConfig.JoinExperience, "Join");

            ApplyPlayerVisuals(ev.Player);
        }

        private void OnLeft(LeftEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            ResetPlayerVisuals(ev.Player);
            ForgetHud(ev.Player);
        }

        private void OnDied(DiedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            PlayerLevelData victim = GetData(ev.Player);
            victim.deaths++;
            AddExperience(ev.Player, _config.LevelSystemConfig.DeathExperience, null);

            Player attacker = ev.Attacker;
            if (attacker != null && attacker != ev.Player)
            {
                PlayerLevelData killer = GetData(attacker);
                killer.kills++;
                AddExperience(attacker, _config.LevelSystemConfig.KillExperience, "Kill");
            }

            SaveData();
            ApplyPlayerVisuals(ev.Player);
        }

        private void OnEscaped(EscapedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            PlayerLevelData data = GetData(ev.Player);
            data.escapes++;
            AddExperience(ev.Player, _config.LevelSystemConfig.EscapeExperience, "Escape");
        }

        private void AddExperience(Player player, int amount, string reason)
        {
            if (player == null)
                return;

            PlayerLevelData data = GetData(player);
            if (amount <= 0)
            {
                SaveData();
                ApplyPlayerVisuals(player);
                return;
            }

            data.xp += amount;
            data.total_xp += amount;

            bool leveledUp = false;
            while (data.level < GetMaxLevel() && data.xp >= GetRequiredExperience(data.level))
            {
                data.xp -= GetRequiredExperience(data.level);
                data.level++;
                leveledUp = true;
            }

            SaveData();
            ApplyPlayerVisuals(player);

            if (leveledUp)
            {
                player.ShowHint(RenderTemplate(_config.LevelSystemConfig.LevelUpText, player, data, amount, reason), 4f);
            }
            else if (!string.IsNullOrEmpty(reason))
            {
                player.ShowHint(RenderTemplate(_config.LevelSystemConfig.ExperienceGainText, player, data, amount, reason), 2.5f);
            }
        }

        private void CreateOrRefreshHud(Player player)
        {
            if (player == null || !_config.LevelSystemConfig.ShowHud)
                return;

            string key = GetPlayerKey(player);
            string panelId = GetPanelId(key);
            _playerPanels[key] = panelId;

            UIPanel panel = _uiManager.CreatePanel(panelId, "Level HUD");
            TextHintElement element = panel.GetElement("level_hud") as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, "level_hud", string.Empty);
                element.Alignment = HintAlignment.Center;
            }

            element.XCoordinate = Clamp(_config.LevelSystemConfig.HudXCoordinate, -1100f, 1100f);
            element.YCoordinate = Clamp(_config.LevelSystemConfig.HudYCoordinate, 0f, 1030f);
            element.FontSize = Math.Max(8, Math.Min(60, _config.LevelSystemConfig.HudFontSize));
            element.Content = BuildHudText(player, GetData(player, false));
            element.Update();

            _uiManager.ShowPanel(player, panelId);
        }

        private void RefreshHud(Player player)
        {
            if (player == null || !_config.LevelSystemConfig.ShowHud || _uiManager == null)
                return;

            string key = GetPlayerKey(player);
            if (!_playerPanels.TryGetValue(key, out string panelId))
            {
                CreateOrRefreshHud(player);
                return;
            }

            TextHintElement element = _uiManager.GetElement(panelId, "level_hud") as TextHintElement;
            if (element == null)
            {
                CreateOrRefreshHud(player);
                return;
            }

            element.Content = BuildHudText(player, GetData(player, false));
            element.Update();
            _uiManager.ShowPanel(player, panelId);
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

        private PlayerLevelData GetData(Player player, bool createIfMissing = true)
        {
            string key = GetPlayerKey(player);
            if (!_levelData.TryGetValue(key, out PlayerLevelData data))
            {
                if (!createIfMissing)
                    return new PlayerLevelData();

                data = new PlayerLevelData();
                _levelData[key] = data;
            }

            bool changed = NormalizeData(data);
            string nickname = player?.Nickname ?? string.Empty;
            if (createIfMissing && string.IsNullOrEmpty(data.name) && !string.IsNullOrEmpty(nickname))
            {
                data.name = nickname;
                changed = true;
            }

            if (changed)
                SaveData();

            return data;
        }

        private void EnsurePlayerData(Player player)
        {
            if (player == null)
                return;

            string key = GetPlayerKey(player);
            if (!_levelData.TryGetValue(key, out PlayerLevelData data))
            {
                data = new PlayerLevelData
                {
                    name = player.Nickname ?? string.Empty,
                    rankname = GetRankNameForLevel(1),
                };

                _levelData[key] = data;
                SaveData();
                return;
            }

            bool changed = NormalizeData(data);
            if (changed)
                SaveData();
        }

        private void LoadData()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_dataFilePath));

                if (!File.Exists(_dataFilePath))
                {
                    SaveData();
                    return;
                }

                string json = File.ReadAllText(_dataFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    Dictionary<string, PlayerLevelData> loaded = JsonConvert.DeserializeObject<Dictionary<string, PlayerLevelData>>(json);
                    if (loaded != null)
                    {
                        _levelData.Clear();
                        foreach (KeyValuePair<string, PlayerLevelData> pair in loaded)
                            _levelData[pair.Key] = pair.Value ?? new PlayerLevelData();
                    }
                }

                if (NormalizeAllData())
                    SaveData();
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to load level data: {ex}");
                TryBackupBrokenDataFile();
                _levelData.Clear();
                SaveData();
            }
        }

        private void SaveData()
        {
            if (string.IsNullOrEmpty(_dataFilePath))
                return;

            try
            {
                NormalizeAllData();
                Directory.CreateDirectory(Path.GetDirectoryName(_dataFilePath));
                string json = JsonConvert.SerializeObject(_levelData, Formatting.Indented);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to save level data: {ex}");
            }
        }

        private bool NormalizeAllData()
        {
            bool changed = false;
            foreach (PlayerLevelData data in _levelData.Values)
                changed |= NormalizeData(data);

            return changed;
        }

        private bool NormalizeData(PlayerLevelData data)
        {
            bool changed = false;
            if (data.level < 1)
            {
                data.level = 1;
                changed = true;
            }

            int maxLevel = GetMaxLevel();
            if (data.level > maxLevel)
            {
                data.level = maxLevel;
                changed = true;
            }

            if (data.xp < 0)
            {
                data.xp = 0;
                changed = true;
            }

            while (data.level < maxLevel && data.xp >= GetRequiredExperience(data.level))
            {
                data.xp -= GetRequiredExperience(data.level);
                data.level++;
                changed = true;
            }

            if (data.level >= maxLevel)
            {
                int maxLevelRequiredExperience = GetRequiredExperience(maxLevel);
                if (data.xp > maxLevelRequiredExperience)
                {
                    data.xp = maxLevelRequiredExperience;
                    changed = true;
                }
            }

            if (data.total_xp < 0)
            {
                data.total_xp = 0;
                changed = true;
            }

            if (data.kills < 0)
            {
                data.kills = 0;
                changed = true;
            }

            if (data.deaths < 0)
            {
                data.deaths = 0;
                changed = true;
            }

            if (data.escapes < 0)
            {
                data.escapes = 0;
                changed = true;
            }

            if (data.name == null)
            {
                data.name = string.Empty;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(data.title) && string.IsNullOrWhiteSpace(data.rankname))
            {
                data.rankname = data.title;
                changed = true;
            }

            if (_config.LevelSystemConfig.AutoUpdateRankNameByLevel)
            {
                string expectedRankName = GetRankNameForLevel(data.level);
                if (data.rankname != expectedRankName)
                {
                    data.rankname = expectedRankName;
                    changed = true;
                }
            }

            if (string.IsNullOrWhiteSpace(data.rankname))
            {
                data.rankname = _config.LevelSystemConfig.DefaultRankName ?? string.Empty;
                changed = true;
            }

            return changed;
        }

        private int GetRequiredExperience(int level)
        {
            List<Config.LevelExperienceRange> ranges = _config.LevelSystemConfig.ExperienceRanges;
            if (ranges != null)
            {
                foreach (Config.LevelExperienceRange range in ranges)
                {
                    if (range == null)
                        continue;

                    int min = Math.Max(1, range.MinLevel);
                    int max = Math.Max(min, range.MaxLevel);
                    if (level >= min && level <= max)
                        return Math.Max(1, range.RequiredExperience);
                }
            }

            return Math.Max(1, level * 100);
        }

        private string BuildHudText(Player player, PlayerLevelData data)
        {
            return RenderTemplate(_config.LevelSystemConfig.HudText, player, data, 0, string.Empty);
        }

        private string RenderTemplate(string template, Player player, PlayerLevelData data, int gainedXp, string reason, bool useRichLevelRankName = true)
        {
            if (string.IsNullOrEmpty(template))
                template = "名字: {name} | 等级: {level} | EXP: {xp}/{required_xp} | 称号: {title}\n<align=center>段位称号: {level_rankname}</align>";

            int required = GetRequiredExperience(data.level);
            int percent = Math.Max(0, Math.Min(100, (int)Math.Round((double)data.xp / required * 100)));
            string key = player == null ? string.Empty : GetPlayerKey(player);
            string levelRankName = GetRankNameForLevel(data.level);
            string levelRankColor = GetRankColorForLevel(data.level);
            string displayLevelRankName = useRichLevelRankName ? Colorize(levelRankName, levelRankColor) : levelRankName;
            string titleRankName = TitleModule.Instance?.GetOverrideRankName(key) ?? string.Empty;
            string titleColor = TitleModule.Instance?.GetOverrideRankColor(key) ?? string.Empty;
            string effectiveRankName = GetEffectiveRankName(key, data);

            return template
                .Replace("{name}", data.name ?? string.Empty)
                .Replace("{steamid}", key)
                .Replace("{level}", data.level.ToString())
                .Replace("{xp}", data.xp.ToString())
                .Replace("{required_xp}", required.ToString())
                .Replace("{total_xp}", data.total_xp.ToString())
                .Replace("{title}", effectiveRankName)
                .Replace("{rankname}", effectiveRankName)
                .Replace("{level_rankname}", displayLevelRankName)
                .Replace("{level_rankname_raw}", levelRankName)
                .Replace("{level_rankcolor}", levelRankColor)
                .Replace("{title_rankname}", titleRankName)
                .Replace("{title_color}", titleColor)
                .Replace("{progress_bar}", BuildProgressBar(data.xp, required))
                .Replace("{progress_percent}", percent.ToString())
                .Replace("{kills}", data.kills.ToString())
                .Replace("{deaths}", data.deaths.ToString())
                .Replace("{escapes}", data.escapes.ToString())
                .Replace("{gained_xp}", gainedXp.ToString())
                .Replace("{reason}", reason ?? string.Empty);
        }

        private void UpdateDisplayNickname(Player player)
        {
            if (player == null || !_config.LevelSystemConfig.UpdateDisplayNickname)
                return;

            try
            {
                PlayerLevelData data = GetData(player, false);
                string template = string.IsNullOrEmpty(_config.LevelSystemConfig.DisplayNameText)
                    ? "[Lv.{level}] {name}"
                    : _config.LevelSystemConfig.DisplayNameText;

                player.DisplayNickname = RenderTemplate(template, player, data, 0, string.Empty, false);
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to update display nickname for {player.Nickname}: {ex.Message}");
            }
        }

        private void ResetDisplayNickname(Player player)
        {
            if (player == null || !_config.LevelSystemConfig.UpdateDisplayNickname)
                return;

            try
            {
                player.DisplayNickname = null;
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to reset display nickname for {player.Nickname}: {ex.Message}");
            }
        }

        private void UpdateRankName(Player player)
        {
            if (player == null)
                return;

            try
            {
                PlayerLevelData data = GetData(player, false);
                player.RankName = GetEffectiveRankName(GetPlayerKey(player), data);
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to update rankname for {player.Nickname}: {ex.Message}");
            }
        }

        private void ResetRankName(Player player)
        {
            if (player == null)
                return;

            try
            {
                player.RankName = null;
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to reset rankname for {player.Nickname}: {ex.Message}");
            }
        }

        private void UpdateRankColor(Player player)
        {
            if (player == null)
                return;

            try
            {
                string color = TitleModule.Instance?.GetOverrideRankColor(GetPlayerKey(player)) ?? string.Empty;
                player.RankColor = string.IsNullOrWhiteSpace(color) ? null : color;
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to update rankcolor for {player.Nickname}: {ex.Message}");
            }
        }

        private void ResetRankColor(Player player)
        {
            if (player == null)
                return;

            try
            {
                player.RankColor = null;
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to reset rankcolor for {player.Nickname}: {ex.Message}");
            }
        }

        private void ApplyPlayerVisuals(Player player)
        {
            if (player == null)
                return;

            TitleModule.Instance?.EnsurePlayerData(player);
            UpdateDisplayNickname(player);
            UpdateRankName(player);
            UpdateRankColor(player);
            RefreshHud(player);
        }

        private void StartReloadCoroutine()
        {
            if (_reloadCoroutineStarted)
                return;

            _reloadCoroutineStarted = true;
            _reloadCoroutine = Timing.RunCoroutine(ReloadDataLoop());
        }

        private void StopReloadCoroutine()
        {
            if (!_reloadCoroutineStarted)
                return;

            Timing.KillCoroutines(_reloadCoroutine);
            _reloadCoroutineStarted = false;
        }

        private void ResetPlayerVisuals(Player player)
        {
            ResetDisplayNickname(player);
            ResetRankName(player);
            ResetRankColor(player);
        }

        private IEnumerator<float> ReloadDataLoop()
        {
            while (_reloadCoroutineStarted)
            {
                yield return Timing.WaitForSeconds(1f);
                ReloadDataFromDisk();
            }
        }

        private void ReloadDataFromDisk()
        {
            try
            {
                if (string.IsNullOrEmpty(_dataFilePath) || !File.Exists(_dataFilePath))
                    return;

                LoadData();

                foreach (Player player in Player.List)
                    ApplyPlayerVisuals(player);
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to reload level data: {ex.Message}");
            }
        }

        private string GetRankNameForLevel(int level)
        {
            List<Config.LevelRankNameRange> ranges = _config.LevelSystemConfig.RankNameRanges;
            if (ranges != null)
            {
                foreach (Config.LevelRankNameRange range in ranges)
                {
                    if (range == null)
                        continue;

                    int min = Math.Max(1, range.MinLevel);
                    int max = Math.Max(min, range.MaxLevel);
                    if (level >= min && level <= max)
                        return string.IsNullOrWhiteSpace(range.RankName) ? (_config.LevelSystemConfig.DefaultRankName ?? string.Empty) : range.RankName;
                }
            }

            return _config.LevelSystemConfig.DefaultRankName ?? string.Empty;
        }

        private string GetRankColorForLevel(int level)
        {
            List<Config.LevelRankNameRange> ranges = _config.LevelSystemConfig.RankNameRanges;
            if (ranges != null)
            {
                foreach (Config.LevelRankNameRange range in ranges)
                {
                    if (range == null)
                        continue;

                    int min = Math.Max(1, range.MinLevel);
                    int max = Math.Max(min, range.MaxLevel);
                    if (level >= min && level <= max)
                        return string.IsNullOrWhiteSpace(range.Color) ? (_config.LevelSystemConfig.DefaultRankColor ?? string.Empty) : range.Color;
                }
            }

            return _config.LevelSystemConfig.DefaultRankColor ?? string.Empty;
        }

        private static string Colorize(string text, string color)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(color))
                return text;

            return $"<color={color}>{text}</color>";
        }

        private string GetEffectiveRankName(string steamId, PlayerLevelData data)
        {
            string overrideRankName = TitleModule.Instance?.GetOverrideRankName(steamId);
            if (!string.IsNullOrWhiteSpace(overrideRankName))
                return overrideRankName;

            return data == null ? string.Empty : GetRankNameForLevel(data.level);
        }

        private int GetMaxLevel()
        {
            return Math.Max(1, _config.LevelSystemConfig.MaxLevel);
        }

        private string BuildProgressBar(int current, int max)
        {
            int length = Math.Max(1, Math.Min(40, _config.LevelSystemConfig.ProgressBarLength));
            if (max <= 0)
                max = 1;

            string filledChar = string.IsNullOrEmpty(_config.LevelSystemConfig.ProgressBarFilled) ? "|" : _config.LevelSystemConfig.ProgressBarFilled.Substring(0, 1);
            string emptyChar = string.IsNullOrEmpty(_config.LevelSystemConfig.ProgressBarEmpty) ? "." : _config.LevelSystemConfig.ProgressBarEmpty.Substring(0, 1);
            int filled = Math.Max(0, Math.Min(length, (int)Math.Round((double)current / max * length)));

            return $"[{new string(filledChar[0], filled)}{new string(emptyChar[0], length - filled)}]";
        }

        private string GetDataFilePath()
        {
            string fileName = string.IsNullOrWhiteSpace(_config.LevelSystemConfig.DataFileName)
                ? "SGJ_LevelSystem.json"
                : _config.LevelSystemConfig.DataFileName;

            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "EXILED", "Config", fileName);
        }

        private void TryBackupBrokenDataFile()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                    return;

                string backup = _dataFilePath + ".broken." + DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Copy(_dataFilePath, backup, true);
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to backup broken level data file: {ex.Message}");
            }
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private static string GetPanelId(string playerKey)
        {
            return "level_hud_" + playerKey.Replace("@", "_").Replace(".", "_").Replace(":", "_");
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

        public class PlayerLevelData
        {
            public string name { get; set; } = string.Empty;
            public int xp { get; set; }
            public int level { get; set; } = 1;
            public int total_xp { get; set; }
            public string title { get; set; }
            public string rankname { get; set; } = string.Empty;
            public int kills { get; set; }
            public int deaths { get; set; }
            public int escapes { get; set; }

            public bool ShouldSerializetitle()
            {
                return false;
            }
        }
    }
}
