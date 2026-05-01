using UnityEngine;
using TMPro;

/// <summary>
/// AgentController - Core brain of the virtual agent
/// 
/// This script coordinates the entire conversation flow:
///   1. Receives the user's text (from SpeechToText)
///   2. Sends it to the LLM API (Claude or Gemini)
///   3. Receives the agent's response
///   4. Sends it to TTS to be spoken out loud
///   5. Updates the on-screen text (Canvas)
/// 
/// DEPENDENCIES:
///   - Requires SpeechToText.cs to be present in the scene
///   - Requires TextToSpeech.cs to be present in the scene
///   - Requires a reference to the TextMeshPro component on the Canvas
/// </summary>

[RequireComponent(typeof(LLMService))]
[RequireComponent(typeof(TextToSpeech))]
public class AgentController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI responseText;

    [Header("Microphone Settings")]
    public int sampleWindow = 64;
    private AudioClip microphoneClip;
    private AudioSource audioSource;
    private LLMService llmService;
    private TextToSpeech textToSpeech;
    public bool isAgentSpeaking;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        llmService = GetComponent<LLMService>();
        textToSpeech = GetComponent<TextToSpeech>();

        MicrophoneToAudioClip(); // ← add this back

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

        if (responseText != null)
            responseText.text = "Agent: " + replyText;

        if (textToSpeech != null && !textToSpeech.IsSpeaking)
        {
            isAgentSpeaking = true; // ← add this
            textToSpeech.Speak(replyText);
        }
    }

    private void OnSpeechFinished()
    {
        Debug.Log("Agent finished speaking.");
        isAgentSpeaking = false;
    }

    private void OnDestroy()
    {
        if (textToSpeech != null)
        {
            textToSpeech.OnPlaybackComplete -= OnSpeechFinished;
        }
    }

    public float GetLoudnessFromMicrophone()
    {
        if (microphoneClip == null || Microphone.devices.Length == 0) return 0;

        return GetLoudnessFromAudioClip(
            Microphone.GetPosition(Microphone.devices[0]),
            microphoneClip
        );
    }

    public float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
    {
        if (clip == null) return 0;

        int startPosition = clipPosition - sampleWindow;
        if (startPosition < 0) return 0;

        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPosition);

        float totalLoudness = 0;
        for (int i = 0; i < sampleWindow; i++)
            totalLoudness += Mathf.Abs(waveData[i]);

        return totalLoudness / sampleWindow;
    }

    public void MicrophoneToAudioClip()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone found!");
            return;
        }
        string microphoneName = Microphone.devices[0];
        microphoneClip = Microphone.Start(microphoneName, true, 20, AudioSettings.outputSampleRate);
        audioSource.clip = microphoneClip;
        audioSource.loop = true;
        while (!(Microphone.GetPosition(microphoneName) > 0)) { }
        audioSource.Play();
        Debug.Log("Microphone started: " + microphoneName);
    }
}
