using Exiled.Events.EventArgs.Player;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using HintServiceMeow.Core.Enum;
using MEC;
using Newtonsoft.Json;
using PlayerRoles;
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
        private const string LevelHudElementId = "level_hud";
        private const string ExperienceHintElementId = "level_exp_hint";

        private readonly Config _config;
        private readonly Dictionary<string, PlayerLevelData> _levelData = new Dictionary<string, PlayerLevelData>();
        private readonly Dictionary<string, string> _playerPanels = new Dictionary<string, string>();
        private readonly Dictionary<string, Dictionary<string, AssistDamageInfo>> _assistDamage = new Dictionary<string, Dictionary<string, AssistDamageInfo>>();
        private readonly Dictionary<string, List<TopRightHintMessage>> _topRightHints = new Dictionary<string, List<TopRightHintMessage>>();
        private UIManager _uiManager;
        private string _dataFilePath;
        private CoroutineHandle _reloadCoroutine;
        private bool _reloadCoroutineStarted;

        public static LevelModule Instance { get; private set; }

        public override string Name => "Level System Module";

        public LevelModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            Instance = this;

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
            CustomPlayerEvents.Hurt += OnHurt;
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
            CustomPlayerEvents.Hurt -= OnHurt;
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
            _assistDamage.Clear();
            _topRightHints.Clear();
            if (Instance == this)
                Instance = null;

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
            RemoveAssistData(ev.Player);
        }

        private void OnHurt(HurtEventArgs ev)
        {
            if (!_config.LevelSystemConfig.AssistExperienceEnabled || ev?.Player == null || ev.Attacker == null)
                return;

            if (ev.Attacker == ev.Player || ev.Amount <= 0f)
                return;

            string victimKey = GetPlayerKey(ev.Player);
            string attackerKey = GetPlayerKey(ev.Attacker);
            if (string.IsNullOrWhiteSpace(victimKey) || string.IsNullOrWhiteSpace(attackerKey))
                return;

            if (!_assistDamage.TryGetValue(victimKey, out Dictionary<string, AssistDamageInfo> attackers))
            {
                attackers = new Dictionary<string, AssistDamageInfo>();
                _assistDamage[victimKey] = attackers;
            }

            if (!attackers.TryGetValue(attackerKey, out AssistDamageInfo info))
            {
                info = new AssistDamageInfo { Attacker = ev.Attacker };
                attackers[attackerKey] = info;
            }

            info.Attacker = ev.Attacker;
            info.Damage += ev.Amount;
            info.LastDamageAt = DateTime.UtcNow;
        }

        private void OnDied(DiedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            PlayerLevelData victim = GetData(ev.Player);
            victim.deaths++;
            AddExperience(ev.Player, _config.LevelSystemConfig.DeathExperience, null);

            Player attacker = ev.Attacker;
            int killExperience = GetKillExperienceForRole(ev.TargetOldRole);
            if (attacker != null && attacker != ev.Player)
            {
                PlayerLevelData killer = GetData(attacker);
                killer.kills++;
                AddExperience(attacker, killExperience, "Kill");
            }

            AwardAssistExperience(ev.Player, attacker, killExperience);
            RemoveAssistData(ev.Player);

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
                ShowTopRightHint(player, RenderTemplate(_config.LevelSystemConfig.LevelUpText, player, data, amount, reason), 4f);
            }
            else if (!string.IsNullOrEmpty(reason))
            {
                ShowTopRightHint(player, RenderTemplate(_config.LevelSystemConfig.ExperienceGainText, player, data, amount, reason), 2.5f);
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
            TextHintElement element = panel.GetElement(LevelHudElementId) as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, LevelHudElementId, string.Empty);
                element.Alignment = HintAlignment.Center;
            }

            TextHintElement experienceElement = panel.GetElement(ExperienceHintElementId) as TextHintElement;
            if (experienceElement == null)
            {
                experienceElement = _uiManager.CreateTextHint(panelId, ExperienceHintElementId, string.Empty);
                experienceElement.Alignment = HintAlignment.Right;
                experienceElement.IsVisible = false;
            }

            element.XCoordinate = Clamp(_config.LevelSystemConfig.HudXCoordinate, -1100f, 1100f);
            element.YCoordinate = Clamp(_config.LevelSystemConfig.HudYCoordinate, 0f, 1030f);
            element.FontSize = Math.Max(8, Math.Min(60, _config.LevelSystemConfig.HudFontSize));
            element.Content = BuildViewerHudText(player);
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

            TextHintElement element = _uiManager.GetElement(panelId, LevelHudElementId) as TextHintElement;
            if (element == null)
            {
                CreateOrRefreshHud(player);
                return;
            }

            element.Content = BuildViewerHudText(player);
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
                    rankname = string.Empty,
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

            if (data.rankname == null)
            {
                data.rankname = string.Empty;
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

        private int GetKillExperienceForRole(RoleTypeId role)
        {
            List<Config.RoleExperienceReward> rewards = _config.LevelSystemConfig.KillExperienceByRole;
            if (rewards != null)
            {
                string roleName = role.ToString();
                foreach (Config.RoleExperienceReward reward in rewards)
                {
                    if (reward == null || string.IsNullOrWhiteSpace(reward.Role))
                        continue;

                    if (string.Equals(reward.Role, roleName, StringComparison.OrdinalIgnoreCase))
                        return Math.Max(0, reward.Experience);
                }
            }

            return Math.Max(0, _config.LevelSystemConfig.KillExperience);
        }

        private void AwardAssistExperience(Player victim, Player killer, int killExperience)
        {
            if (!_config.LevelSystemConfig.AssistExperienceEnabled || victim == null || killExperience <= 0)
                return;

            string victimKey = GetPlayerKey(victim);
            if (!_assistDamage.TryGetValue(victimKey, out Dictionary<string, AssistDamageInfo> attackers))
                return;

            DateTime now = DateTime.UtcNow;
            float expireSeconds = Math.Max(1f, _config.LevelSystemConfig.AssistDamageExpireSeconds);
            float minDamage = Math.Max(0f, _config.LevelSystemConfig.AssistMinimumDamage);
            int assistExperience = Math.Max(0, (int)Math.Round(killExperience * Math.Max(0f, _config.LevelSystemConfig.AssistExperiencePercent)));
            if (assistExperience <= 0)
                return;

            string killerKey = killer == null ? string.Empty : GetPlayerKey(killer);
            foreach (KeyValuePair<string, AssistDamageInfo> pair in attackers)
            {
                AssistDamageInfo info = pair.Value;
                if (info == null || info.Attacker == null)
                    continue;

                if (string.Equals(pair.Key, killerKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (info.Damage < minDamage)
                    continue;

                if ((now - info.LastDamageAt).TotalSeconds > expireSeconds)
                    continue;

                AddExperience(info.Attacker, assistExperience, "Assist");
            }
        }

        private void RemoveAssistData(Player player)
        {
            if (player == null)
                return;

            string key = GetPlayerKey(player);
            _assistDamage.Remove(key);

            foreach (Dictionary<string, AssistDamageInfo> attackers in _assistDamage.Values)
                attackers.Remove(key);
        }

        public void ShowTopRightHint(Player player, string text, float duration)
        {
            if (player == null || _uiManager == null || string.IsNullOrWhiteSpace(text))
                return;

            string key = GetPlayerKey(player);
            if (!_topRightHints.TryGetValue(key, out List<TopRightHintMessage> messages))
            {
                messages = new List<TopRightHintMessage>();
                _topRightHints[key] = messages;
            }

            DateTime now = DateTime.UtcNow;
            messages.RemoveAll(message => message.ExpireAt <= now);
            messages.Insert(0, new TopRightHintMessage
            {
                Text = text,
                ExpireAt = now.AddSeconds(Math.Max(0.5f, duration)),
            });

            RefreshTopRightHint(player);
        }

        private void RefreshTopRightHint(Player player)
        {
            if (player == null || _uiManager == null)
                return;

            string key = GetPlayerKey(player);
            if (!_topRightHints.TryGetValue(key, out List<TopRightHintMessage> messages))
                messages = new List<TopRightHintMessage>();

            DateTime now = DateTime.UtcNow;
            messages.RemoveAll(message => message.ExpireAt <= now);

            string panelId = GetPanelId(key);
            _playerPanels[key] = panelId;

            UIPanel panel = _uiManager.CreatePanel(panelId, "Level HUD");
            TextHintElement element = panel.GetElement(ExperienceHintElementId) as TextHintElement;
            if (element == null)
            {
                element = _uiManager.CreateTextHint(panelId, ExperienceHintElementId, string.Empty);
                element.Alignment = HintAlignment.Right;
            }

            element.Alignment = HintAlignment.Right;
            element.XCoordinate = Clamp(_config.LevelSystemConfig.ExperienceHintXCoordinate, -1100f, 1100f);
            element.YCoordinate = Clamp(_config.LevelSystemConfig.ExperienceHintYCoordinate, 0f, 1030f);
            element.FontSize = Math.Max(8, Math.Min(60, _config.LevelSystemConfig.ExperienceHintFontSize));
            element.Content = BuildTopRightHintText(messages, now);
            element.IsVisible = !string.IsNullOrWhiteSpace(element.Content);
            element.Update();
            _uiManager.ShowPanel(player, panelId);
        }

        private static string BuildTopRightHintText(List<TopRightHintMessage> messages, DateTime now)
        {
            if (messages == null || messages.Count == 0)
                return string.Empty;

            List<string> lines = new List<string>();
            foreach (TopRightHintMessage message in messages)
            {
                int seconds = Math.Max(0, (int)Math.Ceiling((message.ExpireAt - now).TotalSeconds));
                lines.Add($"[{seconds}s] {message.Text}");
            }

            return string.Join("\n", lines);
        }

        private string BuildHudText(Player player, PlayerLevelData data)
        {
            return RenderTemplate(_config.LevelSystemConfig.HudText, player, data, 0, string.Empty);
        }

        public string BuildHudTextFor(Player target, bool includeRoleLine = true)
        {
            PlayerLevelData data = GetData(target, false);
            string rendered = RenderTemplate(_config.LevelSystemConfig.HudText, target, data, 0, string.Empty);
            return includeRoleLine ? rendered : RemoveRoleLine(rendered);
        }

        private string BuildViewerHudText(Player viewer)
        {
            if (viewer == null)
                return string.Empty;

            if (viewer.Role.Type == RoleTypeId.Spectator)
            {
                Player observed = GetObservedPlayer(viewer);
                if (_config.SpectatorHudConfig.ShowObservedPlayerLevelHud && observed != null)
                    return BuildHudTextFor(observed, false);

                return BuildHudTextFor(viewer, false);
            }

            return BuildHudTextFor(viewer, true);
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
            string effectiveTitleName = GetEffectiveTitleName(key);
            string roleName = player == null || player.Role.Type == RoleTypeId.Spectator
                ? string.Empty
                : GetChineseRoleName(player.Role.Type);
            string roleColor = GetRoleColor(player);

            return template
                .Replace("{name}", data.name ?? string.Empty)
                .Replace("{steamid}", key)
                .Replace("{level}", data.level.ToString())
                .Replace("{xp}", data.xp.ToString())
                .Replace("{required_xp}", required.ToString())
                .Replace("{total_xp}", data.total_xp.ToString())
                .Replace("{title}", effectiveTitleName)
                .Replace("{rankname}", effectiveTitleName)
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
                .Replace("{reason}", reason ?? string.Empty)
                .Replace("{role_name}", roleName)
                .Replace("{rolecolor}", roleColor)
                .Replace("{role_color}", roleColor)
                .Replace("{role}", player?.Role.Type.ToString() ?? string.Empty);
        }

        private string GetRoleColor(Player player)
        {
            if (player == null)
                return "#FFFFFF";

            List<Config.TeamChatColor> colors = _config.ChatConfig?.TeamColors;
            if (colors != null)
            {
                string teamName = player.Role.Team.ToString();
                foreach (Config.TeamChatColor color in colors)
                {
                    if (color == null || string.IsNullOrWhiteSpace(color.Team))
                        continue;

                    if (string.Equals(color.Team, teamName, StringComparison.OrdinalIgnoreCase))
                        return string.IsNullOrWhiteSpace(color.Color) ? "#FFFFFF" : color.Color;
                }
            }

            return "#FFFFFF";
        }

        private static string RemoveRoleLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string[] lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            List<string> keptLines = new List<string>();
            foreach (string line in lines)
            {
                if (line.IndexOf("你正在扮演", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                keptLines.Add(line);
            }

            return string.Join("\n", keptLines);
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
                string titleRankName = TitleModule.Instance?.GetOverrideRankName(GetPlayerKey(player)) ?? string.Empty;
                player.RankName = string.IsNullOrWhiteSpace(titleRankName) ? null : titleRankName;
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
                {
                    ApplyPlayerVisuals(player);
                    RefreshTopRightHint(player);
                }
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

        private string GetEffectiveTitleName(string steamId)
        {
            string overrideRankName = TitleModule.Instance?.GetOverrideRankName(steamId);
            if (!string.IsNullOrWhiteSpace(overrideRankName))
                return overrideRankName;

            return "无";
        }

        internal static string GetChineseRoleName(RoleTypeId role)
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
            string newPath = Path.Combine(appData, "EXILED", "Configs", fileName);
            string oldPath = Path.Combine(appData, "EXILED", "Config", fileName);
            TryMigrateDataFile(oldPath, newPath);
            TryMigrateDataFile(Path.Combine(appData, "EXILED", "Config", "SGJ_LevelSystem.json"), newPath);
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

        private class AssistDamageInfo
        {
            public Player Attacker { get; set; }
            public float Damage { get; set; }
            public DateTime LastDamageAt { get; set; }
        }

        private class TopRightHintMessage
        {
            public string Text { get; set; } = string.Empty;
            public DateTime ExpireAt { get; set; }
        }
    }
}
