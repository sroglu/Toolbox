using System;
using System.Collections.Generic;
using Sroglu.Toolbox.Assets;
using Sroglu.Toolbox.Pooling;
using UnityEngine;

namespace Sroglu.Toolbox.Spawning
{
    /// <summary>
    /// Spawns and recycles prefab instances by string id or by component type.
    /// Prefabs are resolved through an <see cref="AssetRegistry"/> — by id directly,
    /// or by finding the registered prefab whose root carries the requested component.
    /// Pools are keyed by the prefab reference, so a prefab reached through its id and
    /// the same prefab reached through a component type share ONE pool: instances
    /// spawned by one path can be recycled and re-served through the other.
    /// </summary>
    public class Spawner
    {
        private readonly AssetRegistry _registry;
        private readonly Transform _poolRoot;

        // Pooling: keyed by the prefab reference so both resolution paths funnel to
        // the same pool for a given prefab.
        private readonly Dictionary<GameObject, GameObjectPool> _pools =
            new Dictionary<GameObject, GameObjectPool>();

        // Type resolution: typeof(T) -> the registered prefab whose root carries T.
        // Built lazily on the first Spawn<T>/Prewarm<T> for each type.
        private readonly Dictionary<Type, GameObject> _prefabsByType =
            new Dictionary<Type, GameObject>();

        // Instance tracking: spawned instance -> the prefab it came from, so a bare
        // Despawn recycles it into the correct prefab-keyed pool regardless of the
        // path (id or type) it was spawned through.
        private readonly Dictionary<GameObject, GameObject> _prefabByInstance =
            new Dictionary<GameObject, GameObject>();

        /// <summary>
        /// Creates a spawner.
        /// </summary>
        /// <param name="registry">Resolves prefabs by id and supplies the prefab set for type lookup. Required.</param>
        /// <param name="poolRoot">
        /// Optional parent under which each prefab's pool parks its idle instances.
        /// When null, each pool creates its own hidden root.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is null.</exception>
        public Spawner(AssetRegistry registry, Transform poolRoot = null)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            _registry = registry;
            _poolRoot = poolRoot;
        }

        /// <summary>
        /// Takes an instance of the prefab registered under <paramref name="id"/> from
        /// that prefab's pool (creating the pool on first use), positions it, and
        /// optionally parents it.
        /// </summary>
        /// <param name="id">Registry id of the prefab to spawn.</param>
        /// <param name="position">World position for the instance.</param>
        /// <param name="rotation">World rotation for the instance.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <exception cref="KeyNotFoundException">Thrown when no prefab is registered under the id.</exception>
        public GameObject Spawn(string id, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject prefab = _registry.Get<GameObject>(id);
            return SpawnFromPrefab(prefab, position, rotation, parent);
        }

        /// <summary>
        /// Takes an instance of the registered prefab whose root carries a
        /// <typeparamref name="T"/> component from that prefab's pool (creating the
        /// pool on first use), positions it, optionally parents it, and returns the
        /// <typeparamref name="T"/> on the instance.
        /// </summary>
        /// <typeparam name="T">Component type the prefab's root must carry.</typeparam>
        /// <param name="position">World position for the instance.</param>
        /// <param name="rotation">World rotation for the instance.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no registered prefab's root carries a <typeparamref name="T"/>.
        /// </exception>
        public T Spawn<T>(Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            GameObject prefab = ResolvePrefabByType<T>();
            GameObject instance = SpawnFromPrefab(prefab, position, rotation, parent);
            return instance.GetComponent<T>();
        }

        /// <summary>
        /// Returns an instance to the pool of the prefab registered under
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id whose prefab the instance was spawned from.</param>
        /// <param name="instance">The instance to recycle. Required.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no prefab is registered under the id, or no pool exists for it yet.
        /// </exception>
        public void Despawn(string id, GameObject instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            GameObject prefab = _registry.Get<GameObject>(id);
            if (!_pools.TryGetValue(prefab, out GameObjectPool pool))
                throw new KeyNotFoundException($"No pool exists for id '{id}'.");

            _prefabByInstance.Remove(instance);
            pool.Release(instance);
        }

        /// <summary>
        /// Returns an instance to the pool it was spawned from, using the prefab
        /// tracked at spawn time. Works whether the instance was spawned by id or by
        /// component type.
        /// </summary>
        /// <param name="instance">The instance to recycle. Required.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the instance was not produced by this spawner.
        /// </exception>
        public void Despawn(GameObject instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (!_prefabByInstance.TryGetValue(instance, out GameObject prefab))
                throw new KeyNotFoundException("Instance was not spawned by this spawner.");
            if (!_pools.TryGetValue(prefab, out GameObjectPool pool))
                throw new KeyNotFoundException("No pool exists for the instance's prefab.");

            _prefabByInstance.Remove(instance);
            pool.Release(instance);
        }

        /// <summary>
        /// Convenience overload that recycles the GameObject a component belongs to.
        /// Forwards to <see cref="Despawn(GameObject)"/>.
        /// </summary>
        /// <typeparam name="T">Component type spawned via <see cref="Spawn{T}"/>.</typeparam>
        /// <param name="instance">The component whose GameObject to recycle. Required.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        public void Despawn<T>(T instance) where T : Component
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Despawn(instance.gameObject);
        }

        /// <summary>
        /// Pre-instantiates <paramref name="count"/> idle instances into the pool for
        /// the prefab registered under <paramref name="id"/>, creating the pool on
        /// first use.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when no prefab is registered under the id.</exception>
        public void Prewarm(string id, int count)
        {
            GameObject prefab = _registry.Get<GameObject>(id);
            GetOrCreatePool(prefab).Prewarm(count);
        }

        /// <summary>
        /// Pre-instantiates <paramref name="count"/> idle instances into the pool for
        /// the registered prefab whose root carries a <typeparamref name="T"/>
        /// component, creating the pool on first use.
        /// </summary>
        /// <typeparam name="T">Component type the prefab's root must carry.</typeparam>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no registered prefab's root carries a <typeparamref name="T"/>.
        /// </exception>
        public void Prewarm<T>(int count) where T : Component
        {
            GameObject prefab = ResolvePrefabByType<T>();
            GetOrCreatePool(prefab).Prewarm(count);
        }

        /// <summary>
        /// Destroys every pooled instance across all prefabs and drops all pools and
        /// instance tracking. Instances currently taken are unaffected by the pool
        /// clear, but they can no longer be despawned. The lazily-built type-to-prefab
        /// resolution cache is retained, since it reflects the (unchanged) registry.
        /// </summary>
        public void Clear()
        {
            foreach (GameObjectPool pool in _pools.Values)
                pool.Clear();

            _pools.Clear();
            _prefabByInstance.Clear();
        }

        // Both resolution paths funnel here: one pool per prefab reference.
        private GameObjectPool GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out GameObjectPool pool))
                return pool;

            pool = new GameObjectPool(prefab, _poolRoot);
            _pools[prefab] = pool;
            return pool;
        }

        private GameObject SpawnFromPrefab(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObjectPool pool = GetOrCreatePool(prefab);
            GameObject instance = pool.Get();

            Transform t = instance.transform;
            if (parent != null)
                t.SetParent(parent, false);

            t.SetPositionAndRotation(position, rotation);

            _prefabByInstance[instance] = prefab;
            return instance;
        }

        // Lazily maps typeof(T) to the registered prefab whose ROOT carries a T,
        // mirroring the ViewManager root-check. First match wins when several
        // registered prefabs carry a T; the first found is cached for the type.
        private GameObject ResolvePrefabByType<T>() where T : Component
        {
            Type type = typeof(T);
            if (_prefabsByType.TryGetValue(type, out GameObject cached))
                return cached;

            foreach (GameObject prefab in _registry.Prefabs)
            {
                // Documented resolution predicate (not a defensive guard): a prefab
                // qualifies only when its root carries a T. GetComponent returns null
                // when it does not.
                T component = prefab.GetComponent<T>();
                if (component != null)
                {
                    _prefabsByType[type] = prefab;
                    return prefab;
                }
            }

            throw new KeyNotFoundException(
                $"No registered prefab carries a '{typeof(T).Name}' component on its root.");
        }
    }
}
