using Interaction;
using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    [Header("Interactive Type")]
    [SerializeField] private InteractiveType interactiveType;
    [Header("Requirements")]
    [SerializeField] private Item requiredItem;
    [SerializeField] private EquipmentObject requiredEquipment;
    [Header("Requirement Options")]
    [SerializeField] private bool consumeItemRequirement;
    [SerializeField] private bool oneTimeRequirement;
    [Header("Interaction Options")]
    [SerializeField] private bool firstInteractionDurationUnique;
    [SerializeField] private float interactionDuration;
    [SerializeField] private float secondInteractionDuration;
    [Header("Detection Options")]
    [SerializeField] private bool isInteractionSuspicious;
    [SerializeField] private bool isSecondInteractionSuspicious;
    [Header("UI Options")]
    [SerializeField] protected string objectName;
    [SerializeField] private bool hasCustomInteractionMessage;
    [SerializeField] private string customInteractionMessage;

    public InteractiveType InteractiveType
    {
        get => interactiveType;
        protected set => interactiveType = value;
    }

    public float InteractionDuration
    {
        get => interactionDuration;
        private set => interactionDuration = value;
    }
    
    public Item RequirementObject => requiredItem;
    
    public EquipmentObject RequirementEquipment => requiredEquipment;

    public bool HasRequirement { get; private set; }
    
    public bool ConsumeItemRequirement => consumeItemRequirement;
    
    public bool OneTimeRequirement => oneTimeRequirement;
    
    public bool IsInteractionSuspicious => isInteractionSuspicious;
    
    private void Start()
    {
        HasRequirement = requiredItem != null || requiredEquipment != null;
    }

    public virtual void Interact()
    {
        OneTimeRequirementCheck();
        UniqueFirstInteractionDurationCheck();
        isInteractionSuspicious = isSecondInteractionSuspicious;
    }

    private void OneTimeRequirementCheck()
    {
        if (HasRequirement && oneTimeRequirement)
        {
            interactiveType = InteractiveType.DirectNoRequirement;
            HasRequirement = false;
        }
    }

    private void UniqueFirstInteractionDurationCheck()
    {
        if (firstInteractionDurationUnique) InteractionDuration = secondInteractionDuration;
    }

    public virtual string GetInteractionText(bool requirementsMet)
    {
        if (!requirementsMet) return "Requires " + GetRequirementNames();
        
        switch (interactiveType)
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
        
        if (requiredItem != null)
        {
            names += requiredItem.name;
            if (requiredEquipment != null)
                names += " and " + requiredEquipment.name;
        }
        else if (requiredEquipment != null)
        {
            names += requiredEquipment.name;
        }

        return names;
    }
}
