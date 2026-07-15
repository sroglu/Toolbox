using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sroglu.Toolbox.Mvp.Samples
{
    /// <summary>
    /// Concrete passive view: a MonoBehaviour that renders the count and raises an
    /// input event. It holds no model reference — it only shows what it is told and
    /// reports what the user does.
    /// </summary>
    public class CounterView : MonoBehaviour, ICounterView
    {
        [SerializeField] private Text label;
        [SerializeField] private Button button;

        /// <inheritdoc />
        public event Action IncrementClicked;

        private void Awake()
        {
            button.onClick.AddListener(() => IncrementClicked?.Invoke());
        }

        /// <inheritdoc />
        public void SetCount(int count)
        {
            label.text = count.ToString();
        }
    }
}
