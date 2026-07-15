# Service Locator

Zero-dependency service registry in the `Sroglu.Toolbox.Services` namespace. Pure
C# — no engine references. `ServiceLocator` stores one instance per type, backed by
a `Dictionary<Type, object>`.

- **`Register<T>`** — store an instance under type `T`. Registering a type that is
  already present **replaces** the previous instance.
- **`Unregister<T>`** — remove the service for `T`.
- **`Resolve<T>`** — fetch the service; throws `KeyNotFoundException` if missing.
- **`TryResolve<T>`** — non-throwing fetch returning a bool.
- **`IsRegistered<T>`** — membership check.
- **`Clear`** — drop every registration.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.services#main
```

## Usage

```csharp
using Sroglu.Toolbox.Services;

var services = new ServiceLocator();
services.Register<IAudioService>(new AudioService());

if (services.TryResolve<IAudioService>(out var audio))
    audio.PlayMusic();

var required = services.Resolve<IAudioService>();  // throws if unregistered
services.Unregister<IAudioService>();
```
