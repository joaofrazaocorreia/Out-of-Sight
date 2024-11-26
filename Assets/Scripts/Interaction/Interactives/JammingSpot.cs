using Interaction;
using UnityEngine;

public class JammingSpot : InteractiveObject
{
    [SerializeField] private GameObject jammable;
    [SerializeField] private GameObject jammerModel;
    public IJammable Jammable => jammable.GetComponent<IJammable>();
    public override void Interact()
    {
        if (jammable != null)
        {
            Jammable.ToggleJammed();
            ToggleJammerModel();
        }
    }
    
    private void ToggleJammerModel() => jammerModel.SetActive(!jammerModel.activeSelf);
}
