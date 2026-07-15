namespace Sroglu.Toolbox.Mvp
{
    using System;

    /// <summary>Convenience base: call RaiseChanged() after mutating state.</summary>
    public abstract class ObservableModel : IObservableModel
    {
        public event Action Changed;

        protected void RaiseChanged() => Changed?.Invoke();
    }
}
