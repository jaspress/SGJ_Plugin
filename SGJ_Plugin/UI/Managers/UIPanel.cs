using SGJ_Plugin.UI.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SGJ_Plugin.UI.Managers
{
    /// <summary>
    /// UI面板
    /// 用于组织和管理多个UI元素
    /// </summary>
    public class UIPanel
    {
        /// <summary>
        /// 面板ID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// 面板标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 面板中的UI元素
        /// </summary>
        private Dictionary<string, UIElement> _elements = new Dictionary<string, UIElement>();

        /// <summary>
        /// 面板是否可见
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// 面板创建时间
        /// </summary>
        public DateTime CreatedTime { get; private set; }

        /// <summary>
        /// 面板宽度（用于布局）
        /// </summary>
        public int Width { get; set; } = 80;

        /// <summary>
        /// 面板高度（用于布局）
        /// </summary>
        public int Height { get; set; } = 20;

        /// <summary>
        /// 面板背景颜色（HSM支持）
        /// </summary>
        public string BackgroundColor { get; set; } = "transparent";

        public UIPanel(string id, string title = "")
        {
            Id = id;
            Title = title;
            CreatedTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 添加UI元素
        /// </summary>
        public void AddElement(UIElement element)
        {
            if (element == null)
                return;

            if (_elements.ContainsKey(element.Id))
            {
                Log.Warning($"[UIPanel] 元素 '{element.Id}' 已存在于面板 '{Id}'");
            }

            _elements[element.Id] = element;
        }

        /// <summary>
        /// 移除UI元素
        /// </summary>
        public bool RemoveElement(string elementId)
        {
            if (_elements.Remove(elementId))
            {
                Log.Debug($"[UIPanel] 从面板 '{Id}' 移除元素 '{elementId}'");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取UI元素
        /// </summary>
        public UIElement GetElement(string elementId)
        {
            _elements.TryGetValue(elementId, out UIElement element);
            return element;
        }

        /// <summary>
        /// 检查元素是否存在
        /// </summary>
        public bool ContainsElement(string elementId)
        {
            return _elements.ContainsKey(elementId);
        }

        /// <summary>
        /// 获取所有元素
        /// </summary>
        public IEnumerable<UIElement> GetAllElements()
        {
            return _elements.Values;
        }

        /// <summary>
        /// 清空所有元素
        /// </summary>
        public void ClearAll()
        {
            foreach (var element in _elements.Values)
            {
                element?.Dispose();
            }
            _elements.Clear();
        }

        /// <summary>
        /// 获取面板的文本表示
        /// 用于调试或导出
        /// </summary>
        public string GetTextRepresentation()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== Panel: {Title} ({Id}) ===");
            sb.AppendLine($"Visible: {IsVisible}");
            sb.AppendLine($"Elements: {_elements.Count}");

            foreach (var element in _elements.Values)
            {
                sb.AppendLine($"  [{element.Id}] {element.Content}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 元素数量
        /// </summary>
        public int ElementCount => _elements.Count;
    }
}
