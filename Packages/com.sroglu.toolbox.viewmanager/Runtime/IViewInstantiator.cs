using UnityEngine;

namespace Sroglu.Toolbox.ViewManagement
{
    /// <summary>
    /// The pluggable seam the <see cref="ViewManager"/> uses to create instance
    /// views from prefabs. Swapping the implementation lets the manager run
    /// standalone (the <see cref="DefaultViewInstantiator"/>) or, in a game, route
    /// through a pooling backend (for example a Spawner-backed adapter) — all
    /// without this package depending on any spawner package.
    /// </summary>
    public interface IViewInstantiator
    {
        /// <summary>
        /// Creates a live instance of <paramref name="prefab"/> and returns it.
        /// </summary>
        /// <typeparam name="T">The concrete view type being instantiated.</typeparam>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <returns>The instantiated view.</returns>
        T Instantiate<T>(T prefab) where T : ViewComponent;
    }

    /// <summary>
    /// The out-of-the-box instantiator: a straight call to
    /// <see cref="Object.Instantiate{T}(T)"/>. This is what lets the
    /// <see cref="ViewManager"/> work with zero setup; a game can replace it with a
    /// pooling implementation by assigning <see cref="ViewManager.Instantiator"/>.
    /// </summary>
    public class DefaultViewInstantiator : IViewInstantiator
    {
        /// <inheritdoc />
        public T Instantiate<T>(T prefab) where T : ViewComponent
        {
            return Object.Instantiate(prefab);
        }
    }
}
