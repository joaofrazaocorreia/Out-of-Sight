using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] [Range(5f, 20f)] private float UIspeed = 10f;
    [SerializeField] private GameObject UIBackground;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;

    public static bool gamePaused;
    private bool settingsActive;
    private Dictionary<Transform,Vector3> originalUIPositions;
    private PlayerInput playerInput;
    private float deltaTime;

    private void Start()
    {
        gamePaused = false;
        settingsActive = false;
        Time.timeScale = 1f;

        originalUIPositions = new Dictionary<Transform, Vector3>();

        for(int i = 0; i < transform.childCount; i++)
        {
            originalUIPositions.Add(
                transform.GetChild(i), transform.GetChild(i).localPosition);
        }
        
        playerInput = FindAnyObjectByType<PlayerInput>();
        deltaTime = Time.deltaTime;
    }

    private void Update()
    {
        if(playerInput.actions["Pause Game"].WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    
    public void TogglePause()
    {
        StopAllCoroutines();
        gamePaused = !gamePaused;

        // Hides the settings menu if it's active when the pause is toggled
        if(settingsMenu.activeSelf)
        {
            ReturnUIToOrigin(settingsMenu.transform);
        }

        // Pauses the game and displays the pause menu
        if(gamePaused)
        {
            Time.timeScale = 0f;

            UIBackground.SetActive(true);
            MoveUIToPosition(pauseMenu.transform, Vector3.zero);

            Cursor.lockState = CursorLockMode.None;
        }

        // Unpauses the game and hides the menus
        else
        {
            Time.timeScale = 1f;

            UIBackground.SetActive(false);
            ReturnUIToOrigin(pauseMenu.transform);

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ToggleSettings()
    {
        StopAllCoroutines();
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

    public void LoadScene(int scene)
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    private IEnumerator MoveUI(Transform uiTransform, Vector3 newPos)
    {
        while(uiTransform.localPosition != newPos)
        {
            Vector3 difference = newPos - uiTransform.localPosition;
            float movespeed = deltaTime * UIspeed;

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
}
