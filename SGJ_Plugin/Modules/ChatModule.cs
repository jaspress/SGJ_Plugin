using CommandSystem;
using Exiled.API.Features;
using HintServiceMeow.Core.Enum;
using MEC;
using Newtonsoft.Json;
using PlayerRoles;
using PluginHelper = SGJ_Plugin.Helper.Helper;
using SGJ_Plugin.UI.Elements;
using SGJ_Plugin.UI.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class ChatModule : ModuleBase
    {
        private const string GlobalElementId = "global_chat";
        private const string TeamElementId = "team_chat";
        private static readonly Regex RichTextTagRegex = new Regex("<[^>]*>", RegexOptions.Compiled);

        private readonly Config _config;
        private readonly List<ActiveChatMessage> _activeMessages = new List<ActiveChatMessage>();
        private readonly Dictionary<string, string> _playerPanels = new Dictionary<string, string>();
        private List<string> _blockedWords = new List<string>();
        private ChatDatabase _database = new ChatDatabase();
        private UIManager _uiManager;
        private string _chatLogFilePath;
        private string _blockedWordsFilePath;
        private CoroutineHandle _refreshCoroutine;
        private bool _refreshCoroutineStarted;

        public static ChatModule Instance { get; private set; }

        public override string Name => "Chat UI Module";

        public ChatModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            Instance = this;

            if (!_config.ChatConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            _chatLogFilePath = GetDataFilePath(_config.ChatConfig.DataFileName, "ChatLog.json");
            _blockedWordsFilePath = GetDataFilePath(_config.ChatConfig.BlockedWordsDataFileName, "ChatBlockedWords.json");
            LoadData();
            LoadBlockedWords();

            _uiManager = UIManager.Instance;
            if (!_uiManager.Initialize())
                throw new InvalidOperationException("UIManager failed to initialize.");

            CustomPlayerEvents.Verified += OnVerified;
            CustomPlayerEvents.Left += OnLeft;

            foreach (Player player in Player.List)
                CreateOrRefreshHud(player);

            StartRefreshCoroutine();
            Log.Info($"[{Name}] Enabled. Chat log: {_chatLogFilePath}, blocked words: {_blockedWordsFilePath}");
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

            if (Instance == this)
                Instance = null;

            Log.Info($"[{Name}] Disabled.");
        }

        public bool SendChat(Player sender, ChatChannel channel, string rawContent, out string response)
        {
            response = string.Empty;
            if (sender == null)
            {
                response = "Only players can use this command.";
                return false;
            }

            string content = SanitizeContent(rawContent);
            if (string.IsNullOrWhiteSpace(content))
            {
                response = "Message cannot be empty.";
                return false;
            }

            string blockedWord = GetMatchedBlockedWord(content);
            if (!string.IsNullOrEmpty(blockedWord))
            {
                if (_config.ChatConfig.LogBlockedMessages)
                    Log.Info($"[聊天系统] [{sender.Nickname}] ：{content} [被违禁词系统拦截: {blockedWord}]");

                response = $"Message contains blocked word: {blockedWord}";
                return false;
            }

            ChatRecord record = CreateRecord(sender, channel, content);
            AddRecord(record);

            string rendered = RenderTemplate(channel == ChatChannel.Global ? _config.ChatConfig.GlobalChatTemplate : _config.ChatConfig.TeamChatTemplate, record);
            AddActiveMessage(channel, record, rendered);

            RefreshAllHuds();
            Timing.CallDelayed(0.1f, RefreshAllHuds);
            response = channel == ChatChannel.Global ? "Global chat sent." : "Team chat sent.";
            if (_config.ChatConfig.LogChatMessages)
                Log.Info($"[聊天系统] [{sender.Nickname}] ：{content}");
            return true;
        }

        public bool SendUiTest(Player sender, out string response)
        {
            response = string.Empty;
            if (sender == null)
            {
                response = "Only players can use this command.";
                return false;
            }

            ChatRecord record = CreateRecord(sender, ChatChannel.Global, "聊天UI测试消息");
            record.Channel = ChatChannel.Global;
            string rendered = "<b><color=#00FFFF>[聊天系统]</color> 如果你能看到这行，聊天UI显示正常。</b>";
            AddActiveMessage(ChatChannel.Global, record, rendered, Math.Max(10f, GetVisibleSeconds(ChatChannel.Global)));

            CreateOrRefreshHud(sender);
            RefreshAllHuds();
            Timing.CallDelayed(0.1f, RefreshAllHuds);
            Timing.CallDelayed(0.5f, RefreshAllHuds);
            response = "Chat UI test message sent.";
            Log.Info($"[聊天系统] UI test sent by [{sender.Nickname}]");
            return true;
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
            if (player == null || !_config.ChatConfig.IsEnabled || _uiManager == null)
                return;

            string key = GetPlayerKey(player);
            string panelId = GetPanelId(key);
            _playerPanels[key] = panelId;

            UIPanel panel = _uiManager.CreatePanel(panelId, "Chat UI");
            TextHintElement globalElement = panel.GetElement(GlobalElementId) as TextHintElement;
            if (globalElement == null)
                globalElement = _uiManager.CreateTextHint(panelId, GlobalElementId, string.Empty);

            TextHintElement teamElement = panel.GetElement(TeamElementId) as TextHintElement;
            if (teamElement == null)
                teamElement = _uiManager.CreateTextHint(panelId, TeamElementId, string.Empty);

            ConfigureElement(globalElement, _config.ChatConfig.GlobalXCoordinate, _config.ChatConfig.GlobalYCoordinate);
            ConfigureElement(teamElement, _config.ChatConfig.TeamXCoordinate, _config.ChatConfig.TeamYCoordinate);
            RefreshHud(player);
        }

        private void ConfigureElement(TextHintElement element, float x, float y)
        {
            if (element == null)
                return;

            element.Alignment = HintAlignment.Left;
            element.XCoordinate = Clamp(x, -950f, 950f);
            element.YCoordinate = Clamp(y, 0f, 1030f);
            element.FontSize = 16;
        }

        private void RefreshHud(Player player)
        {
            if (player == null || !_config.ChatConfig.IsEnabled || _uiManager == null)
                return;

            string key = GetPlayerKey(player);
            if (!_playerPanels.TryGetValue(key, out string panelId))
            {
                CreateOrRefreshHud(player);
                return;
            }

            TextHintElement globalElement = _uiManager.GetElement(panelId, GlobalElementId) as TextHintElement;
            TextHintElement teamElement = _uiManager.GetElement(panelId, TeamElementId) as TextHintElement;
            if (globalElement == null || teamElement == null)
            {
                CreateOrRefreshHud(player);
                return;
            }

            globalElement.Content = BuildVisibleText(player, ChatChannel.Global);
            teamElement.Content = BuildVisibleText(player, ChatChannel.Team);
            globalElement.IsVisible = !string.IsNullOrWhiteSpace(globalElement.Content);
            teamElement.IsVisible = !string.IsNullOrWhiteSpace(teamElement.Content);
            globalElement.Update();
            teamElement.Update();
            _uiManager.ShowPanel(player, panelId);

            if (_config.ChatConfig.LogUiDebug && (!string.IsNullOrWhiteSpace(globalElement.Content) || !string.IsNullOrWhiteSpace(teamElement.Content)))
                Log.Info($"[聊天系统] UI refresh for {player.Nickname}: global={globalElement.Content.Length} chars, team={teamElement.Content.Length} chars.");
        }

        private string BuildVisibleText(Player viewer, ChatChannel channel)
        {
            int maxLines = channel == ChatChannel.Global
                ? Math.Max(1, _config.ChatConfig.GlobalMaxVisibleMessages)
                : Math.Max(1, _config.ChatConfig.TeamMaxVisibleMessages);

            IEnumerable<ActiveChatMessage> messages = _activeMessages
                .Where(message => message.Record.Channel == channel && CanSee(viewer, message.Record))
                .Where(message => message.ExpireAt > DateTime.UtcNow)
                .OrderByDescending(message => message.Record.UtcTime)
                .Take(maxLines);

            return string.Join("\n", messages.Select(message => message.RenderedText));
        }

        private bool CanSee(Player viewer, ChatRecord record)
        {
            if (viewer == null || record == null)
                return false;

            if (record.Channel == ChatChannel.Global)
                return true;

            return viewer.Role.Team.ToString() == record.Team;
        }

        private void RefreshAllHuds()
        {
            PruneExpiredMessages();
            foreach (Player player in Player.List)
                RefreshHud(player);
        }

        private void AddActiveMessage(ChatChannel channel, ChatRecord record, string rendered, float? visibleSeconds = null)
        {
            DateTime expireAt = DateTime.UtcNow.AddSeconds(visibleSeconds ?? GetVisibleSeconds(channel));
            int maxLines = channel == ChatChannel.Global
                ? Math.Max(1, _config.ChatConfig.GlobalMaxVisibleMessages)
                : Math.Max(1, _config.ChatConfig.TeamMaxVisibleMessages);

            List<ActiveChatMessage> channelMessages = _activeMessages
                .Where(message => message.Record.Channel == channel)
                .OrderByDescending(message => message.Record.UtcTime)
                .ToList();

            while (channelMessages.Count >= maxLines)
            {
                ActiveChatMessage oldest = channelMessages[channelMessages.Count - 1];
                _activeMessages.Remove(oldest);
                channelMessages.RemoveAt(channelMessages.Count - 1);
            }

            foreach (ActiveChatMessage message in channelMessages)
                message.ExpireAt = expireAt;

            _activeMessages.Add(new ActiveChatMessage
            {
                Record = record,
                RenderedText = rendered,
                ExpireAt = expireAt,
            });
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
                RefreshAllHuds();
            }
        }

        private void PruneExpiredMessages()
        {
            // Keep the active window records after they expire so the next message can
            // show the previous window again. Expiration only hides the UI.
        }

        private ChatRecord CreateRecord(Player sender, ChatChannel channel, string content)
        {
            return new ChatRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                UtcTime = DateTime.UtcNow,
                Channel = channel,
                SteamId = GetPlayerKey(sender),
                Name = sender.Nickname ?? string.Empty,
                Team = sender.Role.Team.ToString(),
                TeamColor = GetTeamColor(sender.Role.Team),
                Role = sender.Role.Type.ToString(),
                RoleName = PluginHelper.GetChineseRoleName(sender.Role.Type),
                Content = content,
            };
        }

        private void AddRecord(ChatRecord record)
        {
            if (record.Channel == ChatChannel.Global)
                _database.Global.Add(record);
            else
                _database.Team.Add(record);

            TrimStoredMessages(_database.Global);
            TrimStoredMessages(_database.Team);
            SaveData();
        }

        private void TrimStoredMessages(List<ChatRecord> records)
        {
            int max = Math.Max(1, _config.ChatConfig.MaxStoredMessages);
            if (records.Count <= max)
                return;

            records.RemoveRange(0, records.Count - max);
        }

        private string RenderTemplate(string template, ChatRecord record)
        {
            if (string.IsNullOrWhiteSpace(template))
                template = "<size=16>[{channel}][<color={team_color}>{role_name}</color>][{name}]: {content}</size>";

            return template
                .Replace("{channel}", record.Channel == ChatChannel.Global ? "全体" : "团队")
                .Replace("{team_color}", record.TeamColor ?? "#FFFFFF")
                .Replace("{role_color}", record.TeamColor ?? "#FFFFFF")
                .Replace("{rolecolor}", record.TeamColor ?? "#FFFFFF")
                .Replace("{role_name}", record.RoleName ?? string.Empty)
                .Replace("{role}", record.Role ?? string.Empty)
                .Replace("{team}", record.Team ?? string.Empty)
                .Replace("{steamid}", record.SteamId ?? string.Empty)
                .Replace("{name}", record.Name ?? string.Empty)
                .Replace("{content}", record.Content ?? string.Empty);
        }

        private string SanitizeContent(string rawContent)
        {
            string content = rawContent ?? string.Empty;
            content = RichTextTagRegex.Replace(content, string.Empty);
            content = content.Replace("\r", " ").Replace("\n", " ").Trim();
            return content;
        }

        private string GetMatchedBlockedWord(string content)
        {
            List<string> blockedWords = _blockedWords;
            if (blockedWords == null || blockedWords.Count == 0)
                return string.Empty;

            foreach (string word in blockedWords)
            {
                if (string.IsNullOrWhiteSpace(word))
                    continue;

                if (content.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    return word;
            }

            return string.Empty;
        }

        private float GetVisibleSeconds(ChatChannel channel)
        {
            float seconds = channel == ChatChannel.Global
                ? _config.ChatConfig.GlobalChatVisibleSeconds
                : _config.ChatConfig.TeamChatVisibleSeconds;

            return Math.Max(1f, seconds);
        }

        private string GetTeamColor(Team team)
        {
            List<Config.TeamChatColor> colors = _config.ChatConfig.TeamColors;
            if (colors != null)
            {
                string teamName = team.ToString();
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

        private void LoadData()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_chatLogFilePath));

                if (!File.Exists(_chatLogFilePath))
                {
                    SaveData();
                    return;
                }

                string json = File.ReadAllText(_chatLogFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                    _database = JsonConvert.DeserializeObject<ChatDatabase>(json) ?? new ChatDatabase();

                NormalizeDatabase();
                SaveData();
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to load chat data: {ex.Message}");
                _database = new ChatDatabase();
                SaveData();
            }
        }

        private void SaveData()
        {
            try
            {
                NormalizeDatabase();
                Directory.CreateDirectory(Path.GetDirectoryName(_chatLogFilePath));
                File.WriteAllText(_chatLogFilePath, JsonConvert.SerializeObject(_database, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to save chat data: {ex}");
            }
        }

        private void LoadBlockedWords()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_blockedWordsFilePath));

                if (!File.Exists(_blockedWordsFilePath))
                {
                    _blockedWords = NormalizeBlockedWords(_config.ChatConfig.BlockedWords);
                    SaveBlockedWords();
                    return;
                }

                string json = File.ReadAllText(_blockedWordsFilePath);
                _blockedWords = string.IsNullOrWhiteSpace(json)
                    ? new List<string>()
                    : NormalizeBlockedWords(JsonConvert.DeserializeObject<List<string>>(json));

                SaveBlockedWords();
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to load blocked words: {ex.Message}");
                _blockedWords = new List<string>();
                SaveBlockedWords();
            }
        }

        private void SaveBlockedWords()
        {
            try
            {
                _blockedWords = NormalizeBlockedWords(_blockedWords);
                Directory.CreateDirectory(Path.GetDirectoryName(_blockedWordsFilePath));
                File.WriteAllText(_blockedWordsFilePath, JsonConvert.SerializeObject(_blockedWords, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to save blocked words: {ex}");
            }
        }

        private static List<string> NormalizeBlockedWords(IEnumerable<string> blockedWords)
        {
            if (blockedWords == null)
                return new List<string>();

            return blockedWords
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Select(word => word.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void NormalizeDatabase()
        {
            if (_database == null)
                _database = new ChatDatabase();

            if (_database.Global == null)
                _database.Global = new List<ChatRecord>();

            if (_database.Team == null)
                _database.Team = new List<ChatRecord>();
        }

        private string GetDataFilePath(string configuredFileName, string defaultFileName)
        {
            string fileName = string.IsNullOrWhiteSpace(configuredFileName)
                ? defaultFileName
                : configuredFileName;

            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "EXILED", "Configs", fileName);
        }

        private static string JoinArguments(ArraySegment<string> arguments)
        {
            return arguments == null || arguments.Count == 0
                ? string.Empty
                : string.Join(" ", arguments);
        }

        private static string GetPanelId(string playerKey)
        {
            return "chat_hud_" + playerKey.Replace("@", "_").Replace(".", "_").Replace(":", "_");
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

        [CommandHandler(typeof(ClientCommandHandler))]
        public class BroadcastChatCommand : ICommand
        {
            public string Command => "bc";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Send a global UI chat message.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                Player player = Player.Get(sender);
                if (Instance == null)
                {
                    response = "Chat module is not enabled.";
                    return false;
                }

                return Instance.SendChat(player, ChatChannel.Global, JoinArguments(arguments), out response);
            }
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class TeamChatCommand : ICommand
        {
            public string Command => "c";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Send a team UI chat message.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                Player player = Player.Get(sender);
                if (Instance == null)
                {
                    response = "Chat module is not enabled.";
                    return false;
                }

                return Instance.SendChat(player, ChatChannel.Team, JoinArguments(arguments), out response);
            }
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class ChatUiTestCommand : ICommand
        {
            public string Command => "chatuitest";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Show a local chat UI test message.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                Player player = Player.Get(sender);
                if (Instance == null)
                {
                    response = "Chat module is not enabled.";
                    return false;
                }

                return Instance.SendUiTest(player, out response);
            }
        }

        public enum ChatChannel
        {
            Global,
            Team,
        }

        private class ActiveChatMessage
        {
            public ChatRecord Record { get; set; }
            public string RenderedText { get; set; } = string.Empty;
            public DateTime ExpireAt { get; set; }
        }

        private class ChatDatabase
        {
            public List<ChatRecord> Global { get; set; } = new List<ChatRecord>();
            public List<ChatRecord> Team { get; set; } = new List<ChatRecord>();
        }

        private class ChatRecord
        {
            public string Id { get; set; } = string.Empty;
            public DateTime UtcTime { get; set; }
            public ChatChannel Channel { get; set; }
            public string SteamId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Team { get; set; } = string.Empty;
            public string TeamColor { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string RoleName { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }
    }
}
