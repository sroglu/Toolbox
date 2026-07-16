using UnityEngine;

namespace Sroglu.Toolbox.ViewManagement.Samples
{
    /// <summary>
    /// An instance view: a popup created on demand from a prefab (assigned to the
    /// ViewManager's Instance Prefabs list) rather than pre-placed in the scene.
    /// Logs when it is shown or hidden.
    /// </summary>
    public class PopupView : ViewComponent
    {
        /// <inheritdoc />
        public override void Show()
        {
            base.Show();
            Debug.Log("PopupView.Show");
        }

        /// <inheritdoc />
        public override void Hide()
        {
            Debug.Log("PopupView.Hide");
            base.Hide();
        }
    }
}
