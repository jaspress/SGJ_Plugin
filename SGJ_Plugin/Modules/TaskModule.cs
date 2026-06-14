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
    public class TaskModule : ModuleBase
    {
        private readonly Config _config;
        private readonly Dictionary<string, PlayerTaskState> _tasks = new Dictionary<string, PlayerTaskState>();

        public override string Name => "Task Module";

        public TaskModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            if (!_config.TaskSystemConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            CustomPlayerEvents.Spawned += OnSpawned;
            CustomPlayerEvents.Left += OnLeft;
            CustomPlayerEvents.Died += OnDied;
            CustomPlayerEvents.Escaped += OnEscaped;

            foreach (Player player in Player.List)
                AssignTaskDelayed(player);
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Spawned -= OnSpawned;
            CustomPlayerEvents.Left -= OnLeft;
            CustomPlayerEvents.Died -= OnDied;
            CustomPlayerEvents.Escaped -= OnEscaped;
            _tasks.Clear();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            AssignTaskDelayed(ev.Player);
        }

        private void OnLeft(LeftEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            _tasks.Remove(GetPlayerKey(ev.Player));
        }

        private void OnDied(DiedEventArgs ev)
        {
            Player attacker = ev?.Attacker;
            Player victim = ev?.Player;
            if (attacker == null || victim == null || attacker == victim)
                return;

            ProgressKillTask(attacker, victim);
        }

        private void OnEscaped(EscapedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            ProgressTask(ev.Player, "Escape", 1);
        }

        private void AssignTaskDelayed(Player player)
        {
            if (player == null || !_config.TaskSystemConfig.AssignOnSpawn)
                return;

            Timing.CallDelayed(Math.Max(0f, _config.TaskSystemConfig.AssignDelaySeconds), () => AssignTask(player));
        }

        private void AssignTask(Player player)
        {
            if (player == null || !_config.TaskSystemConfig.IsEnabled)
                return;

            Config.SpecialRoleDefinition role = SpecialContentModule.Instance?.GetAssignedRole(player);
            Config.TaskDefinition task = PickTask(role, player.Role.Type);
            if (task == null)
                return;

            _tasks[GetPlayerKey(player)] = new PlayerTaskState
            {
                Task = task,
                Progress = 0,
                Completed = false,
            };

            if (_config.TaskSystemConfig.ShowAssignedHint)
                ShowTaskHint(player, _config.TaskSystemConfig.AssignedHintText, task, 0);
        }

        private Config.TaskDefinition PickTask(Config.SpecialRoleDefinition role, RoleTypeId vanillaRole)
        {
            List<Config.TaskDefinition> tasks = _config.TaskSystemConfig.Tasks ?? new List<Config.TaskDefinition>();
            Config.TaskDefinition roleTask = tasks.FirstOrDefault(task =>
                task != null
                && task.IsEnabled
                && role != null
                && !string.IsNullOrWhiteSpace(task.MatchRoleName)
                && string.Equals(task.MatchRoleName, role.Name, StringComparison.OrdinalIgnoreCase));

            if (roleTask != null)
                return roleTask;

            Config.TaskDefinition campTask = tasks.FirstOrDefault(task =>
                task != null
                && task.IsEnabled
                && role != null
                && !string.IsNullOrWhiteSpace(task.MatchCamp)
                && string.Equals(task.MatchCamp, role.Camp, StringComparison.OrdinalIgnoreCase));

            if (campTask != null)
                return campTask;

            string fallbackCamp = GetVanillaCamp(vanillaRole);
            return tasks.FirstOrDefault(task =>
                task != null
                && task.IsEnabled
                && !string.IsNullOrWhiteSpace(task.MatchCamp)
                && string.Equals(task.MatchCamp, fallbackCamp, StringComparison.OrdinalIgnoreCase));
        }

        private void ProgressKillTask(Player attacker, Player victim)
        {
            PlayerTaskState state = GetTask(attacker);
            if (state == null || state.Completed)
                return;

            string type = state.Task.TaskType ?? string.Empty;
            if (type.Equals("KillPlayers", StringComparison.OrdinalIgnoreCase))
            {
                ProgressTask(attacker, type, 1);
                return;
            }

            if (type.Equals("KillScps", StringComparison.OrdinalIgnoreCase) && victim.Role.Team == Team.SCPs)
            {
                ProgressTask(attacker, type, 1);
                return;
            }

            if (type.Equals("KillHumans", StringComparison.OrdinalIgnoreCase) && victim.Role.Team != Team.SCPs && victim.Role.Type != RoleTypeId.Spectator)
                ProgressTask(attacker, type, 1);
        }

        private void ProgressTask(Player player, string taskType, int amount)
        {
            PlayerTaskState state = GetTask(player);
            if (state == null || state.Completed)
                return;

            if (!string.Equals(state.Task.TaskType, taskType, StringComparison.OrdinalIgnoreCase))
                return;

            state.Progress = Math.Min(Math.Max(1, state.Task.TargetCount), state.Progress + Math.Max(1, amount));
            if (state.Progress >= Math.Max(1, state.Task.TargetCount))
            {
                CompleteTask(player, state);
                return;
            }

            if (_config.TaskSystemConfig.ShowProgressHint)
                ShowTaskHint(player, _config.TaskSystemConfig.ProgressHintText, state.Task, state.Progress);
        }

        private void CompleteTask(Player player, PlayerTaskState state)
        {
            state.Completed = true;
            int reward = Math.Max(0, state.Task.RewardExperience);
            if (reward > 0)
                LevelModule.Instance?.AwardExperience(player, reward, "Task");

            if (_config.TaskSystemConfig.ShowCompletedHint)
                ShowTaskHint(player, _config.TaskSystemConfig.CompletedHintText, state.Task, Math.Max(1, state.Task.TargetCount));
        }

        private PlayerTaskState GetTask(Player player)
        {
            if (player == null)
                return null;

            return _tasks.TryGetValue(GetPlayerKey(player), out PlayerTaskState state) ? state : null;
        }

        private void ShowTaskHint(Player player, string template, Config.TaskDefinition task, int progress)
        {
            string text = RenderTaskTemplate(template, task, progress);
            PluginHelper.ShowTopRightHint(player, text, 4f);
        }

        private static string RenderTaskTemplate(string template, Config.TaskDefinition task, int progress)
        {
            int target = Math.Max(1, task?.TargetCount ?? 1);
            return (template ?? string.Empty)
                .Replace("{task_name}", task?.Name ?? string.Empty)
                .Replace("{description}", task?.Description ?? string.Empty)
                .Replace("{progress}", Math.Max(0, progress).ToString())
                .Replace("{target}", target.ToString())
                .Replace("{reward_xp}", Math.Max(0, task?.RewardExperience ?? 0).ToString());
        }

        private static string GetVanillaCamp(RoleTypeId role)
        {
            switch (role)
            {
                case RoleTypeId.ClassD:
                    return "D级人员";
                case RoleTypeId.Scientist:
                    return "科研人员";
                case RoleTypeId.FacilityGuard:
                    return "安保人员";
                case RoleTypeId.ChaosConscript:
                case RoleTypeId.ChaosMarauder:
                case RoleTypeId.ChaosRepressor:
                case RoleTypeId.ChaosRifleman:
                    return "混沌分裂者";
                case RoleTypeId.Scp049:
                case RoleTypeId.Scp0492:
                case RoleTypeId.Scp079:
                case RoleTypeId.Scp096:
                case RoleTypeId.Scp106:
                case RoleTypeId.Scp173:
                case RoleTypeId.Scp939:
                case RoleTypeId.Scp3114:
                    return "SCP";
                default:
                    return "九尾狐小队";
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

        private class PlayerTaskState
        {
            public Config.TaskDefinition Task { get; set; }
            public int Progress { get; set; }
            public bool Completed { get; set; }
        }
    }
}
