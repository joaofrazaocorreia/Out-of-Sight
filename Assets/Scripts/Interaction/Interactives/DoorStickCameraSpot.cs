using Interaction;
using Interaction.Equipments;
using Unity.Cinemachine;
using UnityEngine;

public class DoorStickCameraSpot : InteractiveObject
{
    [SerializeField] private CinemachineCamera LinkedCamera;
    private bool _onUse;
    private EquipmentType _requirement;

    private void Start()
    {
        _requirement = requiredEquipment;
    }
    public override void Interact()
    {
        base.Interact();
        _onUse = !_onUse;
        LinkedCamera.Priority.Value = _onUse ? 1 : -1;
        requiredEquipment = _onUse ? EquipmentType.None : _requirement;
        customInteractionMessage = _onUse ? "Stop looking under door" : "Look under door";
        InteractiveType = requiredEquipment != EquipmentType.None ? InteractiveType.DirectEquipmentRequirement: InteractiveType.DirectNoRequirement;
        HasRequirement = RequiredItem != ItemType.None || RequiredEquipment != EquipmentType.None;
    }
}
