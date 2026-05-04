using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChatManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject messageBubblePrefab;
    [SerializeField] private Transform chatContent;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Colores")]
    [SerializeField] private Color userColor = new Color(0.2f, 0.5f, 1f, 0.9f);
    [SerializeField] private Color agentColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);

    public void AddUserMessage(string text)
    {
        AddMessage(text, userColor, false);
    }

    public void AddAgentMessage(string text)
    {
        AddMessage(text, agentColor, true);
    }

    private void AddMessage(string text, Color color, bool isAgent)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, chatContent);

        Image image = bubble.GetComponent<Image>();
        if (image != null)
            image.color = color;

        TextMeshProUGUI tmp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.alignment = isAgent
                ? TextAlignmentOptions.Left
                : TextAlignmentOptions.Right;
        }

        HorizontalLayoutGroup hlg = bubble.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
            hlg = bubble.AddComponent<HorizontalLayoutGroup>();

        hlg.childAlignment = isAgent
            ? TextAnchor.MiddleLeft
            : TextAnchor.MiddleRight;

        hlg.padding = new RectOffset(10, 10, 5, 5);
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
