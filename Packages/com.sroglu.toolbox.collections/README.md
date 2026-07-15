# Collections

Zero-dependency collections for Unity. Pure C# (no UnityEngine), in the
`Sroglu.Toolbox.Collections` namespace.

- **`PriorityQueue<TKey, TValue>`** — a double-ended priority queue backed by a
  min-max heap: O(log n) `Add`, `PopMin` and `PopMax`, O(1) `Min`/`Max` peeks.
  Ordering is by `TKey` via a supplied (or default) comparer.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.collections#main
```

## Usage

```csharp
using Sroglu.Toolbox.Collections;

var pq = new PriorityQueue<int, string>();
pq.Add(5, "five");
pq.Add(1, "one");
pq.Add(9, "nine");

string lowest  = pq.PopMin(); // "one"
string highest = pq.PopMax(); // "nine"
```
