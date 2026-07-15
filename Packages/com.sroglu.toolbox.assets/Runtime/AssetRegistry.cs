using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sroglu.Toolbox.Assets
{
    /// <summary>
    /// A runtime facade over an <see cref="AssetCatalog"/> that resolves assets by
    /// id and instantiates prefabs. Typed lookups verify the stored asset matches
    /// the requested type.
    /// </summary>
    public class AssetRegistry
    {
        private readonly AssetCatalog _catalog;

        /// <summary>
        /// Creates a registry over the given catalog.
        /// </summary>
        /// <param name="catalog">The backing catalog. Required.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="catalog"/> is null.</exception>
        public AssetRegistry(AssetCatalog catalog)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            _catalog = catalog;
        }

        /// <summary>
        /// Returns the asset stored under <paramref name="id"/> as type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Expected asset type.</typeparam>
        /// <param name="id">Lookup key.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the id is absent or the stored asset is not a <typeparamref name="T"/>.
        /// </exception>
        public T Get<T>(string id) where T : UnityEngine.Object
        {
            if (!_catalog.TryGet(id, out UnityEngine.Object asset))
                throw new KeyNotFoundException($"No asset registered for id '{id}'.");

            if (asset is T typed)
                return typed;

            throw new KeyNotFoundException(
                $"Asset for id '{id}' is a {asset.GetType().Name}, not a {typeof(T).Name}.");
        }

        /// <summary>
        /// Attempts to fetch the asset stored under <paramref name="id"/> as type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Expected asset type.</typeparam>
        /// <param name="id">Lookup key.</param>
        /// <param name="asset">The typed asset, or null when absent or mistyped.</param>
        /// <returns>True when a matching asset was found; otherwise false.</returns>
        public bool TryGet<T>(string id, out T asset) where T : UnityEngine.Object
        {
            if (_catalog.TryGet(id, out UnityEngine.Object found) && found is T typed)
            {
                asset = typed;
                return true;
            }

            asset = null;
            return false;
        }

        /// <summary>
        /// Instantiates the prefab stored under <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Lookup key of a <see cref="GameObject"/> prefab.</param>
        public GameObject Instantiate(string id)
        {
            GameObject prefab = Get<GameObject>(id);
            return UnityEngine.Object.Instantiate(prefab);
        }

        /// <summary>
        /// Instantiates the prefab stored under <paramref name="id"/> at a world
        /// position and rotation.
        /// </summary>
        public GameObject Instantiate(string id, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = Get<GameObject>(id);
            return UnityEngine.Object.Instantiate(prefab, position, rotation);
        }

        /// <summary>
        /// Instantiates the prefab stored under <paramref name="id"/> under the given
        /// parent transform.
        /// </summary>
        public GameObject Instantiate(string id, Transform parent)
        {
            GameObject prefab = Get<GameObject>(id);
            return UnityEngine.Object.Instantiate(prefab, parent);
        }

        /// <summary>
        /// Instantiates the asset stored under <paramref name="id"/> as type
        /// <typeparamref name="T"/> (for example a component on a prefab), optionally
        /// under a parent.
        /// </summary>
        /// <typeparam name="T">Expected asset type to instantiate.</typeparam>
        /// <param name="id">Lookup key.</param>
        /// <param name="parent">Optional parent transform for the new instance.</param>
        public T Instantiate<T>(string id, Transform parent = null) where T : UnityEngine.Object
        {
            T asset = Get<T>(id);
            return parent != null
                ? UnityEngine.Object.Instantiate(asset, parent)
                : UnityEngine.Object.Instantiate(asset);
        }
    }
}
