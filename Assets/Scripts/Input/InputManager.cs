using System;
using UnityEngine;

public static class InputManager
{
    // Events (Private set)
    public static event Action OnShootPressed;
    public static event Action OnShootReleased;
    public static event Action OnJumpPressed;
    public static event Action OnDashPressed;
    public static event Action OnSlamPressed;
    public static event Action OnAimPressed;
    public static event Action OnAimReleased;
    public static event Action OnBulletSwitch;
    
    // Public invoke methods
    public static void InvokeDashPressed() => OnDashPressed?.Invoke();
    public static void InvokeJumpPressed() => OnJumpPressed?.Invoke();
    public static void InvokeSlamPressed() => OnSlamPressed?.Invoke();
    public static void InvokeShootPressed() => OnShootPressed?.Invoke();
    public static void InvokeShootReleased() => OnShootReleased?.Invoke();
    public static void InvokeAimPressed() => OnAimPressed?.Invoke();
    public static void InvokeAimReleased() => OnAimReleased?.Invoke();
    public static void InvokeBulletSwitch() => OnBulletSwitch?.Invoke();

    // Input values
    public static Vector2 MoveInput;
    public static Vector2 LookInput;
    public static bool IsAiming;
    public static bool IsJumping;
    public static bool IsDashing;
    public static bool IsSlaming;
    public static bool IsShooting;
}