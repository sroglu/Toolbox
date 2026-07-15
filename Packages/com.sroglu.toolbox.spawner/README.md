# Spawner

Id-based GameObject spawning with per-id pooling, in the `Sroglu.Toolbox.Spawning`
namespace. `Spawner` combines an `AssetRegistry` (id → prefab) with a
`GameObjectPool` per id, so repeated spawn/despawn cycles reuse instances.

- `Spawn(id, position, rotation, parent = null)` — take a pooled instance (creating
  the id's pool on first use), place it, and optionally parent it.
- `Despawn(id, instance)` — return an instance to that id's pool.
- `Despawn(instance)` — return an instance using the id tracked at spawn time.
- `Prewarm(id, count)` — pre-instantiate idle instances for an id.
- `Clear()` — destroy all pooled instances and drop all pools.

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

GameObject goblin = spawner.Spawn("enemy.goblin", spawnPoint.position, Quaternion.identity);
// ...
spawner.Despawn(goblin);   // tracked-id overload
```
