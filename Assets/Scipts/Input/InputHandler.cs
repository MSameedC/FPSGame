using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour, IInputProvider
{
    public event Action OnShootPressed;
    public event Action OnShootReleased;
    public event Action OnJumpPressed;        // NEW: Single-tap events
    public event Action OnDashPressed;
    public event Action OnSlamPressed;
    public event Action OnAimPressed;
    public event Action OnAimReleased;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public bool IsAiming { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSlaming { get; private set; }
    public bool IsShooting { get; private set; }

    public void Move(InputAction.CallbackContext context) => MoveInput = context.ReadValue<Vector2>();
    public void Look(InputAction.CallbackContext context) => LookInput = context.ReadValue<Vector2>();

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsShooting = true;
            OnShootPressed?.Invoke();
        }
        if (context.canceled)
        {
            IsShooting = false;
            OnShootReleased?.Invoke();
        }
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsAiming = true;
            OnAimPressed?.Invoke();
        }
        if (context.canceled)
        {
            IsAiming = false;
            OnAimReleased?.Invoke();
        }
    }

    public void Slam(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsSlaming = true;
            OnSlamPressed?.Invoke();
        }
        if (context.canceled)
        {
            IsSlaming = false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsJumping = true;
            OnJumpPressed?.Invoke();
        }
        if (context.canceled)
        {
            IsJumping = false;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsDashing = true;
            OnDashPressed?.Invoke();
        }
        if (context.canceled)
        {
            IsDashing = false;
        }
    }
}