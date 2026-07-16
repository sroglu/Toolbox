# Mahjong Solitaire (MVP sample)

A worked example of the `Sroglu.Toolbox.Mvp` pattern on a real game: **Mahjong
Solitaire**. The rule engine is engine-free (no `UnityEngine`), so it runs and is
unit-tested outside Unity; only the view, gizmo, and demo bootstrap touch the engine.
This mirrors the **Counter** sample — a passive view, an observable model, and a
presenter that mediates between them.

## The model

### Half-tile grid

Tiles live on a **half-tile grid**: a tile fills a 2×2 block of fine cells, so
coordinates are integers in half-tile units and neighbouring tiles differ by **2** on
an axis. The root tile is at `(0, 0)`.

### Base vs relative tiles

A layout is a list of **slots** (`TileSlot`), each either:

- **base** — a bottom tile that carries explicit `X`/`Y` coordinates (layer 0), or
- **relative** — a tile that sits on 1, 2 or 4 tiles below it and has **no stored
  coordinates**. Its `RestsOn` holds the indices of its supports; its position is
  derived: `X = avg(supports.X)`, `Y = avg(supports.Y)` (an integer in half-units,
  since the support count is 1, 2 or 4), and `Layer = max(supports.Layer) + 1`.

### RestsOn / CoveredBy

Stacking is stored in **one direction** on the live `TileNode`: `RestsOn` ("I am on top
of these"). The reverse edge `CoveredBy` (who rests on me) is built once at load, along
with the same-layer neighbours `Left`/`Right` along the slide axis.

### The free rule

A tile is **free** (can be taken) when it is **not covered** *and* has at least **one
long side open**:

```
IsFree = !Removed && !IsCovered && (LeftOpen || RightOpen)
```

`IsCovered` is true while any tile still resting on it is on the board. `LeftOpen` /
`RightOpen` are true when no tile on the board sits immediately to that side along the
slide axis.

### SlideAxis

`SlideAxis` (`X` or `Y`) is the **single board-global long axis** tiles are pulled out
along. It is the one source of truth that both the view orientation *and* the free rule
read, so they can never disagree about which direction is "long". The `Left`/`Right`
neighbours are computed along this axis; the depth axis is the other one.

### Matching

`TileFaceSet.Matches(a, b)` pairs two faces when they share the same `Id`, **or** both
have the same non-zero `MatchGroup` (the flowers/seasons rule, where four different
faces all pair with each other). The board core never sees the ScriptableObject — the
matcher is passed into `Load` as a `Func<string, string, bool>`, which keeps the core
testable with fake data.

### Generating a board

`MahjongGenerator.Fill` builds a bag where each face appears an **even** number of times
(pairs), then shuffles it with a small **deterministic** xorshift seeded by an `int`
(no `System.Random`/clock), so the same seed always reproduces the same board. The slot
count must be even.

### Validate (author-time lint)

`MahjongValidator.Validate(slots)` returns human-readable problem messages (empty = OK),
each naming the offending slot:

- a relative tile's supports are out of range,
- support count is not 1, 2 or 4 (3 has no integer centre),
- a **floating** tile that doesn't sit on the layer directly below with an overlapping
  footprint,
- two tiles overlapping on the same layer,
- an odd tile total, or any face id with an odd (unmatchable) count.

### Gizmo (editor author-feedback)

`MahjongBoardGizmo` draws, for every tile, a wire cube at its derived world position, a
short arrow along the **slide axis** (so orientation is visible), coloured by state
(green = free, red = blocked, grey = removed). `OnDrawGizmos` runs only in the editor.

## MVP wiring

- **`MahjongBoard : ObservableModel`** — the model; raises `Changed` after a load or a
  successful match.
- **`MahjongBoardView : MonoBehaviour, IView`** — passive view; spawns a tile view per
  node, repaints on `Refresh`, raises `TileClicked`. Holds no board.
- **`MahjongTileView`** — one tile; shows a sprite when the face has one, otherwise a
  plain quad + a text label, so it runs with **zero imported art**.
- **`MahjongPresenter : Presenter<MahjongBoardView, MahjongBoard>`** — subscribes to the
  model's `Changed` in `OnBind`, turns two clicks into a `TryMatch`, unsubscribes in
  `OnUnbind`.

## Run the demo

1. Import this sample.
2. Add an empty GameObject to a scene and put **`MahjongDemo`** on it.
3. Press play. It builds a small layout in code (a 2×2 base with one tile resting on all
   four, plus a few spaced-out free tiles), generates faces, wires everything up, and
   logs the free tiles. Click two matching free tiles to remove them; enable **Gizmos**
   to see the free/blocked colouring and the slide-axis arrows.

No scene asset or prefab is required — the demo builds everything procedurally.

## Test the core (no Unity needed)

The engine-free core is covered by a standalone runner built with `mono`/`csc`:

```
csc -nologo -warn:0 -out:/tmp/mahjong.exe \
  Packages/com.sroglu.toolbox.mvp/Runtime/IModel.cs \
  Packages/com.sroglu.toolbox.mvp/Runtime/IObservableModel.cs \
  Packages/com.sroglu.toolbox.mvp/Runtime/ObservableModel.cs \
  Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/SlideAxis.cs \
  Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/TileSlot.cs \
  Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/TileNode.cs \
  Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/MahjongBoard.cs \
  Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/MahjongValidator.cs \
  Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/MahjongGenerator.cs \
  Packages/com.sroglu.toolbox.mvp/Samples~/Mahjong/Tests/Program.cs \
&& mono /tmp/mahjong.exe
```

The `.Unity.cs` partials and the view/gizmo/demo files are left out of that compile —
they depend on `UnityEngine` and are verified in the editor.
