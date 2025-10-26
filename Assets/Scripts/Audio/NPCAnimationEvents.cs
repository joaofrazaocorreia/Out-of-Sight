using UnityEngine;

public class NPCAnimationEvents : MonoBehaviour
{
    public void PlayFootstepSFX()
    {
        GetComponentInParent<EnemyMovement>().Footstep();
    }
}
