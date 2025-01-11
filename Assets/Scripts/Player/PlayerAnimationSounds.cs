using UnityEngine;

public class PlayerAnimationSounds : MonoBehaviour
{
    [SerializeField] private PlayAudio footstepPlayer;
    [SerializeField] private PlayAudio interactPlayer;

    public void PlayInteractSound() => interactPlayer.Play();
    public void PlayFootstepSound() => footstepPlayer.Play();
}
