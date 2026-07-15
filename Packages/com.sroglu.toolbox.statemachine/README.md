# State Machine

Lightweight finite-state machine for Unity. Pure C# (no UnityEngine), in the
`Sroglu.Toolbox.StateMachines` namespace.

- **`IState`** — `Enter` / `Update` / `Exit` lifecycle hooks.
- **`StateMachine`** — holds the current state, transitions with proper
  `Exit`/`Enter` ordering, and raises `StateChanged(previous, next)`.
- **`StateMachine<TId>`** — register states by id and switch by id.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.statemachine#main
```

## Usage

```csharp
using Sroglu.Toolbox.StateMachines;

var sm = new StateMachine<string>();
sm.AddState("idle", new IdleState());
sm.AddState("run", new RunState());
sm.StateChanged += (from, to) => Debug.Log($"{from} -> {to}");

sm.ChangeState("idle");
// each frame:
sm.Update();
sm.ChangeState("run");
```
