using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(TutorialDialogueTriggers))]
public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToTrack;
    [SerializeField] private List<Enemy> enemiesToTrack;
    [SerializeField] private List<JammingSpot> jammablesToTrack;
    private MapArea currentCheckpoint;
    private Dictionary<TutorialDialogueTriggers, bool> dialogueTriggersList;
    private Dictionary<GameObject, bool> activeGameobjectsList;
    private Dictionary<Enemy, Vector3> enemyPositionsList;
    private Dictionary<EnemyMovement, float> enemySpeedsList;
    private Dictionary<JammingSpot, bool> jammablesStatesList;
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

    private void UpdateLists()
    {
        dialogueTriggersList = new Dictionary<TutorialDialogueTriggers, bool>();
        foreach (TutorialDialogueTriggers t in FindObjectsByType<TutorialDialogueTriggers>(FindObjectsSortMode.None))
        {
            dialogueTriggersList.Add(t, t.HasShownDialogue);
        }

        activeGameobjectsList = new Dictionary<GameObject, bool>();
        foreach (GameObject go in objectsToTrack)
        {
            activeGameobjectsList.Add(go, go.activeSelf);
        }

        enemyPositionsList = new Dictionary<Enemy, Vector3>();
        enemySpeedsList = new Dictionary<EnemyMovement, float>();
        foreach (Enemy e in enemiesToTrack)
        {
            EnemyMovement em = e.GetComponent<EnemyMovement>();

            enemyPositionsList.Add(e, e.transform.position);
            enemySpeedsList.Add(em, em.WalkSpeed);
        }

        jammablesStatesList = new Dictionary<JammingSpot, bool>();
        foreach (JammingSpot js in jammablesToTrack)
        {
            jammablesStatesList.Add(js, js.Jammable.Jammed);
        }
    }

    public void UpdateAddJammedSpot(JammingSpot jammedSpot)
    {
        if (jammablesStatesList.ContainsKey(jammedSpot))
        {
            jammablesStatesList[jammedSpot] = true;
        }
    }

    public void UpdateRemoveJammedSpot(JammingSpot unjammedSpot)
    {
        if (jammablesStatesList.ContainsKey(unjammedSpot))
        {
            jammablesStatesList[unjammedSpot] = false;
        }
    }

    public void UpdateCheckpoint(MapArea checkpoint)
    {
        UpdateLists();
        currentCheckpoint = checkpoint;

        checkpoint?.gameObject.SetActive(false);
    }

    public void ReturnToCheckpoint()
    {
        StartCoroutine(ReturnToCheckpointCoroutine());
    }

    private IEnumerator ReturnToCheckpointCoroutine()
    {
        CameraEffects.Instance.InvertedShake(1f, 3, 0.5f);
        CameraEffects.Instance.PlayGlitchSound();

        if (!CameraEffects.Instance.GlitchEnabled)
            CameraEffects.Instance.ToggleGlitchEffects();
            
        CameraEffects.Instance.SetDigitalGlitchIntensity(0.5f);
        CameraEffects.Instance.SetAnalogGlitchJitter(1f);
        CameraEffects.Instance.SetAnalogGlitchJump(0.3f);
        CameraEffects.Instance.SetAnalogGlitchShake(1f);
        CameraEffects.Instance.SetAnalogGlitchDrift(1f);

        yield return new WaitForSeconds(0.5f);
        CameraEffects.Instance.SetDigitalGlitchIntensity(1f);
        yield return new WaitForSeconds(0.6f);

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

            foreach (GameObject go in activeGameobjectsList.Keys)
            {
                go.SetActive(activeGameobjectsList[go]);
            }

            foreach(Enemy e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                e.Detection.DetectionMeter = 0f;
                e.AlarmedTimer = 0f;
                e.BecomeNormal(true);
            }

            foreach (Enemy e in enemyPositionsList.Keys)
            {
                e.gameObject.SetActive(false);
                e.transform.position = enemyPositionsList[e];
                e.gameObject.SetActive(true);
            }
            
            foreach (EnemyMovement em in enemySpeedsList.Keys)
            {
                em.Halted = false;
                em.MoveTo(em.SpawnPos);
                em.WalkSpeed = enemySpeedsList[em];
            }

            foreach (JammingSpot js in jammablesStatesList.Keys)
            {
                if (js.Jammable.Jammed != jammablesStatesList[js])
                    js.Interact();
            }

            yield return new WaitForSeconds(0.1f);

            CameraEffects.Instance.ResetAllEffects();
            //CameraEffects.Instance.ToggleGlitchEffects();
            uiManager.TogglePlayerControls(false, false, false);
        }

        else
            uiManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
    }
}
