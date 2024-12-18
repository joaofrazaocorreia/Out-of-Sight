using Interaction;
using Unity.VisualScripting;
using UnityEngine;

public class JammingSpot : InteractiveObject
{
    [SerializeField] private GameObject jammable;
    [SerializeField] private GameObject jammerModel;
    private Jammer jammer;
    public IJammable Jammable => jammable.GetComponent<IJammable>();

    private void Start()
    {
        jammer = FindFirstObjectByType<Jammer>();
    }
    
    public override void Interact()
    {
        if (jammable != null)
        {
            Jammable.ToggleJammed();
            ToggleJammerModel();
            if (!Jammable.Jammed) jammer.CurrentAmmo++;
            UpdateRequirements(Jammable.Jammed);
        }
    }
    
    private void ToggleJammerModel() => jammerModel.SetActive(!jammerModel.activeSelf);

    private void UpdateRequirements(bool isJammed)
    {
        InteractiveType = isJammed ? InteractiveType.DirectNoRequirement : InteractiveType.DirectEquipmentRequirement ;
    }
}
