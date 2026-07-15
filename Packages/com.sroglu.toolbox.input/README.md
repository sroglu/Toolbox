# Input

A lightweight reader over Unity's **new Input System**, in the
`Sroglu.Toolbox.Inputs` namespace. `GameInput` polls devices directly — **no
`.inputactions` asset needed** — and exposes pointer, movement, and press/release
events.

## Requirements

This package depends on the **Input System** package (`com.unity.inputsystem`),
declared in its `package.json`, so UPM installs it automatically. Your project must
have the new Input System **enabled**:

> **Edit → Project Settings → Player → Active Input Handling** → choose
> **Input System Package** or **Both**.

If the project is set to the old **Input Manager** only, `GameInput` will not
receive device input until you switch to **Input System Package** or **Both**.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.input#main
```

## Usage

Add `GameInput` to a GameObject and read it each frame:

```csharp
using Sroglu.Toolbox.Inputs;

public class Player : MonoBehaviour
{
    [SerializeField] private GameInput input;

    private void OnEnable()  => input.PointerPressed += OnTap;
    private void OnDisable() => input.PointerPressed -= OnTap;

    private void Update()
    {
        Vector2 move = input.Move;                 // WASD/arrows + gamepad left stick, clamped
        transform.Translate(move * Time.deltaTime);

        if (input.PointerHeld)
            Debug.Log(input.PointerPosition);      // mouse or primary touch
    }

    private void OnTap(Vector2 screenPos) { /* ... */ }
}
```

### API

- `Vector2 PointerPosition` — pointer position in screen space (mouse or touch).
- `bool PointerHeld` / `PointerDown` / `PointerUp` — primary button held / pressed
  this frame / released this frame.
- `Vector2 Move` — keyboard WASD/arrows combined with the gamepad left stick,
  clamped to a unit vector.
- `event Action<Vector2> PointerPressed` / `PointerReleased` — fired from `Update`
  with the pointer position.

All device access is null-guarded, so a missing mouse, touchscreen, keyboard, or
gamepad simply contributes nothing.
