# Spawner

GameObject spawning by **string id** or **component type**, in the
`Sroglu.Toolbox.Spawning` namespace. `Spawner` combines an `AssetRegistry` (id →
prefab, plus prefab enumeration for type lookup) with a `GameObjectPool` **keyed by
prefab reference**. A prefab reached through its id and the same prefab reached
through a component type therefore share **one** pool — spawn by id, despawn, then
spawn by type and you get the recycled instance back.

- `Spawn(id, position, rotation, parent = null)` — take a pooled instance of the
  prefab under `id` (creating its pool on first use), place it, optionally parent it.
- `Spawn<T>(position, rotation, parent = null)` — resolve the registered prefab whose
  **root** carries a `T` component, take a pooled instance, and return its `T`. The
  type→prefab map is built lazily on first use (first match wins if several prefabs
  carry a `T`).
- `Despawn(id, instance)` — return an instance to that prefab's pool.
- `Despawn(instance)` — return an instance using the prefab tracked at spawn time;
  works whether it was spawned by id or by type.
- `Despawn<T>(instance)` — convenience that recycles `instance.gameObject`.
- `Prewarm(id, count)` / `Prewarm<T>(count)` — pre-instantiate idle instances.
- `Clear()` — destroy all pooled instances and drop all pools and instance tracking.

## Requires

This package references two other Toolbox packages. Import them alongside it:

- **com.sroglu.toolbox.objectpool**
- **com.sroglu.toolbox.assets**

(They are intentionally *not* listed as UPM `dependencies` — git-URL packages do
not resolve transitively — so add all three from their git URLs.)

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.spawner#main
```

## Usage

```csharp
using Sroglu.Toolbox.Assets;
using Sroglu.Toolbox.Spawning;

var registry = new AssetRegistry(catalog);
var spawner = new Spawner(registry, poolRoot: transform);

spawner.Prewarm("enemy.goblin", 16);

// Spawn by id.
GameObject goblin = spawner.Spawn("enemy.goblin", spawnPoint.position, Quaternion.identity);
spawner.Despawn(goblin);   // tracked overload — recycles into the prefab's pool

// Spawn the SAME prefab by the component on its root — same pool, so this reuses
// the instance just despawned above.
Enemy enemy = spawner.Spawn<Enemy>(spawnPoint.position, Quaternion.identity);
spawner.Despawn(enemy);    // Despawn<T> convenience
```
