using Exiled.API.Features;
using SGJ_Plugin.UI.Core;
using SGJ_Plugin.UI.Elements;
using System;
using System.Collections.Generic;

namespace SGJ_Plugin.UI.Managers
{
    public class UIManager
    {
        private static UIManager _instance;
        private readonly Dictionary<string, UIPanel> _panels = new Dictionary<string, UIPanel>();
        private UICore _uiCore;

        public static UIManager Instance => _instance ?? (_instance = new UIManager());

        public bool IsInitialized { get; private set; }

        private UIManager()
        {
        }

        public bool Initialize()
        {
            if (IsInitialized)
                return true;

            _uiCore = new UICore();
            if (!_uiCore.Initialize())
                return false;

            IsInitialized = true;
            Log.Info("[UIManager] UI manager initialized.");
            return true;
        }

        public UIPanel CreatePanel(string id, string title = "")
        {
            if (!EnsureInitialized())
                return null;

            if (_panels.TryGetValue(id, out UIPanel existing))
                return existing;

            UIPanel panel = new UIPanel(id, title);
            _panels[id] = panel;
            return panel;
        }

        public UIPanel GetPanel(string id)
        {
            _panels.TryGetValue(id, out UIPanel panel);
            return panel;
        }

        public bool RemovePanel(string id)
        {
            if (!_panels.TryGetValue(id, out UIPanel panel))
                return false;

            panel.ClearAll();
            _panels.Remove(id);
            return true;
        }

        public TextHintElement CreateTextHint(string panelId, string elementId, string content = "")
        {
            UIPanel panel = GetPanel(panelId);
            if (panel == null)
            {
                Log.Warn($"[UIManager] Panel '{panelId}' does not exist.");
                return null;
            }

            TextHintElement element = new TextHintElement(elementId, content);
            panel.AddElement(element);
            return element;
        }

        public bool ShowPanel(Player player, string panelId)
        {
            UIPanel panel = GetPanel(panelId);
            if (player == null || panel == null || !panel.IsVisible)
                return false;

            panel.Refresh();
            _uiCore.Show(player, panel.GetAllElements());
            return true;
        }

        public bool HidePanel(Player player, string panelId)
        {
            UIPanel panel = GetPanel(panelId);
            if (player == null || panel == null)
                return false;

            _uiCore.Remove(player, panel.GetAllElements());
            return true;
        }

        public void ClearPlayer(Player player)
        {
            _uiCore?.Clear(player);
        }

        public void ForgetPlayer(Player player)
        {
            _uiCore?.Forget(player);
        }

        public bool RemoveElement(string panelId, string elementId)
        {
            UIPanel panel = GetPanel(panelId);
            return panel != null && panel.RemoveElement(elementId);
        }

        public UIElement GetElement(string panelId, string elementId)
        {
            return GetPanel(panelId)?.GetElement(elementId);
        }

        public void ClearAll()
        {
            _uiCore?.ClearAll();

            foreach (UIPanel panel in _panels.Values)
                panel.ClearAll();

            _panels.Clear();
        }

        public void Shutdown()
        {
            ClearAll();
            _uiCore = null;
            IsInitialized = false;
            Log.Info("[UIManager] UI manager shut down.");
        }

        public IEnumerable<UIPanel> GetAllPanels()
        {
            return _panels.Values;
        }

        private bool EnsureInitialized()
        {
            if (IsInitialized)
                return true;

            return Initialize();
        }
    }
}
