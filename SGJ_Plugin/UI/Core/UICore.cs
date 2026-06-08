using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Managers.Providers;
using System;
using System.Collections.Generic;

namespace SGJ_Plugin.UI.Core
{
    /// <summary>
    /// UI系统基础类
    /// 基于HSM（HintServiceMeow）库的提示系统
    /// </summary>
    public class UICore
    {
        /// <summary>
        /// HSM提示提供者
        /// </summary>
        public IHintProvider HintProvider { get; private set; }

        /// <summary>
        /// 所有激活的UI元素
        /// </summary>
        private Dictionary<string, UIElement> _activeElements = new Dictionary<string, UIElement>();

        /// <summary>
        /// UI系统是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// 初始化UI系统
        /// </summary>
        public bool Initialize()
        {
            try
            {
                // 获取HSM的提示提供者
                HintProvider = HintProvider.AddProvider();

                IsInitialized = true;
                Log.Info("[UICore] UI系统已初始化");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[UICore] 初始化UI系统失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 注册UI元素
        /// </summary>
        public void RegisterElement(string id, UIElement element)
        {
            if (!IsInitialized)
            {
                Log.Warning("[UICore] UI系统未初始化，无法注册元素");
                return;
            }

            if (_activeElements.ContainsKey(id))
            {
                Log.Warning($"[UICore] UI元素 '{id}' 已存在，将被覆盖");
            }

            _activeElements[id] = element;
        }

        /// <summary>
        /// 注销UI元素
        /// </summary>
        public void UnregisterElement(string id)
        {
            if (_activeElements.Remove(id))
            {
                Log.Debug($"[UICore] UI元素 '{id}' 已注销");
            }
        }

        /// <summary>
        /// 获取UI元素
        /// </summary>
        public UIElement GetElement(string id)
        {
            _activeElements.TryGetValue(id, out UIElement element);
            return element;
        }

        /// <summary>
        /// 检查UI元素是否存在
        /// </summary>
        public bool ElementExists(string id)
        {
            return _activeElements.ContainsKey(id);
        }

        /// <summary>
        /// 获取所有激活元素
        /// </summary>
        public IEnumerable<UIElement> GetAllElements()
        {
            return _activeElements.Values;
        }

        /// <summary>
        /// 清空所有UI元素
        /// </summary>
        public void ClearAll()
        {
            _activeElements.Clear();
            Log.Debug("[UICore] 所有UI元素已清空");
        }
    }
}
