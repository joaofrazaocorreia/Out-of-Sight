using UnityEngine;

public class Door : InteractiveObject
{
    private Animator animator;
    private bool _opened;
    
    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    public override void Interact()
    {
        base.Interact();
        
        _opened = !_opened;
        animator.SetTrigger(_opened ? "Open" : "Close");
    }
}
