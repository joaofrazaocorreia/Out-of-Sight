using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeedModifier;
    [SerializeField] private float crouchSpeedModifier;
    
    [SerializeField] private float verticalLookSensitivity;
    [SerializeField] private float horizontalLookSensitivity;

    private Rigidbody _rb;
    private PlayerCameraController _playerCameraController;
    private PlayerInput _playerInput;
    private PlayerInteraction _playerInteraction;
    
    private Vector3 _movementVector;
    private Vector3 _lookVector;
    
    private float _moveSpeed;

    private int _isRunning;
    private int _isCrouching;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerCameraController = GetComponentInChildren<PlayerCameraController>();
        _playerInput = GetComponent<PlayerInput>();
        _playerInteraction = GetComponentInChildren<PlayerInteraction>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        GetMoveSpeed();
        GetMoveVector();
        
        Move();
        Rotate();
    }

    private void GetInputs()
    {
        _lookVector = GetInput(_playerInput.actions["Look"]); 
        _movementVector = GetInput(_playerInput.actions["Move"]);
        GetInput(_playerInput.actions["Crouch"], ToggleCrouch);
        GetInput(_playerInput.actions["Run"], ToggleRun);
        GetInput(_playerInput.actions["Interact"], Interact);
    }

    private void GetInput(InputAction action, Action methodToCall)
    {
        if(action.WasPressedThisFrame())  methodToCall?.Invoke();
    }

    private Vector2 GetInput(InputAction action)
    {
        return action.ReadValue<Vector2>();
    }
    

    private void GetMoveSpeed()
    {
        _moveSpeed = walkSpeed * Mathf.Pow(runSpeedModifier, _isRunning) * Mathf.Pow(crouchSpeedModifier, _isCrouching);
    }

    private void GetMoveVector()
    {
        _movementVector = transform.forward * _movementVector.y + transform.right * _movementVector.x;
    }
    private void Move()
    {
       _rb.AddForce(_movementVector * _moveSpeed, ForceMode.Force);
    }

    private void Rotate()
    {
        RotatePlayer(_lookVector.x * horizontalLookSensitivity);
        _playerCameraController.RotateCamera(-_lookVector.y * verticalLookSensitivity);
    }

    private void RotatePlayer(float angle)
    {
        transform.Rotate(0f, angle, 0f);
    }
    
    private void ToggleRun() => _isRunning = 1 - _isRunning;
    
    private void ToggleCrouch() => _isCrouching = 1 - _isCrouching;

    private void Interact()
    {
        _playerInteraction.TryInteraction();
    }
    
}
