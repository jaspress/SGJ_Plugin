using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;

namespace SGJ_Plugin.Modules
{
    public class DamageManagerModule : ModuleBase
    {
        public override string Name => "Damage Manager Module";

        private readonly Config _config;

        public DamageManagerModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            try
            {
                CustomPlayerEvents.DamagingDoor += OnDamagingDoor;
                CustomPlayerEvents.DamagingShootingTarget += OnDamagingShootingTarget;
                CustomPlayerEvents.DamagingWindow += OnDamagingWindow;
                CustomPlayerEvents.Hurting += OnHurting;
                CustomPlayerEvents.Hurt += OnHurt;
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
                CustomPlayerEvents.DamagingDoor -= OnDamagingDoor;
                CustomPlayerEvents.DamagingShootingTarget -= OnDamagingShootingTarget;
                CustomPlayerEvents.DamagingWindow -= OnDamagingWindow;
                CustomPlayerEvents.Hurting -= OnHurting;
                CustomPlayerEvents.Hurt -= OnHurt;
                Log.Info($"[{Name}] Event handlers unregistered.");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to unregister event handlers: {ex}");
            }
        }

        private void OnDamagingDoor(DamagingDoorEventArgs ev) { }
        private void OnDamagingShootingTarget(DamagingShootingTargetEventArgs ev) { }
        private void OnDamagingWindow(DamagingWindowEventArgs ev) { }
        private void OnHurting(HurtingEventArgs ev) 
        {
            if (_config.DamageManagerConfig.DisabledDamageTypes.Contains(ev.DamageHandler.Type)){
                Log.Debug($"[{Name}] Damage type {ev.DamageHandler.Type} is disabled.");
                ev.IsAllowed = false;
            }
        }
        private void OnHurt(HurtEventArgs ev) { }
    }
}
