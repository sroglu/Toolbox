# Screen Manager

Screen / page navigation with a back-stack, in the `Sroglu.Toolbox.Screens`
namespace. Attach a **`ScreenManager`** to a GameObject, register your screens by
key, and call `Show` / `Back` to move between them.

Two types:

- **`IScreen`** — optional lifecycle hooks (`OnShow()` / `OnHide()`) a screen
  `MonoBehaviour` may implement. Implementing it is not required.
- **`ScreenManager`** — a `MonoBehaviour` that owns the screens and the back-stack.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.screens#main
```

## Setup

Register screens either in the inspector (the **Entries** list — a `key` + a
`screen` GameObject per row) or at runtime:

```csharp
using Sroglu.Toolbox.Screens;

screenManager.Register("menu", menuGo);
screenManager.Register("game", gameGo);
```

## Usage

```csharp
screenManager.Show("menu");   // hides current, activates "menu", pushes previous to back-stack
screenManager.Show("game");   // "menu" is remembered on the stack
screenManager.Back();         // pops the stack, returns to "menu" (alias: ShowLast())

string shown = screenManager.CurrentKey;  // "menu"

screenManager.Hide("menu");   // deactivate a screen by key
screenManager.Clear();        // clear the back-stack + current key (no GameObjects toggled)
```

`Show` and `Hide` toggle `GameObject.SetActive` and invoke `IScreen.OnShow` /
`IScreen.OnHide` when the screen implements `IScreen`. `Show` throws
`KeyNotFoundException` for an unregistered key; `Back` is a no-op when the stack is
empty.

A screen that needs lifecycle callbacks implements `IScreen`:

```csharp
public class MenuScreen : MonoBehaviour, IScreen
{
    public void OnShow() { /* animate in, refresh data */ }
    public void OnHide() { /* pause, release */ }
}
```
