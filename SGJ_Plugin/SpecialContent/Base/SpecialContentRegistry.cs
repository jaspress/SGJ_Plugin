using SGJ_Plugin.SpecialContent.CustomItems;
using SGJ_Plugin.SpecialContent.CustomRoles;
using System;
using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.Base
{
    public static class SpecialContentRegistry
    {
        private static readonly Dictionary<string, Func<CustomItemBase>> ItemFactories = BuildItemFactories();
        private static readonly Dictionary<string, Func<CustomRoleBase>> RoleFactories = BuildRoleFactories();

        private static Dictionary<string, Func<CustomItemBase>> BuildItemFactories()
        {
            Dictionary<string, Func<CustomItemBase>> factories = new Dictionary<string, Func<CustomItemBase>>(StringComparer.OrdinalIgnoreCase);
            Register(factories, "ID卡", () => new IdCardCustomItem());
            Register(factories, "科研权限卡", () => new ScientistKeycardCustomItem());
            Register(factories, "安保权限卡", () => new SecurityKeycardCustomItem());
            Register(factories, "远程武器", () => new RangedWeaponCustomItem());
            Register(factories, "近战武器", () => new MeleeWeaponCustomItem());
            Register(factories, "CI对讲机", () => new CiRadioCustomItem());
            Register(factories, "医疗用品", () => new MedicalCustomItem());
            Register(factories, "食品", () => new FoodCustomItem());
            Register(factories, "SCP-500 万能药", () => new Scp500CustomItem());
            Register(factories, "SCP-207 一箱可乐", () => new Scp207CustomItem());
            Register(factories, "SCP-268 疏忽帽", () => new Scp268CustomItem());
            Register(factories, "防护装备", () => new ArmorCustomItem());
            return factories;
        }

        private static Dictionary<string, Func<CustomRoleBase>> BuildRoleFactories()
        {
            Dictionary<string, Func<CustomRoleBase>> factories = new Dictionary<string, Func<CustomRoleBase>>(StringComparer.OrdinalIgnoreCase);
            Register(factories, "SCP-049 瘟疫医生", () => new Scp049DoctorRole());
            Register(factories, "D级人员 黑客", () => new ClassDHackerRole());
            Register(factories, "高级科研员", () => new SeniorScientistRole());
            Register(factories, "安保部门 上尉", () => new SecurityCaptainRole());
            Register(factories, "九尾狐 指挥官", () => new NtfCommanderRole());
            Register(factories, "九尾狐 狙击手", () => new NtfSniperRole());
            Register(factories, "九尾狐 战斗专家", () => new NtfCombatSpecialistRole());
            Register(factories, "蛇之手 指挥官", () => new SerpentsHandCommanderRole());
            Register(factories, "混沌分裂者 指挥官", () => new ChaosCommanderRole());
            Register(factories, "GOC 指挥官", () => new GocCommanderRole());
            Register(factories, "GRU-P侵入部队 指挥官", () => new GruCommanderRole());
            Register(factories, "UIU特工组 指挥官", () => new UiuCommanderRole());
            Register(factories, "深红王之子 狂信徒", () => new ScarletKingCultistRole());
            Register(factories, "异界特遣队 特工", () => new OtherworldTaskForceAgentRole());
            return factories;
        }

        private static void Register<T>(Dictionary<string, Func<T>> factories, string name, Func<T> factory)
        {
            factories[name] = factory;
            factories[DerivativeNames.ToDerivativeName(name)] = factory;
        }

        public static CustomItemBase CreateItem(CustomItemBase configItem)
        {
            if (configItem == null)
                return null;

            CustomItemBase runtime = ItemFactories.TryGetValue(configItem.Name ?? string.Empty, out Func<CustomItemBase> factory)
                ? factory()
                : new Config.SpecialItemDefinition();

            runtime.CopySettingsFrom(configItem);
            return runtime;
        }

        public static CustomRoleBase CreateRole(CustomRoleBase configRole)
        {
            if (configRole == null)
                return null;

            CustomRoleBase runtime = RoleFactories.TryGetValue(configRole.Name ?? string.Empty, out Func<CustomRoleBase> factory)
                ? factory()
                : new Config.SpecialRoleDefinition();

            runtime.CopySettingsFrom(configRole);
            return runtime;
        }
    }
}
