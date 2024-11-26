using System;
using UnityEngine;

namespace Interaction.Equipments
{
    public class Taser : EquipmentObject, IFreeUseEquipment
    {
        [SerializeField] private Transform raycastOrigin;
        [SerializeField] private float raycastDistance;
        [SerializeField] private int maxAmmo;
        [SerializeField] private LayerMask raycastMask;
        
        private int _currentAmmo;

        protected override void Start()
        {
            base.Start();
            _currentAmmo = maxAmmo;
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
                print(hitenemy);
                
            
                if(hit.collider != null && hitenemy != null) hitenemy.status = EnemyMovement.Status.Tased; 
                print(hitenemy.status);
            }
            
            //print("Taser shot! Remaining ammo: " + _currentAmmo);
        }

        private void Reload()
        {
            CanBeUsed = true;
        }
    }
}