using Exiled.API.Features;
using System;

namespace SGJ_Plugin.Modules
{
    /// <summary>
    /// Base class for plugin feature modules.
    /// </summary>
    public abstract class ModuleBase : IDisposable
    {
        public abstract string Name { get; }

        public bool IsEnabled { get; protected set; }

        public virtual void Enable()
        {
            if (IsEnabled)
                return;

            try
            {
                OnEnable();
                IsEnabled = true;
                Log.Info($"[{Name}] Enabled.");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to enable module: {ex}");
                IsEnabled = false;
            }
        }

        public virtual void Disable()
        {
            if (!IsEnabled)
                return;

            try
            {
                OnDisable();
                IsEnabled = false;
                Log.Info($"[{Name}] Disabled.");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] Failed to disable module: {ex}");
            }
        }

        /// <summary>
        /// Called when the module is enabled.
        /// </summary>
        protected abstract void OnEnable();

        /// <summary>
        /// Called when the module is disabled.
        /// </summary>
        protected abstract void OnDisable();

        /// <summary>
        /// Releases module resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (IsEnabled)
                Disable();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer fallback for module cleanup.
        /// </summary>
        ~ModuleBase()
        {
            Dispose();
        }
    }
}
