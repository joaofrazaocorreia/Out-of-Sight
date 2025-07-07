using System;
using UnityEngine;

namespace Interaction.Equipments
{
    public class Taser : EquipmentObject, IFreeUseEquipment, IHasAmmo
    {
        [SerializeField] private Transform raycastOrigin;
        [SerializeField] private float raycastDistance;
        [SerializeField] private int maxAmmo;
        [SerializeField] private bool knocksOutTargets;
        [SerializeField] private LayerMask raycastMask;
        [SerializeField] private PlayAudio taserShotPlayer;

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

        public event EventHandler OnTaserShot;

        protected override void Start()
        {
            base.Start();
            CurrentAmmo = maxAmmo;
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
            if (Time.timeScale != 0)
            {
                if (Physics.Raycast(raycastOrigin.position, raycastOrigin.forward,
                    out RaycastHit hit, raycastDistance, raycastMask))
                {
                    var hitenemy = hit.collider.GetComponentInParent<Enemy>();

                    if (hit.collider != null && hitenemy != null && hitenemy.EnemyStatus
                        != Enemy.Status.KnockedOut)
                    {
                        hitenemy.GetKnockedOut();
                    }
                }
                OnTaserShot?.Invoke(this, EventArgs.Empty);
                taserShotPlayer.Play();
                base.Used(null);
            }
        }

        private void Reload()
        {
            CanBeUsed = true;
        }
    }
}