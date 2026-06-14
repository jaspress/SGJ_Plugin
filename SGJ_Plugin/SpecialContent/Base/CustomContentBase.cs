using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Enums;
using Exiled.API.Structs;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using PlayerRoles;
using System;
using System.Collections.Generic;
using PluginHelper = SGJ_Plugin.Helper.Helper;

namespace SGJ_Plugin.SpecialContent.Base
{
    public abstract class CustomContentBase
    {
        public string Name { get; set; } = "特殊内容";
        public string SourceUrl { get; set; } = string.Empty;

        public virtual string RenderTemplate(string template, Player player)
        {
            return (template ?? string.Empty)
                .Replace("{name}", Name ?? string.Empty)
                .Replace("{source_url}", SourceUrl ?? string.Empty)
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
        public bool ConsumeOnUse { get; set; } = false;
        public float HealOnUse { get; set; } = 0f;
        public float ArtificialHealthOnUse { get; set; } = 0f;
        public string EffectOnUse { get; set; } = string.Empty;
        public byte EffectOnUseIntensity { get; set; } = 1;
        public float EffectOnUseDuration { get; set; } = 10f;
        public List<string> ExtraItemsOnUse { get; set; } = new List<string>();
        public float HealOnSelect { get; set; } = 0f;
        public float ArtificialHealthOnSelect { get; set; } = 0f;
        public string EffectOnSelect { get; set; } = string.Empty;
        public byte EffectOnSelectIntensity { get; set; } = 1;
        public float EffectOnSelectDuration { get; set; } = 5f;
        public ushort AmmoOnSelect { get; set; } = 0;
        public List<string> AttachmentNames { get; set; } = new List<string>();

        public virtual bool TryGetGameItem(out ItemType itemType)
        {
            return Enum.TryParse(GameItem, true, out itemType);
        }

        public virtual Item GiveTo(Player player)
        {
            if (player == null || !TryGetGameItem(out ItemType itemType) || itemType == ItemType.None)
                return null;

            if (AttachmentNames != null && AttachmentNames.Count > 0 && TryGetFirearmType(itemType, out FirearmType firearmType))
            {
                List<AttachmentIdentifier> attachments = BuildAttachmentIdentifiers(firearmType);
                if (attachments.Count > 0)
                    return player.AddItem(firearmType, attachments);
            }

            return player.AddItem(itemType);
        }

        public virtual void CopySettingsFrom(CustomItemBase source)
        {
            if (source == null)
                return;

            Name = source.Name;
            SourceUrl = source.SourceUrl;
            GameItem = source.GameItem;
            GiveByDefault = source.GiveByDefault;
            IsSpecialItem = source.IsSpecialItem;
            Description = source.Description;
            PickupHintText = source.PickupHintText;
            ConsumeOnUse = source.ConsumeOnUse;
            HealOnUse = source.HealOnUse;
            ArtificialHealthOnUse = source.ArtificialHealthOnUse;
            EffectOnUse = source.EffectOnUse;
            EffectOnUseIntensity = source.EffectOnUseIntensity;
            EffectOnUseDuration = source.EffectOnUseDuration;
            ExtraItemsOnUse = source.ExtraItemsOnUse == null ? new List<string>() : new List<string>(source.ExtraItemsOnUse);
            HealOnSelect = source.HealOnSelect;
            ArtificialHealthOnSelect = source.ArtificialHealthOnSelect;
            EffectOnSelect = source.EffectOnSelect;
            EffectOnSelectIntensity = source.EffectOnSelectIntensity;
            EffectOnSelectDuration = source.EffectOnSelectDuration;
            AmmoOnSelect = source.AmmoOnSelect;
            AttachmentNames = source.AttachmentNames == null ? new List<string>() : new List<string>(source.AttachmentNames);
        }

        public virtual string RenderPickupHint(Player player)
        {
            return RenderTemplate(PickupHintText, player)
                .Replace("{item_name}", Name ?? string.Empty)
                .Replace("{description}", Description ?? string.Empty)
                .Replace("{effect}", BuildEffectSummary())
                .Replace("{item_type}", GameItem ?? string.Empty)
                .Replace("{item_tag}", IsSpecialItem ? "特殊物品" : "普通物品");
        }

        public virtual string RenderIntroduction(string template, Player player)
        {
            return RenderTemplate(template, player)
                .Replace("{item_name}", Name ?? string.Empty)
                .Replace("{description}", Description ?? string.Empty)
                .Replace("{effect}", BuildEffectSummary())
                .Replace("{item_type}", GameItem ?? string.Empty)
                .Replace("{item_tag}", IsSpecialItem ? "特殊物品" : "普通物品");
        }

        public virtual bool ApplyUseEffect(Player player, Func<string, CustomItemBase> itemResolver = null)
        {
            return ApplyEffects(player, HealOnUse, ArtificialHealthOnUse, EffectOnUse, EffectOnUseIntensity, EffectOnUseDuration, ExtraItemsOnUse, itemResolver);
        }

        public virtual bool ApplySelectEffect(Player player, Item item)
        {
            bool applied = ApplyEffects(player, HealOnSelect, ArtificialHealthOnSelect, EffectOnSelect, EffectOnSelectIntensity, EffectOnSelectDuration, null, null);

            if (AmmoOnSelect > 0)
            {
                player.SetAmmo(AmmoType.Nato9, AmmoOnSelect);
                player.SetAmmo(AmmoType.Nato556, AmmoOnSelect);
                player.SetAmmo(AmmoType.Nato762, AmmoOnSelect);
                player.SetAmmo(AmmoType.Ammo12Gauge, AmmoOnSelect);
                player.SetAmmo(AmmoType.Ammo44Cal, AmmoOnSelect);
                applied = true;
            }

            return applied;
        }

        public virtual bool OnSelected(Player player, Item item)
        {
            return ApplySelectEffect(player, item);
        }

        public virtual bool OnConsumed(ConsumingItemEventArgs ev, Func<string, CustomItemBase> itemResolver = null)
        {
            if (ev?.Player == null)
                return false;

            return ApplyUseEffect(ev.Player, itemResolver);
        }

        public virtual string GetUseMessage()
        {
            return BuildEffectSummary();
        }

        public virtual string GetSelectMessage()
        {
            return BuildEffectSummary();
        }

        protected virtual bool ApplyEffects(Player player, float heal, float artificialHealth, string effectName, byte effectIntensity, float effectDuration, List<string> extraItems, Func<string, CustomItemBase> itemResolver)
        {
            if (player == null)
                return false;

            bool applied = false;
            if (heal > 0f)
            {
                player.Heal(heal, true);
                applied = true;
            }

            if (artificialHealth > 0f)
            {
                player.ArtificialHealth = Math.Max(player.ArtificialHealth, artificialHealth);
                applied = true;
            }

            if (!string.IsNullOrWhiteSpace(effectName) && Enum.TryParse(effectName, true, out EffectType effectType))
            {
                player.EnableEffect(effectType, effectIntensity, Math.Max(0.1f, effectDuration), true);
                applied = true;
            }

            if (extraItems != null && itemResolver != null)
            {
                foreach (string itemName in extraItems)
                {
                    CustomItemBase item = itemResolver(itemName);
                    if (item?.GiveTo(player) != null)
                        applied = true;
                }
            }

            return applied;
        }

        protected string BuildEffectSummary()
        {
            List<string> parts = new List<string>();
            if (HealOnUse > 0f) parts.Add($"使用治疗 {HealOnUse:0}");
            if (ArtificialHealthOnUse > 0f) parts.Add($"使用获得护盾 {ArtificialHealthOnUse:0}");
            if (!string.IsNullOrWhiteSpace(EffectOnUse)) parts.Add($"使用获得 {EffectOnUse} {EffectOnUseDuration:0.#}秒");
            if (HealOnSelect > 0f) parts.Add($"切出治疗 {HealOnSelect:0}");
            if (ArtificialHealthOnSelect > 0f) parts.Add($"切出获得护盾 {ArtificialHealthOnSelect:0}");
            if (!string.IsNullOrWhiteSpace(EffectOnSelect)) parts.Add($"切出获得 {EffectOnSelect} {EffectOnSelectDuration:0.#}秒");
            if (AmmoOnSelect > 0) parts.Add($"切出补充弹药 {AmmoOnSelect}");
            if (ExtraItemsOnUse != null && ExtraItemsOnUse.Count > 0) parts.Add("使用获得：" + string.Join("、", ExtraItemsOnUse));
            return parts.Count == 0 ? "无额外效果" : string.Join("；", parts);
        }

        protected virtual List<AttachmentIdentifier> BuildAttachmentIdentifiers(FirearmType firearmType)
        {
            List<AttachmentIdentifier> result = new List<AttachmentIdentifier>();
            foreach (string attachmentName in AttachmentNames ?? new List<string>())
            {
                if (!Enum.TryParse(attachmentName, true, out AttachmentName parsed) || parsed == AttachmentName.None)
                    continue;

                result.Add(AttachmentIdentifier.Get(firearmType, parsed));
            }

            return result;
        }

        protected static bool TryGetFirearmType(ItemType itemType, out FirearmType firearmType)
        {
            switch (itemType)
            {
                case ItemType.GunA7:
                    firearmType = FirearmType.A7;
                    return true;
                case ItemType.GunAK:
                    firearmType = FirearmType.AK;
                    return true;
                case ItemType.GunCOM15:
                    firearmType = FirearmType.Com15;
                    return true;
                case ItemType.GunCOM18:
                    firearmType = FirearmType.Com18;
                    return true;
                case ItemType.GunCom45:
                    firearmType = FirearmType.Com45;
                    return true;
                case ItemType.GunCrossvec:
                    firearmType = FirearmType.Crossvec;
                    return true;
                case ItemType.GunE11SR:
                    firearmType = FirearmType.E11SR;
                    return true;
                case ItemType.GunFRMG0:
                    firearmType = FirearmType.FRMG0;
                    return true;
                case ItemType.GunFSP9:
                    firearmType = FirearmType.FSP9;
                    return true;
                case ItemType.GunLogicer:
                    firearmType = FirearmType.Logicer;
                    return true;
                case ItemType.GunRevolver:
                    firearmType = FirearmType.Revolver;
                    return true;
                case ItemType.GunSCP127:
                    firearmType = FirearmType.Scp127;
                    return true;
                case ItemType.GunShotgun:
                    firearmType = FirearmType.Shotgun;
                    return true;
                case ItemType.ParticleDisruptor:
                    firearmType = FirearmType.ParticleDisruptor;
                    return true;
                default:
                    firearmType = FirearmType.None;
                    return false;
            }
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
        public bool PrimarySkillEnabled { get; set; } = false;
        public string PrimarySkillName { get; set; } = "主技能";
        public string PrimarySkillDescription { get; set; } = "触发当前特殊角色的主技能。";
        public float PrimarySkillCooldownSeconds { get; set; } = 5f;
        public bool SecondarySkillEnabled { get; set; } = false;
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

        public virtual void CopySettingsFrom(CustomRoleBase source)
        {
            if (source == null)
                return;

            Name = source.Name;
            SourceUrl = source.SourceUrl;
            Camp = source.Camp;
            BaseRole = source.BaseRole;
            Health = source.Health;
            ArtificialHealth = source.ArtificialHealth;
            Stamina = source.Stamina;
            Speed = source.Speed;
            BulletResistanceHead = source.BulletResistanceHead;
            BulletResistanceBody = source.BulletResistanceBody;
            BulletResistanceArm = source.BulletResistanceArm;
            BulletResistanceLeg = source.BulletResistanceLeg;
            BadgeColor = source.BadgeColor;
            RoleColor = source.RoleColor;
            KillExperience = source.KillExperience;
            Description = source.Description;
            PrimarySkillEnabled = source.PrimarySkillEnabled;
            PrimarySkillName = source.PrimarySkillName;
            PrimarySkillDescription = source.PrimarySkillDescription;
            PrimarySkillCooldownSeconds = source.PrimarySkillCooldownSeconds;
            SecondarySkillEnabled = source.SecondarySkillEnabled;
            SecondarySkillName = source.SecondarySkillName;
            SecondarySkillDescription = source.SecondarySkillDescription;
            SecondarySkillCooldownSeconds = source.SecondarySkillCooldownSeconds;
            LoadoutItems = source.LoadoutItems == null ? new List<string>() : new List<string>(source.LoadoutItems);
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
            if (!PrimarySkillEnabled)
                return false;

            return UseDefaultSkill(player, "主技能");
        }

        public virtual bool UseSecondarySkill(Player player)
        {
            if (!SecondarySkillEnabled)
                return false;

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
                .Replace("{primary_skill_description}", PrimarySkillDescription ?? string.Empty)
                .Replace("{secondary_skill_description}", SecondarySkillDescription ?? string.Empty)
                .Replace("{primary_cooldown}", PrimarySkillCooldownSeconds.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Replace("{secondary_cooldown}", SecondarySkillCooldownSeconds.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                .Replace("{loadout}", LoadoutItems == null || LoadoutItems.Count == 0 ? "无" : string.Join("、", LoadoutItems));
        }
    }
}
