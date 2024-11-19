
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
            CanBeUsed = currentAmount == 0;
        }
    }
    private void Start()
    {
        currentAmount = maxAmount;
    }

    public override void Used(InteractiveObject activeInteractiveObject)
    {
        switch (activeInteractiveObject.GetComponent<IJammable>().Jammed)
        {
            case true: CurrentAmount++; break;
            case false: CurrentAmount--; break;
        }
    }
}
