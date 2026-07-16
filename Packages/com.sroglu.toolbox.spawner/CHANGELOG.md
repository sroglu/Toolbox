# Changelog

## [1.1.0] - 2026-07-16
- Add typed spawn path: `Spawn<T>()`, `Prewarm<T>()`, and a `Despawn<T>()` convenience. Type resolution finds the registered prefab whose root carries `T` (lazy, first-match-wins) via a new `AssetRegistry.Prefabs` enumeration.
- Pooling is now keyed by prefab reference instead of by id, so a prefab resolved by id and by component type share one pool. Instance tracking changed from instance→id to instance→prefab so `Despawn(GameObject)` works across both paths.

## [1.0.0] - 2026-07-15
- Initial release: `Spawner` — id-based spawning that pairs `AssetRegistry` resolution with per-id `GameObjectPool` recycling.
