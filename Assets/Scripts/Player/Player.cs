using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Status {Normal, Doubtful, Trespassing, Suspicious};
    public enum Disguise {Civillian, Employee, Guard_Tier1, Guard_Tier2};

    public List<Status> status;
    public Disguise disguise;
    public bool detectable;
    public event EventHandler OnDisguiseChanged;
    public event EventHandler OnStatusChanged;

    private void Start()
    {
        detectable = true;

        disguise = Disguise.Civillian;
        status.Add(Status.Normal);
        OnStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public void GainStatus(Status newStatus)
    {
        if (!status.Contains(newStatus))
        {
            status.Add(newStatus);
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void LoseStatus(Status newStatus)
    {
        while (status.Contains(newStatus))
        {
            status.Remove(newStatus);
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void GainDisguise(Disguise newDisguise)
    {
        if (disguise != newDisguise)
        {
            disguise = newDisguise;
            OnDisguiseChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
