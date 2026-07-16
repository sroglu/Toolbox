using UnityEngine;

namespace Sroglu.Toolbox.ViewManagement.Samples
{
    /// <summary>
    /// A code-only walkthrough of the ViewManager API. Put this and a
    /// <see cref="ViewManager"/> on the same (or sibling) GameObject.
    ///
    /// In a real project the ViewManager's <c>pageViews</c> and
    /// <c>instancePrefabs</c> are assigned in the inspector — the pre-placed
    /// MenuView / GameView for the pages, and a PopupView prefab for the instance
    /// view. This demo drives the API assuming those are wired; without them the
    /// type lookups throw an InvalidOperationException naming the missing type,
    /// which is the intended fail-fast behavior.
    /// </summary>
    public class ViewManagerDemo : MonoBehaviour
    {
        private void Start()
        {
            // Optional: recycle instance views through a pool instead of the default
            // Object.Instantiate. See PooledViewInstantiator below.
            // ViewManager.Instance.Instantiator = new PooledViewInstantiator();

            // Page navigation with the back-stack.
            ViewManager.ShowPageView<MenuView>();        // shows Menu
            ViewManager.ShowPageView<GameView>();        // hides Menu (remembered), shows Game
            ViewManager.ShowLastPageView();              // pops the stack, back to Menu

            // Look up a page view without showing it.
            MenuView menu = ViewManager.GetPageView<MenuView>();
            Debug.Log($"Current page after ShowLast: {ViewManager.Instance.CurrentPageView}");

            // Create an instance view (popup) on demand.
            PopupView popup = ViewManager.Instance.CreateInstanceView<PopupView>();
            popup.Show();
            popup.Hide();
        }
    }

    /// <summary>
    /// Illustrative Spawner-backed instantiator. A game would implement this against
    /// its pooling backend (for example the Spawner tool) and assign it to
    /// <see cref="ViewManager.Instantiator"/> so instance views are recycled instead
    /// of freshly allocated. This sample keeps it a self-contained sketch — the
    /// ViewManager package never depends on a spawner package.
    /// </summary>
    public class PooledViewInstantiator : IViewInstantiator
    {
        // In a real adapter you would hold a reference to your pool here, e.g.:
        //   private readonly Spawner spawner;
        //   public PooledViewInstantiator(Spawner spawner) { this.spawner = spawner; }

        /// <inheritdoc />
        public T Instantiate<T>(T prefab) where T : ViewComponent
        {
            // Real implementation: take a pooled GameObject for this prefab and
            // return its ViewComponent, e.g.:
            //   GameObject instance = spawner.Spawn(prefab.name, Vector3.zero, Quaternion.identity);
            //   return instance.GetComponent<T>();
            //
            // Fallback for this illustrative sketch:
            return Object.Instantiate(prefab);
        }
    }
}
