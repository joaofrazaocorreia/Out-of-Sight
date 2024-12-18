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
            if(_lastHitObject == _hit.collider.gameObject) return;
            
            _lastHitObject = _hit.collider.gameObject;
            _hitInteractables = _lastHitObject.GetComponentsInParent<InteractiveObject>();
        }
        else
        {
            _hitInteractables = null;
            _lastHitObject = null;
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
    
    private bool CheckValidInteraction(InteractiveObject interactiveObject)
    {
        return interactiveObject != null && CheckCanInteract(interactiveObject) && interactiveObject.enabled;
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
        if(!CheckValidInteraction(ActiveInteractiveObject)) return;
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
        _lastHitObject = null;
        _hitInteractables = null;
        ActiveInteractiveObject = null;
    }

    private void UpdateInteractionUI()
    {
        
        if (_hitInteractables == null || _hitInteractables.Length == 0)
        {
            DisableInteractionUI();
            return;
        }
        
        _uiManager.UpdateInteractionUi(_hitInteractables, CheckValidInteraction(GetInteractiveObject(0)), CheckValidInteraction(GetInteractiveObject(1)));
    }

    private InteractiveObject GetInteractiveObject(int index)
    {
        return _hitInteractables.Length > index ? _hitInteractables[index] : null;
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