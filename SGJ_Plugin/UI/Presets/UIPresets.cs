using HintServiceMeow.Core.Enum;
using SGJ_Plugin.UI.Elements;
using SGJ_Plugin.UI.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SGJ_Plugin.UI.Presets
{
    public static class UIPresets
    {
        public static UIPanel CreateInfoPanel(string title, string content)
        {
            string panelId = CreatePanelId("info_panel");
            UIPanel panel = UIManager.Instance.CreatePanel(panelId, title);
            UIManager.Instance.CreateTextHint(panelId, "content", content);
            return panel;
        }

        public static UIPanel CreateProgressPanel(string title, int current, int maxValue)
        {
            string panelId = CreatePanelId("progress_panel");
            UIPanel panel = UIManager.Instance.CreatePanel(panelId, title);
            TextHintElement element = UIManager.Instance.CreateTextHint(panelId, "progress", GetProgressBar(current, maxValue));
            element.Alignment = HintAlignment.Center;
            return panel;
        }

        public static UIPanel CreateStatusPanel(string title, Dictionary<string, string> statusItems)
        {
            string panelId = CreatePanelId("status_panel");
            UIPanel panel = UIManager.Instance.CreatePanel(panelId, title);
            int index = 0;

            foreach (KeyValuePair<string, string> item in statusItems)
            {
                UIManager.Instance.CreateTextHint(panelId, $"status_item_{index}", $"{item.Key}: {item.Value}");
                index++;
            }

            return panel;
        }

        public static UIPanel CreateListPanel(string title, List<string> items)
        {
            string panelId = CreatePanelId("list_panel");
            UIPanel panel = UIManager.Instance.CreatePanel(panelId, title);

            for (int i = 0; i < items.Count; i++)
                UIManager.Instance.CreateTextHint(panelId, $"item_{i}", $"{i + 1}. {items[i]}");

            return panel;
        }

        public static UIPanel CreateNotificationPanel(string message)
        {
            string panelId = CreatePanelId("notification_panel");
            UIPanel panel = UIManager.Instance.CreatePanel(panelId, "Notification");
            UIManager.Instance.CreateTextHint(panelId, "message", message);
            return panel;
        }

        public static UIPanel CreateWarningPanel(string title, string message)
        {
            string panelId = CreatePanelId("warning_panel");
            UIPanel panel = UIManager.Instance.CreatePanel(panelId, title);
            UIManager.Instance.CreateTextHint(panelId, "warning", $"[!] {message}");
            return panel;
        }

        public static UIPanel CreateErrorPanel(string title, string errorMessage)
        {
            string panelId = CreatePanelId("error_panel");
            UIPanel panel = UIManager.Instance.CreatePanel(panelId, title);
            UIManager.Instance.CreateTextHint(panelId, "error", $"[X] {errorMessage}");
            return panel;
        }

        public static UIPanel CreateSuccessPanel(string title, string message)
        {
            string panelId = CreatePanelId("success_panel");
            UIPanel panel = UIManager.Instance.CreatePanel(panelId, title);
            UIManager.Instance.CreateTextHint(panelId, "success", $"[OK] {message}");
            return panel;
        }

        private static string GetProgressBar(int current, int max, int length = 20)
        {
            if (max <= 0)
                max = 1;

            int filledLength = Math.Max(0, Math.Min(length, (int)Math.Round((double)current / max * length)));
            int percentage = Math.Max(0, Math.Min(100, (int)Math.Round((double)current / max * 100)));

            StringBuilder bar = new StringBuilder(length);
            bar.Append(new string('|', filledLength));
            bar.Append(new string('.', length - filledLength));

            return $"[{bar}] {percentage}% ({current}/{max})";
        }

        private static string CreatePanelId(string prefix)
        {
            return $"{prefix}_{Guid.NewGuid():N}".Substring(0, prefix.Length + 9);
        }
    }
}
