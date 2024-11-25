using UnityEngine;

namespace Interaction.Equipments
{
    public class Taser : EquipmentObject, IFreeUseEquipment
    {
        [SerializeField] private GameObject raycastOrigin;
        [SerializeField] private float raycastDistance;
        [SerializeField] private int maxAmmo;
        
        private int _currentAmmo;
 
        private void Start()
        {
            CanBeUsed = true;
            _currentAmmo = maxAmmo;
        }

        public override void Used(InteractiveObject activeInteractiveObject)
        {
            throw new System.NotImplementedException();
        }

        public void FreeUse()
        {
            if(_currentAmmo <= 0) return;
            
            Fire();
            CanBeUsed = false;
            _currentAmmo--;
            Reload();
        }

        private void Fire()
        {
            Physics.Raycast(raycastOrigin.transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, raycastDistance);
            
            print("Taser shot! Remaining ammo: " + _currentAmmo);

            var hitenemy = hit.collider.GetComponent<EnemyMovement>();
            
           if(hit.collider != null && hitenemy != null) return; //tase enemy; 
        }

        private void Reload()
        {
            CanBeUsed = true;
        }
    }
}