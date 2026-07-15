using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sroglu.Toolbox.Assets
{
    /// <summary>
    /// A serialized, authored list of id-to-asset entries. Designers fill the list
    /// in the inspector; at runtime a lazily-built dictionary provides fast lookup
    /// by id. This is a lightweight, in-project catalog — not a bundle or remote
    /// content system.
    /// </summary>
    [CreateAssetMenu(fileName = "AssetCatalog", menuName = "Sroglu/Toolbox/Asset Catalog")]
    public class AssetCatalog : ScriptableObject
    {
        /// <summary>A single id-to-asset mapping within a catalog.</summary>
        [Serializable]
        public struct Entry
        {
            /// <summary>Unique lookup key for the asset.</summary>
            public string id;

            /// <summary>The referenced asset (prefab, sprite, material, and so on).</summary>
            public UnityEngine.Object asset;
        }

        [SerializeField]
        private List<Entry> _entries = new List<Entry>();

        private Dictionary<string, UnityEngine.Object> _lookup;

        /// <summary>
        /// The id-to-asset lookup, built on first access from the serialized entries.
        /// </summary>
        public IReadOnlyDictionary<string, UnityEngine.Object> Lookup
        {
            get
            {
                EnsureLookup();
                return _lookup;
            }
        }

        /// <summary>
        /// Attempts to fetch the asset stored under <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Lookup key.</param>
        /// <param name="asset">The found asset, or null when absent.</param>
        /// <returns>True when an asset was found; otherwise false.</returns>
        public bool TryGet(string id, out UnityEngine.Object asset)
        {
            EnsureLookup();
            return _lookup.TryGetValue(id, out asset);
        }

        /// <summary>
        /// Discards the cached lookup so it is rebuilt from the serialized entries on
        /// next access. Call after editing entries at runtime.
        /// </summary>
        public void Invalidate()
        {
            _lookup = null;
        }

        private void EnsureLookup()
        {
            if (_lookup != null)
                return;

            _lookup = new Dictionary<string, UnityEngine.Object>(_entries.Count);
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];
                if (string.IsNullOrEmpty(entry.id))
                    continue;

                _lookup[entry.id] = entry.asset;
            }
        }
    }
}
