using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Status {Normal, Doubtful, Trespassing, CriticalTrespassing, Suspicious};
    public enum Disguise {Civillian, Employee, Guard_Tier1, Guard_Tier2};

    public List<Status> status;
    public Disguise disguise;
    public bool detectable;
    private List<MapArea> currentAreas;
    public event EventHandler OnDisguiseChanged;
    public event EventHandler OnStatusChanged;

    private void Start()
    {
        detectable = true;
        currentAreas = new List<MapArea>();

        GainStatus(Status.Normal);

        // Calls the disguise change event to update the UI
        OnDisguiseChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        CheckAreaStatus();
    }

    private void OnTriggerEnter(Collider other)
    {
        MapArea area = other.GetComponent<MapArea>();
        if(other != null)
        {
            currentAreas.Add(area);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MapArea area = other.GetComponent<MapArea>();
        if(other != null)
        {
            currentAreas.Remove(area);
        }
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

    private void CheckAreaStatus()
    {
        LoseStatus(Status.Trespassing);
        LoseStatus(Status.CriticalTrespassing);
        
        foreach(MapArea a in currentAreas)
        {
            if(a != null && a.UseWhitelist && !a.WhitelistedDisguises.Contains(disguise))
                GainStatus(a.IsCriticalArea ? Status.CriticalTrespassing : Status.Trespassing);
        }
    }
}
