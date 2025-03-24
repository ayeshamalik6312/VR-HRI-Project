using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StartSetup : MonoBehaviour
{
    // When setupAction button pressed, do the things in Setup() func.

    public MoveBody MoveBody;
    public MoveIKTarget MoveIKTarget;
    public InputActionReference setupAction;

    private void Awake()
    {
        setupAction.action.Enable();
        setupAction.action.performed += Setup;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDestroy()
    {
        setupAction.action.Disable();
        setupAction.action.performed -= Setup;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Setup(InputAction.CallbackContext context)
    {
        MoveBody.setup = true;
        MoveIKTarget.move = true;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                setupAction.action.Disable();
                setupAction.action.performed -= Setup;
                break;
            case InputDeviceChange.Reconnected:
                setupAction.action.Enable();
                setupAction.action.performed += Setup;
                break;
        }
    }
}
