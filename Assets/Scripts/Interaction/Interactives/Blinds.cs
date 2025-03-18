using UnityEngine;

public class Blinds : InteractiveObject
{
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
        enabled = false;
    }

    public void AnimationFinished()
    {
        closed = true;
    }
    
}
