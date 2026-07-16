using UnityEngine;

namespace Sroglu.Toolbox.ViewManagement
{
    /// <summary>
    /// The abstract base every managed view derives from. It is a concrete
    /// <c>MonoBehaviour</c> type (not an interface) on purpose: the
    /// <see cref="ViewManager"/> exposes its page views and instance prefabs as
    /// <c>ViewComponent[]</c> in the inspector, and Unity cannot serialize arrays of
    /// interfaces. Games subclass this for their pages and popups.
    ///
    /// The default <see cref="Show"/> / <see cref="Hide"/> toggle the GameObject's
    /// active state; override them to add animation, data refresh, or teardown.
    /// </summary>
    public abstract class ViewComponent : MonoBehaviour, IView
    {
        /// <summary>Makes the view visible. Default: activates the GameObject.</summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>Makes the view hidden. Default: deactivates the GameObject.</summary>
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
