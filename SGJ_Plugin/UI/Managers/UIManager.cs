using Exiled.API.Features;
using SGJ_Plugin.UI.Core;
using SGJ_Plugin.UI.Elements;
using System;
using System.Collections.Generic;

namespace SGJ_Plugin.UI.Managers
{
    /// <summary>
    /// UI管理器
    /// 管理所有UI元素的生命周期
    /// </summary>
    public class UIManager
    {
        private static UIManager _instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UIManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// UI核心系统
        /// </summary>
        private UICore _uiCore;

        /// <summary>
        /// 所有UI面板
        /// </summary>
        private Dictionary<string, UIPanel> _panels = new Dictionary<string, UIPanel>();

        /// <summary>
        /// UI管理器是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        private UIManager()
        {
        }

        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        public bool Initialize()
        {
            try
            {
                _uiCore = new UICore();
                if (!_uiCore.Initialize())
                {
                    Log.Error("[UIManager] 初始化UI核心系统失败");
                    return false;
                }

                IsInitialized = true;
                Log.Info("[UIManager] UI管理器已初始化");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[UIManager] 初始化UI管理器失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建新的UI面板
        /// </summary>
        public UIPanel CreatePanel(string id, string title = "")
        {
            if (!IsInitialized)
            {
                Log.Warning("[UIManager] UI管理器未初始化");
                return null;
            }

            if (_panels.ContainsKey(id))
            {
                Log.Warning($"[UIManager] 面板 '{id}' 已存在");
                return _panels[id];
            }

            var panel = new UIPanel(id, title);
            _panels[id] = panel;

            Log.Debug($"[UIManager] 创建面板 '{id}'");
            return panel;
        }

        /// <summary>
        /// 获取UI面板
        /// </summary>
        public UIPanel GetPanel(string id)
        {
            _panels.TryGetValue(id, out UIPanel panel);
            return panel;
        }

        /// <summary>
        /// 删除UI面板
        /// </summary>
        public bool RemovePanel(string id)
        {
            if (_panels.Remove(id))
            {
                Log.Debug($"[UIManager] 删除面板 '{id}'");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 创建文本提示
        /// </summary>
        public TextHintElement CreateTextHint(string panelId, string elementId, string content = "")
        {
            var panel = GetPanel(panelId);
            if (panel == null)
            {
                Log.Warning($"[UIManager] 面板 '{panelId}' 不存在");
                return null;
            }

            var element = new TextHintElement(elementId, content);
            panel.AddElement(element);

            Log.Debug($"[UIManager] 在面板 '{panelId}' 中创建文本提示 '{elementId}'");
            return element;
        }

        /// <summary>
        /// 从面板中移除元素
        /// </summary>
        public bool RemoveElement(string panelId, string elementId)
        {
            var panel = GetPanel(panelId);
            if (panel != null)
            {
                return panel.RemoveElement(elementId);
            }
            return false;
        }

        /// <summary>
        /// 获取元素
        /// </summary>
        public UIElement GetElement(string panelId, string elementId)
        {
            var panel = GetPanel(panelId);
            if (panel != null)
            {
                return panel.GetElement(elementId);
            }
            return null;
        }

        /// <summary>
        /// 清空所有UI
        /// </summary>
        public void ClearAll()
        {
            foreach (var panel in _panels.Values)
            {
                panel.ClearAll();
            }
            _panels.Clear();
            _uiCore?.ClearAll();
            Log.Info("[UIManager] 所有UI已清空");
        }

        /// <summary>
        /// 关闭UI管理器
        /// </summary>
        public void Shutdown()
        {
            ClearAll();
            _uiCore = null;
            IsInitialized = false;
            Log.Info("[UIManager] UI管理器已关闭");
        }

        /// <summary>
        /// 获取所有面板
        /// </summary>
        public IEnumerable<UIPanel> GetAllPanels()
        {
            return _panels.Values;
        }
    }
}
