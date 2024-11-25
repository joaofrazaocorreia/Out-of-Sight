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
            
            _currentAmmo--;
            Fire();
            CanBeUsed = false;
            Reload();
        }

        private void Fire()
        {
            if (Physics.Raycast(raycastOrigin.transform.position, raycastOrigin.transform.TransformDirection(Vector3.forward), out RaycastHit hit, raycastDistance))
            {
                var hitenemy = hit.collider.GetComponent<EnemyMovement>();
            
                if(hit.collider != null && hitenemy != null) hitenemy.status = EnemyMovement.Status.Tased; 
                print(hitenemy);
            }
            
            print("Taser shot! Remaining ammo: " + _currentAmmo);
        }

        private void Reload()
        {
            CanBeUsed = true;
        }
    }
}