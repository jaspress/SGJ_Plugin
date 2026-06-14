using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using SGJ_Plugin.SpecialContent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEvents = Exiled.Events.Handlers.Player;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.Modules
{
    public class SpecialContentModule : ModuleBase
    {
        private readonly Config _config;
        private readonly Dictionary<string, CustomRoleBase> _assignedRoles = new Dictionary<string, CustomRoleBase>();
        private readonly Dictionary<ushort, string> _specialHeldItems = new Dictionary<ushort, string>();
        private readonly Dictionary<ushort, string> _specialDroppedItems = new Dictionary<ushort, string>();
        private readonly Dictionary<string, PendingSpecialPickup> _pendingPickupItems = new Dictionary<string, PendingSpecialPickup>();
        private readonly Dictionary<string, string> _pendingDropItems = new Dictionary<string, string>();
        private readonly Random _random = new Random();

        public static SpecialContentModule Instance { get; private set; }

        public override string Name => "Special Content Module";

        public SpecialContentModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            Instance = this;

            if (!_config.SpecialContentConfig.IsEnabled)
            {
                Log.Info($"[{Name}] Disabled by config.");
                return;
            }

            CustomPlayerEvents.Spawned += OnSpawned;
            CustomPlayerEvents.Left += OnLeft;
            CustomPlayerEvents.PickingUpItem += OnPickingUpItem;
            CustomPlayerEvents.ItemAdded += OnItemAdded;
            CustomPlayerEvents.DroppingItem += OnDroppingItem;
            CustomPlayerEvents.DroppedItem += OnDroppedItem;
            CustomPlayerEvents.ChangingItem += OnChangingItem;
            CustomPlayerEvents.ConsumingItem += OnConsumingItem;

            foreach (Player player in Player.List)
                ApplySpecialRoleDelayed(player);
        }

        protected override void OnDisable()
        {
            CustomPlayerEvents.Spawned -= OnSpawned;
            CustomPlayerEvents.Left -= OnLeft;
            CustomPlayerEvents.PickingUpItem -= OnPickingUpItem;
            CustomPlayerEvents.ItemAdded -= OnItemAdded;
            CustomPlayerEvents.DroppingItem -= OnDroppingItem;
            CustomPlayerEvents.DroppedItem -= OnDroppedItem;
            CustomPlayerEvents.ChangingItem -= OnChangingItem;
            CustomPlayerEvents.ConsumingItem -= OnConsumingItem;

            foreach (Player player in Player.List)
                ResetPlayer(player);

            _assignedRoles.Clear();
            _specialHeldItems.Clear();
            _specialDroppedItems.Clear();
            _pendingPickupItems.Clear();
            _pendingDropItems.Clear();

            if (Instance == this)
                Instance = null;
        }

        public CustomRoleBase GetAssignedRole(Player player)
        {
            if (player == null)
                return null;

            return _assignedRoles.TryGetValue(GetPlayerKey(player), out CustomRoleBase role) ? role : null;
        }

        public string GetRoleName(Player player)
        {
            return GetAssignedRole(player)?.Name;
        }

        public string GetRoleColor(Player player)
        {
            return GetAssignedRole(player)?.RoleColor;
        }

        public int? GetKillExperience(Player player)
        {
            CustomRoleBase role = GetAssignedRole(player);
            if (role == null || role.KillExperience <= 0)
                return null;

            return role.KillExperience;
        }

        public bool TrySetAssignedRole(Player player, string roleName, out string response)
        {
            response = string.Empty;
            if (player == null)
            {
                response = "Player not found.";
                return false;
            }

            Config.SpecialRoleDefinition role = FindRole(roleName);
            if (role == null)
            {
                response = $"Special role not found: {roleName}";
                return false;
            }

            if (!IsRoleEnabled(role) || !IsCampEnabled(role.Camp))
            {
                response = $"Special role is disabled: {role.Name}";
                return false;
            }

            CustomRoleBase runtimeRole = SpecialContentRegistry.CreateRole(role);
            _assignedRoles[GetPlayerKey(player)] = runtimeRole;
            ApplyRole(player, runtimeRole);
            response = $"Set {player.Nickname} special role to {role.Name}.";
            return true;
        }

        public IEnumerable<string> GetRoleNames()
        {
            return (_config.SpecialContentConfig.Roles ?? new List<Config.SpecialRoleDefinition>())
                .Where(role => role != null)
                .Select(role => role.Name);
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            ApplySpecialRoleDelayed(ev.Player);
        }

        private void OnLeft(LeftEventArgs ev)
        {
            if (ev?.Player == null)
                return;

            string key = GetPlayerKey(ev.Player);
            _assignedRoles.Remove(key);
            _pendingPickupItems.Remove(key);
            _pendingDropItems.Remove(key);
        }

        private void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev?.Player == null || ev.Pickup == null)
                return;

            if (!_config.SpecialContentConfig.EnableSpecialItems)
                return;

            if (!_specialDroppedItems.TryGetValue(ev.Pickup.Serial, out string itemName))
                return;

            _pendingPickupItems[GetPlayerKey(ev.Player)] = new PendingSpecialPickup
            {
                PickupSerial = ev.Pickup.Serial,
                ItemName = itemName,
            };
        }

        private void OnItemAdded(ItemAddedEventArgs ev)
        {
            if (ev?.Player == null || ev.Item == null)
                return;

            string playerKey = GetPlayerKey(ev.Player);
            if (!_pendingPickupItems.TryGetValue(playerKey, out PendingSpecialPickup pending))
                return;

            _pendingPickupItems.Remove(playerKey);
            _specialDroppedItems.Remove(pending.PickupSerial);
            TrackHeldItem(ev.Item, pending.ItemName);

            CustomItemBase customItem = ResolveItem(pending.ItemName);
            if (customItem != null)
                PluginHelper.ShowCenterTopHint(ev.Player, customItem.RenderPickupHint(ev.Player), 3f);
        }

        private void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev?.Player == null || ev.Item == null || ev.Item.Type == ItemType.None)
                return;

            CustomItemBase customItem = ResolveItemForHeldItem(ev.Item);
            bool isSpecial = customItem?.IsSpecialItem == true;

            if (isSpecial)
                customItem.OnSelected(ev.Player, ev.Item);

            if (!_config.SpecialContentConfig.ShowItemIntroductionOnSwitch)
                return;

            string template = isSpecial
                ? _config.SpecialContentConfig.SpecialItemIntroductionText
                : _config.SpecialContentConfig.NormalItemIntroductionText;

            string text = customItem != null
                ? customItem.RenderIntroduction(template, ev.Player)
                : RenderNormalItemIntroduction(template, ev.Player, ev.Item);

            PluginHelper.ShowCenterInfoHint(ev.Player, text, _config.SpecialContentConfig.ItemIntroductionDuration);
        }

        private void OnConsumingItem(ConsumingItemEventArgs ev)
        {
            if (ev?.Player == null || ev.Item == null)
                return;

            CustomItemBase customItem = ResolveItemForHeldItem(ev.Item);
            if (customItem?.IsSpecialItem != true)
                return;

            bool applied = customItem.OnConsumed(ev, ResolveItem);
            if (!applied)
                return;

            PluginHelper.ShowCenterTopHint(
                ev.Player,
                $"<b><size=20><color=#7FFFD4>[特殊物品]</color> 使用：<color=#90EE90>{customItem.Name}</color>\n<size=18>{customItem.GetUseMessage()}</size></size></b>",
                3f);
        }

        private void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev?.Player == null || ev.Item == null)
                return;

            if (!_specialHeldItems.TryGetValue(ev.Item.Serial, out string itemName))
                return;

            _pendingDropItems[GetPlayerKey(ev.Player)] = itemName;
            _specialHeldItems.Remove(ev.Item.Serial);
        }

        private void OnDroppedItem(DroppedItemEventArgs ev)
        {
            if (ev?.Player == null || ev.Pickup == null)
                return;

            string playerKey = GetPlayerKey(ev.Player);
            if (!_pendingDropItems.TryGetValue(playerKey, out string itemName))
                return;

            _specialDroppedItems[ev.Pickup.Serial] = itemName;
            _pendingDropItems.Remove(playerKey);
        }

        private void ApplySpecialRoleDelayed(Player player)
        {
            if (player == null || !_config.SpecialContentConfig.ApplySpecialRolesOnSpawn)
                return;

            Timing.CallDelayed(0.5f, () => ApplySpecialRole(player));
        }

        private void ApplySpecialRole(Player player)
        {
            if (player == null || !_config.SpecialContentConfig.IsEnabled)
                return;

            Config.SpecialRoleDefinition role = PickRoleFor(player.Role.Type);
            if (role == null)
                return;

            CustomRoleBase runtimeRole = SpecialContentRegistry.CreateRole(role);
            _assignedRoles[GetPlayerKey(player)] = runtimeRole;
            ApplyRole(player, runtimeRole);

        }

        private Config.SpecialRoleDefinition PickRoleFor(RoleTypeId baseRole)
        {
            List<Config.SpecialRoleDefinition> candidates = (_config.SpecialContentConfig.Roles ?? new List<Config.SpecialRoleDefinition>())
                .Where(role => IsRoleEnabled(role) && IsCampEnabled(role.Camp) && role.CanUseFor(baseRole))
                .ToList();

            if (candidates.Count == 0)
                return null;

            return candidates[_random.Next(candidates.Count)];
        }

        private Config.SpecialRoleDefinition FindRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return null;

            string normalized = Normalize(roleName);
            return (_config.SpecialContentConfig.Roles ?? new List<Config.SpecialRoleDefinition>())
                .FirstOrDefault(role => role != null && Normalize(role.Name).Contains(normalized));
        }

        private bool IsRoleEnabled(Config.SpecialRoleDefinition role)
        {
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
                return false;

            Dictionary<string, bool> switches = _config.SpecialContentConfig.EnabledRoles;
            return switches == null || !switches.TryGetValue(role.Name, out bool enabled) || enabled;
        }

        private bool IsCampEnabled(string camp)
        {
            if (string.IsNullOrWhiteSpace(camp))
                return true;

            Dictionary<string, bool> switches = _config.SpecialContentConfig.EnabledCamps;
            return switches == null || !switches.TryGetValue(camp, out bool enabled) || enabled;
        }

        private bool IsItemEnabled(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return false;

            Dictionary<string, bool> switches = _config.SpecialContentConfig.EnabledItems;
            return switches == null || !switches.TryGetValue(itemName, out bool enabled) || enabled;
        }

        private void ApplyRole(Player player, CustomRoleBase role)
        {
            try
            {
                if (_config.SpecialContentConfig.ClearInventoryBeforeLoadout && _config.SpecialContentConfig.GiveRoleLoadouts)
                    player.ClearInventory();

                role.ApplyTo(player, _config.SpecialContentConfig, ResolveItem, IsItemEnabled, TrackGivenItem);
                ShowRoleIntroduction(player, role);
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] Failed to apply special role {role?.Name} to {player?.Nickname}: {ex.Message}");
            }
        }

        private void ShowRoleIntroduction(Player player, CustomRoleBase role)
        {
            if (player == null || role == null)
                return;

            if (_config.SpecialContentConfig.ShowAssignedRoleHint)
                PluginHelper.ShowTopRightHint(player, role.RenderTemplate(_config.SpecialContentConfig.AssignedRoleHintText, player), _config.SpecialContentConfig.AssignedRoleHintDuration);

            if (_config.SpecialContentConfig.ShowRoleIntroduction)
                PluginHelper.ShowCenterInfoHint(player, role.RenderTemplate(_config.SpecialContentConfig.RoleIntroductionText, player), _config.SpecialContentConfig.RoleIntroductionDuration);
        }

        private void TrackGivenItem(Item item, CustomItemBase customItem)
        {
            if (item == null || customItem == null)
                return;

            TrackHeldItem(item, customItem.Name);
        }

        private void TrackHeldItem(Item item, string itemName)
        {
            if (item == null || string.IsNullOrWhiteSpace(itemName))
                return;

            _specialHeldItems[item.Serial] = itemName;
        }

        private CustomItemBase ResolveItem(string name)
        {
            Config.SpecialItemDefinition item = (_config.SpecialContentConfig.Items ?? new List<Config.SpecialItemDefinition>())
                .FirstOrDefault(entry => entry != null && string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase));

            return SpecialContentRegistry.CreateItem(item);
        }

        private CustomItemBase ResolveItemForHeldItem(Item item)
        {
            if (item == null)
                return null;

            if (_specialHeldItems.TryGetValue(item.Serial, out string itemName))
                return ResolveItem(itemName);

            Config.SpecialItemDefinition matchedItem = (_config.SpecialContentConfig.Items ?? new List<Config.SpecialItemDefinition>())
                .FirstOrDefault(customItem => customItem != null && customItem.IsSpecialItem && customItem.TryGetGameItem(out ItemType type) && type == item.Type);

            return SpecialContentRegistry.CreateItem(matchedItem);
        }

        private string RenderNormalItemIntroduction(string template, Player player, Item item)
        {
            string itemType = item?.Type.ToString() ?? ItemType.None.ToString();
            return PluginHelper.FormatTemplate(template, player, _config)
                .Replace("{item_name}", PluginHelper.GetChineseItemName(item?.Type ?? ItemType.None))
                .Replace("{description}", PluginHelper.GetChineseItemName(item?.Type ?? ItemType.None))
                .Replace("{item_type}", itemType)
                .Replace("{item_tag}", "普通物品");
        }

        public string RenderRoleIntroductionFor(Player player)
        {
            CustomRoleBase role = GetAssignedRole(player);
            if (role == null)
                return "当前没有特殊角色。";

            return role.RenderTemplate(_config.SpecialContentConfig.RoleIntroductionText, player);
        }

        public bool ShowRoleIntroductionCommand(Player player, out string response)
        {
            response = RenderRoleIntroductionFor(player);
            CustomRoleBase role = GetAssignedRole(player);
            if (player != null && role != null)
                PluginHelper.ShowCenterInfoHint(player, response, _config.SpecialContentConfig.RoleIntroductionDuration);

            return role != null;
        }

        private void ResetPlayer(Player player)
        {
            if (player == null)
                return;

            try
            {
                player.CustomInfo = string.Empty;
            }
            catch
            {
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

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        }

        private class PendingSpecialPickup
        {
            public ushort PickupSerial { get; set; }
            public string ItemName { get; set; }
        }

        [CommandHandler(typeof(RemoteAdminCommandHandler))]
        public class SpecialRoleRaCommand : ICommand
        {
            public string Command => "sgjrole";
            public string[] Aliases => new[] { "srole", "sgjr" };
            public string Description => "Set or list SGJ special roles. Usage: sgjrole set <player> <role name> | sgjrole list";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                if (Instance == null)
                {
                    response = "Special content module is not enabled.";
                    return false;
                }

                if (arguments.Count == 0 || string.Equals(arguments.At(0), "help", StringComparison.OrdinalIgnoreCase))
                {
                    response = "Usage: sgjrole set <player name/id> <role name> | sgjrole list";
                    return true;
                }

                if (string.Equals(arguments.At(0), "list", StringComparison.OrdinalIgnoreCase))
                {
                    response = string.Join(", ", Instance.GetRoleNames().Take(80));
                    return true;
                }

                if (string.Equals(arguments.At(0), "set", StringComparison.OrdinalIgnoreCase) && arguments.Count >= 3)
                {
                    Player target = FindPlayer(arguments.At(1));
                    string roleName = string.Join(" ", arguments.Skip(2));
                    return Instance.TrySetAssignedRole(target, roleName, out response);
                }

                response = "Usage: sgjrole set <player name/id> <role name> | sgjrole list";
                return false;
            }

            private static Player FindPlayer(string query)
            {
                if (string.IsNullOrWhiteSpace(query))
                    return null;

                string normalized = Normalize(query);
                return Player.List.FirstOrDefault(player =>
                    player != null
                    && (Normalize(player.Nickname).Contains(normalized)
                        || Normalize(player.Id.ToString()) == normalized
                        || Normalize(player.UserId).Contains(normalized)
                        || Normalize(player.RawUserId).Contains(normalized)));
            }
        }

        [CommandHandler(typeof(ClientCommandHandler))]
        public class RoleIntroductionCommand : ICommand
        {
            public string Command => "js";
            public string[] Aliases => Array.Empty<string>();
            public string Description => "Show current special role introduction.";

            public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
            {
                Player player = Player.Get(sender);
                if (Instance == null)
                {
                    response = "Special content module is not enabled.";
                    return false;
                }

                return Instance.ShowRoleIntroductionCommand(player, out response);
            }
        }
    }
}
