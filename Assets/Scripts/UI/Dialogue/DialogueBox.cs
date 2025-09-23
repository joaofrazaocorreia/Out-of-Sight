using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float timeBeforeTextProceed = 1.5f;

    public static DialogueBox Instance;
    private CanvasGroup dialogueCanvas;
    private float textSpeed;
    private Coroutine showDialogueCoroutine;
    private Dictionary<char, float> characterCustomWaitTimes;
    private Dictionary<int, Action> onReachSpecificLines;

    private void Start()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;

        dialogueCanvas = GetComponent<CanvasGroup>();
        ResetCanvas();

        textSpeed = 1;
        SetUpCharacterCustomWaitTimes();
    }

    private void ResetCanvas()
    {
        dialogueCanvas.alpha = 0;
        speakerText.text = string.Empty;
        dialogueText.text = string.Empty;
    }

    private void SetUpCharacterCustomWaitTimes()
    {
        characterCustomWaitTimes = new Dictionary<char, float>()
        {
            {'.', 2f},
            {',', 1f},
            {'!', 2f},
            {'?', 2f},
            {':', 1f},
            {';', 2f},
            {'-', 0.5f},
        };
    }

    public void ShowDialogue(List<string> textStrings, string speaker, float textSpeed = 1f, Dictionary<int, Action> onReachSpecificLinesEvents = null)
    {
        // If interrupting an active coroutine, stop it and execute all scheduled actions
        if (showDialogueCoroutine != null)
        {
            StopCoroutine(showDialogueCoroutine);

            if (onReachSpecificLines != null)
            {
                foreach (int i in onReachSpecificLines.Keys)
                {
                    onReachSpecificLines[i]();
                }
            }
        }

        // Reset text box
        speakerText.text = speaker;
        dialogueText.text = string.Empty;

        // Setup text speed and scheduled actions
        this.textSpeed = textSpeed;
        onReachSpecificLines = onReachSpecificLinesEvents;

        // Start displaying dialogue
        showDialogueCoroutine = StartCoroutine(StartShowingText(textStrings, dialogueText));
    }

    private IEnumerator StartShowingText(List<string> textStrings, TextMeshProUGUI textBox)
    {
        // Fade in Canvas
        while (dialogueCanvas.alpha < 1)
        {
            dialogueCanvas.alpha += Time.deltaTime * 3f;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        // Start displaying each line
        for (int i = 0; i < textStrings.Count; i++)
        {
            string text = textStrings[i];

            dialogueText.text = string.Empty; // resets the text box

            // If an Action is scheduled for this line, execute it, then remove it to mark as done
            if (onReachSpecificLines != null && onReachSpecificLines.ContainsKey(i))
            {
                onReachSpecificLines[i]();
                onReachSpecificLines.Remove(i);
            }

            // Adds each character in the string to the text
            foreach (char c in text)
            {
                textBox.text += c;

                // Checks for custom wait times for specific characters like punctuation
                if (characterCustomWaitTimes.ContainsKey(c))
                {
                    yield return new WaitForSeconds(characterCustomWaitTimes[c] / textSpeed);
                }

                else
                {
                    yield return new WaitForSeconds(0.1f / textSpeed);
                }
            }

            // Waits before proceeding to next line or fading out
            yield return new WaitForSeconds(timeBeforeTextProceed);
        }

        // Fade out Canvas
        while (dialogueCanvas.alpha > 0)
        {
            dialogueCanvas.alpha -= Time.deltaTime * 3f;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}
