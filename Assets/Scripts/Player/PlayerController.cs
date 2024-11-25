using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _forwardAcceleration = 5;
    [SerializeField] private float _backwardAcceleration = -2;
    [SerializeField] private float _strafeAcceleration = 5;
    [SerializeField] private float _gravityAcceleration = -10;
    [SerializeField] private float runSpeedModifier = 1.5f;
    [SerializeField] private float crouchSpeedModifier = 0.75f;
    [SerializeField] private float _maxForwardVelocity = 5;
    [SerializeField] private float _maxBackwardVelocity = -2;
    [SerializeField] private float _maxStrafeVelocity = 5;
    [SerializeField] private float _maxFallVelocity = -10;
    [SerializeField] private float verticalLookSensitivity = 2f;
    [SerializeField] private float horizontalLookSensitivity = 2f;
    [SerializeField] private float _maxHeadUpAngle = 70;
    [SerializeField] private float _minHeadDownAngle = 290;
    /*
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] [Range(0.1f, 2f)] private float footstepInterval = 0.45f;
    */
    [SerializeField] [Range(1f,30f)] private float HeadbobAmount = 5f;
    [SerializeField] [Range(1f,30f)] private float HeadbobSmoothness = 5f;
    [SerializeField] [Range(1f,30f)] private float HeadbobFrequency = 12f;
    [SerializeField] private bool _isCrouchToggle;
    [SerializeField] private bool _isRunToggle;

    private float speedBoost;
    public float SpeedBoost
    {
        get => speedBoost * Mathf.Pow(runSpeedModifier, _isRunning) * Mathf.Pow(crouchSpeedModifier, _isCrouching);
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
    private Vector3 _startCamPos;
    /*
    private float _footstepTimer;
    */
    private PlayerInput _playerInput;
    private PlayerInteraction _playerInteraction;
    private PlayerEquipment _playerEquipment;
    private Vector2 _movementVector;
    private Vector3 _lookVector;
    private Transform _horizontalRotationPivot;
    private Transform _originalHorizontalRotationPivot;
    private Transform _verticalRotationPivot;
    private Transform _originalVerticalRotationPivot;
    private int _isRunning;
    private int _isCrouching;
    private bool _canMove = true;
    private bool _canLook = true;
    private float[] _selectedEquipment;

    void Start()
    {
        _controller     = GetComponent<CharacterController>();
        _head           = GetComponentInChildren<CinemachineCamera>();
        _acceleration   = Vector3.zero;
        _velocity       = Vector3.zero;
        _motion         = Vector3.zero;
        _sinPI4         = Mathf.Sin(Mathf.PI / 4);
        _startCamPos    = _head.transform.localPosition;
        /*
        _footstepTimer  = Time.time;
        */
        SpeedBoost      = 1f;
        _playerInput = GetComponent<PlayerInput>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _playerEquipment = GetComponent<PlayerEquipment>();
        _selectedEquipment = new float[9];
        _horizontalRotationPivot = transform;
        _originalHorizontalRotationPivot = _horizontalRotationPivot;
        _verticalRotationPivot = _head.transform;
        _originalVerticalRotationPivot = _horizontalRotationPivot;
    }

    private void Update()
    {
        GetInputs();
        CheckEquipmentChanged();

        if (_head.enabled && Cursor.lockState == CursorLockMode.Locked && _canLook)
        {
            UpdateBodyRotation();
            UpdateHeadRotation();
            TriggerHeadbob();
        }
    }
    
    private void GetInputs()
    {
        if (!_isCrouchToggle) _isCrouching = 0;
        if (!_isRunToggle) _isRunning = 0;
        
        _lookVector = GetInput(_playerInput.actions["Look"]); 
        _movementVector = GetInput(_playerInput.actions["Move"]);
        GetInput(_playerInput.actions["Crouch"], EnableCrouch, !_isCrouchToggle);
        GetInput(_playerInput.actions["Run"], EnableRunning, !_isRunToggle);
        GetInput(_playerInput.actions["Interact"], Interact, true);
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

    private Vector2 GetInput(InputAction action)
    {
        return action.ReadValue<Vector2>();
    }

    private void EnableRunning() => _isRunning = _isRunToggle ? 1 - _isRunning : 1;
    

    private void EnableCrouch() => _isCrouching = _isCrouchToggle ? 1 - _isCrouching : 1;

    private void Interact()
    {
        _playerInteraction.TryInteraction();
    }
    
    private void UseEquipment()
    {
        _playerEquipment.TryUseEquipment();
    }

    public void ToggleControls(bool canMovePlayer, bool canMoveCamera)
    {
        _canLook = canMoveCamera;
        _canMove = canMovePlayer;
    }

    public void ExtendedCameraInUse(bool isInUse, Transform cameraTransform)
    {
        print(cameraTransform);
        _horizontalRotationPivot = isInUse ? cameraTransform : _originalHorizontalRotationPivot;
        _verticalRotationPivot = isInUse ? cameraTransform : _originalVerticalRotationPivot;
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
        UpdateAcceleration();
        UpdateVelocity();
        UpdatePosition();
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

        _motion = transform.TransformVector(_motion);

        _controller.Move(_motion);
    }

    private void TriggerHeadbob()
    {
        if (_movementVector != Vector2.zero)
            Headbob();
        
        else if (_head.transform.localPosition != _startCamPos)
            _head.transform.localPosition = 
                Vector3.Lerp(_head.transform.localPosition, _startCamPos, HeadbobSmoothness * Time.deltaTime);
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

        /*
        if(_footstepTimer + footstepInterval <= Time.time)
        {
            int index = UnityEngine.Random.Range(0, footstepSounds.Length);
            float pitch = UnityEngine.Random.Range(0.8f, 1.2f);

            playerAudioSource.pitch = pitch;
            playerAudioSource.PlayOneShot(footstepSounds[index]);
            _footstepTimer = Time.time;
        }
        */
    }
}
