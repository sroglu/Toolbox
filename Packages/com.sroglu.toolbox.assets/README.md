# Asset Registry

A lightweight id-to-asset catalog for Unity in the `Sroglu.Toolbox.Assets`
namespace. Two types:

- **`AssetCatalog`** — a `ScriptableObject` holding a serialized list of
  `{ id, asset }` entries (create via **Assets → Create → Sroglu → Toolbox →
  Asset Catalog**). It exposes a lazily-built `id → UnityEngine.Object` lookup.
- **`AssetRegistry`** — a runtime facade over a catalog:
  - `Get<T>(id)` / `TryGet<T>(id, out asset)` — typed lookup (throws
    `KeyNotFoundException` on a missing id or a type mismatch).
  - `Instantiate(id)`, `Instantiate(id, position, rotation)`,
    `Instantiate(id, parent)` — spawn a prefab.
  - `Instantiate<T>(id, parent = null)` — spawn a typed asset.

This is a simple in-project catalog, **not** a bundle or remote content system.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.assets#main
```

## Usage

```csharp
using Sroglu.Toolbox.Assets;

// catalog is an authored AssetCatalog asset
var registry = new AssetRegistry(catalog);

Sprite icon = registry.Get<Sprite>("icon.coin");
GameObject enemy = registry.Instantiate("enemy.goblin", spawnPoint.position, Quaternion.identity);

if (registry.TryGet<AudioClip>("sfx.jump", out var clip))
    source.PlayOneShot(clip);
```
