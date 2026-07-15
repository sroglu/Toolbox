namespace Sroglu.Toolbox.Screens
{
    /// <summary>
    /// Optional lifecycle hooks a screen <c>MonoBehaviour</c> may implement.
    /// The <see cref="ScreenManager"/> invokes these when it shows or hides a
    /// screen — implementing the interface is not required.
    /// </summary>
    public interface IScreen
    {
        /// <summary>Called after the screen's GameObject is activated.</summary>
        void OnShow();

        /// <summary>Called before the screen's GameObject is deactivated.</summary>
        void OnHide();
    }
}
