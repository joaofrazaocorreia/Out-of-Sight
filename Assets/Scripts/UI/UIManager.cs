using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction.Equipments;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using static Player.Status;
using static Enums.Disguise;

public class UIManager : MonoBehaviour
{
    [Header("Detection Audio")]
    [SerializeField] private PlayAudio detectionLoop;
    private float maxDetectionVolume;
    
    [Header("UI Settings")]
    [SerializeField] private float startingTimeScale = 0f;
    [SerializeField] [Range(0.001f, 20f)] private float UISpeed = 3f;
    [SerializeField] private float UIScaleWidthIncrease = 50f;
    [SerializeField] private float UIScaleHeightIncrease = 10f;
    [SerializeField] private float UIScaleFontIncrease = 12f;
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private CanvasGroup UIBackground;
    [SerializeField] private CanvasGroup missionBriefingScreen;
    [SerializeField] private TextMeshProUGUI disguiseText;
    [SerializeField] private Image disguiseImage;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Sprite emptyInventorySlotSprite;
    [SerializeField] private GameObject ammoDisplay;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image ammoSprite;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private CanvasGroup restartConfirmationMenu;
    [SerializeField] private CanvasGroup quitConfirmationMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private CanvasGroup victoryScreen;
    [SerializeField] private CanvasGroup gameOverScreen;
    [SerializeField] private Image[] equipmentIcons;
    [SerializeField] private Image[] inventoryIcons;
    [SerializeField] private GameObject[] interactionUI;
    [SerializeField] private TextMeshProUGUI[] interactionMessages;
    [SerializeField] private Image[] interactionIcons;
    [SerializeField] private GameObject interactingBar;
    [SerializeField] private RectTransform interactingBarFill;
    [SerializeField] private GameObject attackUI;
    [SerializeField] private GameObject CarryingUI;
    [SerializeField] private TextMeshProUGUI objectivesTitle;
    [SerializeField] private Transform objectivesTextParent;
    [SerializeField] private GameObject objectiveTextPrefab;
    [SerializeField] private GameObject globalDetection;
    [SerializeField] private GameObject detectionArrowPrefab;
    [SerializeField] private Transform detectionArrowsParent;
    [SerializeField] private GameObject detectionIcon;
    [SerializeField] private GameObject alarmIcon;
    [SerializeField] private Image detectionFill;
    [SerializeField] private Transform NPCsParent;
    [SerializeField] private Transform CamerasParent;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private PlayAudio objectiveAudioPlayer;
    
    [SerializeField] private Sprite civillianDisguiseSprite;
    [SerializeField] private Sprite workerDisguiseSprite;
    [SerializeField] private Sprite guardTier1DisguiseSprite;
    [SerializeField] private Sprite guardTier2DisguiseSprite;

    [SerializeField] private GameObject mapOverlay;

    public Image[] EquipmentIcons { get => equipmentIcons; }

    private static bool gamePaused;
    private bool settingsActive;
    private Dictionary<Transform, Vector3> originalUIPositions;
    private Dictionary<RectTransform, (Coroutine, float, float, float)> uiButtonScaleUpCoroutines;
    private Dictionary<RectTransform, (Coroutine, float, float, float)> uiButtonScaleDownCoroutines;
    private Player player;
    private PlayerInput playerInput;
    private PlayerMelee playerMelee;
    private PlayerCarryInventory carryInventory;
    private PlayerController playerController;
    private PlayerEquipment playerEquipment;
    private PlayerInteraction playerInteraction;
    private PlayerInventory playerInventory;
    private Dictionary<string, ObjectiveText> objectiveTexts;
    private List<Detection> enemyDetections;
    private Dictionary<Detection, GameObject> detectionArrows;
    private Alarm alarm;
    private float primaryInteractionTextOffset;
    private float secondaryInteractionTextOffset;

    private void Awake()
    {
        // Game begins with timeScale = 0f to show the mission briefing before starting the level
        gamePaused = false;
        settingsActive = false;
        Time.timeScale = startingTimeScale;

        originalUIPositions = new Dictionary<Transform, Vector3>();
        uiButtonScaleUpCoroutines = new Dictionary<RectTransform, (Coroutine, float, float, float)>();
        uiButtonScaleDownCoroutines = new Dictionary<RectTransform, (Coroutine, float, float, float)>();

        for(int i = 0; i < transform.childCount; i++)
        {
            originalUIPositions.Add(
                transform.GetChild(i), transform.GetChild(i).localPosition);
        }

        player = FindAnyObjectByType<Player>();
        player.OnStatusChanged += OnStatusChanged;
        player.OnDisguiseChanged += OnDisguiseChanged;

        playerInput = FindAnyObjectByType<PlayerInput>();

        playerMelee = FindAnyObjectByType<PlayerMelee>();
        playerMelee.OnAttackAvailable += OnAttackAvailable;
        playerMelee.OnAttackNotAvailable += OnAttackNotAvailable;

        carryInventory = FindAnyObjectByType<PlayerCarryInventory>();
        carryInventory.OnCarryPickup += OnCarryPickup;
        carryInventory.OnCarryDrop += OnCarryDrop;

        playerController = FindFirstObjectByType<PlayerController>();
        playerController.OnStaminaUpdate += OnStaminaUpdate;
        playerController.ToggleMap += ToggleMap;
        staminaSlider.gameObject.SetActive(playerController.UseStamina);

        playerEquipment = FindAnyObjectByType<PlayerEquipment>();
        playerEquipment.OnEquipmentAdded += OnEquipmentAdded;
        playerEquipment.OnEquipmentChanged += OnEquipmentChanged;

        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        playerInteraction.WhileInteracting += WhileInteracting;
        playerInteraction.OnInteractionStop += OnInteractionStop;
        playerInteraction.OnHitInteractableChanged += OnHitInteractableChanged;

        playerInventory = FindAnyObjectByType<PlayerInventory>();
        playerInventory.OnInventoryUpdated += OnInventoryUpdated;
        
        objectiveTexts = new Dictionary<string, ObjectiveText>();
        enemyDetections = new List<Detection>();
        detectionArrows = new Dictionary<Detection, GameObject>();
        alarm = FindAnyObjectByType<Alarm>();
        primaryInteractionTextOffset = interactionMessages[0].transform.localPosition.x;
        secondaryInteractionTextOffset = interactionMessages[1].transform.localPosition.x;

        loadingScreen.gameObject.SetActive(true);
        missionBriefingScreen.gameObject.SetActive(true);
        UnlockCursor();
        StartCoroutine(FadeOutUI(loadingScreen, UISpeed));
    }

    private void Start()
    {
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", sensitivitySlider.value/2f) * 2f;
        detectionLoop.AudioSource.outputAudioMixerGroup.audioMixer.GetFloat("Volume", out maxDetectionVolume);
    }

    private void Update()
    {
        // Updates the HUD while the game is unpaused
        if(Time.timeScale != 0)
        {
            UpdateGlobalDetectionFill();
            UpdateDetectionArrows();

            for (int i = 0; i < interactionUI.Length; i++)
            {
                if (interactionUI[i].activeSelf)
                {
                    UpdateInteractionUi();
                    break;
                }
            }
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
            DialogueBox.Instance.DialogueAudio.Pause();
            DialogueBox.Instance.NoiseAudio.Pause();
            UnlockCursor();

            StartCoroutine(FadeInUI(UIBackground, UISpeed));
            MoveUIToPosition(pauseMenu.transform, Vector3.zero);
        }

        // Unpauses the game and hides the menus
        else
        {
            Time.timeScale = 1f;
            DialogueBox.Instance.DialogueAudio.UnPause();
            DialogueBox.Instance.NoiseAudio.UnPause();
            LockCursor();

            StartCoroutine(FadeOutUI(UIBackground, UISpeed));
            ReturnUIToOrigin(pauseMenu.transform);
        }

        playerController.IsPaused = gamePaused;
    }

    public void ToggleSettings()
    {
        StopAllCoroutines();
        UnlockCursor();
        settingsActive = !settingsActive;

        settingsMenu.GetComponent<CanvasGroup>().blocksRaycasts = settingsActive;
        settingsMenu.GetComponent<CanvasGroup>().interactable = settingsActive;

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
    
    private void ToggleAmmoDisplay(bool? toggle = null)
    {
        if(toggle == null)
            ammoDisplay.SetActive(ammoDisplay.activeSelf);
        
        else
            ammoDisplay.SetActive((bool)toggle);
        
        if(ammoDisplay.activeSelf) UpdateAmmoIcon();
    }

    private void UpdateAmmoIcon()
    {
        ammoSprite.sprite = playerEquipment.CurrentEquipment.Icon;
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
        StartCoroutine(FadeInUI(victoryScreen, UISpeed));
        UnlockCursor();
        Time.timeScale = 0f;
    }

    public void Lose()
    {
        StartCoroutine(FadeInUI(gameOverScreen, UISpeed));
        UnlockCursor();
        Time.timeScale = 0f;
    }
    
    public void LoadScene(int scene)
    {
        Time.timeScale = 1f;
        StopAllCoroutines();
        UnlockCursor();
        StartCoroutine(StartLoadingScene(scene));
    }

    private IEnumerator StartLoadingScene(int scene)
    {
        StartCoroutine(FadeInUI(loadingScreen, UISpeed));

        while(loadingScreen.alpha < 1f)
        {
            yield return null;
        }

        SceneManager.LoadScene(scene);
    }

    private IEnumerator MoveUI(Transform uiTransform, Vector3 newPos, float speed = 1f)
    {
        while(uiTransform.localPosition != newPos)
        {
            Vector3 difference = newPos - uiTransform.localPosition;
            float movespeed = Time.fixedDeltaTime * UISpeed * speed;

            Vector3 translation = new Vector3(
                Mathf.Max(difference.x * movespeed, Mathf.Min(movespeed, difference.x)),
                    Mathf.Max(difference.y * movespeed, Mathf.Min(movespeed, difference.y)),
                        difference.z);

            uiTransform.localPosition += translation;
            yield return null;
        }
    }

    public void MoveUIToPosition(Transform uiTransform, Vector3 newPos, float speed = -1)
    {
        if (speed < 0)
            speed = UISpeed * 2;
        
        StartCoroutine(MoveUI(uiTransform, newPos, speed));
    }

    private void ReturnUIToOrigin(Transform uiTransform)
    {
        if(originalUIPositions.Keys.Contains(uiTransform))
            StartCoroutine(MoveUI(uiTransform, originalUIPositions[uiTransform], UISpeed));
    }

    private void FadeToggleScreen(CanvasGroup screen)
    {
        if(screen.alpha > 0.5f)
        {
            StopCoroutine(FadeInUI(screen, UISpeed));
            StartCoroutine(FadeOutUI(screen, UISpeed));
        }

        else
        {
            StopCoroutine(FadeOutUI(screen, UISpeed));
            StartCoroutine(FadeInUI(screen, UISpeed));
        }
    }

    public IEnumerator FadeInUI(CanvasGroup ui, float speed = 1f)
    {
        ui.blocksRaycasts = true;

        while(ui.alpha < 1f)
        {
            ui.alpha += Time.fixedDeltaTime * UISpeed * speed;
            yield return null;
        }
    }

    public IEnumerator FadeOutUI(CanvasGroup ui, float speed = 1f)
    {
        ui.blocksRaycasts = false;
        
        while(ui.alpha > 0f)
        {
            ui.alpha -= Time.fixedDeltaTime * UISpeed * speed;
            yield return null;
        }
    }

    public IEnumerator ScaleUI(RectTransform uiTransform, float widthIncrement, float heightIncrement, float textIncrement, float speed = 1f)
    {
        TextMeshProUGUI buttonText = uiTransform.GetComponentInChildren<TextMeshProUGUI>();

        float targetWidth = uiTransform.sizeDelta.x + widthIncrement;
        float targetHeight = uiTransform.sizeDelta.y + heightIncrement;
        float targetTextSize = buttonText.fontSize + textIncrement;

        while(uiTransform.sizeDelta.x != targetWidth || uiTransform.sizeDelta.y != targetHeight || buttonText.fontSize != targetTextSize)
        {
            float widthDifference = targetWidth - uiTransform.sizeDelta.x;
            float heightDifference = targetHeight - uiTransform.sizeDelta.y;
            float textDifference = targetTextSize - buttonText.fontSize;

            if(widthDifference != 0)
                widthDifference = Mathf.Clamp(widthDifference, -(Time.fixedDeltaTime * UISpeed * speed) * 50, Time.fixedDeltaTime * UISpeed * speed * 50);

            if(heightDifference != 0)
                heightDifference = Mathf.Clamp(heightDifference, -(Time.fixedDeltaTime * UISpeed * speed) * 30, Time.fixedDeltaTime * UISpeed * speed * 30);

            if(textDifference != 0)
                textDifference = Mathf.Clamp(textDifference, -(Time.fixedDeltaTime * UISpeed * speed) * 30, Time.fixedDeltaTime * UISpeed * speed * 30);

            uiTransform.sizeDelta += new Vector2(widthDifference, heightDifference);
            buttonText.fontSize += textDifference;
            yield return null;
        }
    }

    public void HoverScaleUpUI(RectTransform uiTransform)
    {
        TextMeshProUGUI buttonText = uiTransform.GetComponentInChildren<TextMeshProUGUI>();
        if(uiButtonScaleDownCoroutines.Keys.Contains(uiTransform))
        {
            StopCoroutine(uiButtonScaleDownCoroutines[uiTransform].Item1);

            uiTransform.sizeDelta = new Vector2(
                uiButtonScaleDownCoroutines[uiTransform].Item2, uiButtonScaleDownCoroutines[uiTransform].Item3);
            buttonText.fontSize = uiButtonScaleDownCoroutines[uiTransform].Item4;

            uiButtonScaleDownCoroutines.Remove(uiTransform);
        }

        if(!uiButtonScaleUpCoroutines.Keys.Contains(uiTransform))
        {
            uiButtonScaleUpCoroutines.Add(uiTransform, (StartCoroutine(ScaleUI(uiTransform, UIScaleWidthIncrease,
                UIScaleHeightIncrease, UIScaleFontIncrease, 3f)), uiTransform.sizeDelta.x + UIScaleWidthIncrease,
                    uiTransform.sizeDelta.y + UIScaleHeightIncrease, buttonText.fontSize + UIScaleFontIncrease));
        }
    }

    public void HoverScaleDownUI(RectTransform uiTransform)
    {
        TextMeshProUGUI buttonText = uiTransform.GetComponentInChildren<TextMeshProUGUI>();
        if(uiButtonScaleUpCoroutines.Keys.Contains(uiTransform))
        {
            StopCoroutine(uiButtonScaleUpCoroutines[uiTransform].Item1);

            uiTransform.sizeDelta = new Vector2(
                uiButtonScaleUpCoroutines[uiTransform].Item2, uiButtonScaleUpCoroutines[uiTransform].Item3);
            buttonText.fontSize = uiButtonScaleUpCoroutines[uiTransform].Item4;

            uiButtonScaleUpCoroutines.Remove(uiTransform);
        }

        if(!uiButtonScaleDownCoroutines.Keys.Contains(uiTransform))
        {
            uiButtonScaleDownCoroutines.Add(uiTransform, (StartCoroutine(ScaleUI(uiTransform, -UIScaleWidthIncrease,
                -UIScaleHeightIncrease, -UIScaleFontIncrease, 3f)), uiTransform.sizeDelta.x - UIScaleWidthIncrease,
                    uiTransform.sizeDelta.y -UIScaleHeightIncrease, buttonText.fontSize - UIScaleFontIncrease));
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    
    private void UpdateStatusUI()
    {
        var status = player.status;
        
        
        if (status.Contains(CriticalTrespassing))
            UpdateStatusText("Invading", new Color(0.4f, 0f ,0f));
        
        else if(status.Contains(Suspicious))
            UpdateStatusText("Suspicious", Color.red);
        
        else if(status.Contains(Trespassing))
            UpdateStatusText("Trespassing", Color.yellow);
        
        else if(status.Contains(Doubtful))
            UpdateStatusText("Doubtful", new Color(0.75f, 0.75f, 0.3f));

        else
            UpdateStatusText("Concealed", Color.white);
        
    }

    private void UpdateStatusText(string text, Color color)
    {
        if(statusText.text != text)
            statusText.text = text;
            
        if(statusText.color != color)
            statusText.color = color;
    }
    
    private void UpdateDisguiseUI()
    {
        
        string newText;
        Sprite newImage;

        switch(player.disguise)
        {
            case Civillian:
            {
                newText = "Disguise: Civilian";
                newImage = civillianDisguiseSprite;
                break;
            }
            case Employee:
            {
                newText = "Disguise: Employee";
                newImage = workerDisguiseSprite;
                break;
            }
            
            case Guard:
            {
                newText = "Disguise: Security";
                newImage = guardTier1DisguiseSprite;
                break;
            }
            
            case Police:
            {
                newText = "Disguise: Police";
                newImage = guardTier2DisguiseSprite;
                break;
            }
            
            default:
            {
                newText = $"Disguise: {player.disguise}";
                newImage = civillianDisguiseSprite;
                break;
            }
            
        }
        UpdateDisguiseText(newText);
        UpdateDisguiseImage(newImage);
    }


    private void UpdateDisguiseText(string text)
    {
        if(disguiseText.text != text)
            disguiseText.text = text;
    }

    private void UpdateDisguiseImage(Sprite sprite)
    {
        if(disguiseImage.sprite != sprite)
            disguiseImage.sprite = sprite;
    }

    private void UpdateAmmoText(string text)
    {
        if(ammoText.text != text)
            ammoText.text = text;
    }

    private void UpdateEquipmentIcon(Sprite newIcon, int index)
    {
        if(index < equipmentIcons.Length && equipmentIcons[index] != null) equipmentIcons[index].sprite = newIcon;
    }

    public void ResetEquipmentIcon(int index)
    {
        if(index < equipmentIcons.Length && equipmentIcons[index] != null)
            equipmentIcons[index].sprite = emptyInventorySlotSprite;
    }

    private void UpdateEquipmentUI()
    {
        var equipment = playerEquipment.CurrentEquipment;
        if(equipment is not IHasAmmo ammo) ToggleAmmoDisplay(false);
        else
        {
            ToggleAmmoDisplay(true);
            
            if (ammo.MaxAmmo == -1)
                UpdateAmmoText("inf / inf");

            else
                UpdateAmmoText(ammo.CurrentAmmo + " / " + ammo.MaxAmmo);
        }
    }


    private void UpdateInventoryIcon(Sprite newIcon, int index)
    {
        if(index < inventoryIcons.Length && inventoryIcons[index] != null) inventoryIcons[index].sprite = newIcon;
    }

    private void UpdateInteractionUi()
    {
        var hitInteractables = playerInteraction.HitInteractables;
        if (hitInteractables == null || hitInteractables.Length == 0)
        {
            DisableInteractionMessage();
            return;
        }
        UpdateInteractionUi(hitInteractables, 
            playerInteraction.CheckValidInteraction(playerInteraction.GetInteractiveObject(0)), 
            playerInteraction.CheckValidInteraction(playerInteraction.GetInteractiveObject(1)));
    }

    private void UpdateInteractionUi(InteractiveObject[] interactiveObjects, bool canInteractPrimary, bool canInteractSecondary)
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
            ToggleInteractionIcon(i==0 ? canInteractPrimary : canInteractSecondary, i);
        }
    }

    private void ToggleInteractionMessage(bool? toggle, int index)
    {
        try
        {
            if(interactionMessages[index].text == "")
                interactionUI[index].SetActive(false);

            else if(toggle != null)
                interactionUI[index].SetActive((bool)toggle);
            
            else
                interactionUI[index].SetActive(!interactingBar.activeSelf);
        }
        catch
        {
            Debug.LogWarning($"index {index} is outside bounds of the array (interactionMessages.Count == {interactionMessages.Count()}; interactionUI.Count == {interactionUI.Count()})");
        }
        
    }

    private void DisableInteractionMessage()
    {
        ToggleInteractionMessage(false, 0);
        ToggleInteractionMessage(false, 1);
    }

    private void ToggleInteractionIcon(bool? toggle, int index)
    {
        if(toggle != null)
            interactionIcons[index].enabled = (bool)toggle;
        
        else
            interactionIcons[index].enabled = !interactingBar.activeSelf;



        if(interactionIcons[index].enabled)
        {
            interactionMessages[index].transform.localPosition = new Vector3(
                index == 0 ? primaryInteractionTextOffset : secondaryInteractionTextOffset,
                    interactionMessages[0].transform.localPosition.y, interactionMessages[1].transform.localPosition.z);        
        }

        else
            interactionMessages[index].transform.localPosition = Vector3.zero;
    }

    private void UpdateInteractionText(string text, int index)
    {
        interactionMessages[index].text = text;
    }

    private void ToggleInteractingBar(bool? toggle)
    {
        if(toggle != null)
            interactingBar.SetActive((bool)toggle);
        
        else
            interactingBar.SetActive(!interactingBar.activeSelf);
    }

    private void UpdateInteractionBar()
    {
        var scale = 1 - playerInteraction._interactionDuration /
            playerInteraction.ActiveInteractiveObject.InteractionDuration;
        interactingBarFill.localScale = new Vector3(scale, 1f, 1f);
        ToggleInteractingBar(true);
    }

    public void ChangeObjectivesTitle(string newText)
    {
        objectivesTitle.text = newText;
    }

    public void EditObjective(string objectiveName, string newText, float textOpacity = 1f)
    {
        if(objectiveTexts.Keys.Contains(objectiveName))
        {
            objectiveTexts[objectiveName].UpdateText(newText, textOpacity);
        }

        else
        {
            Debug.Log($"Objective edit failed: \"{objectiveName}\"" +
                " not found. Creating new objective...");

            ObjectiveText newObjective = Instantiate
                (objectiveTextPrefab, objectivesTextParent).GetComponent<ObjectiveText>();

            newObjective.UpdateText(newText, textOpacity);

            objectiveTexts.Add(objectiveName, newObjective);
        }
        
        objectiveAudioPlayer.Play();
    }

    public void RemoveObjective(string objectiveName)
    {
        if(objectiveTexts.Keys.Contains(objectiveName))
        {
            Destroy(objectiveTexts[objectiveName].gameObject);
            objectiveTexts.Remove(objectiveName);
        }

        else
        {
            Debug.Log($"Tried to delete an objective that wasn't found. (\"{objectiveName}\")");
        }
    }

    public void RemoveAllObjectives()
    {
        while(objectiveTexts.Count > 0)
        {
            RemoveObjective(objectiveTexts.First().Key);
        }
    }

    private void ToggleGlobalDetection(bool? toggle, bool alarm = false)
    {
        if(toggle != null)
            globalDetection.SetActive((bool)toggle);
        
        else
            globalDetection.SetActive(!globalDetection.activeSelf);

        if(alarm)
        {
            detectionIcon.SetActive(false);
            alarmIcon.SetActive(true);
            detectionLoop.Stop();
        }

        else
        {
            detectionIcon.SetActive(true);
            alarmIcon.SetActive(false);
        }
    }

    private bool CheckEnemyDetectionsForUpdate()
    {
        int detectionCount = NPCsParent.childCount + CamerasParent.childCount;

        for(int i = 0; i < NPCsParent.childCount; i++)
        {
            Enemy enemy = NPCsParent.GetChild(i).GetComponent<Enemy>();
            if(enemy == null || enemy.EnemyStatus == Enemy.Status.KnockedOut) 
                detectionCount--;
        }
        for(int i = 0; i < CamerasParent.childCount; i++)
        {
            EnemyCamera ec = CamerasParent.GetChild(i).GetComponent<EnemyCamera>();
            if(ec == null || !ec.IsOn) 
                detectionCount--;
        }

        return enemyDetections.Count != detectionCount;
    }

    private void UpdateEnemyDetectionsList()
    {
        if(CheckEnemyDetectionsForUpdate())
        {
            enemyDetections = new List<Detection>();
            detectionArrows = new Dictionary<Detection, GameObject>();
            for(int i = detectionArrowsParent.childCount-1; i >= 0; i--)
            {
                Destroy(detectionArrowsParent.GetChild(i).gameObject);
            }


            for(int i = 0; i < NPCsParent.childCount; i++)
            {
                Enemy enemy = NPCsParent.GetChild(i).GetComponent<Enemy>();
                if(enemy != null && enemy.EnemyStatus != Enemy.Status.KnockedOut)
                {
                    Detection d = NPCsParent.GetChild(i).GetComponentInChildren<Detection>();

                    if(d)
                    {
                        enemyDetections.Add(d);
                        detectionArrows.Add(d, Instantiate(detectionArrowPrefab, detectionArrowsParent));
                        detectionArrows[d].name = d.transform.parent.name + " Arrow";
                    }
                }
            }

            for(int i = 0; i < CamerasParent.childCount; i++)
            {
                EnemyCamera ec = CamerasParent.GetChild(i).GetComponent<EnemyCamera>();
                if(ec != null && ec.IsOn)
                {
                    Detection d = CamerasParent.GetChild(i).GetComponentInChildren<Detection>();

                    enemyDetections.Add(d);
                    detectionArrows.Add(d, Instantiate(detectionArrowPrefab, detectionArrowsParent));
                    detectionArrows[d].name = d.transform.parent.parent.parent.parent.parent.parent.name + " Arrow";
                }
            }
        }
    }

    private void UpdateGlobalDetectionFill()
    {
        if(!alarm.IsOn)
        {
            float fill = 0f;

            UpdateEnemyDetectionsList();
            foreach(Detection d in enemyDetections)
            {
                if(d.DetectionMeter / d.DetectionLimit > fill)
                {
                    fill = d.DetectionMeter / d.DetectionLimit;
                }
            }

            if(fill > 0)
            {
                ToggleGlobalDetection(true, alarm.IsOn);
                detectionFill.fillAmount = fill;

                float colorDifference = 1 - fill;
                detectionFill.GetComponentInChildren<Image>().color = new Color(1, colorDifference, colorDifference, 1);
                if (fill < 1)
                {
                    if(!detectionLoop.AudioSource.isPlaying) detectionLoop.Play();
                    detectionLoop.AudioSource.volume = Mathf.Lerp(-80, maxDetectionVolume, fill);
                }
            }

            else
            {
                ToggleGlobalDetection(false, alarm.IsOn);
                detectionLoop.Stop();
            }
        }

        else
        {
            ToggleGlobalDetection(true, alarm.IsOn);
        }
    }

    private void UpdateDetectionArrows()
    {
        if(player.detectable)
        {
            Transform player = playerInput.transform;
            float cameraAngle = player.eulerAngles.y - 90;

            UpdateEnemyDetectionsList();
            foreach(Detection d in enemyDetections)
            {
                if(d.DetectionMeter > 0)
                {
                    Vector3 direction =  d.transform.position - player.transform.position;
                    float angle = Vector2.Angle(
                        new Vector2(direction.x, direction.z), new Vector2(player.eulerAngles.y, 0f));
                    
                    if(direction.z < 0)
                        angle *= -1;

                    detectionArrows[d].SetActive(!alarm.IsOn || (alarm.IsOn && d.SeesPlayer));
                    detectionArrows[d].transform.eulerAngles = new Vector3(0, 0, cameraAngle + angle);

                    float colorDifference = 1 - (d.DetectionMeter / d.DetectionLimit);
                    detectionArrows[d].GetComponentInChildren<Image>().color = new Color(1, colorDifference, colorDifference, 1);
                }

                else if(detectionArrows[d].activeSelf)
                {
                    detectionArrows[d].SetActive(false);
                    detectionArrows[d].transform.eulerAngles = Vector3.zero;
                }
            }
        }

        else
        {
            foreach(KeyValuePair<Detection, GameObject> kv in detectionArrows)
            {
                if(kv.Value.activeSelf)
                    kv.Value.SetActive(false);
            }
        }
    }

    public void UpdateMouseSensitivity()
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value / 2f);
        playerController.UpdateMouseSensitivity(PlayerPrefs.GetFloat("Sensitivity"));
    }

    public void StartLevelEffects()
    {
        CameraEffects.Instance.ToggleGlitchEffects();
        CameraEffects.Instance.Shake(1f);
        CameraEffects.Instance.PlayGlitchSound();

        StartCoroutine(StartEffectsCoroutine(1f));
    }

    public IEnumerator StartEffectsCoroutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            CameraEffects.Instance.SetDigitalGlitchIntensity(1 - (elapsed / duration));
            CameraEffects.Instance.SetAnalogGlitchJitter(1 - (elapsed / duration));
            //CameraEffects.Instance.SetAnalogGlitchJump(1 - (elapsed / duration));
            CameraEffects.Instance.SetAnalogGlitchShake(1 - (elapsed / duration));
            CameraEffects.Instance.SetAnalogGlitchDrift(1 - (elapsed / duration));

            elapsed += Time.deltaTime;
            yield return null;

            if(elapsed >= duration)
            {
                CameraEffects.Instance.ResetAllEffects();
                //CameraEffects.Instance.ToggleGlitchEffects();
            }
        }
    }

    public void EndLevelEffects()
    {
        CameraEffects.Instance.InvertedShake(1f);
        CameraEffects.Instance.PlayGlitchSound();

        StartCoroutine(EndEffectsCoroutine(1f));
    }

    public IEnumerator EndEffectsCoroutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            CameraEffects.Instance.SetAllEffects(elapsed / duration, elapsed / duration, 0.3f,
                                                 elapsed / duration, elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;

            if(elapsed >= duration)
            {
                CameraEffects.Instance.SetAllEffects(1, 1, 0.3f, 1, 1);
            }
        }
    }
    private void ToggleMap()
    {
        mapOverlay?.SetActive(!mapOverlay.activeSelf);
    }

    public void TogglePlayerControls(bool lockMove = false, bool lockLook = false, bool lockInteract = false)
    {
        playerController.ForceLockMove = lockMove;
        playerController.ForceLockLook = lockLook;
        playerInteraction.ForceLockInteraction = lockInteract;
    }

    private void ToggleAttackUI(bool state) => attackUI.SetActive(state);
    private void ToggleCarryUI(bool state) => CarryingUI.SetActive(state);

    private void UpdateStamina(float stamina) => staminaSlider.value = stamina;
    private void OnStaminaUpdate(object sender, EventArgs e) => UpdateStamina(playerController._currentStamina);

    private void OnAttackAvailable(object sender, EventArgs e) => ToggleAttackUI(true);
    private void OnAttackNotAvailable(object sender, EventArgs e) => ToggleAttackUI(false);

    private void OnCarryPickup(object sender, EventArgs e) => ToggleCarryUI(true);
    private void OnCarryDrop(object sender, EventArgs e) => ToggleCarryUI(false);
    private void OnEquipmentAdded(object sender, EventArgs e) => UpdateEquipmentIcon(playerEquipment._recentlyAddedEquipment.Icon, Array.IndexOf(playerEquipment.EquipmentObjects.ToArray(), playerEquipment._recentlyAddedEquipment));

    private void OnEquipmentChanged(object sender, EventArgs e) => UpdateEquipmentUI();

    private void WhileInteracting(object sender, EventArgs e) => UpdateInteractionBar();

    private void OnInteractionStop(object sender, EventArgs e)
    {
        DisableInteractionMessage();
        ToggleInteractingBar(false);
    } 

    private void OnHitInteractableChanged(object sender, EventArgs e) => UpdateInteractionUi();

    private void OnInventoryUpdated(object sender, EventArgs e) => UpdateInventoryIcon(playerInventory.NewIcon, playerInventory.UpdatedItemIndex);
    
    private void OnStatusChanged(object sender, EventArgs e) => UpdateStatusUI();
    
    private void OnDisguiseChanged(object sender, EventArgs e) => UpdateDisguiseUI();
    
    private void ToggleMap(object sender, EventArgs e) => ToggleMap();
}