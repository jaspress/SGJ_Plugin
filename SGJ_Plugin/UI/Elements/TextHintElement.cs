using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using SGJ_Plugin.UI.Core;

namespace SGJ_Plugin.UI.Elements
{
    public class TextHintElement : UIElement
    {
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
            _hint.Text = IsVisible ? (Content ?? string.Empty) : string.Empty;
            _hint.FontSize = FontSize;
            _hint.XCoordinate = XCoordinate;
            _hint.YCoordinate = YCoordinate;
            _hint.Hide = !IsVisible;
        }
    }
}
