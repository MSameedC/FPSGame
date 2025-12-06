using System;
using UnityEngine;

public static class InputManager
{
    public static bool IsGamePaused;
    
    // Events (Private set)
    public static event Action OnShootPressed;
    public static event Action OnShootReleased;
    public static event Action OnJumpPressed;
    public static event Action OnDashPressed;
    public static event Action OnSlamPressed;
    public static event Action OnAimPressed;
    public static event Action OnAimReleased;
    public static event Action OnPausePressed;
    
    // Public invoke methods
    public static void InvokeDashPressed() {
        if (IsGamePaused) return;
        OnDashPressed?.Invoke();
        IsDashing = true;
    }
    public static void InvokeJumpPressed() {
        if (IsGamePaused) return;
        OnJumpPressed?.Invoke();
        IsJumping = true;
    }
    public static void InvokeSlamPressed() {
        if (IsGamePaused) return;
        OnSlamPressed?.Invoke();
        IsSlaming = true;
    }
    public static void InvokeShootPressed() {
        if (IsGamePaused) return;
        OnShootPressed?.Invoke();
        IsShooting  = true;
    }
    public static void InvokeShootReleased() {
        if (IsGamePaused) return;
        OnShootReleased?.Invoke();
        IsShooting = false;
    }
    public static void InvokeAimPressed() {
        if (IsGamePaused) return;
        OnAimPressed?.Invoke();
        IsAiming = true;
    }
    public static void InvokeAimReleased() {
        if (IsGamePaused) return;
        OnAimReleased?.Invoke();
        IsAiming = false;
    }
    public static void InvokePausePressed() {
        OnPausePressed?.Invoke();
    }

    // Input values
    public static Vector2 MoveInput;
    public static Vector2 LookInput;
    public static bool IsAiming;
    public static bool IsJumping;
    public static bool IsDashing;
    public static bool IsSlaming;
    public static bool IsShooting;
}