using UnityEngine;
using UnityEngine.Windows.Speech;

/// <summary>
/// SpeechToText - Voice capture and transcription
/// 
/// This script is responsible for:
///   1. Activating the system microphone when the user wants to speak
///   2. Recording the audio while they speak
///   3. Sending the audio to the STT API (OpenAI Whisper recommended)
///   4. Returning the transcribed text to AgentController
/// 
/// RECOMMENDED API: OpenAI Whisper
///   - Endpoint: https://api.openai.com/v1/audio/transcriptions
///   - Accepts audio files in WAV, MP3, and other formats
///   - Documentation: https://platform.openai.com/docs/api-reference/audio
/// 
/// ALTERNATIVE: Deepgram
///   - Better latency for real-time use
///   - Supports audio streaming (more advanced)
///   - Documentation: https://developers.deepgram.com
/// </summary>

public class SpeechToText : MonoBehaviour
{
    private DictationRecognizer dictationRecognizer;
    public AgentController agentController;

    void Start()
    {
        dictationRecognizer = new DictationRecognizer();

        // Fires when a phrase is recognized
        dictationRecognizer.DictationResult += OnDictationResult;

        // Fires on partial results (optional, good for debugging)
        dictationRecognizer.DictationHypothesis += OnDictationHypothesis;

        // Fires on error
        dictationRecognizer.DictationError += OnDictationError;

        dictationRecognizer.Start();
        Debug.Log("Dictation started, listening...");
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log("You said: " + text);
        // This is where we'll send text to the LLM later
        agentController.ReceiveUserInput(text);
    }

    private void OnDictationHypothesis(string text)
    {
        Debug.Log("Hearing: " + text); // live partial result
    }

    private void OnDictationError(string error, int hresult)
    {
        Debug.LogError("Dictation error: " + error);
        // Restart on error
        dictationRecognizer.Start();
    }

    void OnDestroy()
    {
        dictationRecognizer.DictationResult -= OnDictationResult;
        dictationRecognizer.Stop();
        dictationRecognizer.Dispose();
    }
}