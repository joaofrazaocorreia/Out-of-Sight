using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ObjectiveText : MonoBehaviour
{
    private RectTransform rectTransform;
    private TextMeshProUGUI text;
    public TextMeshProUGUI Text { get => text; }
    private Vector2 originalScale;

    private void OnEnable()
    {
        text = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        originalScale = new(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
    }

    public void UpdateText(string newText, float alpha = 1f)
    {
        text.text = newText;
        text.alpha = alpha;

        CheckTextLength();
    }

    public void CheckTextLength()
    {
        float limitPerLine = 2* originalScale.x / text.fontSize;

        rectTransform.sizeDelta = new(originalScale.x, Mathf.Ceil(text.text.Length / limitPerLine) * originalScale.y);
    }
}
