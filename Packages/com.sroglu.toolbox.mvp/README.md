# MVP

Clean **Model-View-Presenter** for Unity, in the `Sroglu.Toolbox.Mvp` namespace.
The runtime base is engine-free (no `UnityEngine`), so it compiles anywhere.

In clean MVP the **view is passive and never knows the model**. The **presenter**
owns both model and view: it pushes data to the view and handles the view's input
events. The view only exposes:

- **set-methods** the presenter calls to display data, and
- **events** the view raises for user input.

Two types:

- **`IView`** — marker interface a concrete view implements.
- **`Presenter<TView, TModel>`** — base presenter. Construct it with a view and a
  model (both non-null), call `Initialize()` to bind (subscribe to view events +
  push initial state), and `Dispose()` to unbind (leak-safe). Implement `OnBind()`
  and `OnUnbind()` in your subclass. `Initialize()`/`Dispose()` are idempotent and
  `IsBound` reflects the current state.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.mvp#main
```

## Usage

The passive view exposes set-methods + input events, and holds **no model reference**:

```csharp
using System;
using Sroglu.Toolbox.Mvp;

public class CounterModel { public int Count; }

public interface ICounterView : IView
{
    void SetCount(int count);        // presenter pushes data in
    event Action IncrementClicked;   // view raises input out
}
```

The presenter mediates — it mutates the model and pushes the result to the view:

```csharp
public class CounterPresenter : Presenter<ICounterView, CounterModel>
{
    public CounterPresenter(ICounterView view, CounterModel model)
        : base(view, model) { }

    protected override void OnBind()
    {
        View.IncrementClicked += OnIncrement;
        View.SetCount(Model.Count);              // push initial state
    }

    protected override void OnUnbind()
    {
        View.IncrementClicked -= OnIncrement;    // leak-safe
    }

    private void OnIncrement()
    {
        Model.Count++;
        View.SetCount(Model.Count);
    }
}

// wire-up
var presenter = new CounterPresenter(myCounterView, new CounterModel());
presenter.Initialize();
// ... later ...
presenter.Dispose();
```

The concrete `CounterView` is a `MonoBehaviour` implementing `ICounterView`: it wires
a `Button.onClick` to raise `IncrementClicked` and sets a `Text.text` in `SetCount`.
It never touches `CounterModel`. See the **Counter** sample.

## MVP vs a `View<Model>`-style MVC

In an MVC-style `View<Model>` the view holds the model and reads it directly. In clean
MVP the flow is inverted: the **presenter pushes** to the view, the view exposes
**set-methods + input events**, and the view **holds no model reference**. This keeps
the view dumb and trivially testable, and puts all presentation logic in the presenter.
