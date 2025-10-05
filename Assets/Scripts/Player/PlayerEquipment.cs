using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction.Equipments;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    //[SerializeField] private GameObject[] equipments;
    [SerializeField] private List<GameObject> equipments;
    [SerializeField] private PlayAudio equipingPlayer;
    [SerializeField] private Animator animator;
    
    public Animator Animator => animator;

    private List<EquipmentObject> _equipmentObjects;
    public EquipmentObject _recentlyAddedEquipment {get; private set;}
    
    public List<EquipmentObject> EquipmentObjects => _equipmentObjects;
    
    private PlayerMelee _playerMelee;
    
    private int _currentEquipmentNum;
    private bool suspiciousCheck;
    
    private int _storedEquipmentNum = -1;

    public int CurrentEquipmentNum
    {
        get => _currentEquipmentNum;

        private set
        {
            if (CurrentEquipment != null)
            {
                CurrentEquipment.Equipped(false);
                animator.SetBool("Equipped", false);
                animator.SetTrigger("Unequip");
            }
            
            _currentEquipmentNum = value;
            CurrentEquipment = EquipmentObjects[_currentEquipmentNum];
            StartCoroutine(EquipDelay());
            
            if (CurrentEquipment != null && CurrentEquipment.EquipAnimationName != "None") animator.SetTrigger(CurrentEquipment.EquipAnimationName);
            
            OnEquipmentChanged?.Invoke(this, EventArgs.Empty);
            suspiciousCheck = CurrentEquipment.IsSuspicious;
        }
    }
    
    public EquipmentObject CurrentEquipment { get; private set; }

    
    public event EventHandler OnEquipmentChanged;
    public event EventHandler OnEquipmentAdded;

    public void NewEquipmentSelected(int index)
    {
        if (index < 0 || index >= equipments.Count || !inventoryUI.activeSelf) return;
        if (index == CurrentEquipmentNum)
        {
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
        if(CurrentEquipment != null) CurrentEquipment.Equipped(false);
        _currentEquipmentNum = -1;
        CurrentEquipment = null;

        GetComponent<Player>().LoseStatus(Player.Status.Suspicious);
        OnEquipmentChanged?.Invoke(this, EventArgs.Empty);
        animator.SetBool("Equipped", false);
        animator.SetTrigger("Unequip");
    }

    public void TryUseEquipment()
    {
        if (CurrentEquipment is IFreeUseEquipment equipment && CurrentEquipment.CanBeUsed)
        {
            equipment.FreeUse();
            EquipmentUsed();
        } 
    }
    private void Start()
    {
        RegisterEquipmentObjects();

        _playerMelee = GetComponent<PlayerMelee>();
        _playerMelee.OnKnockout += OnMeleeAttack;
        _playerMelee.OnAttackEnd += OnAttackEnd;
        
        Unequip();
    }

    public void AddEquipment(GameObject equipment)
    {
        if(!equipments.Contains(equipment))
            equipments.Add(equipment);
            
        RegisterEquipmentObjects();
    }

    private void RegisterEquipmentObjects()
    {
        _equipmentObjects = new List<EquipmentObject>();
        for (int i = 0; i < equipments.Count; i++)
        {
            _equipmentObjects.Add(equipments[i].GetComponent<EquipmentObject>());
            _recentlyAddedEquipment = _equipmentObjects[i];
            OnEquipmentAdded?.Invoke(this, EventArgs.Empty);
        }
    }

    public void EquipmentUsed()
    {
        if (CurrentEquipment.UseAnimation != "None") animator.SetTrigger(CurrentEquipment.UseAnimation);
        OnEquipmentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void EquipmentChanged()
    {
       if (CurrentEquipment != null && suspiciousCheck != CurrentEquipment.IsSuspicious) 
       {
            suspiciousCheck = CurrentEquipment.IsSuspicious;
            if (suspiciousCheck) GetComponent<Player>().GainStatus(Player.Status.Suspicious);
            else GetComponent<Player>().LoseStatus(Player.Status.Suspicious);
       }
       OnEquipmentChanged?.Invoke(this, EventArgs.Empty); 
    }

    private IEnumerator EquipDelay()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Equipped", true);
        CurrentEquipment.Equipped(true);
    }

    private void OnMeleeAttack(object sender, EventArgs e)
    {
        if(CurrentEquipment != null) StoreEquipmentWhileAttacking();
        Unequip();  
    }

    private void StoreEquipmentWhileAttacking()
    {
        _storedEquipmentNum = CurrentEquipmentNum;
    }

    private void OnAttackEnd(object sender, EventArgs e)
    {
        if(_storedEquipmentNum != -1) NewEquipmentSelected(_storedEquipmentNum);
    }
}
