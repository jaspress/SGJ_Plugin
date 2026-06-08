using HintServiceMeow.Core.Models.Hints;
using System;

namespace SGJ_Plugin.UI.Core
{
    public abstract class UIElement : IDisposable
    {
        public string Id { get; set; }

        public virtual string Content { get; set; }

        public bool IsVisible { get; set; } = true;

        public int UpdateInterval { get; set; } = 0;

        public int FontSize { get; set; } = 24;

        public float XCoordinate { get; set; } = 0f;

        public float YCoordinate { get; set; } = 700f;

        public DateTime CreatedTime { get; private set; }

        public DateTime LastUpdateTime { get; private set; }

        protected UIElement(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("UI element id cannot be empty.", nameof(id));

            Id = id;
            CreatedTime = DateTime.UtcNow;
            LastUpdateTime = DateTime.UtcNow;
            Content = string.Empty;
        }

        public abstract AbstractHint GetHintObject();

        public virtual void Update()
        {
            LastUpdateTime = DateTime.UtcNow;
            ApplyToHint();
        }

        public bool ShouldUpdate()
        {
            if (UpdateInterval <= 0)
                return false;

            return (DateTime.UtcNow - LastUpdateTime).TotalMilliseconds >= UpdateInterval;
        }

        protected virtual void ApplyToHint()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
