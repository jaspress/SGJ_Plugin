using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class TitleModule : ModuleBase
    {
        private readonly Config _config;
        private readonly Dictionary<string, PlayerTitleData> _titleData = new Dictionary<string, PlayerTitleData>();
        private string _dataFilePath;
        private CoroutineHandle _reloadCoroutine;
        private bool _reloadCoroutineStarted;

        public static TitleModule Instance { get; private set; }

        public override string Name => "Title System Module";

        public TitleModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            Instance = this;

            if (!_config.TitleSystemConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            _dataFilePath = GetDataFilePath();
            LoadData();

            CustomPlayerEvents.Verified += OnVerified;

            foreach (Player player in Player.List)
                EnsurePlayerData(player);

            StartReloadCoroutine();
            Log.Info($"[{Name}] Enabled. Data file: {_dataFilePath}");
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Verified -= OnVerified;
            StopReloadCoroutine();

            if (Instance == this)
                Instance = null;

            Log.Info($"[{Name}] Disabled.");
        }

        public string GetOverrideRankName(string steamId)
        {
            if (!_config.TitleSystemConfig.IsEnabled || string.IsNullOrWhiteSpace(steamId))
                return string.Empty;

            if (!_titleData.TryGetValue(steamId, out PlayerTitleData data) || data == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(data.rankname) ? string.Empty : data.rankname;
        }

        public string GetOverrideRankColor(string steamId)
        {
            if (!_config.TitleSystemConfig.IsEnabled || string.IsNullOrWhiteSpace(steamId))
                return string.Empty;

            if (!_titleData.TryGetValue(steamId, out PlayerTitleData data) || data == null || string.IsNullOrWhiteSpace(data.rankname))
                return string.Empty;

            return ResolveRankColor(data.rankcolor);
        }

        public void EnsurePlayerData(Player player)
        {
            if (player == null || !_config.TitleSystemConfig.IsEnabled)
                return;

            string key = GetPlayerKey(player);
            if (!_titleData.TryGetValue(key, out PlayerTitleData data) || data == null)
            {
                _titleData[key] = new PlayerTitleData
                {
                    name = player.Nickname ?? string.Empty,
                    rankname = string.Empty,
                    rankcolor = string.Empty,
                };
                SaveData();
                return;
            }

            bool changed = NormalizeData(data);
            if (string.IsNullOrWhiteSpace(data.name) && !string.IsNullOrWhiteSpace(player.Nickname))
            {
                data.name = player.Nickname;
                changed = true;
            }

            if (changed)
                SaveData();
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            LoadData();
            EnsurePlayerData(ev.Player);
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

        private IEnumerator<float> ReloadDataLoop()
        {
            while (_reloadCoroutineStarted)
            {
                yield return Timing.WaitForSeconds(1f);
                LoadData();
            }
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
                    Dictionary<string, PlayerTitleData> loaded = JsonConvert.DeserializeObject<Dictionary<string, PlayerTitleData>>(json);
                    if (loaded != null)
                    {
                        _titleData.Clear();
                        foreach (KeyValuePair<string, PlayerTitleData> pair in loaded)
                            _titleData[pair.Key] = pair.Value ?? new PlayerTitleData();
                    }
                }

                if (NormalizeAllData())
                    SaveData();
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to load title data: {ex.Message}");
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
                File.WriteAllText(_dataFilePath, JsonConvert.SerializeObject(_titleData, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to save title data: {ex}");
            }
        }

        private bool NormalizeAllData()
        {
            bool changed = false;
            foreach (PlayerTitleData data in _titleData.Values)
                changed |= NormalizeData(data);

            return changed;
        }

        private static bool NormalizeData(PlayerTitleData data)
        {
            bool changed = false;
            if (data.name == null)
            {
                data.name = string.Empty;
                changed = true;
            }

            if (data.rankname == null)
            {
                data.rankname = string.Empty;
                changed = true;
            }

            if (data.rankcolor == null)
            {
                data.rankcolor = string.Empty;
                changed = true;
            }

            return changed;
        }

        private string ResolveRankColor(string rankColor)
        {
            string color = string.IsNullOrWhiteSpace(rankColor)
                ? (_config.TitleSystemConfig.DefaultRankColor ?? string.Empty)
                : rankColor.Trim();

            if (string.Equals(color, "rainbow", StringComparison.OrdinalIgnoreCase))
                return GetRainbowColor();

            return color.ToLowerInvariant();
        }

        private string GetRainbowColor()
        {
            List<string> colors = _config.TitleSystemConfig.RainbowColors;
            if (colors == null || colors.Count == 0)
                return "green";

            int index = Math.Abs((int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond)) % colors.Count;
            string color = colors[index];
            return string.IsNullOrWhiteSpace(color) ? "green" : color.Trim().ToLowerInvariant();
        }

        private string GetDataFilePath()
        {
            string fileName = string.IsNullOrWhiteSpace(_config.TitleSystemConfig.DataFileName)
                ? "SGJ_TitleSystem.json"
                : _config.TitleSystemConfig.DataFileName;

            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string newPath = Path.Combine(appData, "EXILED", "Configs", fileName);
            string oldPath = Path.Combine(appData, "EXILED", "Config", fileName);
            TryMigrateDataFile(oldPath, newPath);
            TryMigrateDataFile(Path.Combine(appData, "EXILED", "Config", "SGJ_TitleSystem.json"), newPath);
            return newPath;
        }

        private void TryMigrateDataFile(string oldPath, string newPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newPath) || File.Exists(newPath) || !File.Exists(oldPath))
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                File.Copy(oldPath, newPath, false);
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to migrate data file '{oldPath}' to '{newPath}': {ex.Message}");
            }
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

        public class PlayerTitleData
        {
            public string name { get; set; } = string.Empty;
            public string rankname { get; set; } = string.Empty;
            public string rankcolor { get; set; } = string.Empty;
        }
    }
}
