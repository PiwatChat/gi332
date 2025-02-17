using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<Vector2> MoveEvent;
    public event Action<bool> JumpEvent;
    public event Action<bool> SprintEvent;
    public event Action<bool> CrouchEvent;
    public event Action<bool> InteractEvent;
    
    private Controls controls;

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }
        
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            InteractEvent?.Invoke(false);
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CrouchEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            CrouchEvent?.Invoke(false);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            JumpEvent?.Invoke(false);
        }
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            SprintEvent?.Invoke(false);
        }
    }
}
