using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool lockCursorOnStart = false;
    
    [Header("Acceleration Variables")]
    [SerializeField] private float _forwardAcceleration = 5;
    [SerializeField] private float _backwardAcceleration = -2;
    [SerializeField] private float _strafeAcceleration = 5;
    [SerializeField] private float _gravityAcceleration = -10;
    
    [Header("Velocity Variables")]
    [SerializeField] private float _maxForwardVelocity = 5;
    [SerializeField] private float _maxBackwardVelocity = -2;
    [SerializeField] private float _maxStrafeVelocity = 5;
    [SerializeField] private float _maxFallVelocity = -10;
    [SerializeField] private float _bodyCarryVelocityPercent = 50;
    
    [Header("Camera Variables")]
    [SerializeField] private float verticalLookSensitivity = 2f;
    [SerializeField] private float horizontalLookSensitivity = 2f;
    [SerializeField] private float _maxHeadUpAngle = 70;
    [SerializeField] private float _minHeadDownAngle = 290;
    [SerializeField] [Range(1f,30f)] private float HeadbobAmount = 5f;
    [SerializeField] [Range(1f,30f)] private float HeadbobSmoothness = 5f;
    [SerializeField] [Range(1f,30f)] private float HeadbobFrequency = 12f;
    [SerializeField] private GameObject _charHead;
    private Vector3 _startCharHeadPos;
    private Vector3 _currentCharHeadPos;
    private Vector3 _startCamPos;
    
    [Header("Crouch Variables")]
    [SerializeField] private bool isCrouchToggle;
    private int _isCrouching;
    private int IsCrouching
    {
        get => _isCrouching;
        set
        {
            if(value == _isCrouching) return;
            _isCrouching = value;
            OnCrouch();
        }
    }
    [SerializeField] private float crouchSpeedModifier = 0.75f;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchCenterY;
    private float _crouchCenterYDelta;
    private float _defaultPlayerHeight;
    private float _defaultPlayerCenterY;

    [Header("Run Variables")] [SerializeField]
    private bool isRunToggle;
    private int _isRunning;
    private int IsRunning
    {
        get => _isRunning;
        set
        {
            if(value == _isRunning) return;
            _isRunning = value;
            OnRun();
        }
    }
    private int _canRun;
    [SerializeField] private float runSpeedModifier = 1.5f;
    private const float maxStamina = 1;
    private float _currentStamina = 1;
    [SerializeField] private float staminaRegenSpeed = 0.25f;
    [SerializeField] private float staminaUsagePerSecond = 0.1f;
    
    private float speedBoost;
    public float SpeedBoost
    {
        get => speedBoost * Mathf.Pow(runSpeedModifier, _isRunning * _canRun) * Mathf.Pow(crouchSpeedModifier, _isCrouching);
        set 
        {
            speedBoost = value;
        }
    }
    private CharacterController _controller;
    private CinemachineCamera  _head;
    private Vector3 _acceleration;
    private Vector3 _velocity;
    private Vector3 _motion;
    private float   _sinPI4;
    private Player _player;
    private PlayerInput _playerInput;
    private PlayerInteraction _playerInteraction;
    private PlayerEquipment _playerEquipment; 
    private PlayerCarryInventory _playerCarryInventory;
    private UIManager _uiManager;
    private Animator _animator;
    private Vector2 _movementVector;
    private Vector3 _lookVector;
    private Transform _horizontalRotationPivot;
    private Transform _originalHorizontalRotationPivot;
    private Transform _verticalRotationPivot;
    private Transform _originalVerticalRotationPivot;
    private bool _canMove = true;
    private bool _canLook = true;
    private float[] _selectedEquipment;
    private int _resetInteract;

    void Start()
    {
        _controller     = GetComponent<CharacterController>();
        _head           = GetComponentInChildren<CinemachineCamera>();
        _acceleration   = Vector3.zero;
        _velocity       = Vector3.zero;
        _motion         = Vector3.zero;
        _sinPI4         = Mathf.Sin(Mathf.PI / 4);
        _startCamPos    = _head.transform.localPosition;
        _uiManager = FindAnyObjectByType<UIManager>();
        SpeedBoost      = 1f;
        _player = GetComponent<Player>();
        _playerInput = GetComponent<PlayerInput>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _playerEquipment = GetComponent<PlayerEquipment>();
        _playerCarryInventory = GetComponent<PlayerCarryInventory>();
        _animator = GetComponentInChildren<Animator>();
        _selectedEquipment = new float[9];
        _horizontalRotationPivot = transform;
        _originalHorizontalRotationPivot = _horizontalRotationPivot;
        _verticalRotationPivot = _head.transform.parent.transform;
        _originalVerticalRotationPivot = _horizontalRotationPivot;
        _defaultPlayerHeight = _controller.height;
        _defaultPlayerCenterY = _controller.center.y;
        _crouchCenterYDelta = crouchCenterY - _startCamPos.y;
        _startCharHeadPos = _charHead.transform.localPosition;
        _currentCharHeadPos = _startCharHeadPos;
        
        if(lockCursorOnStart)
            Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        CheckHeadActiveCamera();
        GetInputs();
        CheckEquipmentChanged();

        if (_head.enabled && Cursor.lockState == CursorLockMode.Locked && _canLook)
        {
            UpdateBodyRotation();
            UpdateHeadRotation();
            TriggerHeadbob();
        }
    }

    private void CheckHeadActiveCamera()
    {
        ToggleControls(_head.IsLive, _head.IsLive);
    }
    
    private void GetInputs()
    {
        _lookVector = GetInput(_playerInput.actions["Look"]); 
        _movementVector = GetInput(_playerInput.actions["Move"]);
        GetInput(_playerInput.actions["Crouch"], EnableCrouch, DisableCrouch, !isCrouchToggle);
        GetInput(_playerInput.actions["Run"], EnableRunning, DisableRun, !isRunToggle);
        _resetInteract = 0;
        GetInput(_playerInput.actions["Interact"], Interact, ResetInteract, true);
        GetInput(_playerInput.actions["InteractSecondary"], InteractSecondary, ResetInteract, true);
        GetInput(_playerInput.actions["UseEquipment"], UseEquipment, false);
        
        _selectedEquipment[0] = _playerInput.actions["EquipmentHotbar1"].WasPressedThisFrame() ? 1 : 0;
        _selectedEquipment[1] = _playerInput.actions["EquipmentHotbar2"].WasPressedThisFrame() ? 1 : 0;
        _selectedEquipment[2] = _playerInput.actions["EquipmentHotbar3"].WasPressedThisFrame() ? 1 : 0;
        _selectedEquipment[3] = _playerInput.actions["EquipmentHotbar4"].WasPressedThisFrame() ? 1 : 0;
        _selectedEquipment[4] = _playerInput.actions["EquipmentHotbar5"].WasPressedThisFrame() ? 1 : 0;
        /*
         _selectedEquipment[5] = _playerInput.actions["EquipmentHotbar6"].WasPressedThisFrame() ? 1 : 0;
        _selectedEquipment[6] = _playerInput.actions["EquipmentHotbar7"].WasPressedThisFrame() ? 1 : 0;
        _selectedEquipment[7] = _playerInput.actions["EquipmentHotbar8"].WasPressedThisFrame() ? 1 : 0;
        _selectedEquipment[8] = _playerInput.actions["EquipmentHotbar9"].WasPressedThisFrame() ? 1 : 0;
        */
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

    private void GetInput(InputAction action, Action methodToCall, bool holdInput)
    {
        if(holdInput ? action.IsPressed() : action.WasPressedThisFrame())  methodToCall?.Invoke();
    }

    private void GetInput(InputAction action, Action methodToCallWhenSuccessfull, Action methodToCallWhenUnsuccessful,
        bool holdInput)
    {
        if(holdInput ? action.IsPressed() : action.WasPressedThisFrame())  methodToCallWhenSuccessfull?.Invoke();
        else methodToCallWhenUnsuccessful?.Invoke();
    }

    private Vector2 GetInput(InputAction action)
    {
        return action.ReadValue<Vector2>();
    }

    private void EnableRunning()
    {
        IsRunning = isRunToggle ? 1 - IsRunning : 1;
    }

    private void DisableRun()
    {
        if(isRunToggle) return;
        IsRunning = 0;
    }

    private void DisableCrouch()
    {
        if(isCrouchToggle) return;
        IsCrouching = 0;
    }
    
    private void EnableCrouch()
    {
        IsCrouching = isCrouchToggle ? 1 - IsCrouching : 1;
    }

    private void OnCrouch()
    {
        switch (IsCrouching)
        {
            case 1:
            {
                _controller.height = crouchHeight;
                _controller.center = new Vector3(_controller.center.x, crouchCenterY, _controller.center.z);
                _currentCharHeadPos.y += _crouchCenterYDelta; 
                _charHead.transform.localPosition = new Vector3(_currentCharHeadPos.x, _currentCharHeadPos.y, _currentCharHeadPos.z);
                IsRunning = 0;
                break;
            }
            case 0:
            {
                _controller.height = _defaultPlayerHeight;
                _controller.center = new Vector3(_controller.center.x, _defaultPlayerCenterY, _controller.center.z);
                _currentCharHeadPos = _startCharHeadPos;
                _charHead.transform.localPosition = new Vector3(_currentCharHeadPos.x, _currentCharHeadPos.y, _currentCharHeadPos.z);
                break;
            }
        }
    }

    private void Interact()
    {
        _playerInteraction.TryInteraction(true);
    }
    
    private void InteractSecondary()
    {
        _playerInteraction.TryInteraction(false);
    }

    private void ResetInteract()
    {
        if (_resetInteract != 1)
        {
            _resetInteract++;
            return;
        }
        _playerInteraction.ResetInteract();
    }
    
    private void UseEquipment()
    {
        _playerEquipment.TryUseEquipment();
    }

    public void ToggleControls(bool canMovePlayer, bool canMoveCamera)
    {
        _canLook = canMoveCamera;
        _canMove = canMovePlayer;
        if (!canMovePlayer){ _velocity = Vector3.zero;}
    }

    private void UpdateBodyRotation()
    {
        float rotation = _lookVector.x * horizontalLookSensitivity;

        _horizontalRotationPivot.Rotate(0f, rotation, 0f);
    }

    private void UpdateHeadRotation()
    {
        float rotation = _verticalRotationPivot.localEulerAngles.x;

        rotation -= _lookVector.y * verticalLookSensitivity;

        if (rotation < 180)
            rotation = Mathf.Min(rotation, _maxHeadUpAngle);
        else
            rotation = Mathf.Max(rotation, _minHeadDownAngle);

        _verticalRotationPivot.localEulerAngles = new Vector3(rotation, 0f, 0f);
    }

    void FixedUpdate()
    {
        if (_canMove)
        {
            UpdateAcceleration();
            UpdateVelocity();  
        }
        
        UpdatePosition();
        UpdateStamina();
        UpdateAnimator();
    }

    private void UpdateAcceleration()
    {
        UpdateForwardAcceleration();
        UpdateStrafeAcceleration();
        UpdateVerticalAcceleration();
    }

    private void UpdateForwardAcceleration()
    {
        float forwardAxis = _movementVector.y;

        if (!_head.enabled || Cursor.lockState != CursorLockMode.Locked || !_canMove)
            forwardAxis = 0f;
        

        if (forwardAxis > 0f)
            _acceleration.z = _forwardAcceleration;

        else if (forwardAxis < 0f)
            _acceleration.z = _backwardAcceleration;

        else
            _acceleration.z = 0f;
    }

    private void UpdateStrafeAcceleration()
    {
        float strafeAxis = _movementVector.x;

        if (!_head.enabled || Cursor.lockState != CursorLockMode.Locked || !_canMove )
            strafeAxis = 0f;


        if (strafeAxis == 0f)
            _acceleration.x = 0f;

        else
            _acceleration.x = _strafeAcceleration * _movementVector.x;
    }

    private void UpdateVerticalAcceleration()
    {
        _acceleration.y = _gravityAcceleration;
    }

    private void UpdateVelocity()
    {
        _velocity += 0.5f * (_acceleration * Time.fixedDeltaTime) * SpeedBoost;


        if (_acceleration.z == 0f || _acceleration.z * _velocity.z < 0f)
        {
            if (_velocity.z > 0.5f)
                _velocity.z -= _velocity.z/5;
            else if (_velocity.z < -0.5f)
                _velocity.z -= _velocity.z/5;


            if (_velocity.z < 0.5f && _velocity.z > -0.5f)
                _velocity.z = 0f;
        }

        else if (_velocity.x == 0f)
            _velocity.z = Mathf.Clamp
                (_velocity.z, _maxBackwardVelocity * SpeedBoost, _maxForwardVelocity * SpeedBoost);

        else
            _velocity.z = Mathf.Clamp
                (_velocity.z, _maxBackwardVelocity * SpeedBoost * _sinPI4, _maxForwardVelocity * SpeedBoost * _sinPI4);
       


        if (_acceleration.x == 0f || _acceleration.x * _velocity.x < 0f)
        {
            if (_velocity.x > 0.5f)
                _velocity.x -= _velocity.x/5;
            else if (_velocity.x < -0.5f)
                _velocity.x -= _velocity.x/5;


            if (_velocity.x < 0.5f && _velocity.x > -0.5f)
                _velocity.x = 0f;
        }

        else if (_velocity.z == 0f)
            _velocity.x = Mathf.Clamp
                (_velocity.x, -_maxStrafeVelocity * SpeedBoost, _maxStrafeVelocity * SpeedBoost);

        else
            _velocity.x = Mathf.Clamp
                (_velocity.x, -_maxStrafeVelocity * SpeedBoost * _sinPI4, _maxStrafeVelocity * SpeedBoost * _sinPI4);



        if (_controller.isGrounded)
            _velocity.y = -0.1f;

        else
            _velocity.y = Mathf.Max(_velocity.y, _maxFallVelocity);
    }

    private void UpdatePosition()
    {
        _motion = _velocity * Time.fixedDeltaTime;

        if(_playerCarryInventory.CarryingBody)
            _motion *= _bodyCarryVelocityPercent / 100;

        _motion = transform.TransformVector(_motion);

        _controller.Move(_motion);
    }

    private void UpdateStamina()
    {
        _currentStamina = _isRunning == 1 && _velocity.magnitude > 1f ? _currentStamina -= staminaUsagePerSecond * Time.fixedDeltaTime : _currentStamina += staminaRegenSpeed * Time.fixedDeltaTime;
        _canRun = _currentStamina > 0f ? 1 : 0;
        _currentStamina = math.clamp(_currentStamina, 0f, maxStamina);
        _uiManager.UpdateStamina(_currentStamina);
    }

    private void UpdateAnimator()
    {
        _animator.SetBool("IsWalking", _velocity.magnitude > 0.1f);
        _animator.SetBool("IsRunning", IsRunning == 1);
    }

    private void OnRun()
    {
        if (IsRunning == 1)
        {
            IsCrouching = 0;

            if(!_player.status.Contains(Player.Status.Doubtful))
                _player.status.Add(Player.Status.Doubtful);
        }

        else if(_player.status.Contains(Player.Status.Doubtful))
        {
            _player.status.Remove(Player.Status.Doubtful);
        }
    }

    private void TriggerHeadbob()
    {
        if (_movementVector != Vector2.zero)
            Headbob();
        
        else if (_head.transform.localPosition != _startCamPos)
            _head.transform.localPosition = Vector3.Lerp(_head.transform.localPosition, _startCamPos, HeadbobSmoothness * Time.deltaTime);
    }

    private void Headbob()
    {
        if(PlayerPrefs.GetInt("Headbob", 1) == 1)
        {
            Vector3 pos = Vector3.zero;

            pos.y = Mathf.Lerp(pos.y, Mathf.Sin
                (Time.time * HeadbobFrequency) * (HeadbobAmount / 100), HeadbobSmoothness * Time.deltaTime);
            pos.x = Mathf.Lerp(pos.x, Mathf.Cos
                (Time.time * HeadbobFrequency / 2) * (HeadbobAmount / 100), HeadbobSmoothness * Time.deltaTime);

            _head.transform.localPosition += pos;
        }
    }
}
