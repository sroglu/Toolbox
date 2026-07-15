# Event Bus

Zero-dependency publish/subscribe in the `Sroglu.Toolbox.Events` namespace. Pure
C# — no engine references. `EventBus` keys handlers by event type, backed by a
`Dictionary<Type, Delegate>`.

- **`Subscribe<T>` / `Unsubscribe<T>`** — register or remove a typed handler.
- **`Publish<T>`** — deliver an event to all handlers of that type. Dispatch is
  re-entrancy safe: handlers may subscribe/unsubscribe while an event is being
  delivered (the invocation list is snapshotted first).
- **`Clear`** — drop every handler.

Single-threaded (main-thread) use is assumed.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.events#main
```

## Usage

```csharp
using Sroglu.Toolbox.Events;

struct PlayerDied { public int Score; }

var bus = new EventBus();

void OnDeath(PlayerDied e) => ShowGameOver(e.Score);

bus.Subscribe<PlayerDied>(OnDeath);
bus.Publish(new PlayerDied { Score = 4200 });
bus.Unsubscribe<PlayerDied>(OnDeath);
```
