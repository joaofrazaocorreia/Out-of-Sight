using System;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class Player : MonoBehaviour
{
    public enum Status {Normal, Doubtful, Trespassing, CriticalTrespassing, Suspicious};

    public List<Status> status;
    public Disguise disguise;
    public bool detectable;
    [SerializeField] private float normalDetectionMultiplier = 0f;
    [SerializeField] private float doubtfulDetectionMultiplier = 0f;
    [SerializeField] private float trespassingDetectionMultiplier = 1f;
    [SerializeField] private float suspiciousDetectionMultiplier = 2.5f;
    private DetectableObject detectableObject;
    private List<MapArea> currentAreas;
    private CharacterController characterController;
    public bool IsMoving { get => characterController.velocity.magnitude >= 1f; }
    public event EventHandler OnDisguiseChanged;
    public event EventHandler OnStatusChanged;

    private void Start()
    {
        detectable = true;
        detectableObject = GetComponentInChildren<DetectableObject>();
        characterController = GetComponentInChildren<CharacterController>();
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
        status.Add(newStatus);
        UpdateDetectableMultiplier();
        OnStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoseStatus(Status newStatus)
    {
        if (status.Contains(newStatus))
        {
            status.Remove(newStatus);
            UpdateDetectableMultiplier();
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void GainStatusIfNew(Status newStatus)
    {
        if (!status.Contains(newStatus))
        {
            status.Add(newStatus);
            UpdateDetectableMultiplier();
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void LoseStatusCompletely(Status newStatus)
    {
        if (status.Contains(newStatus))
        {
            while (status.Contains(newStatus))
            {
                status.Remove(newStatus);
            }

            UpdateDetectableMultiplier();
            OnStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateDetectableMultiplier()
    {
        if(status.Contains(Status.Suspicious))
            detectableObject.DetectionMultiplier = suspiciousDetectionMultiplier;
        
        else if(status.Contains(Status.Trespassing))
            detectableObject.DetectionMultiplier = trespassingDetectionMultiplier;
        
        else if(status.Contains(Status.Doubtful))
            detectableObject.DetectionMultiplier = doubtfulDetectionMultiplier;
        
        else
            detectableObject.DetectionMultiplier = normalDetectionMultiplier;
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
        LoseStatusCompletely(Status.Trespassing);
        LoseStatusCompletely(Status.CriticalTrespassing);
        
        foreach(MapArea a in currentAreas)
        {
            if(a != null && a.UseWhitelist && !a.WhitelistedDisguises.Contains(disguise))
                GainStatusIfNew(a.IsCriticalArea ? Status.CriticalTrespassing : Status.Trespassing);
        }
    }
}
