using System;
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
        if(index < 0 || index >= equipments.Length || index == CurrentEquipmentNum) return;

        CurrentEquipmentNum = index;
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
        
        if(_equipmentObjects.Length > 0) CurrentEquipment = _equipmentObjects[0];
    }
}
