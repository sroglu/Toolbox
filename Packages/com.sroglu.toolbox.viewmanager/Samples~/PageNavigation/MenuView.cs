using UnityEngine;

namespace Sroglu.Toolbox.ViewManagement.Samples
{
    /// <summary>
    /// A page view: the main menu. Derives from <see cref="ViewComponent"/> and
    /// logs when it is shown or hidden so the navigation flow is visible in the
    /// console.
    /// </summary>
    public class MenuView : ViewComponent
    {
        /// <inheritdoc />
        public override void Show()
        {
            base.Show();
            Debug.Log("MenuView.Show");
        }

        /// <inheritdoc />
        public override void Hide()
        {
            Debug.Log("MenuView.Hide");
            base.Hide();
        }
    }
}
