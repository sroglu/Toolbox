using System;
using System.Collections.Generic;
using Sroglu.Toolbox.Assets;
using Sroglu.Toolbox.Pooling;
using UnityEngine;

namespace Sroglu.Toolbox.Spawning
{
    /// <summary>
    /// Spawns and recycles prefab instances by id. Prefabs are resolved through an
    /// <see cref="AssetRegistry"/>; each id gets its own <see cref="GameObjectPool"/>,
    /// created on first use, so repeated spawns and despawns reuse instances instead
    /// of allocating.
    /// </summary>
    public class Spawner
    {
        private readonly AssetRegistry _registry;
        private readonly Transform _poolRoot;
        private readonly Dictionary<string, GameObjectPool> _pools = new Dictionary<string, GameObjectPool>();
        private readonly Dictionary<GameObject, string> _spawnedIds = new Dictionary<GameObject, string>();

        /// <summary>
        /// Creates a spawner.
        /// </summary>
        /// <param name="registry">Resolves prefabs by id. Required.</param>
        /// <param name="poolRoot">
        /// Optional parent under which each id's pool parks its idle instances. When
        /// null, each pool creates its own hidden root.
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
        /// its pool (creating the pool on first use), positions it, and optionally
        /// parents it.
        /// </summary>
        /// <param name="id">Registry id of the prefab to spawn.</param>
        /// <param name="position">World position for the instance.</param>
        /// <param name="rotation">World rotation for the instance.</param>
        /// <param name="parent">Optional parent transform.</param>
        public GameObject Spawn(string id, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObjectPool pool = GetOrCreatePool(id);
            GameObject instance = pool.Get();

            Transform t = instance.transform;
            if (parent != null)
                t.SetParent(parent, false);

            t.SetPositionAndRotation(position, rotation);

            _spawnedIds[instance] = id;
            return instance;
        }

        /// <summary>
        /// Returns an instance to the pool for <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id the instance was spawned under.</param>
        /// <param name="instance">The instance to recycle. Required.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no pool exists for the id.</exception>
        public void Despawn(string id, GameObject instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (!_pools.TryGetValue(id, out GameObjectPool pool))
                throw new KeyNotFoundException($"No pool exists for id '{id}'.");

            _spawnedIds.Remove(instance);
            pool.Release(instance);
        }

        /// <summary>
        /// Returns an instance to the pool it was spawned from, using the id tracked
        /// at spawn time.
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
            if (!_spawnedIds.TryGetValue(instance, out string id))
                throw new KeyNotFoundException("Instance was not spawned by this spawner.");

            Despawn(id, instance);
        }

        /// <summary>
        /// Pre-instantiates <paramref name="count"/> idle instances into the pool for
        /// <paramref name="id"/>, creating the pool on first use.
        /// </summary>
        public void Prewarm(string id, int count)
        {
            GetOrCreatePool(id).Prewarm(count);
        }

        /// <summary>
        /// Destroys every pooled instance across all ids and drops all pools and
        /// tracking. Instances currently taken are unaffected by the pool clear, but
        /// they can no longer be despawned by tracked id.
        /// </summary>
        public void Clear()
        {
            foreach (GameObjectPool pool in _pools.Values)
                pool.Clear();

            _pools.Clear();
            _spawnedIds.Clear();
        }

        private GameObjectPool GetOrCreatePool(string id)
        {
            if (_pools.TryGetValue(id, out GameObjectPool pool))
                return pool;

            GameObject prefab = _registry.Get<GameObject>(id);
            pool = new GameObjectPool(prefab, _poolRoot);
            _pools[id] = pool;
            return pool;
        }
    }
}
