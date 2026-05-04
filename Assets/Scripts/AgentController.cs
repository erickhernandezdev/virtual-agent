using UnityEngine;
using TMPro;

[RequireComponent(typeof(LLMService))]
[RequireComponent(typeof(TextToSpeech))]
public class AgentController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI responseText;

    [Header("Chat")]
    [SerializeField] private ChatManager chatManager;

    [Header("Animator")]
    [SerializeField] private Animator avatarAnimator;
    private int currentTalkingIndex = 0;
    private int totalTalkingAnimations = 3;

    [Header("Microphone Settings")]
    private AudioSource audioSource;
    private LLMService llmService;
    private TextToSpeech textToSpeech;
    public bool isAgentSpeaking;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        llmService = GetComponent<LLMService>();
        textToSpeech = GetComponent<TextToSpeech>();

        if (textToSpeech != null)
            textToSpeech.OnPlaybackComplete += OnSpeechFinished;
        else
            Debug.LogError("TextToSpeech component not found.");

        if (responseText == null)
            Debug.LogWarning("Response TextMeshPro not assigned.");
    }

    public void ReceiveUserInput(string userText)
    {
        if (string.IsNullOrEmpty(userText)) return;

        Debug.Log("Agent received: " + userText);

        if (chatManager != null)
            chatManager.AddUserMessage(userText);

        if (responseText != null)
        {
            responseText.text = "User: " + userText;
        }

        if (llmService != null)
        {
            llmService.SendToLLM(userText);
        }
        else
        {
            Debug.LogError("LLMService not found on this GameObject.");
        }
    }

    public void ReceiveAgentReply(string replyText)
    {
        if (string.IsNullOrEmpty(replyText)) return;
        Debug.Log("Agent reply: " + replyText);

        if (chatManager != null)
            chatManager.AddAgentMessage(replyText);

        if (responseText != null)
            responseText.text = "Agent: " + replyText;

        if (textToSpeech != null && !textToSpeech.IsSpeaking)
        {
            isAgentSpeaking = true;
            textToSpeech.Speak(replyText);
        }

        if (avatarAnimator != null)
        {
            currentTalkingIndex = (currentTalkingIndex % totalTalkingAnimations) + 1;
            Debug.Log("Talking animation index: " + currentTalkingIndex);
            avatarAnimator.SetInteger("talkingIndex", currentTalkingIndex);
        }
    }

    private void OnSpeechFinished()
    {
        Debug.Log("Agent finished speaking.");
        isAgentSpeaking = false;

        if (avatarAnimator != null)
            avatarAnimator.SetInteger("talkingIndex", 0);
    }

    private void OnDestroy()
    {
        if (textToSpeech != null)
        {
            textToSpeech.OnPlaybackComplete -= OnSpeechFinished;
        }
    }
}
