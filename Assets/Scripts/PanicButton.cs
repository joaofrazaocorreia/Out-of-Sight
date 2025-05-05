using UnityEngine;

public class PanicButton : MonoBehaviour
{
    private Alarm alarm;

    private void Start()
    {
        alarm = FindAnyObjectByType<Alarm>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy nearbyEnemy = other.GetComponentInParent<Enemy>();

        if(nearbyEnemy != null && nearbyEnemy.IsAlarmed && !alarm.IsOn)
        {
            Debug.Log("A panic button was triggered!");
            alarm.TriggerAlarm(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
}
