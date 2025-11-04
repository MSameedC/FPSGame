using System;
using UnityEngine;

// Abstraction
public interface IInputProvider
{
    Vector2 MoveInput { get; }
    Vector2 LookInput { get; }
    bool IsJumping { get; }
    bool IsDashing { get; }
    bool IsShooting { get; }
    bool IsSlaming { get; }
    bool IsAiming { get; }
    public event Action OnShootPressed;
    public event Action OnShootReleased;
    public event Action OnJumpPressed;        // NEW: Single-tap events
    public event Action OnDashPressed;
    public event Action OnSlamPressed;
    public event Action OnAimPressed;
    public event Action OnAimReleased;
}