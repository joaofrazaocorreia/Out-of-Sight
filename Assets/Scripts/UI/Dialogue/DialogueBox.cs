using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float timeBeforeTextProceed = 1.5f;

    public static DialogueBox Instance;
    private PlayerInput playerInput;
    private CanvasGroup dialogueCanvas;
    private float textSpeed;
    private Coroutine showDialogueCoroutine;
    private Dictionary<char, float> characterCustomWaitTimes;
    private Dictionary<int, Action> onReachSpecificLines;
    private Dictionary<char, string> allowedFormatting;
    private int skipTextDisplay;

    private void Start()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;

        playerInput = FindAnyObjectByType<PlayerInput>();
        dialogueCanvas = GetComponent<CanvasGroup>();
        ResetCanvas();

        textSpeed = 1;
        SetUpCharacterCustomWaitTimes();
        SetUpAllowedFormatting();

        skipTextDisplay = -1;
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

    private void SetUpAllowedFormatting()
    {
        allowedFormatting = new Dictionary<char, string>()
        {
            {'b', "<b>"},
        };
    }

    private void Update()
    {
        if (playerInput.actions["Jump"].WasPressedThisFrame())
        {
            if (skipTextDisplay < 0)
                skipTextDisplay = 0;

            else
            {
                skipTextDisplay = 1;
            }
        }
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
        if (onReachSpecificLines != null && onReachSpecificLines.ContainsKey(-1))
        {
            onReachSpecificLines[-1]();
            onReachSpecificLines.Remove(-1);
        }

        // Fade in Canvas
        while (dialogueCanvas.alpha < 1)
        {
            dialogueCanvas.alpha += Time.deltaTime * 5f;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        // Start displaying each line
        for (int i = 0; i < textStrings.Count; i++)
        {
            //string text = textStrings[i];
            string text = string.Empty;
            skipTextDisplay = -1;

            //dialogueText.text = string.Empty; // resets the text box
            textBox.text = $"<#FFFFFF00>{textStrings[i]}</color>"; // adds all text to the box but transparent

            // If an Action is scheduled for this line, execute it, then remove it to mark as done
            if (onReachSpecificLines != null && onReachSpecificLines.ContainsKey(i))
            {
                onReachSpecificLines[i]();
                onReachSpecificLines.Remove(i);
            }

            bool ignoreWaitTime = false;
            bool ignoreCharacters = false;
            int ignoredChars = 0;
            string invisFormatting = string.Empty;

            // Adds each character in the string to the text
            //foreach (char c in text)
            for (int j = 0; j < textStrings[i].Count(); j++)
            {
                string fullText = textStrings[i];
                char c = fullText[j];

                if (c == '<')
                {
                    ignoreWaitTime = true;

                    if (!(allowedFormatting.ContainsKey(fullText[j + 1]) ||
                       (fullText[j + 1] == '/' && allowedFormatting.ContainsKey(fullText[j + 2]))))
                    {
                        ignoreCharacters = true;
                    }

                    else if (allowedFormatting.ContainsKey(fullText[j + 1]))
                    {
                        invisFormatting = allowedFormatting[fullText[j + 1]];
                    }

                    else if (fullText[j + 1] == '/' && allowedFormatting.ContainsKey(fullText[j + 2]))
                    {
                        invisFormatting = string.Empty;
                    }
                }

                if (c == '>')
                {
                    if (ignoreWaitTime)
                    {
                        ignoreWaitTime = false;
                    }

                    if (ignoreCharacters)
                    {
                        ignoreCharacters = false;
                        ignoredChars++;
                    }
                }

                if (ignoreCharacters)
                    ignoredChars++;


                //textBox.text += c;
                text += c;
                textBox.text = FormatDialogueText(textStrings[i], text, ignoredChars, invisFormatting);


                if (!ignoreWaitTime && skipTextDisplay < 0)
                {
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
            }

            float timer = timeBeforeTextProceed;
            while (timer > 0 && skipTextDisplay <= 0)
            {
                timer -= Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }

        if (onReachSpecificLines != null)
        {
            while(onReachSpecificLines.Keys.Count() > 0)
            {
                onReachSpecificLines[onReachSpecificLines.Keys.First()]();
                onReachSpecificLines.Remove(onReachSpecificLines.Keys.First());
                yield return null;
            }
        }

        // Fade out Canvas
        while (dialogueCanvas.alpha > 0)
        {
            dialogueCanvas.alpha -= Time.deltaTime * 5f;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    private string FormatDialogueText(string fullText, string visibleText, int ignoredChars, string invisFormatting = "")
    {
        string unformattedString = string.Empty;
        string invisibleText;

        bool ignoring = false;
        for(int i = 0; i < fullText.Count(); i++)
        {
            char c = fullText[i];
            
            if (c == '<')
            {
                if (!(allowedFormatting.ContainsKey(fullText[i + 1]) ||
                  (fullText[i + 1] == '/' && allowedFormatting.ContainsKey(fullText[i + 2]))))
                {
                    ignoring = true;
                }
            }
            if (ignoring && c == '>')
            {
                ignoring = false;
                continue;
            }

            if (!ignoring)
                unformattedString += c;
        }

        invisibleText = invisFormatting + unformattedString[(visibleText.Count() - ignoredChars)..];

        return $"<#FFFFFF>{visibleText}</color></b>" +
                   $"<#FFFFFF00>{invisibleText}</color></b>";
    }
}
