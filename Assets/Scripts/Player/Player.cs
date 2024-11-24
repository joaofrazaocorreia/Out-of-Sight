using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Status {Normal, Trespassing, Suspicious};

    public List<Status> status;

    private void Start()
    {
        status.Add(Status.Normal);
        //GainStatus(Status.Suspicious);
        GainStatus(Status.Trespassing);
    }

    public void GainStatus(Status newStatus)
    {
        if(!status.Contains(newStatus))
            status.Add(newStatus);
    }

    public void LoseStatus(Status newStatus)
    {
        while(status.Contains(newStatus))
            status.Remove(newStatus);
    }
}
