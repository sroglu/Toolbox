# Path Finding

Grid **A\*** pathfinding for Unity, in the `Sroglu.Toolbox.PathFinding` namespace.
Self-contained: it ships its own internal binary min-heap, so importing this package
alone is enough — no dependency on any collections package.

Works over a rectangular `width x height` cell grid addressed with `Vector2Int`. You
supply an `isWalkable(cell)` predicate; the grid can be as sparse or dynamic as you
like (obstacles, doors, terrain costs are all just "can I enter this cell?").

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.pathfinding#main
```

## Usage

```csharp
using System.Collections.Generic;
using UnityEngine;
using Sroglu.Toolbox.PathFinding;

// A 10x10 grid where a couple of cells are blocked.
bool IsWalkable(Vector2Int c) => !(c.x == 3 && c.y >= 1 && c.y <= 8);

List<Vector2Int> path = AStar.FindPath(
    start: new Vector2Int(0, 0),
    goal:  new Vector2Int(9, 0),
    width: 10,
    height: 10,
    isWalkable: IsWalkable,
    allowDiagonal: false);   // 4-neighbor; pass true for 8-neighbor

if (path.Count == 0)
{
    // no route exists
}
else
{
    // path[0] == start, path[^1] == goal, inclusive
}
```

## Behavior

- Returns the path from `start` to `goal` **inclusive**, or an **empty list** when no
  route exists (also empty if either endpoint is out of bounds or not walkable).
- `allowDiagonal: false` → 4 orthogonal neighbors, **Manhattan** heuristic.
- `allowDiagonal: true` → 8 neighbors, **octile** heuristic (diagonal step cost √2).
- Bounds are checked against `width`/`height`; only cells passing `isWalkable` are
  entered.
- The open set is an internal binary min-heap; the path is rebuilt from a `cameFrom` map.
