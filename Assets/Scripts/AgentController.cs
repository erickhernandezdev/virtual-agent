using UnityEngine;

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


public class AgentController : MonoBehaviour
{
    public int sampleWindow = 64;
    private AudioClip microphoneClip;
    private AudioSource audioSource;
    private LLMService llmService;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        llmService = GetComponent<LLMService>();
        MicrophoneToAudioClip();
    }

    public void MicrophoneToAudioClip()
    {
        string microphoneName = Microphone.devices[0];
        microphoneClip = Microphone.Start(microphoneName, true, 20, AudioSettings.outputSampleRate);
        audioSource.clip = microphoneClip;
        audioSource.loop = true;

        while (!(Microphone.GetPosition(microphoneName) > 0)) { }
        audioSource.Play();
    }

    public float GetLoudnessFromMicrophone()
    {
        return GetLoudnessFromAudioClip(
            Microphone.GetPosition(Microphone.devices[0]),
            microphoneClip
        );
    }

    public float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
    {
        int startPosition = clipPosition - sampleWindow;
        if (startPosition < 0) return 0;

        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPosition);

        float totalLoudness = 0;
        for (int i = 0; i < sampleWindow; i++)
            totalLoudness += Mathf.Abs(waveData[i]);

        return totalLoudness / sampleWindow;
    }

    public void ReceiveUserInput(string userText)
    {
        Debug.Log("Agent received: " + userText);
        llmService.SendToLLM(userText);
    }

    public void ReceiveAgentReply(string replyText)
    {
        Debug.Log("Agent will say: " + replyText);
        // Next step: send to Text-to-Speech
    }


}
