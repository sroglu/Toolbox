using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sroglu.Toolbox.Screens
{
    /// <summary>
    /// Drives navigation between a set of screen GameObjects. Each screen is
    /// registered under a string key — either in the inspector via
    /// <see cref="entries"/> or at runtime via <see cref="Register"/>. Showing a
    /// screen activates its GameObject (and calls <see cref="IScreen.OnShow"/> if
    /// implemented) while hiding the previous one; the previous key is pushed onto
    /// a back-stack so <see cref="Back"/> can return to it.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        /// <summary>One inspector-configured screen: a key and the GameObject it maps to.</summary>
        [Serializable]
        public struct Entry
        {
            /// <summary>The key used to show/hide this screen.</summary>
            public string key;

            /// <summary>The screen GameObject toggled by <see cref="SetActive"/>.</summary>
            public GameObject screen;
        }

        [Tooltip("Screens configured in the inspector. Registered on Awake.")]
        [SerializeField] private List<Entry> entries = new List<Entry>();

        private readonly Dictionary<string, GameObject> screens = new Dictionary<string, GameObject>();
        private readonly Stack<string> history = new Stack<string>();
        private string currentKey;

        /// <summary>The key of the screen currently shown, or null if none.</summary>
        public string CurrentKey => currentKey;

        private void Awake()
        {
            foreach (Entry entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.key) && entry.screen != null)
                {
                    screens[entry.key] = entry.screen;
                }
            }
        }

        /// <summary>
        /// Registers a screen under <paramref name="key"/> at runtime, replacing any
        /// screen already registered under that key.
        /// </summary>
        /// <param name="key">The key to show/hide the screen by.</param>
        /// <param name="screen">The screen GameObject.</param>
        public void Register(string key, GameObject screen)
        {
            screens[key] = screen;
        }

        /// <summary>
        /// Hides the current screen, pushes it onto the back-stack, then activates
        /// the screen registered under <paramref name="key"/> and calls
        /// <see cref="IScreen.OnShow"/> if it implements <see cref="IScreen"/>.
        /// </summary>
        /// <param name="key">The key of the screen to show.</param>
        /// <exception cref="KeyNotFoundException">If no screen is registered under <paramref name="key"/>.</exception>
        public void Show(string key)
        {
            if (!screens.TryGetValue(key, out GameObject target))
            {
                throw new KeyNotFoundException($"No screen registered under key '{key}'.");
            }

            if (currentKey == key)
            {
                Activate(target);
                return;
            }

            if (currentKey != null)
            {
                Deactivate(screens[currentKey]);
                history.Push(currentKey);
            }

            currentKey = key;
            Activate(target);
        }

        /// <summary>
        /// Pops the back-stack and shows the previous screen. No-op if the stack is
        /// empty. Alias: this is the "show last" operation.
        /// </summary>
        public void Back()
        {
            if (history.Count == 0)
            {
                return;
            }

            string previous = history.Pop();

            if (currentKey != null && screens.TryGetValue(currentKey, out GameObject current))
            {
                Deactivate(current);
            }

            currentKey = previous;

            if (screens.TryGetValue(previous, out GameObject target))
            {
                Activate(target);
            }
        }

        /// <summary>Alias for <see cref="Back"/> — shows the previous screen on the back-stack.</summary>
        public void ShowLast()
        {
            Back();
        }

        /// <summary>
        /// Deactivates the screen registered under <paramref name="key"/> and calls
        /// <see cref="IScreen.OnHide"/> if implemented. Clears the current key if it
        /// was the shown screen. No-op if the key is not registered.
        /// </summary>
        /// <param name="key">The key of the screen to hide.</param>
        public void Hide(string key)
        {
            if (!screens.TryGetValue(key, out GameObject target))
            {
                return;
            }

            Deactivate(target);

            if (currentKey == key)
            {
                currentKey = null;
            }
        }

        /// <summary>Clears the back-stack and the current key. Does not toggle any GameObject.</summary>
        public void Clear()
        {
            history.Clear();
            currentKey = null;
        }

        private static void Activate(GameObject screen)
        {
            if (screen == null)
            {
                return;
            }

            screen.SetActive(true);

            if (screen.TryGetComponent(out IScreen hook))
            {
                hook.OnShow();
            }
        }

        private static void Deactivate(GameObject screen)
        {
            if (screen == null)
            {
                return;
            }

            if (screen.TryGetComponent(out IScreen hook))
            {
                hook.OnHide();
            }

            screen.SetActive(false);
        }
    }
}
