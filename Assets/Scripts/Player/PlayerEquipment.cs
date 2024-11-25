using System;
using Interaction.Equipments;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private GameObject[] equipments;

    private EquipmentObject[] _equipmentObjects;
    
    public EquipmentObject[] EquipmentObjects => _equipmentObjects;
    
    private int _currentEquipmentNum;

    public int CurrentEquipmentNum
    {
        get => _currentEquipmentNum;

        private set
        {
            _currentEquipmentNum = value;
            CurrentEquipment = EquipmentObjects[_currentEquipmentNum];
            EquipmentChanged();
        }
    }
    
    public EquipmentObject CurrentEquipment { get; private set; }

    public void NewEquipmentSelected(int index)
    {
        if (index < 0 || index >= equipments.Length) return;
        if (index == CurrentEquipmentNum) Unequip(); 
        else CurrentEquipmentNum = index;
    }

    private void Unequip()
    {
        print("Unequipped");
        _currentEquipmentNum = -1;
        CurrentEquipment = null;
    }

    public void TryUseEquipment()
    {
        if(CurrentEquipment is IFreeUseEquipment equipment && CurrentEquipment.CanBeUsed) equipment.FreeUse(); 
    }

    private void EquipmentChanged()
    {
        print("Equipment changed to:" + CurrentEquipment.name);
        // Call animator for equipAnimations;
    }

    private void Start()
    {
        _equipmentObjects = new EquipmentObject[equipments.Length];
        for (int i = 0; i < equipments.Length; i++)
        {
            _equipmentObjects[i] = equipments[i].GetComponent<EquipmentObject>();
        }
        
        Unequip();
    }
}
