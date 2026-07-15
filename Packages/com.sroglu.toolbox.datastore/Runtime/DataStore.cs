using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Sroglu.Toolbox.Data
{
    /// <summary>
    /// A simple typed key-value store with JSON save/load. Each value is kept as its
    /// <see cref="JsonUtility"/> JSON string, so the store holds any Unity-serializable
    /// type without boxing per-type collections.
    /// </summary>
    /// <remarks>
    /// Because it uses <see cref="JsonUtility"/>, only Unity-serializable types round-trip:
    /// classes/structs marked <c>[Serializable]</c> (or Unity types like <c>Vector3</c>).
    /// There is no polymorphism (the declared type is what deserializes) and dictionaries
    /// are not serializable as values. To store a plain <c>int</c>/<c>float</c>/<c>string</c>
    /// wrap it in a serializable container.
    /// </remarks>
    public class DataStore
    {
        private readonly Dictionary<string, string> _entries = new Dictionary<string, string>();

        /// <summary>Stores <paramref name="value"/> under <paramref name="key"/>, replacing any existing value.</summary>
        /// <typeparam name="T">A Unity-serializable type.</typeparam>
        public void Set<T>(string key, T value)
        {
            _entries[key] = JsonUtility.ToJson(value);
        }

        /// <summary>
        /// Reads the value stored under <paramref name="key"/> as <typeparamref name="T"/>.
        /// Returns <paramref name="fallback"/> if the key is missing or deserialization fails.
        /// </summary>
        /// <typeparam name="T">A Unity-serializable type.</typeparam>
        public T Get<T>(string key, T fallback = default)
        {
            if (!_entries.TryGetValue(key, out string json))
            {
                return fallback;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>True if a value is stored under <paramref name="key"/>.</summary>
        public bool Has(string key)
        {
            return _entries.ContainsKey(key);
        }

        /// <summary>Removes the value under <paramref name="key"/>. Returns true if one was removed.</summary>
        public bool Remove(string key)
        {
            return _entries.Remove(key);
        }

        /// <summary>Removes every stored value.</summary>
        public void Clear()
        {
            _entries.Clear();
        }

        /// <summary>
        /// Serializes the whole store to a JSON file at <paramref name="path"/>, creating
        /// any missing parent directories.
        /// </summary>
        public void Save(string path)
        {
            var snapshot = new Snapshot();
            foreach (KeyValuePair<string, string> entry in _entries)
            {
                snapshot.keys.Add(entry.Key);
                snapshot.values.Add(entry.Value);
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonUtility.ToJson(snapshot));
        }

        /// <summary>
        /// Loads the store from a JSON file at <paramref name="path"/>, replacing current
        /// contents. No-op if the file does not exist.
        /// </summary>
        public void Load(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var snapshot = JsonUtility.FromJson<Snapshot>(File.ReadAllText(path));

            _entries.Clear();
            if (snapshot == null)
            {
                return;
            }

            int count = Math.Min(snapshot.keys.Count, snapshot.values.Count);
            for (int i = 0; i < count; i++)
            {
                _entries[snapshot.keys[i]] = snapshot.values[i];
            }
        }

        /// <summary>Convenience: a path under <see cref="Application.persistentDataPath"/> for the given file name.</summary>
        public static string DefaultPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        [Serializable]
        private class Snapshot
        {
            public List<string> keys = new List<string>();
            public List<string> values = new List<string>();
        }
    }
}
