using Interaction;
using UnityEngine;

public class CodeDoorAttachment : InteractiveObject
{
    [SerializeField] private CodeDoor associatedDoor;
    public override void Interact()
    {
        if(associatedDoor != null) associatedDoor.Interact();
    }
}
