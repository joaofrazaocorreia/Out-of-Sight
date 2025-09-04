using UnityEngine;

public class Blinds : InteractiveObject
{
    [Header("Blinds properties")]
    [SerializeField] private InteractionSpot interactionSpot;
    private Animator animator;
    private bool closed;

    [SerializeField] private PlayAudio OpenAudioPlayer;
    [SerializeField] private PlayAudio CloseAudioPlayer;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact()
    {
        base.Interact();
        closed = !closed;
        animator.SetTrigger(closed ? "Close" : "Open");
        interactionSpot.enabled = false;
        
        switch (closed)
        {
            case true:
                if(CloseAudioPlayer != null) CloseAudioPlayer.Play();
                break;
            case false:
                if(OpenAudioPlayer != null) OpenAudioPlayer.Play();
                break;
        }
    }

    public void AnimationFinished()
    {
        interactionSpot.enabled = true;
    }
    
}
