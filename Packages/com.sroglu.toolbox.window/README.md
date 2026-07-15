# Toolbox Window

An editor window that lists every **sroglu Toolbox** tool and installs any of them
with one click via the Unity Package Manager.

Import **this** package once (git URL below), then open **Tools → Toolbox** to browse
and one-click-install the rest.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.window#main
```

## Usage

1. Open **Tools → Toolbox**.
2. Each tool shows its name, description, and a state:
   - **Install** — click to add the package (`UnityEditor.PackageManager.Client.Add`).
   - **Installed** — already added to this project.
   - **Local** — an embedded copy already exists under `Packages/`.
3. **Refresh** re-reads the tool index and the installed-package list.

The tool list is fetched from `toolbox-index.json` at the repo root; if it can't be
reached, the window falls back to a built-in list so it always works offline.
