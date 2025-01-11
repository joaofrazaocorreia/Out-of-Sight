
using System;
using Interaction;
using Interaction.Equipments;
using UnityEngine;

public class Jammer : EquipmentObject, IHasAmmo
{
    [SerializeField] private int maxAmmo;
    [SerializeField] private PlayAudio jammerPickupPlayer;
    [SerializeField] private PlayAudio jammerPlacePlayer;

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
            CanBeUsed = _currentAmount != 0;
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

    public void Pickup()
    {
        CurrentAmmo++;
        jammerPickupPlayer.Play();
    }
}
