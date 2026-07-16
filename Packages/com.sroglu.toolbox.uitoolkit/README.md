# UI Tools

Editor play-mode helpers for **uGUI** (Canvas / Graphic + EventSystem).

## UI Element Picker (Ctrl + Hover)

While playing in the editor, **hold Ctrl** (left or right Control — on both Windows and macOS;
**not** Command) and hover the mouse over your game UI. The tool finds the **topmost uGUI element
under the cursor** and reveals it in the **Hierarchy** by selecting and pinging it.

Moving the mouse with Ctrl held live-updates the selection; a given element is pinged only when it
becomes the new topmost, so there's no ping spam.

### Zero setup

It just works. When the tool is enabled, a hidden helper is auto-injected in Play mode — no
component to place. Toggle it from the menu:

- **Tools → Toolbox → UI Element Picker (Ctrl+Hover)** — checkmark reflects the state, persisted in
  `EditorPrefs`. Default **on**. When off, nothing is injected.

### Requirements

- A uGUI **EventSystem** must be present in the scene (that's what raycasts the Canvas). If it's
  missing while you hold Ctrl, the tool logs a single warning and does nothing else.
- Editor only — selection and ping are `UnityEditor` APIs.

### Input backend

Works with either input backend and needs neither an InputActions asset nor a hard dependency:

- **Legacy Input Manager** — `Input.mousePosition` + `Input.GetKey(LeftControl/RightControl)`.
- **Input System** — `Mouse.current.position` + `Keyboard.current.leftCtrlKey/rightCtrlKey`.

Input System code is compiled only under `ENABLE_INPUT_SYSTEM`, so the package compiles in projects
that don't have the Input System package installed.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.uitoolkit#main
```
