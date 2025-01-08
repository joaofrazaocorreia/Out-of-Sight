using System.Collections.Generic;
using Interaction;
using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    [Header("Interactive Type")]
    [SerializeField] private InteractiveType interactiveType;
    [SerializeField] protected bool updateIfIndirect; 
    [Header("Requirements")]
    [SerializeField] protected Item requiredItem;
    [SerializeField] protected EquipmentObject requiredEquipment;
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

    public InteractiveType InteractiveType
    {
        get => interactiveType;
        protected set => interactiveType = value;
    }
    
    public Item RequiredItem => requiredItem;
    public EquipmentObject RequiredEquipment => requiredEquipment;
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
    
    
    private void Start()
    {
        HasRequirement = RequiredItem != null || RequiredEquipment != null;
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
        
        if (RequiredItem != null)
        {
            names += RequiredItem.name;
            if (RequiredEquipment != null)
                names += " and " + RequiredEquipment.name;
        }
        else if (RequiredEquipment != null)
        {
            names += RequiredEquipment.name;
        }

        return names;
    }
}
