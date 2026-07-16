namespace Sroglu.Toolbox.ViewManagement
{
    /// <summary>
    /// The contract every view the <see cref="ViewManager"/> drives must satisfy:
    /// it can be shown and hidden. This is the manager's own managed-view contract
    /// and is intentionally independent of any other package's <c>IView</c> marker.
    /// </summary>
    public interface IView
    {
        /// <summary>Makes the view visible / active.</summary>
        void Show();

        /// <summary>Makes the view hidden / inactive.</summary>
        void Hide();
    }
}
