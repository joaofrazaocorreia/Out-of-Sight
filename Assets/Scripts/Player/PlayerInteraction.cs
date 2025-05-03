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
    private Animator _animator;

    public float _interactionDuration;
    private float _interactionCooldownTimer;
    private bool _finishedInteraction;
    private bool _interactionReady;
    private bool _startedInteraction;
    private bool _isInteractionSuspicious;

    private RaycastHit _hit;

    private GameObject _activeObject;
    
    private GameObject _lastHitObject;
    private List<InteractiveObject> _tempHitInteractableObjects = new List<InteractiveObject>();
    private InteractiveObject[] _hitInteractables;

    public InteractiveObject[] HitInteractables
    {
        get => _hitInteractables;
        private set
        {
            if (_hitInteractables == value) return;
            _hitInteractables = value;
            OnHitInteractableChanged?.Invoke(this, EventArgs.Empty);
        } 
    }
    private int _hitIndex;
    private bool _interactionAudioPlaying;

    public event EventHandler OnInteractionStart;
    public event EventHandler OnInteractionStop;
    public event EventHandler WhileInteracting;
    public event EventHandler OnHitInteractableChanged;
    public event EventHandler OnSuspiciousAction;
    
    public InteractiveObject ActiveInteractiveObject
    {
        get => _activeInteractiveObject;
        private set
        {
            if (value == _activeInteractiveObject) return;

            if (_activeInteractiveObject != null)
            {
                if (_interactionAudioPlaying )
                {
                    _activeInteractiveObject.WhileInteractAudioPlayer.Stop();
                    _interactionAudioPlaying = false;
                }
                
                if(value == null) StopInteraction();
            }
            
            _activeInteractiveObject = value;
            _interactionDuration = _activeInteractiveObject ? _activeInteractiveObject.InteractionDuration : 0;
            if(ActiveInteractiveObject != null) OnInteractionStart?.Invoke(this, EventArgs.Empty);
        }
    }
    
    
    [CanBeNull] private InteractiveObject _activeInteractiveObject;

    private void Start()
    {
        _playerInventory = GetComponent<PlayerInventory>();
        _playerEquipment = GetComponent<PlayerEquipment>();
        _player = GetComponent<Player>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        GetInteractiveObject();
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
                var interactable = _tempHitInteractableObjects[i];
                if (interactable.InteractiveType == InteractiveType.Indirect) _tempHitInteractableObjects.Remove(interactable);
            }
            HitInteractables = _tempHitInteractableObjects.ToArray();
            
        }
        else
        {
            HitInteractables = null;
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

        if (HitInteractables == null)
        {
            return;
        }
        StartInteract(isPrimaryInteraction);
    }

    public void ResetInteract()
    {
        ActiveInteractiveObject = null;
    }   
    
    public bool CheckValidInteraction(InteractiveObject interactiveObject)
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
        switch (HitInteractables.Length)
        {
            case 1:
                ActiveInteractiveObject = isPrimaryInteraction ? HitInteractables[0] : null;
                break;
            case 2: 
                ActiveInteractiveObject = isPrimaryInteraction ? HitInteractables[0] : HitInteractables[1];
                break;
            default: ActiveInteractiveObject = null; break;
        }

        if (!CheckValidInteraction(ActiveInteractiveObject))
        {
            ActiveInteractiveObject = null;
            return;
        }
        Interact(ActiveInteractiveObject.InteractiveType);
    }

    private void Interact(InteractiveType interactiveType)
    {
        _isInteractionSuspicious = ActiveInteractiveObject.IsInteractionSuspicious && !ActiveInteractiveObject.WhitelistedDisguises.Contains(_player.disguise);

        if(_isInteractionSuspicious &&  Mathf.Approximately(_interactionDuration, ActiveInteractiveObject.InteractionDuration))
            _player.GainStatus(Player.Status.Suspicious);

        _interactionDuration = Mathf.Max(_interactionDuration - Time.deltaTime, 0f);


        if (ActiveInteractiveObject.WhileInteractAudioPlayer != null && !_interactionAudioPlaying)
        {
            ActiveInteractiveObject.WhileInteractAudioPlayer.Play();
            _interactionAudioPlaying = true;
        }

        if (_interactionDuration > 0f)
        {
            WhileInteracting?.Invoke(this, EventArgs.Empty);
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
        
        ActiveInteractiveObject.Interact();
        _finishedInteraction = true;
        ActiveInteractiveObject = null;
    }

    private void StopInteraction()
    {
        if (ActiveInteractiveObject != null)
        {
            if (_isInteractionSuspicious)
            {
                if(_finishedInteraction) OnSuspiciousAction?.Invoke(this, EventArgs.Empty);
                _player.LoseStatus(Player.Status.Suspicious);
            }
        
            if(ActiveInteractiveObject.WhileInteractAudioPlayer != null) ActiveInteractiveObject.WhileInteractAudioPlayer.Stop();
        }     
        _finishedInteraction = true;
        _interactionReady = false;
        _lastHitObject = null;
        HitInteractables = null;
        _interactionAudioPlaying = false;
        
        OnInteractionStop?.Invoke(this, EventArgs.Empty);
    }

    public InteractiveObject GetInteractiveObject(int index)
    {
        return HitInteractables.Length > index ? HitInteractables[index] : null;
    }
    
    private void InteractAnimation()
    {
        _animator.SetTrigger("IsInteracting");
    }

    
}