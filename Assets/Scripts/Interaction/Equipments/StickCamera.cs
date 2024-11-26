using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Interaction.Equipments
{
    public class StickCamera : EquipmentObject, IFreeUseEquipment
    {
        [SerializeField] private CinemachineCamera localCamera;

        private PlayerController player;
        private bool _inUse;

        private void Start()
        {
            player = FindFirstObjectByType<PlayerController>();
            CanBeUsed = true;
        }

        public override void Used(InteractiveObject activeInteractiveObject)
        {
            throw new System.NotImplementedException();
        }

        public void FreeUse()
        {
            _inUse = !_inUse;
            localCamera.enabled = _inUse;
            player.ToggleControls(!_inUse, true);
            //player.ExtendedCameraInUse(_inUse, transform);
        }
    }
}