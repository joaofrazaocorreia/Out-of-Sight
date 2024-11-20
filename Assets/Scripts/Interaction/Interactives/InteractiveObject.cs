using System;
using Interaction;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class InteractiveObject : MonoBehaviour
{
    [SerializeField] private InteractiveType interactiveType;
    [SerializeField] private Item requiredItem;
    [SerializeField] private EquipmentObject requiredEquipment;
    [SerializeField] private bool consumeItemRequirement;
    [SerializeField] private bool oneTimeRequirement;
    [SerializeField] private bool firstInteractionDurationUnique;
    [SerializeField] private float interactionDuration;
    [SerializeField] private float interactionSecondDuration;

    public InteractiveType InteractiveType
    {
        get => interactiveType;
        private set => interactiveType = value;
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

    private void Start()
    {
        HasRequirement = requiredItem != null || requiredEquipment != null;
    }

    public virtual void Interact()
    {
        OneTimeRequirementCheck();
        UniqueFirstInteractionDurationCheck();
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
        if (firstInteractionDurationUnique) InteractionDuration = interactionSecondDuration;
    }
}
