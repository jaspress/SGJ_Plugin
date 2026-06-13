using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class GuardOffDutyModule : ModuleBase
    {
        public override string Name => "Guard Off Duty Module";

        private readonly Config _config;

        /// <summary>
        /// Facility Guard role that is converted when escaping.
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
                CustomPlayerEvents.Escaping += OnPlayerEscaping;

                Log.Info($"[{Name}] Event handlers registered.");

                if (_config.Debug)
                {
                    Log.Debug($"[{Name}] Guard escape role: {_config.GuardOffDutyConfig.EscapeRole}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to register event handlers: {ex}");
                throw;
            }
        }

        protected override void OnDisable()
        {
            try
            {
                CustomPlayerEvents.Escaping -= OnPlayerEscaping;

                Log.Info($"[{Name}] Event handlers unregistered.");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to unregister event handlers: {ex}");
            }
        }

        /// <summary>
        /// Converts escaping Facility Guards to the role configured by the server owner.
        /// </summary>
        private void OnPlayerEscaping(EscapingEventArgs ev)
        {
            if (ev == null || ev.Player == null || !IsEnabled)
                return;

            if (!_config.GuardOffDutyConfig.IsEnabled)
                return;

            try
            {
                string playerName = ev.Player.Nickname;
                string playerId = ev.Player.UserId;

                if (ev.Player.Role != GuardRole)
                {
                    if (_config.Debug)
                        Log.Debug($"[{Name}] Skipped {playerName}; current role is {ev.Player.Role}, not {GuardRole}.");
                    return;
                }

                RoleTypeId originalRole = ev.Player.Role;
                RoleTypeId originalNewRole = ev.NewRole;
                RoleTypeId targetRole = _config.GuardOffDutyConfig.EscapeRole;

                ev.NewRole = targetRole;
                ev.IsAllowed = true;
                if (_config.Debug)
                {
                    Log.Debug($"[{Name}] Converted escaping guard '{playerName}': {originalRole} -> {ev.NewRole}");
                    Log.Debug($"[{Name}] =============== Guard Off Duty Event ===============");
                    Log.Debug($"[{Name}] Player name: {playerName}");
                    Log.Debug($"[{Name}] Player ID: {playerId}");
                    Log.Debug($"[{Name}] Original role: {originalRole}");
                    Log.Debug($"[{Name}] Original escape role: {originalNewRole}");
                    Log.Debug($"[{Name}] Final escape role: {ev.NewRole}");
                    Log.Debug($"[{Name}] Processed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
                    Log.Debug($"[{Name}] ==========================================");
                }
                else
                {
                    Log.Info($"[{Name}] Converted guard '{playerName}' to {ev.NewRole}.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to handle escaping event: {ex}");
            }
        }
    }
}
