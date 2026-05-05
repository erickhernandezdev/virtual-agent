using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class TextToSpeech : MonoBehaviour
{
    [Header("TTS Settings")]
    public int speakingRate = 1; // 0.25 to 4.0
    public float pitch = 0f;     // -20 to 20

    public bool IsSpeaking { get; private set; }
    public event Action OnPlaybackComplete;

    private string apiKey;
    private string googleTTSUrl = "https://texttospeech.googleapis.com/v1/text:synthesize";
    private AudioSource audioSource;
    private AgentController agentController;

    void Start()
    {
        agentController = GetComponent<AgentController>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        apiKey = System.Environment.GetEnvironmentVariable("GOOGLE_TTS_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            Debug.LogError("GOOGLE_TTS_API_KEY environment variable not set!");
    }

    public void Speak(string text)
    {
        if (IsSpeaking)
            StopSpeaking();

        StartCoroutine(SpeakCoroutine(text));
    }

    private IEnumerator SpeakCoroutine(string text)
    {
        IsSpeaking = true;
        if (agentController != null)
            agentController.isAgentSpeaking = true;

        string json = "{" +
            "\"input\": { \"text\": \"" + text.Replace("\"", "\\\"") + "\" }," +
            "\"voice\": {" +
                "\"languageCode\": \"es-US\"," +
                "\"name\": \"es-US-Neural2-B\"," +
                "\"ssmlGender\": \"MALE\"" +
            "}," +
            "\"audioConfig\": {" +
                "\"audioEncoding\": \"LINEAR16\"," +
                "\"speakingRate\": " + speakingRate + "," +
                "\"pitch\": " + pitch +
            "}" +
        "}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        string url = googleTTSUrl + "?key=" + apiKey;

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;

                GoogleTTSResponse parsed = JsonUtility.FromJson<GoogleTTSResponse>(response);
                if (parsed != null && !string.IsNullOrEmpty(parsed.audioContent))
                {
                    byte[] audioBytes = Convert.FromBase64String(parsed.audioContent);
                    yield return StartCoroutine(PlayAudio(audioBytes));
                }
                else
                {
                    Debug.LogError("Empty audio response from Google TTS");
                    FinishSpeaking();
                }
            }
            else
            {
                Debug.LogError("Google TTS error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                FinishSpeaking();
            }
        }
    }

    private IEnumerator PlayAudio(byte[] audioBytes)
    {
        int sampleCount = (audioBytes.Length - 44) / 2;
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(audioBytes, 44 + i * 2);
            samples[i] = sample / 32768f;
        }

        AudioClip clip = AudioClip.Create("tts", sampleCount, 1, 24000, false);
        clip.SetData(samples, 0);

        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitWhile(() => audioSource.isPlaying);

        Destroy(clip);
        FinishSpeaking();
    }

    private void FinishSpeaking()
    {
        IsSpeaking = false;
        if (agentController != null)
            agentController.isAgentSpeaking = false;
        OnPlaybackComplete?.Invoke();
        Debug.Log("Finished speaking");
    }

    public void StopSpeaking()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
        FinishSpeaking();
    }

    void OnDestroy()
    {
        StopSpeaking();
    }

    [Serializable]
    private class GoogleTTSResponse
    {
        public string audioContent;
    }
}