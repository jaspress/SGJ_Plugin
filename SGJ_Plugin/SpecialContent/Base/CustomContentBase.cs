using Exiled.API.Features;
using Exiled.API.Features.Items;
using PlayerRoles;
using System;
using System.Collections.Generic;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.SpecialContent.Base
{
    public abstract class CustomContentBase
    {
        public string Name { get; set; } = "特殊内容";

        public virtual string RenderTemplate(string template, Player player)
        {
            return (template ?? string.Empty)
                .Replace("{name}", Name ?? string.Empty)
                .Replace("{base_name}", player?.Nickname ?? string.Empty);
        }
    }

    public abstract class CustomItemBase : CustomContentBase
    {
        public string GameItem { get; set; } = ItemType.None.ToString();
        public bool GiveByDefault { get; set; } = false;
        public bool IsSpecialItem { get; set; } = true;
        public string Description { get; set; } = "一个特殊物品。";
        public string PickupHintText { get; set; } = "<b><size=22><color=#7FFFD4>[特殊物品]</color>\n获得：<color=#90EE90>{item_name}</color></size></b>";

        public virtual bool TryGetGameItem(out ItemType itemType)
        {
            return Enum.TryParse(GameItem, true, out itemType);
        }

        public virtual Item GiveTo(Player player)
        {
            if (player == null || !TryGetGameItem(out ItemType itemType) || itemType == ItemType.None)
                return null;

            return player.AddItem(itemType);
        }

        public virtual string RenderPickupHint(Player player)
        {
            return RenderTemplate(PickupHintText, player)
                .Replace("{item_name}", Name ?? string.Empty)
                .Replace("{description}", Description ?? string.Empty)
                .Replace("{item_type}", GameItem ?? string.Empty)
                .Replace("{item_tag}", IsSpecialItem ? "特殊物品" : "普通物品");
        }

        public virtual string RenderIntroduction(string template, Player player)
        {
            return RenderTemplate(template, player)
                .Replace("{item_name}", Name ?? string.Empty)
                .Replace("{description}", Description ?? string.Empty)
                .Replace("{item_type}", GameItem ?? string.Empty)
                .Replace("{item_tag}", IsSpecialItem ? "特殊物品" : "普通物品");
        }
    }

    public abstract class CustomRoleBase : CustomContentBase
    {
        public string Camp { get; set; } = "特殊阵营";
        public string BaseRole { get; set; } = RoleTypeId.Tutorial.ToString();
        public int Health { get; set; } = 100;
        public int ArtificialHealth { get; set; } = 0;
        public int Stamina { get; set; } = 100;
        public float Speed { get; set; } = 0f;
        public int BulletResistanceHead { get; set; } = 0;
        public int BulletResistanceBody { get; set; } = 0;
        public int BulletResistanceArm { get; set; } = 0;
        public int BulletResistanceLeg { get; set; } = 0;
        public string BadgeColor { get; set; } = "default";
        public string RoleColor { get; set; } = "#90EE90";
        public int KillExperience { get; set; } = 0;
        public string Description { get; set; } = "一个特殊角色。";
        public string PrimarySkillName { get; set; } = "主技能";
        public string PrimarySkillDescription { get; set; } = "触发当前特殊角色的主技能。";
        public float PrimarySkillCooldownSeconds { get; set; } = 5f;
        public string SecondarySkillName { get; set; } = "副技能";
        public string SecondarySkillDescription { get; set; } = "触发当前特殊角色的副技能。";
        public float SecondarySkillCooldownSeconds { get; set; } = 5f;
        public List<string> LoadoutItems { get; set; } = new List<string>();

        public virtual bool CanUseFor(RoleTypeId role)
        {
            return TryGetBaseRole(out RoleTypeId baseRole) && baseRole == role;
        }

        public virtual bool TryGetBaseRole(out RoleTypeId role)
        {
            return Enum.TryParse(BaseRole, true, out role);
        }

        public virtual void ApplyTo(Player player, Config.SpecialContentConfigClass config, Func<string, CustomItemBase> itemResolver, Func<string, bool> itemEnabled, Action<Item, CustomItemBase> itemGiven = null)
        {
            if (player == null || config == null)
                return;

            ApplyStats(player);
            ApplyVisuals(player, config);

            if (config.GiveRoleLoadouts)
                GiveLoadout(player, itemResolver, itemEnabled, itemGiven);
        }

        public virtual void ApplyStats(Player player)
        {
            if (player == null)
                return;

            if (Health > 0)
            {
                player.MaxHealth = Math.Max(player.MaxHealth, Health);
                player.Health = Math.Max(player.Health, Health);
            }

            if (ArtificialHealth > 0)
                player.ArtificialHealth = Math.Max(player.ArtificialHealth, ArtificialHealth);
        }

        public virtual void ApplyVisuals(Player player, Config.SpecialContentConfigClass config)
        {
            if (player == null || config == null)
                return;

            if (config.UpdateDisplayNickname)
                player.DisplayNickname = RenderTemplate(config.DisplayNicknameTemplate, player);

            player.CustomInfo = $"{Camp} | {Name}";

            if (!string.IsNullOrWhiteSpace(BadgeColor))
                player.RankColor = BadgeColor;
        }

        public virtual void GiveLoadout(Player player, Func<string, CustomItemBase> itemResolver, Func<string, bool> itemEnabled, Action<Item, CustomItemBase> itemGiven = null)
        {
            if (player == null || LoadoutItems == null || itemResolver == null)
                return;

            foreach (string itemName in LoadoutItems)
            {
                if (itemEnabled != null && !itemEnabled(itemName))
                    continue;

                CustomItemBase customItem = itemResolver(itemName);
                Item item = customItem?.GiveTo(player);
                if (item != null && customItem != null)
                    itemGiven?.Invoke(item, customItem);
            }
        }

        public virtual bool UsePrimarySkill(Player player)
        {
            return UseDefaultSkill(player, "主技能");
        }

        public virtual bool UseSecondarySkill(Player player)
        {
            return UseDefaultSkill(player, "副技能");
        }

        protected virtual bool UseDefaultSkill(Player player, string skillName)
        {
            if (player == null)
                return false;

            PluginHelper.ShowTopRightHint(player, $"<b><size=20><color=#7FFFD4>[技能]</color> {Name} 使用了 {skillName}</size></b>", 3f);
            return true;
        }

        public override string RenderTemplate(string template, Player player)
        {
            return base.RenderTemplate(template, player)
                .Replace("{role_name}", Name ?? string.Empty)
                .Replace("{camp_name}", Camp ?? string.Empty)
                .Replace("{base_role}", BaseRole ?? string.Empty)
                .Replace("{role_color}", RoleColor ?? string.Empty)
                .Replace("{description}", Description ?? string.Empty)
                .Replace("{health}", Health.ToString())
                .Replace("{stamina}", Stamina.ToString())
                .Replace("{speed}", Speed <= 0f ? "默认" : Speed.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Replace("{resistance_head}", BulletResistanceHead.ToString())
                .Replace("{resistance_body}", BulletResistanceBody.ToString())
                .Replace("{resistance_arm}", BulletResistanceArm.ToString())
                .Replace("{resistance_leg}", BulletResistanceLeg.ToString())
                .Replace("{primary_skill}", PrimarySkillName ?? string.Empty)
                .Replace("{secondary_skill}", SecondarySkillName ?? string.Empty)
                .Replace("{primary_cooldown}", PrimarySkillCooldownSeconds.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Replace("{secondary_cooldown}", SecondarySkillCooldownSeconds.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Replace("{loadout}", LoadoutItems == null || LoadoutItems.Count == 0 ? "无" : string.Join("、", LoadoutItems));
        }
    }
}
