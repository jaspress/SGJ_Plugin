using Hints;
using HintServiceMeow.Core.Models.Hints;
using SGJ_Plugin.UI.Core;
using System;

namespace SGJ_Plugin.UI.Elements
{
    /// <summary>
    /// 简单文本提示元素
    /// 用于显示静态或动态文本内容
    /// </summary>
    public class TextHintElement : UIElement
    {
        private TextHint _hint;

        public TextHintElement(string id, string content = "") : base(id)
        {
            Content = content;
            _hint = new TextHint(content, new HintParameter(1, 1), null, null);
        }

        public override TextHint GetHintObject()
        {
            return _hint;
        }

        public override void Update()
        {
            base.Update();
            _hint.Content = Content;
        }

        public override void Dispose()
        {
            _hint = null;
            base.Dispose();
        }
    }
}
