using UnityEngine;

public interface IDelayBetweenInteractions
{
    public float DelayBetweenInteractions {get;}
    public float DelayTimer {get;}
    public bool TimerStarted {get;}
    public bool TimerFinished {get;}
}
