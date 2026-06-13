using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class SurrenderModule : ModuleBase
    {
        public override string Name => "Surrender Module";

        private Config _config;

        public SurrenderModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            if (!_config.SurrenderModuleConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }
            Log.Info($"[{Name}] Enabled.");
            try
            {
                CustomPlayerEvents.Handcuffing += OnHandcuffing;
                CustomPlayerEvents.Escaping += OnEscaping;  // 添加逃离事件  
                Log.Info($"[{Name}] Event handlers registered.");
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
                CustomPlayerEvents.Handcuffing -= OnHandcuffing;
                CustomPlayerEvents.Escaping -= OnEscaping;  // 移除逃离事件  
                Log.Info($"[{Name}] Event handlers unregistered.");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to unregister event handlers: {ex}");
            }
        }

        private void OnEscaping(EscapingEventArgs ev)
        {
            if (!ev.Player.IsCuffed)
                return;

            Player cuffer = ev.Player.Cuffer;
            if (cuffer is null)
                return;

            // 排除SCP阵营  
            if (cuffer.Role.Team == Team.SCPs)
                return;

            // 根据捆绑者的阵营设置对应阵营的最低等角色  
            ev.NewRole = cuffer.Role.Team switch
            {
                Team.ChaosInsurgency => RoleTypeId.ChaosConscript,  // 混沌分裂者最低等  
                Team.FoundationForces => RoleTypeId.NtfPrivate,      // 九尾狐最低等  
                Team.Scientists => RoleTypeId.NtfPrivate,           // 科学家盟友 -> MTF最低等  
                Team.ClassD => RoleTypeId.ChaosConscript,           // D级人员盟友 -> Chaos最低等  
                _ => ev.NewRole  // 其他阵营保持原样  
            };
        }

        private void OnHandcuffing(HandcuffingEventArgs ev)
        {
            
        }
    }
}
