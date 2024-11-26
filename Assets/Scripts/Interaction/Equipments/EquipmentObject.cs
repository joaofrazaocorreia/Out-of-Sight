using System;
using UnityEngine;

public abstract class EquipmentObject : MonoBehaviour
{
    private GameObject _equipmentModel;
    public bool CanBeUsed { get; protected set; }

    public abstract void Used(InteractiveObject activeInteractiveObject);
    
    public void Equipped(bool isEquipped) => _equipmentModel.SetActive(isEquipped);

    protected virtual void Start()
    {
        _equipmentModel = transform.GetChild(0).gameObject;
        Equipped(false);
        CanBeUsed = true;
    }
}

