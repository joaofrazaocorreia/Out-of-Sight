
using System;
using Interaction;
using UnityEngine;

public class Jammer : EquipmentObject
{
    [SerializeField] private int maxAmount;
    
    private int currentAmount;

    public int CurrentAmount
    {
        get => currentAmount;
        set
        {
            currentAmount = value; 
            CanBeUsed = currentAmount != 0;
            print("Available Jammers: " + currentAmount);
        }
    }

    protected override void Start()
    {
        base.Start();
        
        currentAmount = maxAmount;
    }

    public override void Used(InteractiveObject activeInteractiveObject)
    {
        switch (((JammingSpot)activeInteractiveObject).Jammable.Jammed)
        {
            case true: CurrentAmount++; break;
            case false: CurrentAmount--; break;
        }
    }
}
