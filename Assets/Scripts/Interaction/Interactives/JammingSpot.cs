using Interaction;
using UnityEngine;

public class JammingSpot : InteractiveObject
{
    [SerializeField] private GameObject jammable;
    [SerializeField] private GameObject jammerModel;
    [SerializeField] private PlayAudio audioPlayer;
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
            if (!Jammable.Jammed) jammer.Pickup();
            UpdateRequirements(Jammable.Jammed);

            onInteractionComplete?.Invoke();
        }
    }
    
    private void ToggleJammerModel()
    {
        jammerModel.SetActive(!jammerModel.activeSelf);
        ToggleAudioPlayer(jammerModel.activeSelf);
    }

    private void ToggleAudioPlayer(bool value)
    {
        if (value) audioPlayer.Play();
        else audioPlayer.Stop();
    }

    private void UpdateRequirements(bool isJammed)
    {
        InteractiveType = isJammed ? InteractiveType.DirectNoRequirement : InteractiveType.DirectEquipmentRequirement ;
    }
}
