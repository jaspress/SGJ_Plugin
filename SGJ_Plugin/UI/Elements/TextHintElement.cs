using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using SGJ_Plugin.UI.Core;

namespace SGJ_Plugin.UI.Elements
{
    public class TextHintElement : UIElement
    {
        private const int MaxTextBytes = 12000;
        private readonly Hint _hint;

        public TextHintElement(string id, string content = "") : base(id)
        {
            Content = content ?? string.Empty;
            _hint = new Hint
            {
                Id = id,
                Text = Content,
                FontSize = FontSize,
                XCoordinate = XCoordinate,
                YCoordinate = YCoordinate,
                Alignment = HintAlignment.Center,
                SyncSpeed = HintSyncSpeed.UnSync,
            };
        }

        public HintAlignment Alignment
        {
            get => _hint.Alignment;
            set => _hint.Alignment = value;
        }

        public override AbstractHint GetHintObject()
        {
            return _hint;
        }

        protected override void ApplyToHint()
        {
            _hint.Id = Id;
            _hint.Text = IsVisible ? TrimToByteLimit(Content ?? string.Empty, MaxTextBytes) : string.Empty;
            _hint.FontSize = FontSize;
            _hint.XCoordinate = XCoordinate;
            _hint.YCoordinate = YCoordinate;
            _hint.Hide = !IsVisible;
        }

        private static string TrimToByteLimit(string value, int maxBytes)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (System.Text.Encoding.UTF8.GetByteCount(value) <= maxBytes)
                return value;

            string suffix = "\n<size=14><color=#FF9999>[UI内容过长，已自动截断]</color></size>";
            int suffixBytes = System.Text.Encoding.UTF8.GetByteCount(suffix);
            int budget = System.Math.Max(128, maxBytes - suffixBytes);
            int end = value.Length;

            while (end > 0 && System.Text.Encoding.UTF8.GetByteCount(value.Substring(0, end)) > budget)
                end = System.Math.Max(0, end - 128);

            return value.Substring(0, end) + suffix;
        }
    }
}
