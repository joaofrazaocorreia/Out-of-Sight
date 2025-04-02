using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScripts : MonoBehaviour
{
    [SerializeField] [Range(0.001f, 20f)] private float UISpeed = 3f;
    [SerializeField] private float UIScaleWidthIncrease = 100f;
    [SerializeField] private float UIScaleHeightIncrease = 0f;
    [SerializeField] private float UIScaleFontIncrease = 12f;
    [SerializeField] private GameObject background;
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelectMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;
    private Dictionary<Transform,Vector3> originalUIPositions;
    private Dictionary<RectTransform, (Coroutine, float, float, float)> uiButtonScaleUpCoroutines;
    private Dictionary<RectTransform, (Coroutine, float, float, float)> uiButtonScaleDownCoroutines;
    private float deltaTime;

    private void Start()
    {
        originalUIPositions = new Dictionary<Transform, Vector3>();
        uiButtonScaleUpCoroutines = new Dictionary<RectTransform, (Coroutine, float, float, float)>();
        uiButtonScaleDownCoroutines = new Dictionary<RectTransform, (Coroutine, float, float, float)>();
        
        #if UNITY_EDITOR
            UISpeed *= 2;
        #endif

        deltaTime = Time.fixedDeltaTime * UISpeed;

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
            Vector3 difference = newPos - uiTransform.localPosition;;

            Vector3 translation = new Vector3(
                Mathf.Max(difference.x * deltaTime, Mathf.Min(deltaTime, difference.x)),
                    Mathf.Max(difference.y * deltaTime, Mathf.Min(deltaTime, difference.y)),
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

    private IEnumerator ScaleUI(RectTransform uiTransform, float widthIncrement, float heightIncrement, float textIncrement)
    {
        TextMeshProUGUI buttonText = uiTransform.GetComponentInChildren<TextMeshProUGUI>();

        float targetWidth = uiTransform.sizeDelta.x + widthIncrement;
        float targetHeight = uiTransform.sizeDelta.y + heightIncrement;
        float targetTextSize = buttonText.fontSize + textIncrement;

        while(uiTransform.sizeDelta.x != targetWidth ||uiTransform.sizeDelta.y != targetHeight ||
            buttonText.fontSize != targetTextSize)
        {
            float widthDifference = targetWidth - uiTransform.sizeDelta.x;
            float heightDifference = targetHeight - uiTransform.sizeDelta.y;
            float textDifference = targetTextSize - buttonText.fontSize;

            if(widthDifference != 0)
                widthDifference = Mathf.Clamp(widthDifference, -deltaTime*100, deltaTime*100);

            if(heightDifference != 0)
                heightDifference = Mathf.Clamp(heightDifference, -deltaTime*60, deltaTime*60);

            if(textDifference != 0)
                textDifference = Mathf.Clamp(textDifference, -deltaTime*60, deltaTime*60);

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
                UIScaleHeightIncrease, UIScaleFontIncrease)), uiTransform.sizeDelta.x + UIScaleWidthIncrease,
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
                -UIScaleHeightIncrease, -UIScaleFontIncrease)), uiTransform.sizeDelta.x - UIScaleWidthIncrease,
                    uiTransform.sizeDelta.y - UIScaleHeightIncrease, buttonText.fontSize - UIScaleFontIncrease));
        }
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
        StartCoroutine(FadeInUI(loadingScreen, 0.2f));

        while(loadingScreen.alpha < 1f)
        {
            yield return null;
        }

        SceneManager.LoadScene(scene);
    }

    private IEnumerator FadeInUI(CanvasGroup ui, float speed = 1f)
    {
        ui.blocksRaycasts = true;

        while(ui.alpha < 1f)
        {
            ui.alpha += Time.fixedDeltaTime * speed;
            yield return null;
        }
    }

    private IEnumerator FadeOutUI(CanvasGroup ui, float speed = 1f)
    {
        ui.blocksRaycasts = false;
        
        while(ui.alpha > 0f)
        {
            ui.alpha -= Time.fixedDeltaTime * speed;
            yield return null;
        }
    }

    public void StartFadingInUI(CanvasGroup screen)
    {
        screen.alpha = 0f;
        StartCoroutine(FadeInUI(screen));
    }

    public void StartFadingOutUI(CanvasGroup screen)
    {
        screen.alpha = 1f;
        StartCoroutine(FadeOutUI(screen));
    }
}
