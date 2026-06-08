using Exiled.API.Features;
using SGJ_Plugin.UI.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SGJ_Plugin.UI.Managers
{
    public class UIPanel
    {
        private readonly Dictionary<string, UIElement> _elements = new Dictionary<string, UIElement>();

        public string Id { get; private set; }

        public string Title { get; set; }

        public bool IsVisible { get; set; } = true;

        public DateTime CreatedTime { get; private set; }

        public UIPanel(string id, string title = "")
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Panel id cannot be empty.", nameof(id));

            Id = id;
            Title = title ?? string.Empty;
            CreatedTime = DateTime.UtcNow;
        }

        public void AddElement(UIElement element)
        {
            if (element == null)
                return;

            if (_elements.ContainsKey(element.Id))
                Log.Warn($"[UIPanel] Element '{element.Id}' already exists in panel '{Id}', replacing it.");

            _elements[element.Id] = element;
        }

        public bool RemoveElement(string elementId)
        {
            if (!_elements.TryGetValue(elementId, out UIElement element))
                return false;

            element.Dispose();
            _elements.Remove(elementId);
            return true;
        }

        public UIElement GetElement(string elementId)
        {
            _elements.TryGetValue(elementId, out UIElement element);
            return element;
        }

        public bool ContainsElement(string elementId)
        {
            return _elements.ContainsKey(elementId);
        }

        public IEnumerable<UIElement> GetAllElements()
        {
            return _elements.Values;
        }

        public void Refresh()
        {
            foreach (UIElement element in _elements.Values)
                element.Update();
        }

        public void ClearAll()
        {
            foreach (UIElement element in _elements.Values)
                element.Dispose();

            _elements.Clear();
        }

        public string GetTextRepresentation()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== Panel: {Title} ({Id}) ===");
            sb.AppendLine($"Visible: {IsVisible}");
            sb.AppendLine($"Elements: {_elements.Count}");

            foreach (UIElement element in _elements.Values)
                sb.AppendLine($"  [{element.Id}] {element.Content}");

            return sb.ToString();
        }

        public int ElementCount => _elements.Count;
    }
}
