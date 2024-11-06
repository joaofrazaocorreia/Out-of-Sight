using UnityEngine;

public class Detection : MonoBehaviour
{
    [SerializeField] [Range(10f, 180f)] private float detectionMaxAngle = 75.0f;
    [SerializeField] private float detectionRange;
    [SerializeField] private float proximityDetectionRange;
    [SerializeField] [Range(0f, 20f)] private float baseDetectionRate = 1.0f;

    private GameObject player;
    //private Alarm alarm;
    private float detectionMeter;


    void Start()
    {
        player = FindAnyObjectByType<PlayerController>().gameObject;
    }

    void FixedUpdate()
    {
        Vector3 distanceToPlayer = player.transform.position - transform.position;
        
        if(distanceToPlayer.magnitude <= proximityDetectionRange)
        {
            // look at player
        }

        if(distanceToPlayer.magnitude <= detectionRange)
        {
            // Checks if player is in front of the NPC
            if(Vector3.Angle(transform.TransformDirection(Vector3.forward), distanceToPlayer) <= detectionMaxAngle)
            {
                // Sends a raycast towards the player and checks if it's uninterrupted by obstacles
                RaycastHit hit;

                if (Physics.Raycast(transform.position, distanceToPlayer, out hit, detectionRange))
                {
                    if(hit.transform.tag == "Player")
                    {
                        Debug.DrawRay(transform.position, distanceToPlayer * hit.distance, Color.red);

                        // Checks if detection should be raised


                            // Calculates how much detection is raised

                            
                    }
                    
                    else
                        Debug.DrawRay(transform.position, distanceToPlayer * hit.distance, Color.yellow);
                }
            }
        }
    }
}
