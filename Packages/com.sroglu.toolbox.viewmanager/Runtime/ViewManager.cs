using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sroglu.Toolbox.ViewManagement
{
    /// <summary>
    /// Drives two kinds of view. <b>Page views</b> are pre-placed, persistent
    /// <see cref="ViewComponent"/>s (menu, game, settings…): exactly one is shown at
    /// a time, the previous one is remembered on a back-stack, and none of them are
    /// ever instantiated. <b>Instance views</b> are prefabs (popups, dialogs…)
    /// created on demand through the pluggable <see cref="Instantiator"/>.
    ///
    /// This is pure view lifecycle — showing, hiding, and the back-stack. It does no
    /// input handling: wire buttons and gestures to your own controllers, and let
    /// them call into this manager.
    /// </summary>
    public class ViewManager : MonoBehaviour
    {
        /// <summary>The active manager. Set in <see cref="Awake"/>.</summary>
        public static ViewManager Instance { get; private set; }

        [Tooltip("Pre-placed, persistent page views. Shown and hidden, never instantiated.")]
        [SerializeField] private ViewComponent[] pageViews;

        [Tooltip("Prefabs instantiated on demand by CreateInstanceView<T>().")]
        [SerializeField] private ViewComponent[] instancePrefabs;

        private ViewComponent currentPageView;
        private readonly Stack<ViewComponent> history = new Stack<ViewComponent>();

        private IViewInstantiator instantiator;

        /// <summary>
        /// How instance views are created. Defaults to a
        /// <see cref="DefaultViewInstantiator"/> so the manager works with zero
        /// setup; assign a pooling implementation to recycle instance views.
        /// </summary>
        public IViewInstantiator Instantiator
        {
            get => instantiator ??= new DefaultViewInstantiator();
            set => instantiator = value;
        }

        /// <summary>The page view currently shown, or null if none has been shown yet.</summary>
        public ViewComponent CurrentPageView => currentPageView;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Creates a new instance view by finding the prefab of type
        /// <typeparamref name="T"/> in <see cref="instancePrefabs"/> and instantiating
        /// it through <see cref="Instantiator"/>. The caller owns the returned
        /// instance's lifetime (showing, hiding, destroying/despawning it).
        /// </summary>
        /// <typeparam name="T">The instance-view type to create.</typeparam>
        /// <returns>The freshly instantiated view.</returns>
        /// <exception cref="InvalidOperationException">
        /// If no prefab of type <typeparamref name="T"/> is registered.
        /// </exception>
        public T CreateInstanceView<T>() where T : ViewComponent
        {
            foreach (ViewComponent prefab in instancePrefabs)
            {
                if (prefab is T match)
                {
                    return Instantiator.Instantiate(match);
                }
            }

            throw new InvalidOperationException(
                $"No instance-view prefab of type '{typeof(T).Name}' is registered on the ViewManager.");
        }

        /// <summary>
        /// Returns the page view of type <typeparamref name="T"/> from the active
        /// manager's <see cref="pageViews"/>.
        /// </summary>
        /// <typeparam name="T">The page-view type to look up.</typeparam>
        /// <returns>The matching page view.</returns>
        /// <exception cref="InvalidOperationException">
        /// If no page view of type <typeparamref name="T"/> is present.
        /// </exception>
        public static T GetPageView<T>() where T : ViewComponent
        {
            foreach (ViewComponent view in Instance.pageViews)
            {
                if (view is T match)
                {
                    return match;
                }
            }

            throw new InvalidOperationException(
                $"No page view of type '{typeof(T).Name}' is registered on the ViewManager.");
        }

        /// <summary>
        /// Shows the page view of type <typeparamref name="T"/>. If a page view is
        /// already shown it is hidden and, when <paramref name="remember"/> is true,
        /// pushed onto the back-stack so <see cref="ShowLastPageView"/> can return to
        /// it.
        /// </summary>
        /// <typeparam name="T">The page-view type to show.</typeparam>
        /// <param name="remember">Whether to push the outgoing page view onto the back-stack.</param>
        public static void ShowPageView<T>(bool remember = true) where T : ViewComponent
        {
            ShowPageView(GetPageView<T>(), remember);
        }

        /// <summary>
        /// Shows an explicit page view. If a page view is already shown it is hidden
        /// and, when <paramref name="remember"/> is true, pushed onto the back-stack.
        /// </summary>
        /// <param name="view">The page view to show.</param>
        /// <param name="remember">Whether to push the outgoing page view onto the back-stack.</param>
        public static void ShowPageView(ViewComponent view, bool remember = true)
        {
            Instance.ShowPageViewInternal(view, remember);
        }

        /// <summary>
        /// Returns to the previous page view by popping the back-stack. No-op if the
        /// stack is empty. The page view being returned to is not re-pushed.
        /// </summary>
        public static void ShowLastPageView()
        {
            ViewManager manager = Instance;

            if (manager.history.Count == 0)
            {
                return;
            }

            ViewComponent previous = manager.history.Pop();
            manager.ShowPageViewInternal(previous, remember: false);
        }

        private void ShowPageViewInternal(ViewComponent view, bool remember)
        {
            if (currentPageView != null && currentPageView != view)
            {
                if (remember)
                {
                    history.Push(currentPageView);
                }

                currentPageView.Hide();
            }

            currentPageView = view;
            currentPageView.Show();
        }
    }
}
