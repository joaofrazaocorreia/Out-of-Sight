using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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
    private PlayerEquipment _playerEquipment;
    
    private Vector3 _movementVector;
    private Vector3 _lookVector;

    private Transform _rotationPivot;
    private Transform _originalRotationPivot;
    
    private float _moveSpeed;

    private int _isRunning;
    private int _isCrouching;
    
    private bool _canMove = true;
    private bool _canLook = true;
    
    private float[] _selectedEquipment;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerCameraController = GetComponentInChildren<PlayerCameraController>();
        _playerInput = GetComponent<PlayerInput>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _playerEquipment = GetComponent<PlayerEquipment>();
        _selectedEquipment = new float[9];

        _rotationPivot = transform;
        _originalRotationPivot = _rotationPivot;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        GetInputs();
        CheckEquipmentChanged();
    }
    
    private void GetInputs()
    {
        _lookVector = GetInput(_playerInput.actions["Look"]); 
        _movementVector = GetInput(_playerInput.actions["Move"]);
        GetInput(_playerInput.actions["Crouch"], ToggleCrouch, false);
        GetInput(_playerInput.actions["Run"], ToggleRun, false);
        GetInput(_playerInput.actions["Interact"], Interact, true);
        GetInput(_playerInput.actions["UseEquipment"], UseEquipment, false);
        
        _selectedEquipment[0] = _playerInput.actions["EquipmentHotbar1"].ReadValue<float>();
        _selectedEquipment[1] = _playerInput.actions["EquipmentHotbar2"].ReadValue<float>();
        _selectedEquipment[2] = _playerInput.actions["EquipmentHotbar3"].ReadValue<float>();
        _selectedEquipment[3] = _playerInput.actions["EquipmentHotbar4"].ReadValue<float>();
        _selectedEquipment[4] = _playerInput.actions["EquipmentHotbar5"].ReadValue<float>();
        _selectedEquipment[5] = _playerInput.actions["EquipmentHotbar6"].ReadValue<float>();
        _selectedEquipment[6] = _playerInput.actions["EquipmentHotbar7"].ReadValue<float>();
        _selectedEquipment[7] = _playerInput.actions["EquipmentHotbar8"].ReadValue<float>();
        _selectedEquipment[8] = _playerInput.actions["EquipmentHotbar9"].ReadValue<float>();
    }

    private void CheckEquipmentChanged()
    {
        for (int i = 0; i < _selectedEquipment.Length; i++)
        {
            if (!Mathf.Approximately(_selectedEquipment[i], 1)) continue;
            
            _playerEquipment.NewEquipmentSelected(i);
            break;
        }
    }
    
    private void FixedUpdate()
    {
        if (_canMove)
        {
            GetMoveSpeed();
            GetMoveVector();
            Move();
        }
        
        if(_canLook) Rotate();
    }

    private void GetInput(InputAction action, Action methodToCall, bool holdInput)
    {
        if(holdInput ? action.IsPressed() : action.WasPressedThisFrame())  methodToCall?.Invoke();
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
        _rotationPivot.Rotate(0f, angle, 0f);
    }
    
    private void ToggleRun() => _isRunning = 1 - _isRunning;
    
    private void ToggleCrouch() => _isCrouching = 1 - _isCrouching;

    private void Interact()
    {
        _playerInteraction.TryInteraction();
    }

    private void UseEquipment()
    {
        _playerEquipment.TryUseEquipment();
    }

    public void ToggleControls(bool movement, bool camera)
    {
        _canLook = camera;
        _canMove = movement;
    }

    public void ExtendedCameraInUse(bool isInUse, Transform camera)
    {
        _rotationPivot = isInUse ? camera : _originalRotationPivot;
        _playerCameraController.ExtendedCameraInUse(isInUse, camera);
    }
}
