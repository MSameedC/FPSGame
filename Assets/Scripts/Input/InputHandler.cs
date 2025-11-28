using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public void Move(InputAction.CallbackContext context) => InputManager.MoveInput = context.ReadValue<Vector2>();
    public void Look(InputAction.CallbackContext context) => InputManager.LookInput = context.ReadValue<Vector2>();

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InputManager.IsShooting = true;
            InputManager.InvokeShootPressed();
        }
        if (context.canceled)
        {
            InputManager.IsShooting = false;
            InputManager.InvokeShootReleased();
        }
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InputManager.IsAiming = true;
            InputManager.InvokeAimPressed();
        }
        if (context.canceled)
        {
            InputManager.IsAiming = false;
            InputManager.InvokeAimReleased();
        }
    }

    public void Slam(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InputManager.IsSlaming = true;
            InputManager.InvokeSlamPressed();
        }
        if (context.canceled)
        {
            InputManager.IsSlaming = false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InputManager.IsJumping = true;
            InputManager.InvokeJumpPressed();
        }
        if (context.canceled)
        {
            InputManager.IsJumping = false;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InputManager.IsDashing = true;
            InputManager.InvokeDashPressed();
        }
        if (context.canceled)
        {
            InputManager.IsDashing = false;
        }
    }

    public void SwitchBullet(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InputManager.InvokeBulletSwitch();
        }
    }
}