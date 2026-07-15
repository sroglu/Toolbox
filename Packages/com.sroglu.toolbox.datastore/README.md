# Data Store

A simple typed key-value store with JSON save/load for Unity, in the
`Sroglu.Toolbox.Data` namespace. Set/Get values by string key, then persist the whole
store to a file and reload it.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.datastore#main
```

## Usage

```csharp
using System;
using UnityEngine;
using Sroglu.Toolbox.Data;

[Serializable]
public class Profile
{
    public string name;
    public int level;
}

var store = new DataStore();
store.Set("profile", new Profile { name = "Ada", level = 7 });
store.Set("spawn", new Vector3(1, 0, 3));

bool hasProfile = store.Has("profile");                 // true
Profile p = store.Get("profile", new Profile());        // { Ada, 7 }
Vector3 spawn = store.Get<Vector3>("spawn");            // (1, 0, 3)
int missing = store.Get("coins", -1);                   // -1 (fallback)

string path = DataStore.DefaultPath("save.json");
store.Save(path);   // creates directories as needed

var reloaded = new DataStore();
reloaded.Load(path);   // no-op if the file doesn't exist
```

## API

- `Set<T>(key, value)` — store a value (kept as its `JsonUtility` JSON).
- `Get<T>(key, fallback = default)` — read it back; returns `fallback` if the key is
  missing or deserialization fails.
- `Has(key)` / `Remove(key)` / `Clear()`.
- `Save(path)` — write the whole store to a JSON file, creating parent directories.
- `Load(path)` — read + rebuild; **no-op** if the file doesn't exist.
- `static DefaultPath(fileName)` — `Path.Combine(Application.persistentDataPath, fileName)`.

## JsonUtility limitations

Values are serialized with Unity's `JsonUtility`, which only handles Unity-serializable
types:

- The value type must be a class/struct marked `[Serializable]` (or a Unity type like
  `Vector3`). Plain `int`/`float`/`string` are not serializable on their own — wrap them
  in a serializable container.
- **No polymorphism** — a value deserializes as its declared `T`, not a derived type.
- **No `Dictionary` values** — `JsonUtility` cannot serialize dictionaries. (The store
  itself persists via a serializable wrapper of two parallel lists for this reason.)
