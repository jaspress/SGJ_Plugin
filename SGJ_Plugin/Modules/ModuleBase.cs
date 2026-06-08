using Exiled.API.Features;
using System;

namespace SGJ_Plugin.Modules
{
    /// <summary>
    /// 插件功能模块基类
    /// 提供模块的生命周期管理和错误处理
    /// 符合 EXILED 框架设计模式
    /// </summary>
    public abstract class ModuleBase : IDisposable
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 模块是否已启用
        /// </summary>
        public bool IsEnabled { get; protected set; }

        /// <summary>
        /// 启用模块
        /// </summary>
        public virtual void Enable()
        {
            if (IsEnabled)
                return;

            try
            {
                OnEnable();
                IsEnabled = true;
                Log.Info($"[{Name}] 模块已启用");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] 启用模块时出错: {ex.Message}\n{ex.StackTrace}");
                IsEnabled = false;
            }
        }

        /// <summary>
        /// 禁用模块
        /// </summary>
        public virtual void Disable()
        {
            if (!IsEnabled)
                return;

            try
            {
                OnDisable();
                IsEnabled = false;
                Log.Info($"[{Name}] 模块已禁用");
            }
            catch (Exception ex)
            {
                Log.Error($"[{Name}] 禁用模块时出错: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 模块启用时的处理逻辑（由子类实现）
        /// </summary>
        protected abstract void OnEnable();

        /// <summary>
        /// 模块禁用时的处理逻辑（由子类实现）
        /// </summary>
        protected abstract void OnDisable();

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (IsEnabled)
                Disable();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 析构函数确保资源被释放
        /// </summary>
        ~ModuleBase()
        {
            Dispose();
        }
    }
}
