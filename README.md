# sroglu Toolbox

Small, self-contained Unity tools — import only the ones you need via UPM git URL.

## How to add a tool

Each tool is an embedded UPM package under `Packages/`. To pull one into another
Unity project:

1. Open **Window → Package Manager**.
2. Click **+ → Add package from git URL…**
3. Paste the tool's URL (from the table below) and press **Add**.

Or add it directly to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.sroglu.toolbox.objectpool": "https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.objectpool#main"
  }
}
```

The URL pattern for any tool is:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/<package-name>#main
```

## Tools

| Tool | What it does | Add (git URL) |
| --- | --- | --- |
| Object Pool | Generic object pool + GameObject prefab pool (zero deps) | `https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.objectpool#main` |
| Collections (PriorityQueue) | Double-ended priority queue (min/max heap), pure C# | `https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.collections#main` |
| State Machine | Lightweight FSM: `IState` + `StateMachine` + keyed `StateMachine<TId>` | `https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.statemachine#main` |
| Grid | Generic 2D grid: indexing, bounds, 4/8 neighbors, cell↔world | `https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.grid#main` |

## Adding a new tool

1. Create `Packages/com.sroglu.toolbox.<name>/` with a `package.json` and a
   `Runtime/` folder containing an `.asmdef`.
2. Put the runtime code under `Runtime/`; add a per-package `README.md`.
3. Add a row to the **Tools** table above with the tool's import URL.
