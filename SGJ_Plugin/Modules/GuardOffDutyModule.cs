using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using PlayerRoles;
using System;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class GuardOffDutyModule : ModuleBase
    {
        public override string Name => "保安下班模块";

        private Config _config;

        /// <summary>
        /// 设施安保角色类型
        /// </summary>
        private static readonly RoleTypeId GuardRole = RoleTypeId.FacilityGuard;

        public GuardOffDutyModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            try
            {
                // 订阅玩家逃离进行时事件（在逃离发生时，可以修改新角色）
                CustomPlayerEvents.Escaping += OnPlayerEscaping;

                Log.Info($"[{Name}] 已启用");

                if (_config.Debug)
                {
                    Log.Debug($"[{Name}] 设施安保逃离后角色: {_config.GuardOffDutyConfig.EscapeRole}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] 启用时出错: {ex.Message}");
                throw;
            }
        }

        protected override void OnDisable()
        {
            try
            {
                // 取消订阅玩家逃离事件
                CustomPlayerEvents.Escaping -= OnPlayerEscaping;

                Log.Info($"[{Name}] 已禁用");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] 禁用时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理玩家逃离进行时事件 (Escaping)
        /// 在这个阶段可以修改 ev.NewRole 来改变玩家逃离后的角色
        /// </summary>
        private void OnPlayerEscaping(EscapingEventArgs ev)
        {
            // 参数验证
            if (ev == null || ev.Player == null || !IsEnabled)
                return;

            // 功能开关检查
            if (!_config.GuardOffDutyConfig.IsEnabled)
                return;

            try
            {
                string playerName = ev.Player.Nickname;
                string playerId = ev.Player.UserId;

                // 检查玩家当前角色是否是设施安保
                if (ev.Player.Role != GuardRole)
                {
                    if (_config.Debug)
                        Log.Debug($"[{Name}] 玩家 {playerName} 不是设施安保（当前角色: {ev.Player.Role}），跳过处理");
                    return;
                }

                // 记录原始信息
                RoleTypeId originalRole = ev.Player.Role;
                RoleTypeId originalNewRole = ev.NewRole;
                RoleTypeId targetRole = _config.GuardOffDutyConfig.EscapeRole;

                // 修改新角色为配置指定的角色
                ev.NewRole = targetRole;
                ev.IsAllowed = true;
                if (_config.Debug)
                {
                    Log.Debug($"[{Name}] 玩家 '{playerName}' 逃离时角色转换: {originalRole} → {ev.NewRole}");
                    Log.Debug($"[{Name}] =============== 保安下班事件 ===============");
                    Log.Debug($"[{Name}] 玩家昵称: {playerName}");
                    Log.Debug($"[{Name}] 玩家ID: {playerId}");
                    Log.Debug($"[{Name}] 原始角色: {originalRole}");
                    Log.Debug($"[{Name}] 转换前新角色: {originalNewRole}");
                    Log.Debug($"[{Name}] 转换后新角色: {ev.NewRole}");
                    Log.Debug($"[{Name}] 处理时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
                    Log.Debug($"[{Name}] ==========================================");
                }
                else
                {
                    Log.Info($"[{Name}] 保安 '{playerName}' 已转换为 {ev.NewRole}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] 处理逃离事件时出错: {ex.Message}");
            }
        }
    }
}
