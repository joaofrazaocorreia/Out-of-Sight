using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScripts : MonoBehaviour
{
    [SerializeField] [Range(0.001f, 20f)] private float UISpeed = 3f;
    [SerializeField] private GameObject background;
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelectMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;
    private Dictionary<Transform,Vector3> originalUIPositions;

    private void Start()
    {
        originalUIPositions = new Dictionary<Transform, Vector3>();
        
        #if UNITY_EDITOR
            UISpeed *= 3;
        #endif

        for(int i = 0; i < transform.childCount; i++)
        {
            originalUIPositions.Add(
                transform.GetChild(i), transform.GetChild(i).localPosition);
        }
    }
    private IEnumerator MoveUI(Transform uiTransform, Vector3 newPos)
    {
        while(uiTransform.localPosition != newPos)
        {
            Vector3 difference = newPos - uiTransform.localPosition;
            float movespeed = Time.fixedDeltaTime * UISpeed;

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

    public void QuitGame()
    {
        StopAllCoroutines();
        Application.Quit();
    }

    public void OpenLevelSelect()
    {
        StopAllCoroutines();
        MoveUIToPosition(mainMenu.transform, new Vector3(0, 2000, 0));
        MoveUIToPosition(background.transform, new Vector3(0, 2000, 0));
        MoveUIToPosition(levelSelectMenu.transform, new Vector3(0, 0, 0));
    }

    public void OpenSettings()
    {
        StopAllCoroutines();
        MoveUIToPosition(mainMenu.transform, new Vector3(3000, 0, 0));
        MoveUIToPosition(background.transform, new Vector3(3000, 0, 0));
        MoveUIToPosition(settingsMenu.transform, new Vector3(0, 0, 0));
    }

    public void OpenCredits()
    {
        StopAllCoroutines();
        MoveUIToPosition(mainMenu.transform, new Vector3(-3000, 0, 0));
        MoveUIToPosition(background.transform, new Vector3(-3000, 0, 0));
        MoveUIToPosition(creditsMenu.transform, new Vector3(0, 0, 0));
    }

    public void BackToMainMenu()
    {
        StopAllCoroutines();
        ReturnUIToOrigin(mainMenu.transform);
        ReturnUIToOrigin(levelSelectMenu.transform);
        ReturnUIToOrigin(settingsMenu.transform);
        ReturnUIToOrigin(creditsMenu.transform);
        ReturnUIToOrigin(background.transform);
    }

    public void LoadScene(int scene)
    {
        StopAllCoroutines();
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

    private IEnumerator FadeInUI(CanvasGroup ui)
    {
        ui.blocksRaycasts = true;

        while(ui.alpha < 1f)
        {
            ui.alpha += Time.fixedDeltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutUI(CanvasGroup ui)
    {
        ui.blocksRaycasts = false;
        
        while(ui.alpha > 0f)
        {
            ui.alpha -= Time.fixedDeltaTime;
            yield return null;
        }
    }
}
