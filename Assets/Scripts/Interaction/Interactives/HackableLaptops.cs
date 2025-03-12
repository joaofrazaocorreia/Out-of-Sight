using System;
using Interaction;
using UnityEngine;

public class HackableLaptops : InteractiveObject, IDelayBetweenInteractions
{
    public float DelayBetweenInteractions => delayBetweenInteractions;
    public float DelayTimer { get; private set;}
    public bool TimerStarted { get; private set; }
    public bool TimerFinished { get; private set; }
    
    [SerializeField] private string customRequirementMessage;
    [SerializeField] private float delayBetweenInteractions;

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
    
    public override string GetInteractionText(bool requirementsMet)
    {
        if (!requirementsMet) return customRequirementMessage;

        return customInteractionMessage;
    }

    private void SuccessfulHack()
    {
        
    }
}

