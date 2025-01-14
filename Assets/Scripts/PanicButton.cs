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
        Debug.Log(other.name);
        Enemy nearbyEnemy = other.GetComponentInParent<Enemy>();

        if(nearbyEnemy != null && nearbyEnemy.IsAlarmed)
        {
            Debug.Log("A panic button was triggered!");
            alarm.TriggerAlarm(!alarm.IsOn);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
}
