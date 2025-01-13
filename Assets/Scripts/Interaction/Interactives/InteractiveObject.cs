using System.Collections.Generic;
using Interaction;
using Interaction.Equipments;
using JetBrains.Annotations;
using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    [Header("Interactive Type")]
    [SerializeField] private InteractiveType interactiveType;
    [SerializeField] protected bool updateIfIndirect; 
    [Header("Requirements")]
    [SerializeField] [CanBeNull] protected ItemType requiredItem;
    [SerializeField] protected EquipmentType requiredEquipment;
    [Header("Requirement Options")]
    [SerializeField] protected bool consumeItemRequirement;
    [SerializeField] protected bool oneTimeRequirement;
    [Header("Interaction Options")]
    [SerializeField] protected bool firstInteractionDurationUnique;
    [SerializeField] protected float interactionDuration;
    [SerializeField] protected float secondInteractionDuration; 
    [Header("Detection Options")]
    [SerializeField] protected bool isInteractionSuspicious;
    [SerializeField] protected bool isSecondInteractionSuspicious;
    [Header("UI Options")]
    [SerializeField] protected string objectName;
    [SerializeField] protected string customInteractionMessage;

    [Header("Audio Options")] 
    [SerializeField] protected PlayAudio onInteractAudioPlayer;
    [SerializeField] protected PlayAudio whileInteractAudioPlayer;

    public InteractiveType InteractiveType
    {
        get => interactiveType;
        protected set => interactiveType = value;
    }
    
    public ItemType RequiredItem => requiredItem;
    public EquipmentType RequiredEquipment => requiredEquipment;
    public bool ConsumeItemRequirement => consumeItemRequirement;
    public bool OneTimeRequirement => oneTimeRequirement;
    public bool HasRequirement { get; protected set; }
    
    public bool FirstInteractionDurationUnique => firstInteractionDurationUnique;
    public float InteractionDuration
    {
        get => interactionDuration;
        private set => interactionDuration = value;
    }
    public float SecondInteractionDuration => secondInteractionDuration;
    
    public bool IsInteractionSuspicious => isInteractionSuspicious;
    public bool IsSecondInteractionSuspicious => isSecondInteractionSuspicious;
    
    public string ObjectName => objectName;
    public string CustomInteractionMessage => customInteractionMessage;
    
    public PlayAudio OnInteractAudioPlayer => onInteractAudioPlayer;
    public PlayAudio WhileInteractAudioPlayer => whileInteractAudioPlayer;
    
    
    private void Start()
    {
        HasRequirement = RequiredItem != ItemType.None || RequiredEquipment != EquipmentType.None;
    }

    public virtual void Interact()
    {
        if (InteractiveType == InteractiveType.Indirect)
        {
            if(!updateIfIndirect) return;
        }
        OneTimeRequirementCheck();
        UniqueFirstInteractionDurationCheck();
        isInteractionSuspicious = IsSecondInteractionSuspicious;

        if (onInteractAudioPlayer != null) onInteractAudioPlayer.Play();
    }

    private void OneTimeRequirementCheck()
    {
        if (HasRequirement && OneTimeRequirement)
        {
            interactiveType = InteractiveType.DirectNoRequirement;
            HasRequirement = false;
        }
    }

    private void UniqueFirstInteractionDurationCheck()
    {
        if (FirstInteractionDurationUnique) InteractionDuration = secondInteractionDuration;
    }

    public virtual string GetInteractionText(bool requirementsMet)
    {
        if(CustomInteractionMessage.Length > 0) return CustomInteractionMessage;
        
        if (!requirementsMet) return "Requires " + GetRequirementNames();
        
        switch (InteractiveType)
        {
            case InteractiveType.Item:
            {
                return "Pick up " + objectName; 
            }
            default:
            {
                return "Use " + objectName;
            }
        }
    }

    protected string GetRequirementNames()
    {
        string names = string.Empty;
        
        if (RequiredItem != ItemType.None)
        {
            names += RequiredItem.ToString();
            if (RequiredEquipment != EquipmentType.None)
                names += " and " + RequiredEquipment.ToString();
        }
        else if (RequiredEquipment != EquipmentType.None)
        {
            names += RequiredEquipment.ToString();
        }

        return names;
    }
}
