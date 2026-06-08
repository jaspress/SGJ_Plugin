using Hints;
using HintServiceMeow.Core.Models.Hints;
using System;

namespace SGJ_Plugin.UI.Core
{
    /// <summary>
    /// UI元素基类
    /// 所有UI元素都应继承此类
    /// </summary>
    public abstract class UIElement
    {
        /// <summary>
        /// 元素唯一ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 元素显示内容
        /// </summary>
        public virtual string Content { get; set; }

        /// <summary>
        /// 元素是否可见
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// 元素更新间隔（毫秒）
        /// 0表示仅更新一次
        /// </summary>
        public int UpdateInterval { get; set; } = 0;

        /// <summary>
        /// 元素创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 上次更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 初始化UI元素
        /// </summary>
        public UIElement(string id)
        {
            Id = id;
            CreatedTime = DateTime.UtcNow;
            LastUpdateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 获取HSM提示对象
        /// 由子类实现具体的提示类型
        /// </summary>
        public abstract TextHint GetHintObject();

        /// <summary>
        /// 更新元素内容
        /// </summary>
        public virtual void Update()
        {
            LastUpdateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 检查是否需要更新
        /// </summary>
        public bool ShouldUpdate()
        {
            if (UpdateInterval == 0)
                return false;

            TimeSpan elapsed = DateTime.UtcNow - LastUpdateTime;
            return elapsed.TotalMilliseconds >= UpdateInterval;
        }

        /// <summary>
        /// 销毁元素
        /// </summary>
        public virtual void Dispose()
        {
            // 子类可以重写此方法进行清理
        }
    }
}
