using UnityEngine;

public class DetectableObject : MonoBehaviour
{
    [SerializeField] private float detectionMultiplier;
    public float DetectionMultiplier {get => detectionMultiplier; set => detectionMultiplier = value;}
}
