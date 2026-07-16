using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Sroglu.Toolbox.UITools
{
    /// <summary>
    /// Editor play-mode debug helper for uGUI. While <c>Ctrl</c> (left or right) is held,
    /// it raycasts the UI under the mouse each frame, finds the topmost element, and reveals
    /// it in the Hierarchy (select + ping). Re-selection happens only when the topmost element
    /// changes, so hovering with Ctrl held live-updates the selection without spamming pings.
    /// </summary>
    /// <remarks>
    /// The tool auto-injects itself in Play mode inside the editor (see <see cref="AutoInject"/>)
    /// when the <see cref="EnabledPrefKey"/> preference is on, so no manual component placement
    /// is required. It reads input through the legacy Input Manager or the new Input System,
    /// whichever backend the project has active.
    /// </remarks>
    public sealed class UiElementPicker : MonoBehaviour
    {
        /// <summary>
        /// EditorPrefs key that gates auto-injection and drives the Tools-menu checkmark.
        /// Shared with the editor menu toggle.
        /// </summary>
        public const string EnabledPrefKey = "Sroglu.Toolbox.UITools.UiElementPicker.Enabled";

        readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        GameObject lastRevealed;
        bool warnedMissingEventSystem;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoInject()
        {
            if (!UnityEditor.EditorPrefs.GetBool(EnabledPrefKey, true))
                return;

            GameObject host = new GameObject(nameof(UiElementPicker));
            host.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(host);
            host.AddComponent<UiElementPicker>();
        }
#endif

        void Update()
        {
            if (!IsControlHeld())
            {
                lastRevealed = null;
                return;
            }

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                WarnMissingEventSystemOnce();
                return;
            }

            PointerEventData pointer = new PointerEventData(eventSystem) { position = ReadMousePosition() };
            raycastResults.Clear();
            eventSystem.RaycastAll(pointer, raycastResults);
            if (raycastResults.Count == 0)
                return;

            GameObject topmost = raycastResults[0].gameObject;
            if (topmost == lastRevealed)
                return;

            lastRevealed = topmost;
            Reveal(topmost);
        }

        static void Reveal(GameObject target)
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = target;
            UnityEditor.EditorGUIUtility.PingObject(target);
#endif
        }

        void WarnMissingEventSystemOnce()
        {
            if (warnedMissingEventSystem)
                return;

            warnedMissingEventSystem = true;
            Debug.LogWarning(
                "[UiElementPicker] No EventSystem in the scene. uGUI picking needs an active " +
                "UnityEngine.EventSystems.EventSystem to raycast the Canvas. " +
                "Add one via GameObject → UI → Event System.");
        }

        static bool IsControlHeld()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#else
            return false;
#endif
        }

        static Vector2 ReadMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
#else
            return Vector2.zero;
#endif
        }
    }
}
