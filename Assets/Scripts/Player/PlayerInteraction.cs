using System;
using System.Collections.Generic;
using System.Linq;
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
    private List<InteractiveObject> _tempHitInteractableObjects = new List<InteractiveObject>();
    private InteractiveObject[] _hitInteractables;
    private int _hitIndex;
    private bool _interactionAudioPlaying;
    
    private InteractiveObject ActiveInteractiveObject
    {
        get => _activeInteractiveObject;
        set
        {
            if (value == _activeInteractiveObject) return;

            if (_activeInteractiveObject != null)
            {
                if (_interactionAudioPlaying )
                {
                    _activeInteractiveObject.WhileInteractAudioPlayer.Stop();
                    _interactionAudioPlaying = false;
                }
            }
            
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
            _tempHitInteractableObjects = _lastHitObject.GetComponentsInParent<InteractiveObject>().ToList();
            for (int i = 0; i < _tempHitInteractableObjects.Count; i++)
            {
                if (_tempHitInteractableObjects[i].InteractiveType == InteractiveType.Indirect) _tempHitInteractableObjects.Remove(_tempHitInteractableObjects[i]);
            }
            _hitInteractables = _tempHitInteractableObjects.ToArray();
            
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
        switch (interactiveObject.InteractiveType)
           {
            case InteractiveType.DirectItemRequirement:
                return _playerInventory.HasItem(interactiveObject.RequiredItem);
            
            case InteractiveType.DirectEquipmentRequirement:
                if(_playerEquipment.CurrentEquipment == null) return false;
                return _playerEquipment.CurrentEquipment.EquipmentType == interactiveObject.RequiredEquipment && _playerEquipment.CurrentEquipment.CanBeUsed;
           }
        
        return !interactiveObject.HasRequirement;
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

        if (ActiveInteractiveObject.WhileInteractAudioPlayer != null && !_interactionAudioPlaying)
        {
            ActiveInteractiveObject.WhileInteractAudioPlayer.Play();
            _interactionAudioPlaying = true;
        }

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
                if(ActiveInteractiveObject.ConsumeItemRequirement) _playerInventory.RemoveItem(ActiveInteractiveObject.RequiredItem);
                InteractAnimation();
                break;
            
            case InteractiveType.DirectEquipmentRequirement:
                _playerEquipment.CurrentEquipment.Used(ActiveInteractiveObject);
                break;
            
            case InteractiveType.DirectNoRequirement:
                InteractAnimation();
                break;
        }
        
        
        if(ActiveInteractiveObject.IsInteractionSuspicious) _player.LoseStatus(Player.Status.Suspicious);
        ActiveInteractiveObject.Interact();
        
        _uiManager.ToggleInteractingBar(false);
        
        if(ActiveInteractiveObject.WhileInteractAudioPlayer != null) ActiveInteractiveObject.WhileInteractAudioPlayer.Stop();
        
        _finishedInteraction = true;
        _interactionReady = false;
        _lastHitObject = null;
        _hitInteractables = null;
        ActiveInteractiveObject = null;
        _interactionAudioPlaying = false;
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