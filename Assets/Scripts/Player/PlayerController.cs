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
    [SerializeField] private float lookSensitivity = 1f;
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
    private int _isCrouched;
    private int IsCrouched
    {
        get => _isCrouched;
        set
        {
            if(value == _isCrouched) return;
            _isCrouched = value;
            if (!_isCrouching) _isCrouching = true;
            OnCrouch();
        }
    }
    [SerializeField] private float crouchSpeedModifier = 0.75f;
    [SerializeField] private float crouchAnimDuration;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchCenterY;
    private float _crouchCenterYDelta;
    private float _defaultPlayerHeight;
    private float _defaultPlayerCenterY;
    private bool _isCrouching;
    private float _crouchTimer;
    private float _targetControllerHeight;
    private float _targetControllerCenterY;
    private float _targetPlayerHeadPosY;

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
    public float _currentStamina { get; private set; } = 1;
    public event EventHandler OnStaminaUpdate;
    [SerializeField] private float staminaRegenSpeed = 0.25f;
    [SerializeField] private float staminaUsagePerSecond = 0.1f;
    
    private float speedBoost;
    public float SpeedBoost
    {
        get => speedBoost * Mathf.Pow(runSpeedModifier, _isRunning * _canRun) * Mathf.Pow(crouchSpeedModifier, _isCrouched);
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

    public event EventHandler ToggleMap;
    private bool _isPaused;

    void Start()
    {
        _controller     = GetComponent<CharacterController>();
        _head           = GetComponentInChildren<CinemachineCamera>();
        _acceleration   = Vector3.zero;
        _velocity       = Vector3.zero;
        _motion         = Vector3.zero;
        _sinPI4         = Mathf.Sin(Mathf.PI / 4);
        _startCamPos    = _head.transform.localPosition;
        SpeedBoost      = 1f;
        _player = GetComponent<Player>();
        _playerInput = GetComponent<PlayerInput>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _playerEquipment = GetComponent<PlayerEquipment>();
        _playerCarryInventory = GetComponent<PlayerCarryInventory>();
        _selectedEquipment = new float[9];
        _horizontalRotationPivot = transform;
        _originalHorizontalRotationPivot = _horizontalRotationPivot;
        _verticalRotationPivot = _head.transform.parent.transform;
        _originalVerticalRotationPivot = _horizontalRotationPivot;
        _defaultPlayerHeight = _controller.height;
        _defaultPlayerCenterY = _controller.center.y;
        _crouchCenterYDelta = crouchCenterY - _startCamPos.y + 0.25f;
        _startCharHeadPos = _charHead.transform.localPosition;
        _currentCharHeadPos = _startCharHeadPos;
        
        if(lockCursorOnStart)
            Cursor.lockState = CursorLockMode.Locked;
        
        _playerInput.actions["ToggleMap"].started += ToggleMapOverlay;
    }

    private void Update()
    {
        if (_playerInput.actions["Pause Game"].WasPressedThisFrame()) Pause();
        
        if(_isPaused) return;
        
        CheckHeadActiveCamera();
        GetInputs();
        CheckEquipmentChanged();

        if (_head.enabled && Cursor.lockState == CursorLockMode.Locked && _canLook)
        {
            UpdateBodyRotation();
            UpdateHeadRotation();
            TriggerHeadbob();
            UpdateStateTransitions();
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
        IsCrouched = 0;
    }
    
    private void EnableCrouch()
    {
        IsCrouched = isCrouchToggle ? 1 - IsCrouched : 1;
    }

    private void OnCrouch()
    {
        switch (IsCrouched)
        {
            case 1:
            {
                _targetControllerHeight = crouchHeight;
                _targetControllerCenterY = crouchCenterY;
                _targetPlayerHeadPosY = _startCharHeadPos.y + _crouchCenterYDelta;
                
                IsRunning = 0;
                
                _player.GainStatus(Player.Status.Doubtful);
                    
                break;
            }
            case 0:
            {
                _targetControllerHeight = _defaultPlayerHeight;
                _targetControllerCenterY = _defaultPlayerCenterY;
                _targetPlayerHeadPosY = _startCharHeadPos.y;

                if(_isRunning == 0) _player.LoseStatus(Player.Status.Doubtful);
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
        float rotation = _lookVector.x * lookSensitivity * 0.5f;

        _horizontalRotationPivot.Rotate(0f, rotation, 0f);
    }

    private void UpdateHeadRotation()
    {
        float rotation = _verticalRotationPivot.localEulerAngles.x;

        rotation -= _lookVector.y * lookSensitivity * 0.5f;

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
        _velocity += _acceleration * (0.5f * Time.fixedDeltaTime * SpeedBoost);


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
        OnStaminaUpdate?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateMouseSensitivity(float newSensitivity)
    {
        lookSensitivity = newSensitivity;
    }

    private void OnRun()
    {
        
        if (IsRunning == 1)
        {
            IsCrouched = 0;

            _player.GainStatus(Player.Status.Doubtful);
            return;
        }

        if(_isCrouched == 0) _player.LoseStatus(Player.Status.Doubtful);
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
                (Time.time * HeadbobFrequency * (1 + IsRunning / 2f)) * (HeadbobAmount / 100), HeadbobSmoothness * Time.deltaTime);
            pos.x = Mathf.Lerp(pos.x, Mathf.Cos
                (Time.time * HeadbobFrequency / 2 * (1 + IsRunning / 2f)) * (HeadbobAmount / 100), HeadbobSmoothness * Time.deltaTime);

            _head.transform.localPosition += pos;
        }
    }

    private void UpdateStateTransitions()
    {
        if (_isCrouching)
        {
            var deltaTime = Time.deltaTime / crouchAnimDuration;
            
            _controller.height = Mathf.Lerp(_controller.height, _targetControllerHeight, deltaTime);
            _controller.center = new Vector3(_controller.center.x,
                Mathf.Lerp(_controller.center.y, _targetControllerCenterY, deltaTime), _controller.center.z);
            _currentCharHeadPos.y = Mathf.Lerp(_currentCharHeadPos.y, _targetPlayerHeadPosY, deltaTime);
            _charHead.transform.localPosition = _currentCharHeadPos;
            
            _crouchTimer = Mathf.Lerp(_crouchTimer, _isCrouched, deltaTime);
            if(Mathf.Approximately(_crouchTimer, _isCrouched)) _isCrouching = false;
        }
    }

    private void ToggleMapOverlay(InputAction.CallbackContext a)
    {
        if(_isPaused) return;
        ToggleMap?.Invoke(this, null);
    }

    public void Pause()
    {
        _isPaused = !_isPaused;
    }
}
