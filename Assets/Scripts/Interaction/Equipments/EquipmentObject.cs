using UnityEngine;

public abstract class EquipmentObject : MonoBehaviour
{
    public bool CanBeUsed { get; protected set; }

    public abstract void Used(InteractiveObject activeInteractiveObject);
}

