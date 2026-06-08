using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using InventorySystem.Items.Firearms;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;
using Firearm = Exiled.API.Features.Items.Firearm;

namespace SGJ_Plugin.Modules
{
    public class InfiniteAmmoModule : ModuleBase
    {
        public override string Name => "枪械无限子弹模块";

        private Config _config;
        public InfiniteAmmoModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            try
            {
                // 订阅玩家装弹事件
                CustomPlayerEvents.DroppingAmmo += (ev) => ev.IsAllowed = false; // 禁止丢弃弹药
                CustomPlayerEvents.ChangingItem += OnChangingItem;
                CustomPlayerEvents.ReloadingWeapon += OnReloadingWeapon;
                CustomPlayerEvents.ReloadedWeapon += OnReloadedWeapon;

                Log.Info($"[{Name}] 已启用");

                if (_config.Debug)
                {
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
                // 取消订阅玩家装弹事件
                CustomPlayerEvents.DroppingAmmo -= (ev) => ev.IsAllowed = false;
                CustomPlayerEvents.ChangingItem -= OnChangingItem;
                CustomPlayerEvents.ReloadingWeapon -= OnReloadingWeapon;
                CustomPlayerEvents.ReloadedWeapon -= OnReloadedWeapon;

                Log.Info($"[{Name}] 已禁用");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] 禁用时出错: {ex.Message}");
            }
        }
        private void OnReloadedWeapon(ReloadedWeaponEventArgs ev)
        {
            if (ev.Firearm.Type == ItemType.ParticleDisruptor) return; // 排除粒子干扰器等特殊武器
            ev.Player.SetAmmo(ev.Firearm.AmmoType, (ushort)(ev.Firearm.MaxMagazineAmmo));
        }
        private void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.Item == null || ev.Item.Type == ItemType.None) return;
            if (ev.Item is Firearm firearm)
            {
                ev.Player.SetAmmo(firearm.AmmoType, (ushort)(firearm.MaxMagazineAmmo));
            }
        }
        private void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
        {
            if (ev.Firearm.Type == ItemType.ParticleDisruptor) return; // 排除粒子干扰器等特殊武器
            ev.Player.SetAmmo(ev.Firearm.AmmoType, (ushort)(ev.Firearm.MaxMagazineAmmo));
        }
    }
}
