using System;
using Interaction;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float raycastDistance;
    [SerializeField] private float interactionCooldown;
    [SerializeField] private LayerMask whatIsInteractable;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private GameObject _head;
    
    private PlayerInventory _playerInventory;
    private PlayerEquipment _playerEquipment;
    private Player _player;
    private UIManager _uiManager;
    private Animator _animator;

    private float _interactionDuration;
    private float _interactionCooldownTimer;
    private bool _finishedInteraction;
    private bool _interactionReady;
    private bool _startedInteraction;

    private RaycastHit _hit;

    private GameObject _activeObject;
    
    private GameObject _lastHitObject;
    private InteractiveObject[] _hitInteractables;
    private int _hitIndex;
    
    private InteractiveObject ActiveInteractiveObject
    {
        get => _activeInteractiveObject;
        set
        {
            if (value == _activeInteractiveObject) return;

            _activeInteractiveObject = value;
            
            _interactionDuration = _activeInteractiveObject ? _activeInteractiveObject.InteractionDuration : 0;
        }
    }
    
    [CanBeNull] private InteractiveObject _activeInteractiveObject;

    private void Start()
    {
        _playerInventory = GetComponent<PlayerInventory>();
        _playerEquipment = GetComponent<PlayerEquipment>();
        _player = GetComponent<Player>();
        _uiManager = FindFirstObjectByType<UIManager>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        GetInteractiveObject();
        UpdateInteractionUI();
        if (!_interactionReady) InteractionCooldown();
    }

    private void GetInteractiveObject()
    {
        if (Physics.Raycast(raycastOrigin.position,  raycastOrigin.forward, out _hit, raycastDistance))
        {
            var temp =  _hit.collider.gameObject;
            if(_lastHitObject == temp) return;
            
            _lastHitObject = temp;
            _hitInteractables = _lastHitObject.GetComponentsInParent<InteractiveObject>();
        }
        else
        {
            _hitInteractables = null;
            ActiveInteractiveObject = null;
        }
    }

    private void InteractionCooldown()
    {
        if (_finishedInteraction)
        {
            _interactionCooldownTimer = interactionCooldown;
            _finishedInteraction = false;
        }
        
        _interactionCooldownTimer -= Time.deltaTime;
        
        if(_interactionCooldownTimer > 0) return;

        _interactionReady = true;
    }
    
    public void TryInteraction(bool isPrimaryInteraction)
    {
        if (!_interactionReady)
        {
            _interactionCooldownTimer = interactionCooldown;
            return;
        }
        if(_hitInteractables == null) return;
        StartInteract(isPrimaryInteraction);
    }

    public void ResetInteract()
    {
        ActiveInteractiveObject = null;
        _uiManager.ToggleInteractingBar(false);
    }   
    
    private bool CheckValidInteraction()
    {
        return ActiveInteractiveObject != null && CheckCanInteract(ActiveInteractiveObject) && ActiveInteractiveObject.enabled;
    }

    private bool CheckCanInteract(InteractiveObject interactiveObject)
    {
        if(!interactiveObject.HasRequirement) return true;
        switch (interactiveObject.InteractiveType)
           {
            case InteractiveType.DirectItemRequirement:
                return _playerInventory.HasItem(interactiveObject.RequirementObject);
            
            case InteractiveType.DirectEquipmentRequirement:
                return _playerEquipment.CurrentEquipment == interactiveObject.RequirementEquipment && _playerEquipment.CurrentEquipment.CanBeUsed;
            }
        return false;
    }

    private void StartInteract(bool isPrimaryInteraction)
    {
        switch (_hitInteractables.Length)
        {
            case 1:
                ActiveInteractiveObject = isPrimaryInteraction ? _hitInteractables[0] : null;
                break;
            case 2: 
                ActiveInteractiveObject = isPrimaryInteraction ? _hitInteractables[0] : _hitInteractables[1];
                break;
            default: ActiveInteractiveObject = null; break;
        }
        if(!CheckValidInteraction()) return;
        Interact(ActiveInteractiveObject.InteractiveType);
    }

    private void Interact(InteractiveType interactiveType)
    {
        _interactionDuration = Mathf.Max(_interactionDuration - Time.deltaTime, 0f);

        if(ActiveInteractiveObject.IsInteractionSuspicious) _player.GainStatus(Player.Status.Suspicious);

        if (_interactionDuration > 0f)
        {
            _uiManager.UpdateInteractingBarFillSize(1 - _interactionDuration / ActiveInteractiveObject.InteractionDuration);
            _uiManager.ToggleInteractingBar(true);
            return;
        }
        
        switch (interactiveType)
        {
            case InteractiveType.Item:
                _playerInventory.AddItem(ActiveInteractiveObject.GetComponent<Item>());
                InteractAnimation();
                break;
            
            case InteractiveType.DirectItemRequirement:
                if(ActiveInteractiveObject.ConsumeItemRequirement) _playerInventory.RemoveItem(ActiveInteractiveObject.RequirementObject);
                InteractAnimation();
                break;
            
            case InteractiveType.DirectEquipmentRequirement:
                _playerEquipment.CurrentEquipment.Used(ActiveInteractiveObject);
                break;
            
            case InteractiveType.DirectNoRequirement:
                InteractAnimation();
                break;
        }
        
        ActiveInteractiveObject.Interact();
        
        _uiManager.ToggleInteractingBar(false);
        
        _finishedInteraction = true;
        _interactionReady = false;
        ActiveInteractiveObject = null;
    }

    private void UpdateInteractionUI()
    {
        
        if (_hitInteractables == null || _hitInteractables.Length == 0)
        {
            DisableInteractionUI();
            return;
        }

        switch (_hitInteractables.Length)
        {
            case 1:
                if (!_hitInteractables[0].enabled)
                {
                    _uiManager.ToggleInteractionMessage(false,0);
                    return;
                }
                
                _uiManager.UpdateInteractionUi(_hitInteractables, CheckCanInteract(_hitInteractables[0]), false);
                break;
            case 2:
                if (!_hitInteractables[1].enabled && !_hitInteractables[0].enabled) return;
                
                _uiManager.ToggleInteractionMessage(!_hitInteractables[0].enabled, 0);
                _uiManager.ToggleInteractionMessage(!_hitInteractables[1].enabled, 1);
                
                _uiManager.UpdateInteractionUi(_hitInteractables, CheckCanInteract(_hitInteractables[0]), CheckCanInteract(_hitInteractables[1]));
                break;
        }
        

        
    }

    private void DisableInteractionUI()
    {
        _uiManager.ToggleInteractionMessage(false, 0);
        _uiManager.ToggleInteractionMessage(false, 1);
    }
    
    private void InteractAnimation()
    {
        _animator.SetTrigger("IsInteracting");
    }
}