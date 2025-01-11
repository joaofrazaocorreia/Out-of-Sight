using System;
using Interaction.Equipments;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private GameObject[] equipments;
    [SerializeField] private PlayAudio equipingPlayer;

    private UIManager uIManager;
    private EquipmentObject[] _equipmentObjects;
    
    public EquipmentObject[] EquipmentObjects => _equipmentObjects;
    
    private int _currentEquipmentNum;

    public int CurrentEquipmentNum
    {
        get => _currentEquipmentNum;

        private set
        {
            CurrentEquipment?.Equipped(false);
            _currentEquipmentNum = value;
            CurrentEquipment = EquipmentObjects[_currentEquipmentNum];
            CurrentEquipment.Equipped(true);
            EquipmentChanged();
            uIManager.ToggleAmmoDisplay(CurrentEquipment is IHasAmmo);
            if(CurrentEquipment is IHasAmmo ammo) uIManager.UpdateAmmoText(ammo.CurrentAmmo + " / " + ammo.MaxAmmo);
        }
    }
    
    public EquipmentObject CurrentEquipment { get; private set; }

    public void NewEquipmentSelected(int index)
    {
        if (index < 0 || index >= equipments.Length) return;
        if (index == CurrentEquipmentNum)
        {
            CurrentEquipment.Equipped(false);
            Unequip();
        }
        else
        {
            CurrentEquipmentNum = index;
            GetComponent<Player>().GainStatus(Player.Status.Suspicious);
        }
        equipingPlayer.Play();
    }

    private void Unequip()
    {
        uIManager.ToggleAmmoDisplay(false);
        _currentEquipmentNum = -1;
        CurrentEquipment = null;

        GetComponent<Player>().LoseStatus(Player.Status.Suspicious);
    }

    public void TryUseEquipment()
    {
        if(CurrentEquipment is IFreeUseEquipment equipment && CurrentEquipment.CanBeUsed) equipment.FreeUse(); 
    }

    private void EquipmentChanged()
    {
        // Call animator for equipAnimations;
    }

    private void Start()
    {
        uIManager = FindFirstObjectByType<UIManager>();
        _equipmentObjects = new EquipmentObject[equipments.Length];
        for (int i = 0; i < equipments.Length; i++)
        {
            _equipmentObjects[i] = equipments[i].GetComponent<EquipmentObject>();
            uIManager.UpdateEquipmentIcon(_equipmentObjects[i].Icon, i);
        }
        
        Unequip();
    }

    public void EquipmentUsed()
    {
        if(CurrentEquipment is IHasAmmo ammo) uIManager.UpdateAmmoText(ammo.CurrentAmmo + " / " + ammo.MaxAmmo);
    }
}
