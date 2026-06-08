using Exiled.API.Interfaces;
using Exiled.API.Interfaces;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SGJ_Plugin
{
    /// <summary>
    /// SGJ_Plugin 主配置类
    /// 符合 EXILED IConfig 接口标准
    /// </summary>
    public class Config : IConfig
    {
        /// <inheritdoc/>
        [Description("启用插件")]
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc/>
        [Description("调试模式")]
        public bool Debug { get; set; } = false;

        /// <summary>
        /// 无限子弹模块配置
        /// </summary>
        [Description("仅限枪械武器的无限子弹模块配置")]
        public InfiniteAmmoConfigClass InfiniteAmmoConfig { get; set; } = new InfiniteAmmoConfigClass();

        /// <summary>
        /// 保安下班模块配置
        /// </summary>
        [Description("保安下班功能配置 - 设施安保逃离时转换为九尾狐列兵")]
        public GuardOffDutyConfigClass GuardOffDutyConfig { get; set; } = new GuardOffDutyConfigClass();

        /// <summary>
        /// 无限子弹功能配置类 - 仅限枪械武器
        /// </summary>
        public class InfiniteAmmoConfigClass
        {
            [Description("启用无限子弹功能（仅限枪械武器）")]
            public bool IsEnabled { get; set; } = true;

            [Description("装弹时恢复弹匣（不消耗备弹）")]
            public bool RestoreOnReload { get; set; } = true;

            [Description("提供无限备弹")]
            public bool InfiniteReserveAmmo { get; set; } = true;

            [Description("忽略的玩家ID列表（这些玩家不受无限子弹影响）")]
            public List<string> IgnorePlayers { get; set; } = new List<string>();

            [Description("仅应用于这些枪械（为空时应用于所有枪械）")]
            public List<string> AllowedGuns { get; set; } = new List<string>();

            [Description("排除的特定枪械（这些枪械不受影响）")]
            public List<string> ExcludedGuns { get; set; } = new List<string>();
        }

        /// <summary>
        /// 保安下班功能配置类
        /// </summary>
        public class GuardOffDutyConfigClass
        {
            [Description("启用保安下班功能")]
            public bool IsEnabled { get; set; } = true;

            [Description("设施安保逃离后转换为的角色")]
            public RoleTypeId EscapeRole { get; set; } = RoleTypeId.NtfPrivate;
        }
    }
}
