using Unity.AI.Navigation;
using UnityEngine;

public class Door : InteractiveObject
{
    [Header("Door Properties")]
    [SerializeField] private NavMeshLink navMeshLink;
    [SerializeField] private PlayAudio doorOpenPlayer;
    [SerializeField] private PlayAudio doorClosePlayer;
    
    private Animator animator;
    private bool _opened;
    
    private bool occupied = false;

    public bool Occupied
    {
        get => occupied;
        set
        {
            if(occupied == value) return;
            occupied = value;
            OpenDoor();
        }
    }
    
    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }
    
    void Update()
    {
        if(navMeshLink == null) return;
        Occupied = navMeshLink.occupied;
    }

    public override void Interact()
    {
        base.Interact();
        
        OpenDoor();
    }

    private void OpenDoor()
    {
        _opened = !_opened;
        animator.SetTrigger(_opened ? "Open" : "Close");   
        PlayAudio(_opened ? doorOpenPlayer : doorClosePlayer);
    }

    private void PlayAudio(PlayAudio audioplayer)
    {
        if(audioplayer != null) audioplayer.Play();
    }
}
