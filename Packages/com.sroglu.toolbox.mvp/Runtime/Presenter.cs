using System;

namespace Sroglu.Toolbox.Mvp
{
    /// <summary>
    /// Base class for a clean Model-View-Presenter presenter. The presenter owns
    /// both the model and the (passive) view: it pushes data to the view and
    /// handles the input events the view raises. The view never knows the model.
    /// </summary>
    /// <typeparam name="TView">The view interface this presenter drives.</typeparam>
    /// <typeparam name="TModel">The model type this presenter reads and mutates.</typeparam>
    public abstract class Presenter<TView, TModel> : IDisposable
        where TView : IView
        where TModel : IModel
    {
        /// <summary>The passive view this presenter pushes data to.</summary>
        protected TView View { get; }

        /// <summary>The model this presenter reads from and writes to.</summary>
        protected TModel Model { get; }

        /// <summary>
        /// True once <see cref="Initialize"/> has bound the presenter and before
        /// <see cref="Dispose"/> unbinds it.
        /// </summary>
        protected bool IsBound { get; private set; }

        /// <summary>
        /// Creates the presenter with its view and model. Neither may be null.
        /// Binding is deferred to <see cref="Initialize"/> — nothing is subscribed here.
        /// </summary>
        /// <param name="view">The view to drive; must not be null.</param>
        /// <param name="model">The model to present; must not be null.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="view"/> or <paramref name="model"/> is null.</exception>
        protected Presenter(TView view, TModel model)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            View = view;
            Model = model;
        }

        /// <summary>
        /// Binds the presenter: subscribes to the view's input events and pushes
        /// the initial state to the view. Idempotent — safe to call more than once.
        /// </summary>
        public void Initialize()
        {
            if (IsBound)
            {
                return;
            }

            IsBound = true;
            OnBind();
        }

        /// <summary>
        /// Unbinds the presenter: unsubscribes from the view's input events so no
        /// leaks remain. Idempotent — safe to call more than once.
        /// </summary>
        public void Dispose()
        {
            if (!IsBound)
            {
                return;
            }

            IsBound = false;
            OnUnbind();
        }

        /// <summary>
        /// Called once when the presenter binds. Subscribe to view input events
        /// and push the initial state to the view here.
        /// </summary>
        protected abstract void OnBind();

        /// <summary>
        /// Called once when the presenter unbinds. Unsubscribe from every view
        /// event subscribed in <see cref="OnBind"/> so no references leak.
        /// </summary>
        protected abstract void OnUnbind();
    }
}
