using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Status {Normal, Trespassing, Suspicious};

    public List<Status> status;

    private void Start()
    {
        status.Add(Status.Normal);
        status.Add(Status.Suspicious);
        status.Add(Status.Trespassing);
    }
}
