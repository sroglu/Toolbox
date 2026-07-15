namespace Sroglu.Toolbox.Mvp
{
    /// <summary>
    /// Marker interface implemented by a concrete, passive view.
    /// A view exposes methods for the presenter to push display data and events
    /// it raises for user input. It never references the model.
    /// </summary>
    public interface IView
    {
    }
}
