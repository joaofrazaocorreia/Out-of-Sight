using System;
using Unity.Mathematics;
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
    
    private Vector3 _movementVector;
    private Vector3 _inputVector;
    private Vector3 _lookVector;
    
    private float _moveSpeed;

    private int _isRunning;
    private int _isCrouching;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerCameraController = GetComponentInChildren<PlayerCameraController>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        GetInputs();
        
        Move();
        Rotate();
    }

    private void GetInputs()
    {
        GetLookInput(_playerInput.actions["Look"]); 
        GetMoveInput(_playerInput.actions["Move"]);
        GetCrouchInput(_playerInput.actions["Crouch"]);
        GetRunInput(_playerInput.actions["Run"]);
    }

    private void GetMoveInput(InputAction action)
    {
        _inputVector = action.ReadValue<Vector2>().normalized;
        
        GetMoveSpeed();
        GetMoveVector();
    }
    
    private void GetLookInput(InputAction action)
    {
        _lookVector = action.ReadValue<Vector2>();  
    }

    private void GetCrouchInput(InputAction action)
    {
        ToggleCrouch();
    }

    private void GetRunInput(InputAction action)
    {
        ToggleRun();
    }
    

    private void GetMoveSpeed()
    {
        _moveSpeed = walkSpeed * Mathf.Pow(runSpeedModifier, _isRunning) * Mathf.Pow(crouchSpeedModifier, _isCrouching);
    }

    private void GetMoveVector()
    {
        _movementVector = transform.forward * _inputVector.y + transform.right * _inputVector.x;
    }
    private void Move()
    {
       _rb.AddForce(_movementVector * _moveSpeed, ForceMode.Force);
    }

    private void Rotate()
    {
        RotatePlayer(_lookVector.x * horizontalLookSensitivity);
        _playerCameraController.RotateCamera(_lookVector.y * verticalLookSensitivity);
    }

    private void RotatePlayer(float angle)
    {
        transform.Rotate(0f, angle, 0f);
    }
    
    private void ToggleRun() => _isRunning = 1 - _isRunning;
    
    private void ToggleCrouch() => _isCrouching = 1 - _isCrouching;
    
    
}
