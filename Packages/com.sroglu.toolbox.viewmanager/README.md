# View Manager

A page-view stack plus on-demand instance views, in the
`Sroglu.Toolbox.ViewManagement` namespace. Attach a **`ViewManager`** to a
GameObject, list your page views and instance prefabs in the inspector, and drive
them by type: `ShowPageView<T>()`, `ShowLastPageView()`, `CreateInstanceView<T>()`.

Two roles:

- **Page views** — pre-placed, persistent `ViewComponent`s (menu, game,
  settings…). One is shown at a time; the previous one is remembered on a
  back-stack. They are never instantiated.
- **Instance views** — prefabs (popups, dialogs…) created on demand through a
  pluggable instantiator, so they can be recycled by a pool.

Types:

- **`IView`** — the managed-view contract: `Show()` / `Hide()`. Independent of any
  other package's `IView` marker.
- **`ViewComponent`** — the abstract `MonoBehaviour` base your pages and popups
  derive from. Default `Show` / `Hide` toggle the GameObject; override to animate.
- **`IViewInstantiator`** — the pluggable seam for creating instance views.
  `DefaultViewInstantiator` calls `Object.Instantiate`; swap in a pooling adapter
  to recycle.
- **`ViewManager`** — the `MonoBehaviour` that owns the page views, the back-stack,
  and instance-view creation.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.viewmanager#main
```

## Setup

Derive your views from `ViewComponent`:

```csharp
using Sroglu.Toolbox.ViewManagement;

public class MenuView : ViewComponent { }
public class GameView : ViewComponent { }
public class PopupView : ViewComponent { }   // an instance-view prefab type
```

Add a `ViewManager` to a GameObject, then in the inspector fill:

- **Page Views** — your pre-placed `MenuView`, `GameView`, … in the scene.
- **Instance Prefabs** — your `PopupView` (and other popup) prefabs.

## Usage

```csharp
using Sroglu.Toolbox.ViewManagement;

ViewManager.ShowPageView<MenuView>();          // shows Menu
ViewManager.ShowPageView<GameView>();          // hides Menu (pushed to back-stack), shows Game
ViewManager.ShowLastPageView();                // pops the stack, returns to Menu

ViewManager.ShowPageView<GameView>(remember: false);  // switch without remembering the outgoing page

PopupView popup = ViewManager.Instance.CreateInstanceView<PopupView>();  // instantiate a popup
popup.Show();

MenuView menu = ViewManager.GetPageView<MenuView>();   // look up a page view without showing it
```

Type-based resolution: `ShowPageView<T>` / `GetPageView<T>` find the page view of
type `T` in the inspector list, and `CreateInstanceView<T>` finds the prefab of
type `T`. If none matches, they throw an `InvalidOperationException` naming the
requested type.

This is pure view lifecycle — showing, hiding, and the back-stack. It does **no
input handling**: wire buttons and gestures to your own controllers and let them
call into the manager.

## Pluggable instantiation (works with the Spawner tool)

Instance views are created through `ViewManager.Instance.Instantiator`. Out of the
box that is `DefaultViewInstantiator`, which calls `Object.Instantiate` — the
manager works standalone with zero setup.

To recycle instance views, assign a pooling implementation. This is how you'd wire
it to the [Spawner](../com.sroglu.toolbox.spawner) tool, without this package ever
depending on the spawner package:

```csharp
// A game supplies its own adapter; the manager only knows IViewInstantiator.
public class PooledViewInstantiator : IViewInstantiator
{
    public T Instantiate<T>(T prefab) where T : ViewComponent
    {
        // Delegate to your pool (e.g. the Spawner tool), then return the view.
        // GameObject go = spawner.Spawn(...);
        // return go.GetComponent<T>();
    }
}

ViewManager.Instance.Instantiator = new PooledViewInstantiator();
```

See the **PageNavigation** sample for a runnable, asset-free walkthrough of the API
and a fuller `PooledViewInstantiator` sketch.
