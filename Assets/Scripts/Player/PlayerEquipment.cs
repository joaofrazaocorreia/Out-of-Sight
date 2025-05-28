using System;
using Interaction.Equipments;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private GameObject[] equipments;
    [SerializeField] private PlayAudio equipingPlayer;
    [SerializeField] private Animator animator;

    private EquipmentObject[] _equipmentObjects;
    public EquipmentObject _recentlyAddedEquipment {get; private set;}
    
    public EquipmentObject[] EquipmentObjects => _equipmentObjects;
    
    private int _currentEquipmentNum;
    private bool suspiciousCheck;

    public int CurrentEquipmentNum
    {
        get => _currentEquipmentNum;

        private set
        {
            CurrentEquipment?.Equipped(false);
            _currentEquipmentNum = value;
            CurrentEquipment = EquipmentObjects[_currentEquipmentNum];
            CurrentEquipment.Equipped(true);
            animator.SetBool("Equipped", true);
            print("Current Equipment: " + _currentEquipmentNum);
            if (CurrentEquipment != null)
            {
                if(CurrentEquipment.PlayerAnimationName != "None") animator.SetTrigger(CurrentEquipment.PlayerAnimationName);
            }
            OnEquipmentChanged?.Invoke(this, EventArgs.Empty);
            suspiciousCheck = CurrentEquipment.IsSuspicious;
        }
    }
    
    public EquipmentObject CurrentEquipment { get; private set; }

    
    public event EventHandler OnEquipmentChanged;
    public event EventHandler OnEquipmentAdded;

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
            if(CurrentEquipment == null && EquipmentObjects[index].IsSuspicious) 
                GetComponent<Player>().GainStatus(Player.Status.Suspicious);
            CurrentEquipmentNum = index;
        }

        equipingPlayer.Play();
    }

    private void Unequip()
    {
        _currentEquipmentNum = -1;
        CurrentEquipment = null;

        GetComponent<Player>().LoseStatus(Player.Status.Suspicious);
        OnEquipmentChanged?.Invoke(this, EventArgs.Empty);
        animator.SetBool("Equipped", false);
        animator.SetTrigger("Unequip");
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
        _equipmentObjects = new EquipmentObject[equipments.Length];
        for (int i = 0; i < equipments.Length; i++)
        {
            _equipmentObjects[i] = equipments[i].GetComponent<EquipmentObject>();
            _recentlyAddedEquipment = _equipmentObjects[i];
            OnEquipmentAdded?.Invoke(this, EventArgs.Empty);
        }
        
        Unequip();
    }

    public void EquipmentUsed()
    {
        if (CurrentEquipment != null && suspiciousCheck != CurrentEquipment.IsSuspicious)
        {
            suspiciousCheck = CurrentEquipment.IsSuspicious;
            if (suspiciousCheck) GetComponent<Player>().GainStatus(Player.Status.Suspicious);
            else GetComponent<Player>().LoseStatus(Player.Status.Suspicious);
        }
        OnEquipmentChanged?.Invoke(this, EventArgs.Empty);
    }
}
