
using System;
using Interaction;
using Interaction.Equipments;
using UnityEngine;

public class Jammer : EquipmentObject, IHasAmmo
{
    [SerializeField] private int maxAmount;
    
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
        
        CurrentAmmo = maxAmount;
    }

    public override void Used(InteractiveObject activeInteractiveObject)
    {
        switch (((JammingSpot)activeInteractiveObject).Jammable.Jammed)
        {
            case true: CurrentAmmo++; break;
            case false: CurrentAmmo--; break;
        }
    }
}
