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

    private float _interactionDuration;
    private float _interactionCooldownTimer;
    private bool _finishedInteraction;
    private bool _interactionReady;
    private bool _startedInteraction;

    private RaycastHit _hit;

    private GameObject _activeObject;
    
    private InteractiveObject ActiveInteractiveObject
    {
        get => _activeInteractiveObject;
        set
        {
            if(value == _activeInteractiveObject) return;
            
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
            var hitInteractable = _hit.collider.GetComponentInParent<InteractiveObject>();
            ActiveInteractiveObject = hitInteractable;
        }
        else
        {
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
    
    public void TryInteraction()
    {
        if (!_interactionReady)
        {
            _interactionCooldownTimer = interactionCooldown;
            return;
        }
        if(CheckValidInteraction()) Interact(ActiveInteractiveObject.InteractiveType);
    }

    public void ResetInteract()
    {
        _interactionDuration = _activeInteractiveObject != null ? _activeInteractiveObject.InteractionDuration : 0;
        _player.LoseStatus(Player.Status.Suspicious);
        _uiManager.ToggleInteractingBar(false);
    }
    
    private bool CheckValidInteraction()
    {
        return ActiveInteractiveObject != null && CheckCanInteract() ;
    }

    private bool CheckCanInteract()
    {
        if(!ActiveInteractiveObject.HasRequirement) return true;
        switch (ActiveInteractiveObject.InteractiveType)
           {
            case InteractiveType.DirectItemRequirement:
                return _playerInventory.HasItem(ActiveInteractiveObject.RequirementObject);
            
            case InteractiveType.DirectEquipmentRequirement:
                return _playerEquipment.CurrentEquipment == ActiveInteractiveObject.RequirementEquipment && _playerEquipment.CurrentEquipment.CanBeUsed;
            }
        return false;
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
                _playerInventory.AddItem(ActiveInteractiveObject.GetComponent<Item>(), 1);
                break;
            
            case InteractiveType.DirectItemRequirement:
                if(ActiveInteractiveObject.ConsumeItemRequirement) _playerInventory.RemoveItem(ActiveInteractiveObject.RequirementObject);
                break;
            
            case InteractiveType.DirectEquipmentRequirement:
                _playerEquipment.CurrentEquipment.Used(ActiveInteractiveObject);
                break;
        }
        
        ActiveInteractiveObject.Interact();
        
        if(ActiveInteractiveObject.IsInteractionSuspicious) _player.LoseStatus(Player.Status.Suspicious);
        _uiManager.ToggleInteractingBar(false);
        
        _finishedInteraction = true;
        _interactionReady = false;
        ActiveInteractiveObject = null;
    }

    private void UpdateInteractionUI()
    {
        if (ActiveInteractiveObject == null || !ActiveInteractiveObject.enabled)
        {
            _uiManager.ToggleInteractionMessage(false);
            return;
        }

        _uiManager.UpdateInteractionText(ActiveInteractiveObject.GetInteractionText(CheckCanInteract()));
        _uiManager.ToggleInteractionMessage(true);
    }
}