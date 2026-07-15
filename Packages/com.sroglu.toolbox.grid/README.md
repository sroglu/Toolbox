# Grid

Generic 2D grid for Unity, in the `Sroglu.Toolbox.Grids` namespace.

- **`Grid<T>`** — a rectangular grid of cells with `[x, y]` / `[Vector2Int]` indexing,
  bounds checks (`InBounds`, `TryGet`), 4- or 8-neighbor enumeration, `ForEach`, and
  cell↔world mapping (`CellToWorld` cell center / `WorldToCell`) via a cell size and origin.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.grid#main
```

## Usage

```csharp
using Sroglu.Toolbox.Grids;
using UnityEngine;

var grid = new Grid<int>(8, 6, (x, y) => 0);

grid[3, 2] = 5;
if (grid.TryGet(3, 2, out int v)) { /* v == 5 */ }

foreach (var n in grid.Neighbors(3, 2, diagonal: true))
    Debug.Log(n);

Vector3 center = grid.CellToWorld(3, 2);
Vector2Int cell = grid.WorldToCell(center);
```
