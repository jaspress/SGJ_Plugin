using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.Modules
{
    public class ScpSwapModule : ModuleBase
    {
        private readonly Config _config;
        private readonly Dictionary<string, SwapRequest> _requestsByTarget = new Dictionary<string, SwapRequest>();

        public static ScpSwapModule Instance { get; private set; }
        public override string Name => "SCP Swap Module";

        public ScpSwapModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            Instance = this;
            if (!_config.ScpSwapConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            CustomPlayerEvents.Spawned += OnSpawned;
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Spawned -= OnSpawned;
            _requestsByTarget.Clear();

            if (Instance == this)
                Instance = null;
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev?.Player == null || !IsAllowedScp(ev.Player.Role.Type))
                return;

            Timing.CallDelayed(1f, () => PluginHelper.ShowTopRightHint(ev.Player, FormatSwapHint(_config.ScpSwapConfig.SpawnHintText), 8f));
        }

        public bool RequestSwap(Player requester, string targetRoleQuery, out string response)
        {
            response = string.Empty;
            if (requester == null)
            {
                response = "Only players can use this command.";
                return false;
            }

            if (!IsAllowedScp(requester.Role.Type))
            {
                response = "你当前不是可交换的有害 SCP。";
                return false;
            }

            Player target = FindTargetScp(requester, targetRoleQuery);
            if (target == null)
            {
                response = "未找到匹配的可交换 SCP。";
                return false;
            }

            SwapRequest request = new SwapRequest
            {
                Requester = requester,
                Target = target,
                RequesterRole = requester.Role.Type,
                TargetRole = target.Role.Type,
                ExpiresAt = DateTime.UtcNow.AddSeconds(Math.Max(1f, _config.ScpSwapConfig.RequestTimeoutSeconds)),
            };

            _requestsByTarget[GetPlayerKey(target)] = request;
            ShowRequestHints(request);
            response = "交换请求已发送。";
            return true;
        }

        public bool Answer(Player target, bool accept, out string response)
        {
            response = string.Empty;
            if (target == null)
            {
                response = "Only players can use this command.";
                return false;
            }

            string key = GetPlayerKey(target);
            if (!_requestsByTarget.TryGetValue(key, out SwapRequest request) || request.ExpiresAt <= DateTime.UtcNow)
            {
                _requestsByTarget.Remove(key);
                PluginHelper.ShowTopRightHint(target, _config.ScpSwapConfig.NoRequestText, 3f);
                response = "没有待处理请求。";
                return false;
            }

            _requestsByTarget.Remove(key);
            if (!accept)
            {
                PluginHelper.ShowTopRightHint(target, _config.ScpSwapConfig.DeniedText, 3f);
                PluginHelper.ShowTopRightHint(request.Requester, _config.ScpSwapConfig.DeniedText, 3f);
                response = "已拒绝。";
                return true;
            }

            if (request.Requester == null || request.Target == null || !IsAllowedScp(request.Requester.Role.Type) || !IsAllowedScp(request.Target.Role.Type))
            {
                response = "交换失败，双方角色状态已变化。";
                return false;
            }

            request.Requester.Role.Set(request.TargetRole, SpawnReason.ForceClass);
            request.Target.Role.Set(request.RequesterRole, SpawnReason.ForceClass);
            PluginHelper.ShowTopRightHint(request.Requester, _config.ScpSwapConfig.AcceptedText, 3f);
            PluginHelper.ShowTopRightHint(request.Target, _config.ScpSwapConfig.AcceptedText, 3f);
            response = "已交换。";
            return true;
        }

        private void ShowRequestHints(SwapRequest request)
        {
            int seconds = Math.Max(1, (int)Math.Ceiling((request.ExpiresAt - DateTime.UtcNow).TotalSeconds));
            string sent = FormatSwapHint(_config.ScpSwapConfig.RequestSentText ?? string.Empty)
                .Replace("{target}", request.Target.Nickname ?? string.Empty)
                .Replace("{role}", GetScpDisplayName(request.TargetRole))
                .Replace("{seconds}", seconds.ToString());
            string received = FormatSwapHint(_config.ScpSwapConfig.RequestReceivedText ?? string.Empty)
                .Replace("{requester}", request.Requester.Nickname ?? string.Empty)
                .Replace("{requester_role}", GetScpDisplayName(request.RequesterRole))
                .Replace("{your_role}", GetScpDisplayName(request.TargetRole))
                .Replace("{seconds}", seconds.ToString());

            PluginHelper.ShowTopRightHint(request.Requester, sent, seconds);
            PluginHelper.ShowTopRightHint(request.Target, received, seconds);
            Timing.CallDelayed(seconds, () => ExpireRequest(request));
        }

        private void ExpireRequest(SwapRequest request)
        {
            if (request?.Target == null)
                return;

            string key = GetPlayerKey(request.Target);
            if (!_requestsByTarget.TryGetValue(key, out SwapRequest current) || current != request)
                return;

            _requestsByTarget.Remove(key);
            PluginHelper.ShowTopRightHint(request.Requester, _config.ScpSwapConfig.TimeoutText, 3f);
            PluginHelper.ShowTopRightHint(request.Target, _config.ScpSwapConfig.TimeoutText, 3f);
        }

        private Player FindTargetScp(Player requester, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            string normalized = Normalize(query);
            return Player.List
                .Where(player => player != null && player != requester && IsAllowedScp(player.Role.Type))
                .FirstOrDefault(player => Normalize(GetScpDisplayName(player.Role.Type)).Contains(normalized)
                    || Normalize(player.Role.Type.ToString()).Contains(normalized)
                    || Normalize(player.Nickname).Contains(normalized));
        }

        private bool IsAllowedScp(RoleTypeId role)
        {
            List<string> roles = _config.ScpSwapConfig.AllowedScpRoles;
            return roles != null && roles.Any(value => string.Equals(value, role.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        private static string GetScpDisplayName(RoleTypeId role)
        {
            switch (role)
            {
                case RoleTypeId.Scp049: return "SCP-049";
                case RoleTypeId.Scp079: return "SCP-079";
                case RoleTypeId.Scp096: return "SCP-096";
                case RoleTypeId.Scp106: return "SCP-106";
                case RoleTypeId.Scp173: return "SCP-173";
                case RoleTypeId.Scp939: return "SCP-939";
                case RoleTypeId.Scp3114: return "SCP-3114";
                default: return role.ToString();
            }
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
        }

        private static string FormatSwapHint(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text
                .Replace("可使用 <color", "可使用：\n<color")
                .Replace("可使用：<color", "可使用：\n<color")
                .Replace(" 同意：", "\n\n同意：")
                .Replace(" 拒绝：", "\n拒绝：")
                .Replace("</color> 请求", "</color>\n请求")
                .Replace("</color> 拒绝", "</color>\n拒绝");
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

        [CommandHandler(typeof(ClientCommandHandler))]
        public class SwapCommand : ICommand
        {
            public string Command => "swap";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Request or answer harmful SCP role swap.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                Player player = Player.Get(sender);
                if (Instance == null)
                {
                    response = "SCP swap module is not enabled.";
                    return false;
                }

                if (arguments.Count == 1 && string.Equals(arguments.At(0), "al", StringComparison.OrdinalIgnoreCase))
                    return Instance.Answer(player, true, out response);

                if (arguments.Count == 1 && string.Equals(arguments.At(0), "nal", StringComparison.OrdinalIgnoreCase))
                    return Instance.Answer(player, false, out response);

                if (arguments.Count < 1)
                {
                    response = "用法: .swap [SCP名字] / .swap al / .swap nal";
                    return false;
                }

                return Instance.RequestSwap(player, string.Join(" ", arguments), out response);
            }
        }

        private class SwapRequest
        {
            public Player Requester { get; set; }
            public Player Target { get; set; }
            public RoleTypeId RequesterRole { get; set; }
            public RoleTypeId TargetRole { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
