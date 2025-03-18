using System;
using Interaction;
using UnityEngine;

public class HackableLaptops : InteractiveObject, IDelayBetweenInteractions
{
    public float DelayBetweenInteractions => delayBetweenInteractions;
    public float DelayTimer { get; private set;}
    public bool TimerStarted { get; private set; }
    public bool TimerFinished { get; private set; }
    
    private NPCMoveInteraction npcInteraction;
    
    [SerializeField] private float delayBetweenInteractions;

    private void Start()
    {
        npcInteraction = GetComponent<NPCMoveInteraction>();
    }
    
    private void Update()
    {
        if (TimerStarted && !TimerFinished)
        {
            DelayTimer += Time.deltaTime;
            if(DelayTimer >= DelayBetweenInteractions) TimerFinished = true;
            requiredItem = ItemType.None;
            InteractiveType = InteractiveType.DirectNoRequirement;
        }
    }

    public override void Interact()
    {
        base.Interact();

        if (!TimerStarted && !TimerFinished)
        {
            TimerStarted = true;
            requiredItem = ItemType.LockInteraction;
            InteractiveType = InteractiveType.DirectItemRequirement;
            return;
        }

        else
        {
            SuccessfulHack();
        }
    }

    private void SuccessfulHack()
    {
        npcInteraction.Interact();
    }
}

