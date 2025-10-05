using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(TutorialDialogueTriggers))]
public class CheckpointManager : MonoBehaviour
{
    private MapArea currentCheckpoint;
    private Dictionary<TutorialDialogueTriggers, bool> dialogueTriggersList;
    private TutorialDialogueTriggers dialogueTrigger;
    private UIManager uiManager;
    private Player player;
    private PlayerEquipment playerEquipment;
    private Alarm alarm;


    private void Start()
    {
        dialogueTrigger = GetComponent<TutorialDialogueTriggers>();
        uiManager = FindAnyObjectByType<UIManager>();
        player = FindAnyObjectByType<Player>();
        playerEquipment = player.GetComponent<PlayerEquipment>();
        alarm = FindAnyObjectByType<Alarm>();

        UpdateCheckpoint(null);
    }

    public void UpdateCheckpoint(MapArea checkpoint)
    {
        currentCheckpoint = checkpoint;

        dialogueTriggersList = new Dictionary<TutorialDialogueTriggers, bool>();
        foreach (TutorialDialogueTriggers t in FindObjectsByType<TutorialDialogueTriggers>(FindObjectsSortMode.None))
        {
            dialogueTriggersList.Add(t, t.HasShownDialogue);
        }

        checkpoint?.gameObject.SetActive(false);
    }

    public void ReturnToCheckpoint()
    {
        StartCoroutine(ReturnToCheckpointCoroutine());
    }

    private IEnumerator ReturnToCheckpointCoroutine()
    {
        dialogueTrigger.CaughtDialogue();

        yield return new WaitForSeconds(4f);

        uiManager.ToggleLoadingScreen();

        yield return new WaitForSeconds(2f);

        if (currentCheckpoint != null)
        {
            alarm.AlarmTimer = 0f;

            if(playerEquipment.CurrentEquipment != null)
                playerEquipment.NewEquipmentSelected(playerEquipment.CurrentEquipmentNum);

            player.gameObject.SetActive(false);
            player.transform.position = currentCheckpoint.transform.position;
            player.gameObject.SetActive(true);

            foreach (TutorialDialogueTriggers t in dialogueTriggersList.Keys)
            {
                t.HasShownDialogue = dialogueTriggersList[t];
            }

            foreach (Enemy e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                e.Detection.DetectionMeter = 0f;
                e.AlarmedTimer = 0f;
                e.BecomeNormal(true);
            }

            yield return new WaitForSeconds(0.1f);

            uiManager.ToggleLoadingScreen();
        }

        else
            uiManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
    }
}
