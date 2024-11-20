using Interaction;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class InteractiveObject : MonoBehaviour
{
    [SerializeField] private InteractiveType interactiveType;
    [SerializeField] private Item requiredItem;
    [SerializeField] private EquipmentObject requiredEquipment;
    [SerializeField] private bool consumeRequirement;
    [SerializeField] private bool oneTimeRequirement;
    [SerializeField] private float interactionDuration;

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

    public bool HasRequirement => requiredItem != null || requiredEquipment != null;
    
    public bool ConsumeRequirement => consumeRequirement;
    
    public bool OneTimeRequirement => oneTimeRequirement;
    
    public abstract void Interact();

    protected void OneTimeRequirementCheck()
    {
        if (HasRequirement && oneTimeRequirement) interactiveType = InteractiveType.DirectNoRequirement;
    }
}
