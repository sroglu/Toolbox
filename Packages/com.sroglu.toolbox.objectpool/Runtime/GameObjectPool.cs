using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sroglu.Toolbox.Pooling
{
    /// <summary>
    /// A prefab pool that recycles <see cref="GameObject"/> instances. Idle
    /// instances are deactivated and parented under a pool root; taking one
    /// reactivates and positions it.
    /// </summary>
    public class GameObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _poolRoot;
        private readonly bool _autoExpand;
        private readonly Stack<GameObject> _inactive = new Stack<GameObject>();

        /// <summary>
        /// Creates a prefab pool.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate. Required.</param>
        /// <param name="poolRoot">
        /// Parent for idle instances. When null, a hidden root GameObject named
        /// "[Pool] &lt;prefabName&gt;" is created and used.
        /// </param>
        /// <param name="prewarm">Number of instances to instantiate up front.</param>
        /// <param name="autoExpand">
        /// When true, an empty pool instantiates a new instance on <see cref="Get()"/>;
        /// when false, an empty pool returns null.
        /// </param>
        public GameObjectPool(GameObject prefab, Transform poolRoot = null, int prewarm = 0, bool autoExpand = true)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            _prefab = prefab;
            _autoExpand = autoExpand;

            if (poolRoot != null)
            {
                _poolRoot = poolRoot;
            }
            else
            {
                var rootObject = new GameObject("[Pool] " + prefab.name);
                rootObject.SetActive(false);
                _poolRoot = rootObject.transform;
            }

            if (prewarm > 0)
                Prewarm(prewarm);
        }

        /// <summary>Number of idle instances currently held by the pool.</summary>
        public int CountInactive => _inactive.Count;

        /// <summary>
        /// Takes an instance, activating it in place. Reuses an idle instance when
        /// available; otherwise instantiates a new one if auto-expansion is enabled,
        /// or returns null if it is disabled and the pool is empty.
        /// </summary>
        public GameObject Get()
        {
            GameObject instance = TakeOrExpand();
            if (instance == null)
                return null;

            instance.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Takes an instance and places it at the given world position and rotation.
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject instance = TakeOrExpand();
            if (instance == null)
                return null;

            Transform t = instance.transform;
            t.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Takes an instance and parents it under the given transform.
        /// </summary>
        public GameObject Get(Transform parent)
        {
            GameObject instance = TakeOrExpand();
            if (instance == null)
                return null;

            instance.transform.SetParent(parent, false);
            instance.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Returns an instance to the pool: deactivates it, reparents it under the
        /// pool root, and retains it for reuse.
        /// </summary>
        public void Release(GameObject instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot, false);
            _inactive.Push(instance);
        }

        /// <summary>
        /// Instantiates the given number of idle instances into the pool.
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject instance = CreateInstance();
                instance.SetActive(false);
                _inactive.Push(instance);
            }
        }

        /// <summary>
        /// Destroys every idle instance and empties the pool. Instances that are
        /// currently taken are unaffected.
        /// </summary>
        public void Clear()
        {
            while (_inactive.Count > 0)
            {
                GameObject instance = _inactive.Pop();
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);
            }
        }

        private GameObject TakeOrExpand()
        {
            if (_inactive.Count > 0)
                return _inactive.Pop();

            return _autoExpand ? CreateInstance() : null;
        }

        private GameObject CreateInstance()
        {
            return UnityEngine.Object.Instantiate(_prefab, _poolRoot);
        }
    }
}
