using System;

namespace Sroglu.Toolbox.Mvp.Samples
{
    /// <summary>
    /// Passive view contract for the counter. The presenter calls
    /// <see cref="SetCount"/> to display data and listens to
    /// <see cref="IncrementClicked"/> for input. No model reference.
    /// </summary>
    public interface ICounterView : IView
    {
        /// <summary>Displays the given count.</summary>
        void SetCount(int count);

        /// <summary>Raised when the user asks to increment.</summary>
        event Action IncrementClicked;
    }
}
