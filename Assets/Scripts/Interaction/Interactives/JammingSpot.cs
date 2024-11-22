using Interaction;
using UnityEngine;

public class JammingSpot : InteractiveObject
{
    [SerializeField] private GameObject jammable;
    public IJammable Jammable => jammable.GetComponent<IJammable>();
    public override void Interact()
    {
        if(jammable != null) Jammable.ToggleJammed();
    }
}
