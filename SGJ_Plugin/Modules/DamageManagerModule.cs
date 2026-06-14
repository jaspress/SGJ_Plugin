using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using SGJ_Plugin.SpecialContent.Base;
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
                ev.DamageHandler.Damage = 0; // Prevent damage
                ev.IsAllowed = false;
                return;
            }

            ApplySpecialRoleResistance(ev);
        }

        private void ApplySpecialRoleResistance(HurtingEventArgs ev)
        {
            CustomRoleBase role = SpecialContentModule.Instance?.GetAssignedRole(ev?.Player);
            if (role == null || ev?.DamageHandler == null || ev.DamageHandler.Damage <= 0f)
                return;

            if (ev.DamageHandler.Type != Exiled.API.Enums.DamageType.Firearm)
                return;

            int resistance = GetBulletResistance(role, ev.DamageHandler);
            if (resistance <= 0)
                return;

            resistance = Math.Max(0, Math.Min(95, resistance));
            ev.DamageHandler.Damage *= 1f - (resistance / 100f);
        }

        private static int GetBulletResistance(CustomRoleBase role, object damageHandler)
        {
            string hitbox = GetHitboxName(damageHandler);
            if (hitbox.Contains("Head"))
                return role.BulletResistanceHead;
            if (hitbox.Contains("Arm") || hitbox.Contains("Hand"))
                return role.BulletResistanceArm;
            if (hitbox.Contains("Leg") || hitbox.Contains("Foot"))
                return role.BulletResistanceLeg;

            return role.BulletResistanceBody;
        }

        private static string GetHitboxName(object damageHandler)
        {
            if (damageHandler == null)
                return string.Empty;

            System.Type type = damageHandler.GetType();
            System.Reflection.PropertyInfo property = type.GetProperty("Hitbox") ?? type.GetProperty("HitboxType");
            object value = property?.GetValue(damageHandler, null);
            return value?.ToString() ?? string.Empty;
        }

        private void OnHurt(HurtEventArgs ev) { }
    }
}
