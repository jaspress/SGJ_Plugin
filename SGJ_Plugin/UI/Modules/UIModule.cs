using Exiled.API.Features;
using SGJ_Plugin.Modules;
using SGJ_Plugin.UI.Managers;
using System;

namespace SGJ_Plugin.UI.Modules
{
    public class UIModule : ModuleBase
    {
        private readonly Config _config;
        private UIManager _uiManager;

        public override string Name => "UI System Module";

        public UIModule(Config config)
        {
            _config = config;
        }

        protected override void OnEnable()
        {
            _uiManager = UIManager.Instance;
            if (!_uiManager.Initialize())
                throw new InvalidOperationException("UI manager failed to initialize.");

            if (_config.Debug)
                Log.Debug($"[{Name}] UI system is ready.");
        }

        protected override void OnDisable()
        {
            _uiManager?.Shutdown();
        }

        public UIManager GetUIManager()
        {
            return _uiManager;
        }
    }
}
