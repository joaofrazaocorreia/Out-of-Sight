using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private float startingTimeScale = 0f;
    [SerializeField] [Range(0.001f, 20f)] private float UISpeed = 3f;
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private CanvasGroup UIBackground;
    [SerializeField] private CanvasGroup missionBriefingScreen;
    [SerializeField] private TextMeshProUGUI disguiseText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI missionTimer;
    [SerializeField] private GameObject ammoDisplay;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private CanvasGroup restartConfirmationMenu;
    [SerializeField] private CanvasGroup quitConfirmationMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private CanvasGroup victoryScreen;
    [SerializeField] private CanvasGroup gameOverScreen;
    [SerializeField] private Image[] equipmentIcons;
    [SerializeField] private Image[] inventoryIcons;
    [SerializeField] private GameObject[] interactionUI;
    [SerializeField] private TextMeshProUGUI[] interactionMessages;
    [SerializeField] private GameObject interactingBar;
    [SerializeField] private RectTransform interactingBarFill;
    [SerializeField] private GameObject globalDetection;
    [SerializeField] private GameObject detectionIcon;
    [SerializeField] private GameObject alarmIcon;
    [SerializeField] private Image detectionFill;
    [SerializeField] private Transform NPCsParent;
    [SerializeField] private Transform CamerasParent;
    [SerializeField] private Slider staminaSlider;

    public static bool gamePaused;
    private bool settingsActive;
    private Dictionary<Transform,Vector3> originalUIPositions;
    private PlayerInput playerInput;
    private List<Detection> enemyDetections;
    private Alarm alarm;
    private float deltaTime;
    private float timer;

    private void Start()
    {
        // Game begins with timeScale = 0f to show the mission briefing before starting the level
        gamePaused = false;
        settingsActive = false;
        Time.timeScale = startingTimeScale;

        #if UNITY_EDITOR
            UISpeed *= 3;
        #endif

        originalUIPositions = new Dictionary<Transform, Vector3>();

        for(int i = 0; i < transform.childCount; i++)
        {
            originalUIPositions.Add(
                transform.GetChild(i), transform.GetChild(i).localPosition);
        }
        
        playerInput = FindAnyObjectByType<PlayerInput>();
        enemyDetections = new List<Detection>();
        alarm = FindAnyObjectByType<Alarm>();
        deltaTime = Time.fixedDeltaTime * UISpeed;
        timer = 0;

        loadingScreen.gameObject.SetActive(true);
        missionBriefingScreen.gameObject.SetActive(true);
        UnlockCursor();
        StartCoroutine(FadeOutUI(loadingScreen));
    }

    private void Update()
    {
        if(Time.timeScale != 0)
        {
            timer += Time.deltaTime;
            UpdateTimerText();

            UpdateGlobalDetectionFill();
        }
        
        if(playerInput.actions["Pause Game"].WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    
    public void TogglePause()
    {
        StopAllCoroutines();
        gamePaused = !gamePaused;

        // Closes the settings menu if it's active when pause is toggled
        if(settingsActive)
        {
            gamePaused = true;
            ToggleSettings();
        }

        // Pauses the game and displays the pause menu
        else if(gamePaused)
        {
            Time.timeScale = 0f;
            UnlockCursor();

            StartCoroutine(FadeInUI(UIBackground));
            MoveUIToPosition(pauseMenu.transform, Vector3.zero);
        }

        // Unpauses the game and hides the menus
        else
        {
            Time.timeScale = 1f;
            LockCursor();

            StartCoroutine(FadeOutUI(UIBackground));
            ReturnUIToOrigin(pauseMenu.transform);
        }
    }

    public void ToggleSettings()
    {
        StopAllCoroutines();
        UnlockCursor();
        settingsActive = !settingsActive;

        if(settingsActive)
        {
            MoveUIToPosition(settingsMenu.transform, Vector3.zero);
            ReturnUIToOrigin(pauseMenu.transform);
        }

        else
        {
            ReturnUIToOrigin(settingsMenu.transform);
            MoveUIToPosition(pauseMenu.transform, Vector3.zero);
        }
    }

    public void ToggleLoadingScreen()
    {
        FadeToggleScreen(loadingScreen);
    }

    public void ToggleMissionBriefing()
    {
        FadeToggleScreen(missionBriefingScreen);
        Time.timeScale = 1f;
    }

    public void ToggleUIBackground()
    {
        FadeToggleScreen(UIBackground);
    }
    
    public void ToggleAmmoDisplay(bool? toggle = null)
    {
        if(toggle == null)
            ammoDisplay.SetActive(ammoDisplay.activeSelf);
        
        else
            ammoDisplay.SetActive((bool)toggle);
    }

    public void ToggleRestartConfirmation()
    {
        FadeToggleScreen(restartConfirmationMenu);
    }

    public void ToggleQuitConfirmation()
    {
        FadeToggleScreen(quitConfirmationMenu);
    }

    public void Win()
    {
        StartCoroutine(FadeInUI(victoryScreen));
        UnlockCursor();
        Time.timeScale = 0f;
    }

    public void Lose()
    {
        StartCoroutine(FadeInUI(gameOverScreen));
        UnlockCursor();
        Time.timeScale = 0f;
    }
    
    public void LoadScene(int scene)
    {
        StopAllCoroutines();
        UnlockCursor();
        Time.timeScale = 1f;
        StartCoroutine(StartLoadingScene(scene));
    }

    private IEnumerator StartLoadingScene(int scene)
    {
        StartCoroutine(FadeInUI(loadingScreen));

        while(loadingScreen.alpha < 1f)
        {
            yield return null;
        }

        SceneManager.LoadScene(scene);
    }

    private IEnumerator MoveUI(Transform uiTransform, Vector3 newPos)
    {
        while(uiTransform.localPosition != newPos)
        {
            Vector3 difference = newPos - uiTransform.localPosition;
            float movespeed = deltaTime;

            Vector3 translation = new Vector3(
                Mathf.Max(difference.x * movespeed, Mathf.Min(movespeed, difference.x)),
                    Mathf.Max(difference.y * movespeed, Mathf.Min(movespeed, difference.y)),
                        difference.z);

            uiTransform.localPosition += translation;
            yield return null;
        }
    }

    private void MoveUIToPosition(Transform uiTransform, Vector3 newPos)
    {
        StartCoroutine(MoveUI(uiTransform, newPos));
    }

    private void ReturnUIToOrigin(Transform uiTransform)
    {
        if(originalUIPositions.Keys.Contains(uiTransform))
            StartCoroutine(MoveUI(uiTransform, originalUIPositions[uiTransform]));
    }

    private void FadeToggleScreen(CanvasGroup screen)
    {
        if(screen.alpha > 0.5f)
        {
            StopCoroutine(FadeInUI(screen));
            StartCoroutine(FadeOutUI(screen));
        }

        else
        {
            StopCoroutine(FadeOutUI(screen));
            StartCoroutine(FadeInUI(screen));
        }
    }

    private IEnumerator FadeInUI(CanvasGroup ui)
    {
        ui.blocksRaycasts = true;

        while(ui.alpha < 1f)
        {
            ui.alpha += deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutUI(CanvasGroup ui)
    {
        ui.blocksRaycasts = false;
        
        while(ui.alpha > 0f)
        {
            ui.alpha -= deltaTime;
            yield return null;
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void UpdateStatusText(string text, Color color)
    {
        if(statusText.text != text)
            statusText.text = text;
            
        if(statusText.color != color)
            statusText.color = color;
    }

    public void UpdateDisguiseText(string text)
    {
        if(disguiseText.text != text)
            disguiseText.text = text;
    }

    private void UpdateTimerText()
    {
        string text = "";
        int minutes = (int)timer / 60;
        int seconds = (int)timer % 60;

        if(minutes < 10)
            text += "0";
        text += $"{minutes} : ";
        
        if(seconds < 10)
            text += "0";
        text += $"{seconds}";


        if(missionTimer.text != text)
            missionTimer.text = text;
    }

    public void UpdateAmmoText(string text)
    {
        if(ammoText.text != text)
            ammoText.text = text;
    }

    public void UpdateEquipmentIcon(Sprite newIcon, int index)
    {
        if(index < equipmentIcons.Length && equipmentIcons[index] != null) equipmentIcons[index].sprite = newIcon;
    }
    
    
    public void UpdateInventoryIcon(Sprite newIcon, int index)
    {
        if(index < inventoryIcons.Length && inventoryIcons[index] != null) inventoryIcons[index].sprite = newIcon;
    }

    public void UpdateInteractionUi(InteractiveObject[] interactiveObjects, bool canInteractPrimary, bool canInteractSecondary)
    {
        for (int i = 0; i < interactiveObjects.Length; i++)
        {
            if (interactiveObjects[i] == null || !interactiveObjects[i].enabled)
            {
                ToggleInteractionMessage(false, i);
                continue;
            }
            
            ToggleInteractionMessage(true, i);
            UpdateInteractionText(interactiveObjects[i].GetInteractionText(i==0 ? canInteractPrimary : canInteractSecondary), i);
        }
    }
    
    public void ToggleInteractionMessage(bool? toggle, int index)
    {
        if(toggle != null)
            interactionUI[index].SetActive((bool)toggle);
        
        else
            interactionUI[index].SetActive(!interactingBar.activeSelf);
    }

    private void UpdateInteractionText(string text, int index)
    {
        interactionMessages[index].text = text;
    }

    public void ToggleInteractingBar(bool? toggle)
    {
        if(toggle != null)
            interactingBar.SetActive((bool)toggle);
        
        else
            interactingBar.SetActive(!interactingBar.activeSelf);
    }

    public void UpdateInteractingBarFillSize(float scale)
    {
        interactingBarFill.localScale = new Vector3(scale, 1f, 1f);
    }

    public void ToggleGlobalDetection(bool? toggle, bool alarm = false)
    {
        if(toggle != null)
            globalDetection.SetActive((bool)toggle);
        
        else
            globalDetection.SetActive(!globalDetection.activeSelf);

        if(alarm)
        {
            detectionIcon.SetActive(false);
            alarmIcon.SetActive(true);
        }

        else
        {
            detectionIcon.SetActive(true);
            alarmIcon.SetActive(false);
        }
    }

    private void UpdateEnemyDetectionsList()
    {
        int detectionCount = NPCsParent.childCount + CamerasParent.childCount;

        if(enemyDetections.Count != detectionCount)
        {
            enemyDetections = new List<Detection>();

            for(int i = 0; i < NPCsParent.childCount; i++)
            {
                EnemyMovement em = NPCsParent.GetChild(i).GetComponent<EnemyMovement>();
                if(em != null && em.currentStatus != EnemyMovement.Status.KnockedOut)
                    enemyDetections.Add(NPCsParent.GetChild(i).GetComponentInChildren<Detection>());
            }

            for(int i = 0; i < CamerasParent.childCount; i++)
            {
                EnemyCamera ec = CamerasParent.GetChild(i).GetComponent<EnemyCamera>();
                if(ec != null && ec.IsOn)
                    enemyDetections.Add(CamerasParent.GetChild(i).GetComponentInChildren<Detection>());
            }
        }
    }

    public void UpdateGlobalDetectionFill()
    {
        if(!alarm.IsOn)
        {
            float fill = 0f;

            UpdateEnemyDetectionsList();
            foreach(Detection d in enemyDetections)
            {
                if(d.DetectionMeter > fill)
                {
                    fill = d.DetectionMeter / d.DetectionLimit;
                }
            }

            if(fill > 0)
            {
                ToggleGlobalDetection(true, alarm.IsOn);
                detectionFill.fillAmount = fill;
            }

            else
            {
                ToggleGlobalDetection(false, alarm.IsOn);
            }
        }

        else
        {
            ToggleGlobalDetection(true, alarm.IsOn);
        }
    }

    public void UpdateStamina(float stamina) => staminaSlider.value = stamina;
}
