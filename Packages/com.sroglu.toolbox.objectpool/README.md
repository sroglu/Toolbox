# Object Pool

Zero-dependency pooling for Unity. Two types in the `Sroglu.Toolbox.Pooling` namespace:

- **`ObjectPool<T>`** — a generic, allocation-free pool for any reference type. You
  supply a factory plus optional get/release hooks; it recycles instances via a stack
  with an optional maximum size.
- **`GameObjectPool`** — a prefab pool that activates/deactivates instances and
  reparents inactive ones under a pool root, with optional auto-expansion.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.objectpool#main
```

## Usage

```csharp
using Sroglu.Toolbox.Pooling;

// Generic pool
var pool = new ObjectPool<StringBuilder>(
    factory: () => new StringBuilder(),
    onGet: sb => sb.Clear(),
    prewarm: 8);

var sb = pool.Get();
// ... use sb ...
pool.Release(sb);

// GameObject pool
var bullets = new GameObjectPool(bulletPrefab, prewarm: 32);
var bullet = bullets.Get(spawnPoint.position, Quaternion.identity);
bullets.Release(bullet);
```
