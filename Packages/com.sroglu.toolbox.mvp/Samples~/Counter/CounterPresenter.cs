namespace Sroglu.Toolbox.Mvp.Samples
{
    /// <summary>
    /// Mediates the counter: on increment it mutates the model and pushes the new
    /// value back to the passive view.
    /// </summary>
    public class CounterPresenter : Presenter<ICounterView, CounterModel>
    {
        public CounterPresenter(ICounterView view, CounterModel model)
            : base(view, model)
        {
        }

        protected override void OnBind()
        {
            View.IncrementClicked += OnIncrementClicked;
            View.SetCount(Model.Count);
        }

        protected override void OnUnbind()
        {
            View.IncrementClicked -= OnIncrementClicked;
        }

        private void OnIncrementClicked()
        {
            Model.Count++;
            View.SetCount(Model.Count);
        }
    }
}
