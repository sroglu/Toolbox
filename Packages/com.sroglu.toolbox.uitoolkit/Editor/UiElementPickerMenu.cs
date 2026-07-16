using UnityEditor;
using Sroglu.Toolbox.UITools;

namespace Sroglu.Toolbox.UITools.Editor
{
    /// <summary>
    /// Tools-menu toggle that enables or disables the <see cref="UiElementPicker"/> auto-injector.
    /// The state persists in <see cref="EditorPrefs"/> and is mirrored as a menu checkmark, so it
    /// survives editor restarts. Default is on.
    /// </summary>
    static class UiElementPickerMenu
    {
        const string MenuPath = "Tools/Toolbox/UI Element Picker (Ctrl+Hover)";

        [MenuItem(MenuPath)]
        static void Toggle()
        {
            bool enabled = EditorPrefs.GetBool(UiElementPicker.EnabledPrefKey, true);
            EditorPrefs.SetBool(UiElementPicker.EnabledPrefKey, !enabled);
        }

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(UiElementPicker.EnabledPrefKey, true));
            return true;
        }
    }
}
