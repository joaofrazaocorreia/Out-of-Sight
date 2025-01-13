using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Interaction.Equipments
{
    public class StickCamera : EquipmentObject
    {
        private PlayerController player;
        private bool _inUse;

        protected override void Start()
        {
            base.Start();
            player = FindFirstObjectByType<PlayerController>();
            CanBeUsed = true;
        }

    }
}