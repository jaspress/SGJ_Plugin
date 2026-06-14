using Exiled.API.Features;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SGJ_Plugin.UI.Core
{
    public class UICore
    {
        private const string GroupName = "SGJ_Plugin.UI";
        private const int MaxPlayerHintBytes = 52000;
        private readonly Dictionary<string, List<AbstractHint>> _playerHints = new Dictionary<string, List<AbstractHint>>();

        public bool IsInitialized { get; private set; }

        public bool Initialize()
        {
            IsInitialized = true;
            Log.Info("[UICore] UI system initialized with HintServiceMeow PlayerDisplay.");
            return true;
        }

        public void Show(Player player, IEnumerable<UIElement> elements)
        {
            if (!IsInitialized || player == null || elements == null)
                return;

            string key = GetPlayerKey(player);
            PlayerDisplay display = PlayerDisplay.Get(player);

            if (!_playerHints.TryGetValue(key, out List<AbstractHint> hints))
            {
                hints = new List<AbstractHint>();
                _playerHints[key] = hints;
            }

            int usedBytes = 0;
            foreach (UIElement element in elements.Where(x => x != null))
            {
                AbstractHint hint = element.GetHintObject();
                if (hints.Contains(hint))
                {
                    display.RemoveHint(hint, GroupName);
                    hints.Remove(hint);
                }

                element.Update();
                if (element.IsVisible && !string.IsNullOrWhiteSpace(element.Content))
                {
                    int elementBytes = System.Text.Encoding.UTF8.GetByteCount(element.Content);
                    if (usedBytes + elementBytes > MaxPlayerHintBytes)
                    {
                        Log.Debug($"[UICore] Skipped UI element '{element.Id}' for {player.Nickname}: hint byte budget exceeded.");
                        continue;
                    }

                    display.AddHint(hint, GroupName);
                    hints.Add(hint);
                    usedBytes += elementBytes;
                }
            }

            if (hints.Count == 0)
                _playerHints.Remove(key);

            display.ForceUpdate(true);
        }

        public void Remove(Player player, IEnumerable<UIElement> elements)
        {
            if (player == null || elements == null)
                return;

            string key = GetPlayerKey(player);
            if (!_playerHints.TryGetValue(key, out List<AbstractHint> shownHints))
                return;

            PlayerDisplay display = PlayerDisplay.Get(player);
            foreach (UIElement element in elements.Where(x => x != null))
            {
                AbstractHint hint = element.GetHintObject();
                display.RemoveHint(hint, GroupName);
                shownHints.Remove(hint);
            }

            if (shownHints.Count == 0)
                _playerHints.Remove(key);

            display.ForceUpdate(true);
        }

        public void Clear(Player player)
        {
            if (player == null)
                return;

            string key = GetPlayerKey(player);
            if (!_playerHints.TryGetValue(key, out List<AbstractHint> hints))
                return;

            PlayerDisplay display = PlayerDisplay.Get(player);
            foreach (AbstractHint hint in hints.ToArray())
                display.RemoveHint(hint, GroupName);

            _playerHints.Remove(key);
            display.ForceUpdate(true);
        }

        public void Forget(Player player)
        {
            if (player == null)
                return;

            _playerHints.Remove(GetPlayerKey(player));
        }

        public void ClearAll()
        {
            foreach (Player player in Player.List)
                Clear(player);

            _playerHints.Clear();
        }

        private static string GetPlayerKey(Player player)
        {
            return string.IsNullOrEmpty(player.UserId) ? player.Id.ToString() : player.UserId;
        }
    }
}
