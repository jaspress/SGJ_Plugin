using Exiled.API.Features;
using SGJ_Plugin.Modules;
using SGJ_Plugin.UI.Managers;
using System;

namespace SGJ_Plugin.UI.Modules
{
    /// <summary>
    /// UI系统模块
    /// 负责初始化和管理整个UI系统
    /// </summary>
    public class UIModule : ModuleBase
    {
        public override string Name => "UI系统模块";

        private Config _config;
        private UIManager _uiManager;

        public UIModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            try
            {
                // 初始化UI管理器
                _uiManager = UIManager.Instance;
                if (!_uiManager.Initialize())
                {
                    Log.Error($"[{Name}] 初始化UI管理器失败");
                    throw new Exception("UI管理器初始化失败");
                }

                Log.Info($"[{Name}] 已启用");

                if (_config.Debug)
                {
                    Log.Debug($"[{Name}] UI系统已准备就绪");
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
                // 关闭UI系统
                if (_uiManager != null && _uiManager.IsInitialized)
                {
                    _uiManager.Shutdown();
                }

                Log.Info($"[{Name}] 已禁用");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] 禁用时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取UI管理器
        /// </summary>
        public UIManager GetUIManager()
        {
            return _uiManager;
        }

        /// <summary>
        /// 创建测试UI
        /// </summary>
        public void CreateTestUI()
        {
            try
            {
                // 创建主面板
                var mainPanel = _uiManager.CreatePanel("main_panel", "主界面");
                if (mainPanel != null)
                {
                    var textHint = _uiManager.CreateTextHint("main_panel", "title", "欢迎使用 SGJ_Plugin UI系统");
                    textHint.UpdateInterval = 100;

                    Log.Info($"[{Name}] 测试UI已创建");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] 创建测试UI时出错: {ex.Message}");
            }
        }
    }
}
