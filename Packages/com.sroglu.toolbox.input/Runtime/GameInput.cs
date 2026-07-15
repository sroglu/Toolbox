using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Sroglu.Toolbox.Inputs
{
    /// <summary>
    /// A lightweight reader over Unity's new Input System that polls devices
    /// directly — no <c>.inputactions</c> asset required. Exposes pointer (mouse or
    /// touch), movement (keyboard WASD/arrows or gamepad left stick), and per-frame
    /// press/release events. Everything is null-guarded so it is safe when a device
    /// is absent.
    /// </summary>
    public class GameInput : MonoBehaviour
    {
        /// <summary>Raised in <c>Update</c> when the pointer is pressed this frame, with its position.</summary>
        public event Action<Vector2> PointerPressed;

        /// <summary>Raised in <c>Update</c> when the pointer is released this frame, with its position.</summary>
        public event Action<Vector2> PointerReleased;

        /// <summary>The current pointer position in screen space, or <c>default</c> if no pointer exists.</summary>
        public Vector2 PointerPosition => Pointer.current?.position.ReadValue() ?? default;

        /// <summary>True while the pointer's primary button (mouse left or primary touch) is held.</summary>
        public bool PointerHeld => PointerButton?.isPressed ?? false;

        /// <summary>True on the frame the pointer's primary button is pressed.</summary>
        public bool PointerDown => PointerButton?.wasPressedThisFrame ?? false;

        /// <summary>True on the frame the pointer's primary button is released.</summary>
        public bool PointerUp => PointerButton?.wasReleasedThisFrame ?? false;

        /// <summary>
        /// Combined movement from keyboard (WASD / arrow keys) and gamepad left
        /// stick, clamped to a unit vector. Zero when no movement device is present.
        /// </summary>
        public Vector2 Move
        {
            get
            {
                Vector2 move = Vector2.zero;

                Keyboard keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    {
                        move.x -= 1f;
                    }

                    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    {
                        move.x += 1f;
                    }

                    if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                    {
                        move.y -= 1f;
                    }

                    if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    {
                        move.y += 1f;
                    }
                }

                Vector2 stick = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
                move += stick;

                return Vector2.ClampMagnitude(move, 1f);
            }
        }

        /// <summary>
        /// The active pointer button control: the mouse's left button if a mouse is
        /// present, otherwise the primary touch's press control, otherwise null.
        /// </summary>
        private ButtonControl PointerButton
        {
            get
            {
                Mouse mouse = Mouse.current;
                if (mouse != null)
                {
                    return mouse.leftButton;
                }

                Touchscreen touchscreen = Touchscreen.current;
                if (touchscreen != null)
                {
                    return touchscreen.primaryTouch.press;
                }

                return null;
            }
        }

        private void Update()
        {
            if (PointerDown)
            {
                PointerPressed?.Invoke(PointerPosition);
            }

            if (PointerUp)
            {
                PointerReleased?.Invoke(PointerPosition);
            }
        }
    }
}
