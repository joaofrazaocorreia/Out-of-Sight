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
    [SerializeField] [Range(1f, 20f)] private float UISpeed = 3f;
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
    [SerializeField] private GameObject InteractionUI;
    [SerializeField] private TextMeshProUGUI InteractionMessage;

    public static bool gamePaused;
    private bool settingsActive;
    private Dictionary<Transform,Vector3> originalUIPositions;
    private PlayerInput playerInput;
    private float deltaTime;
    private float timer;

    private void Start()
    {
        // Game begins with timeScale = 0f to show the mission briefing before starting the level
        gamePaused = false;
        settingsActive = false;
        Time.timeScale = startingTimeScale;

        originalUIPositions = new Dictionary<Transform, Vector3>();

        for(int i = 0; i < transform.childCount; i++)
        {
            originalUIPositions.Add(
                transform.GetChild(i), transform.GetChild(i).localPosition);
        }
        
        playerInput = FindAnyObjectByType<PlayerInput>();
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

    public void ToggleInteractionMessage(bool newState)
    {
        InteractionUI.SetActive(newState);
    }

    public void UpdateInteractionText(string text)
    {
        InteractionMessage.text = text;
    }
}
