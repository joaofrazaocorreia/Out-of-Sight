using UnityEngine;

public class Blinds : InteractiveObject
{
    [Header("Blinds properties")]
    [SerializeField] private InteractionSpot interactionSpot;
    private Animator animator;
    private bool closed;
    
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
    }

    public void AnimationFinished()
    {
        interactionSpot.enabled = true;
    }
    
}
