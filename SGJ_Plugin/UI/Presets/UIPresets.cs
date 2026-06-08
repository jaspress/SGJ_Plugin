using SGJ_Plugin.UI.Core;
using SGJ_Plugin.UI.Elements;
using SGJ_Plugin.UI.Managers;
using System;
using System.Collections.Generic;

namespace SGJ_Plugin.UI.Presets
{
    /// <summary>
    /// UI预制组件
    /// 提供常见的UI场景快速创建方法
    /// </summary>
    public static class UIPresets
    {
        /// <summary>
        /// 创建信息面板
        /// </summary>
        public static UIPanel CreateInfoPanel(string title, string content)
        {
            var panelId = $"info_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, title);

            if (panel != null)
            {
                UIManager.Instance.CreateTextHint(panelId, "content", content);
            }

            return panel;
        }

        /// <summary>
        /// 创建计时器面板
        /// </summary>
        public static UIPanel CreateTimerPanel(string title, int durationSeconds)
        {
            var panelId = $"timer_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, title);

            if (panel != null)
            {
                var startTime = DateTime.UtcNow;
                var element = UIManager.Instance.CreateTextHint(panelId, "timer", "");

                element.UpdateInterval = 100;
                element.Content = $"剩余时间: {durationSeconds}s";

                // 更新逻辑由调用者实现
            }

            return panel;
        }

        /// <summary>
        /// 创建进度条面板
        /// </summary>
        public static UIPanel CreateProgressPanel(string title, int maxValue)
        {
            var panelId = $"progress_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, title);

            if (panel != null)
            {
                var element = UIManager.Instance.CreateTextHint(panelId, "progress", "");
                element.UpdateInterval = 100;
                element.Content = GetProgressBar(0, maxValue);
            }

            return panel;
        }

        /// <summary>
        /// 创建状态面板
        /// </summary>
        public static UIPanel CreateStatusPanel(string title, Dictionary<string, string> statusItems)
        {
            var panelId = $"status_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, title);

            if (panel != null)
            {
                int index = 0;
                foreach (var item in statusItems)
                {
                    var element = UIManager.Instance.CreateTextHint(
                        panelId,
                        $"status_item_{index}",
                        $"{item.Key}: {item.Value}"
                    );
                    index++;
                }
            }

            return panel;
        }

        /// <summary>
        /// 创建列表面板
        /// </summary>
        public static UIPanel CreateListPanel(string title, List<string> items)
        {
            var panelId = $"list_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, title);

            if (panel != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    UIManager.Instance.CreateTextHint(
                        panelId,
                        $"item_{i}",
                        $"{i + 1}. {items[i]}"
                    );
                }
            }

            return panel;
        }

        /// <summary>
        /// 获取进度条字符串
        /// </summary>
        private static string GetProgressBar(int current, int max, int length = 20)
        {
            if (max == 0) max = 1;

            int filledLength = (int)((float)current / max * length);
            string bar = new string('█', filledLength) + new string('?', length - filledLength);
            int percentage = (int)((float)current / max * 100);

            return $"[{bar}] {percentage}% ({current}/{max})";
        }

        /// <summary>
        /// 创建简单通知面板
        /// </summary>
        public static UIPanel CreateNotificationPanel(string message, int durationSeconds = 5)
        {
            var panelId = $"notification_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, "通知");

            if (panel != null)
            {
                var element = UIManager.Instance.CreateTextHint(panelId, "message", message);
                element.UpdateInterval = 100;
            }

            return panel;
        }

        /// <summary>
        /// 创建警告面板
        /// </summary>
        public static UIPanel CreateWarningPanel(string title, string message)
        {
            var panelId = $"warning_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, title);

            if (panel != null)
            {
                var warningIcon = "?? ";
                UIManager.Instance.CreateTextHint(panelId, "warning", warningIcon + message);
            }

            return panel;
        }

        /// <summary>
        /// 创建错误面板
        /// </summary>
        public static UIPanel CreateErrorPanel(string title, string errorMessage)
        {
            var panelId = $"error_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, title);

            if (panel != null)
            {
                var errorIcon = "? ";
                UIManager.Instance.CreateTextHint(panelId, "error", errorIcon + errorMessage);
            }

            return panel;
        }

        /// <summary>
        /// 创建成功面板
        /// </summary>
        public static UIPanel CreateSuccessPanel(string title, string message)
        {
            var panelId = $"success_panel_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var panel = UIManager.Instance.CreatePanel(panelId, title);

            if (panel != null)
            {
                var successIcon = "? ";
                UIManager.Instance.CreateTextHint(panelId, "success", successIcon + message);
            }

            return panel;
        }
    }
}
