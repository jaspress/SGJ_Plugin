using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;
using Firearm = Exiled.API.Features.Items.Firearm;

namespace SGJ_Plugin.Modules
{
    public class InfiniteAmmoModule : ModuleBase
    {
        public override string Name => "Infinite Ammo Module";

        private readonly Config _config;

        public InfiniteAmmoModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            try
            {
                CustomPlayerEvents.DroppingAmmo += OnDroppingAmmo;
                CustomPlayerEvents.ChangingItem += OnChangingItem;
                CustomPlayerEvents.ReloadingWeapon += OnReloadingWeapon;
                CustomPlayerEvents.ReloadedWeapon += OnReloadedWeapon;

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
                CustomPlayerEvents.DroppingAmmo -= OnDroppingAmmo;
                CustomPlayerEvents.ChangingItem -= OnChangingItem;
                CustomPlayerEvents.ReloadingWeapon -= OnReloadingWeapon;
                CustomPlayerEvents.ReloadedWeapon -= OnReloadedWeapon;

                Log.Info($"[{Name}] Event handlers unregistered.");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to unregister event handlers: {ex}");
            }
        }

        private void OnDroppingAmmo(DroppingAmmoEventArgs ev)
        {
            ev.IsAllowed = false;
        }

        private void OnReloadedWeapon(ReloadedWeaponEventArgs ev)
        {
            if (ev.Firearm.Type == ItemType.ParticleDisruptor)
                return;

            ev.Player.SetAmmo(ev.Firearm.AmmoType, (ushort)(ev.Firearm.MaxMagazineAmmo));
        }

        private void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.Item == null || ev.Item.Type == ItemType.None)
                return;

            if (ev.Item is Firearm firearm)
            {
                ev.Player.SetAmmo(firearm.AmmoType, (ushort)(firearm.MaxMagazineAmmo));
            }
        }

        private void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
        {
            if (ev.Firearm.Type == ItemType.ParticleDisruptor)
                return;

            ev.Player.SetAmmo(ev.Firearm.AmmoType, (ushort)(ev.Firearm.MaxMagazineAmmo));
        }
    }
}
