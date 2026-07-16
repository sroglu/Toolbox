using UnityEngine;

namespace Sroglu.Toolbox.ViewManagement.Samples
{
    /// <summary>
    /// A page view: the in-game screen. Logs when it is shown or hidden.
    /// </summary>
    public class GameView : ViewComponent
    {
        /// <inheritdoc />
        public override void Show()
        {
            base.Show();
            Debug.Log("GameView.Show");
        }

        /// <inheritdoc />
        public override void Hide()
        {
            Debug.Log("GameView.Hide");
            base.Hide();
        }
    }
}
