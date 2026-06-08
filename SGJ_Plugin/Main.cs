using Exiled.API.Features;
using Exiled.API.Interfaces;
using SGJ_Plugin.Modules;
using System;
using System.Collections.Generic;

namespace SGJ_Plugin
{
    /// <summary>
    /// SGJ_Plugin 主类
    /// 符合 EXILED 框架标准的插件实现
    /// </summary>
    public class Main : Plugin<Config>
    {
        // 单例实例
        public static Main Instance { get; private set; }

        public override string Name => "SGJ_Plugin";
        public override string Author => "WJ";
        public override Version Version => new Version(1, 0, 0);
        public override string Prefix => "SGJ";

        private List<ModuleBase> _modules = new List<ModuleBase>();

        public override void OnEnabled()
        {
            try
            {
                base.OnEnabled();
                Instance = this;

                Log.Info("SGJ_Plugin 已启用");

                // 初始化所有模块
                InitializeModules();

                // 启用所有模块
                foreach (var module in _modules)
                {
                    if (module != null)
                    {
                        module.Enable();
                    }
                }

                Log.Info($"已启用 {_modules.Count} 个功能模块");
            }
            catch (Exception ex)
            {
                Log.Error($"启用插件时发生错误: {ex}");
                OnDisabled();
            }
        }

        public override void OnDisabled()
        {
            try
            {
                base.OnDisabled();

                Log.Info("SGJ_Plugin 正在禁用");

                // 禁用所有模块
                foreach (var module in _modules)
                {
                    if (module != null && module.IsEnabled)
                    {
                        module.Disable();
                        module.Dispose();
                    }
                }

                _modules.Clear();
                Instance = null;

                Log.Info("SGJ_Plugin 已禁用");
            }
            catch (Exception ex)
            {
                Log.Error($"禁用插件时发生错误: {ex}");
            }
        }

        /// <summary>
        /// 初始化所有功能模块
        /// </summary>
        private void InitializeModules()
        {
            // 添加无限子弹模块
            _modules.Add(new InfiniteAmmoModule(Config));

            // 添加保安下班模块
            _modules.Add(new GuardOffDutyModule(Config));

            // 在这里可以添加其他模块
        }
    }
}
