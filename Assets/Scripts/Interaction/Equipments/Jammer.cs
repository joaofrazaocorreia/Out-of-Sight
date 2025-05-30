
using System;
using Interaction;
using Interaction.Equipments;
using UnityEngine;

public class Jammer : EquipmentObject, IHasAmmo
{
    [SerializeField] private int maxAmmo;
    [SerializeField] private PlayAudio jammerPickupPlayer;
    [SerializeField] private PlayAudio jammerPlacePlayer;
    [SerializeField] private GameObject model;

    public int MaxAmmo 
    { 
        get => maxAmmo;
        set => maxAmmo = value;
    }
    
    private int _currentAmount;

    public int CurrentAmmo
    {
        get => _currentAmount;
        set
        {
            _currentAmount = value; 
            CanBeUsed = _currentAmount > 0;
            IsSuspicious = _currentAmount > 0;
            _playerEquipment.EquipmentUsed();
            ToggleModel();
        }
    }

    protected override void Start()
    {
        base.Start();
        
        CurrentAmmo = maxAmmo;
    }

    public override void Used(InteractiveObject activeInteractiveObject)
    {
        CurrentAmmo--;
        
        base.Used(activeInteractiveObject);
        jammerPlacePlayer.Play();
    }
    
    public override void Equipped(bool isEquipped) => _equipmentModel.SetActive(isEquipped && _currentAmount > 0);

    public void Pickup()
    {
        CurrentAmmo++;
        jammerPickupPlayer.Play();
        
    }

    private void ToggleModel()
    {
        if(_playerEquipment.CurrentEquipment == this && _currentAmount > 0) model.SetActive(true);
        else model.SetActive(false);
    }
}
