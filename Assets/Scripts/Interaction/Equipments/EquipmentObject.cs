using System;
using Interaction.Equipments;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class EquipmentObject : MonoBehaviour
{
    [SerializeField] private EquipmentType equipmentType;
    [SerializeField] private Sprite icon;
    [FormerlySerializedAs("playerAnimationName")] [SerializeField] private string equipAnimationName = "None";
    [FormerlySerializedAs("animationTrigger")] [SerializeField] private string useAnimation = "None";
    [FormerlySerializedAs("equipmentName")] [SerializeField] private string continuousUseAnimation = "None";
    public string EquipAnimationName => equipAnimationName;
    public string UseAnimation => useAnimation;
    public string ContinuousUseAnimation => continuousUseAnimation;
    protected GameObject _equipmentModel;
    public GameObject EquipmentModel => _equipmentModel;
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

