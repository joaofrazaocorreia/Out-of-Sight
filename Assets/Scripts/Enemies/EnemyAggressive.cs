using UnityEngine;
using UnityEngine.UI;

public class EnemyAggressive : Enemy
{
    [Header("Aggressive Enemy Variables")]
    [SerializeField] private float aggroTime = 5f; // How long this enemy stays at the last seen player position before searching around it
    [SerializeField] private float playerCatchDistance = 1.5f;
    [SerializeField] private GameObject radioIcon;
    [SerializeField] private Image radioFill;
    [SerializeField] private PlayAudio radioSoundPlayer;

    private float aggroTimer;
    private Vector3 prevPlayerPos;

    protected override void Update()
    {
        if(detection.DetectionMeter >= detection.DetectionLimit && !IsAlarmed)
        {
            BecomeAlarmed();
        }
        
        base.Update();

        RadioTimer();
    }
    
    protected void RadioTimer()
    {
        if(IsConscious && IsAlarmed && !alarm.IsOn)
        {
            TickBehaviorTimers();
            if (radioIcon != null)
            {
                radioIcon.SetActive(true);
                radioFill.fillAmount = 1 - (alarmedTimer / alarmedTime);
            }

            if(alarmedTimer <= 0)
            {
                alarm.TriggerAlarm(!alarm.IsOn);
                radioIcon.SetActive(false);
            }
        }

        else if (radioIcon != null)
            radioIcon.SetActive(false);
    }
    
    public override void BecomeAlarmed()
    {
        if(IsConscious)
        {
            if(alarm == null) Start();
            // On the first time being alarmed, plays the radio loop sound
            if (!alarm.IsOn && !IsAlarmed)
            {
                radioSoundPlayer.Play();
            }
            
            base.BecomeAlarmed();

            // Begins chasing the player unless this enemy specifically ignores the alarm
            if(!ignoresAlarm)
            {
                EnemyStatus = Status.Chasing;
                aggroTimer = aggroTime;
            }
        }
    }
    
    protected override void SearchingBehavior()
    {
        // Becomes alarmed if this enemy (or another) sees the player 
        if(detection.SeesPlayer || Detection.lastPlayerPos != prevPlayerPos)
        {
            prevPlayerPos = Detection.lastPlayerPos;
            BecomeAlarmed();
        }
        
        // Goes back to normal after the alarm ends
        if (!alarm.IsOn)
        {
            BecomeNormal();
        }

        base.SearchingBehavior();
    }

    protected override void ChasingBehavior()
    {
        // Becomes alarmed if this enemy sees the player while chasing them
        if(detection.SeesPlayer)
        {
            BecomeAlarmed();

            // Resets the alarm timer if it's already on
            if(alarm.IsOn)
                alarm.TriggerAlarm(!alarm.IsOn);
        }

        // Keeps the enemy aggro until it reaches its target.
        if(!enemyMovement.IsAtDestination)
        {
            aggroTimer = aggroTime;
        }

        else
        {
            // Remains at the position if it's still aggro
            if(aggroTimer > 0)
            {
                aggroTimer -= Time.deltaTime;
            }

            // Begins searching after aggro period ends
            else
            {
                prevPlayerPos = Detection.lastPlayerPos;
                EnemyStatus = Status.Searching;
            }
        }

        base.ChasingBehavior();


        // Checks how far the player is horizontally and vertically

        float distanceToPlayerVertical = player.transform.position.y - transform.position.y;
        float distanceToPlayerHorizontal = (Vector3.Scale(player.transform.position, new Vector3(1f, 0f, 1f))
            - Vector3.Scale(transform.position, new Vector3(1f, 0f, 1f))).magnitude;

        // The player loses if this enemy is close enough to them
        if(distanceToPlayerHorizontal < playerCatchDistance && distanceToPlayerVertical < playerCatchDistance * 2.25f)
        {
            uiManager.Lose();
        }
    }
}