using System;
using UnityEngine;

namespace Interaction.Equipments
{
    public class Taser : EquipmentObject, IFreeUseEquipment, IHasAmmo
    {
        [SerializeField] private Transform raycastOrigin;
        [SerializeField] private float raycastDistance;
        [SerializeField] private int maxAmmo;
        [SerializeField] private LayerMask raycastMask;

        public int MaxAmmo 
        { 
            get => maxAmmo;
            set => maxAmmo = value;
        }
        
        public int CurrentAmmo
        {
            get => _currentAmmo;
            set
            {
                _currentAmmo = value; 
                CanBeUsed = _currentAmmo != 0;
            }
        }
        
        private int _currentAmmo;

        protected override void Start()
        {
            base.Start();
            CurrentAmmo = maxAmmo;
        }

        public override void Used(InteractiveObject activeInteractiveObject)
        {
            return;
        }

        public void FreeUse()
        {
            if(_currentAmmo <= 0) return;
            
            _currentAmmo--;
            Fire();
            CanBeUsed = false;
            Reload();
        }

        private void Fire()
        {
            if (Physics.Raycast(raycastOrigin.position,  raycastOrigin.forward, out RaycastHit hit, raycastDistance, raycastMask))
            {
                var hitenemy = hit.collider.GetComponentInParent<EnemyMovement>();
            
                if(hit.collider != null && hitenemy != null) hitenemy.status = EnemyMovement.Status.Tased; 
            }
        }

        private void Reload()
        {
            CanBeUsed = true;
        }
    }
}