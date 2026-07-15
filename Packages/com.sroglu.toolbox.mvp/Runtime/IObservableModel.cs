namespace Sroglu.Toolbox.Mvp
{
    using System;

    /// <summary>A model that raises Changed when its data mutates, so a presenter can subscribe and re-render.</summary>
    public interface IObservableModel : IModel
    {
        event Action Changed;
    }
}
