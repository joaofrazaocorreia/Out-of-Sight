using System;
using Interaction.Equipments;
using UnityEngine;

public abstract class EquipmentObject : MonoBehaviour
{
    [SerializeField] private EquipmentType equipmentType;
    [SerializeField] private Sprite icon;
    [SerializeField] private string playerAnimationName = "None";
    public string PlayerAnimationName => playerAnimationName;
    protected GameObject _equipmentModel;
    protected PlayerEquipment _playerEquipment;
    public bool CanBeUsed { get; protected set; }
    public bool IsSuspicious { get; protected set; } = true;
    public Sprite Icon => icon;
    public EquipmentType EquipmentType => equipmentType;

    public virtual void Used(InteractiveObject activeInteractiveObject)
    {
        _playerEquipment.EquipmentUsed();
    }
    
    public virtual void Equipped(bool isEquipped) => _equipmentModel.SetActive(isEquipped);

    protected virtual void Start()
    {
        _playerEquipment = GetComponentInParent<PlayerEquipment>();
        _equipmentModel = transform.GetChild(0).gameObject;
        Equipped(false);
        CanBeUsed = true;
    }
}

