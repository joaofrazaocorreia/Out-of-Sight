using Interaction;
using Interaction.Equipments;
using UnityEngine;

public class InteractionSpot : InteractiveObject
{
    [Header("Interaction Spot Variables")]
    [SerializeField] private InteractiveObject LinkedInteractiveObject;

    public InteractiveObject LinkedObject
    {
        get => linkedObject;
        set => linkedObject = value;
    }

    private InteractiveObject linkedObject;

    private void Start()
    {
        LinkedObject = LinkedInteractiveObject; 
        if(linkedObject != null) CloneLinkedObjectParameters();
    }

    public override void Interact()
    {
        base.Interact();
        
        LinkedObject.Interact();
    }

    private void CloneLinkedObjectParameters()
    {
        if (LinkedObject.RequiredItem != ItemType.None)
        {
            requiredItem = LinkedObject.RequiredItem;
            InteractiveType = InteractiveType.DirectItemRequirement;
        }

        if (LinkedObject.RequiredEquipment != EquipmentType.None)
        {
            requiredEquipment = LinkedObject.RequiredEquipment;
            InteractiveType = InteractiveType.DirectEquipmentRequirement;
        }
        consumeItemRequirement = LinkedObject.ConsumeItemRequirement;
        oneTimeRequirement = LinkedObject.OneTimeRequirement;
        firstInteractionDurationUnique = LinkedObject.FirstInteractionDurationUnique;
        interactionDuration = LinkedObject.InteractionDuration;
        secondInteractionDuration = LinkedObject.SecondInteractionDuration;
        isInteractionSuspicious = LinkedObject.IsInteractionSuspicious;
        isSecondInteractionSuspicious = LinkedObject.IsSecondInteractionSuspicious;
        objectName = LinkedObject.ObjectName;
        customInteractionMessage = LinkedObject.CustomInteractionMessage;
        onInteractAudioPlayer = linkedObject.OnInteractAudioPlayer;
        whileInteractAudioPlayer = linkedObject.WhileInteractAudioPlayer;
        
        HasRequirement = RequiredItem != ItemType.None || RequiredEquipment != EquipmentType.None;
    }
}
