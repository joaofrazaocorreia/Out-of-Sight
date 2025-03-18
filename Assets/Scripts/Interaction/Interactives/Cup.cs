using Unity.VisualScripting;
using UnityEngine;

public class Cup : InteractiveObject
{
    private NPCMoveInteraction NPCMoveInteraction;

    private void Start()
    {
        NPCMoveInteraction = GetComponent<NPCMoveInteraction>();
    }
    public override void Interact()
    {
        base.Interact();
        NPCMoveInteraction.Interact();
        enabled = false;
    }
}
