using UnityEngine;

public class Door : InteractiveObject
{
    private Animator animator;
    private bool _opened;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact()
    {
        OneTimeRequirementCheck();
        _opened = !_opened;
        animator.SetTrigger(_opened ? "Open" : "Close");
    }
}
