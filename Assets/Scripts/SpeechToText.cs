using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using System.Collections;
using System.IO;
using System;

public class SpeechToText : MonoBehaviour
{
    public AgentController agentController;

    private bool isListening = false;
    private AudioClip recordedClip;
    private string microphoneDevice;

    private string apiKey;
    private string googleSTTUrl = "https://speech.googleapis.com/v1/speech:recognize";

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone found!");
            return;
        }
        microphoneDevice = Microphone.devices[0];
        Debug.Log("Microphone ready: " + microphoneDevice);
        apiKey = System.Environment.GetEnvironmentVariable("GOOGLE_STT_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            Debug.LogError("GOOGLE_STT_API_KEY environment variable not set!");
    }

    void Update()
    {
        bool spacePressed = Keyboard.current != null
            ? Keyboard.current.spaceKey.wasPressedThisFrame
            : Input.GetKeyDown(KeyCode.Space);

        if (!spacePressed) return;
        if (agentController == null) return;
        if (agentController.isAgentSpeaking) return;

        if (!isListening)
            StartListening();
        else
            StopListening();
    }

    private void StartListening()
    {
        if (Microphone.IsRecording(microphoneDevice))
            Microphone.End(microphoneDevice);

        if (recordedClip != null)
        {
            Destroy(recordedClip);
            recordedClip = null;
        }

        recordedClip = Microphone.Start(microphoneDevice, false, 120, 16000);
        isListening = true;
        Debug.Log("Microphone ON");
    }

    private void StopListening()
    {
        isListening = false;

        int position = Microphone.GetPosition(microphoneDevice);
        Microphone.End(microphoneDevice);

        if (position <= 0)
        {
            Debug.LogWarning("No audio recorded.");
            return;
        }

        AudioClip trimmed = TrimClip(recordedClip, position);
        Debug.Log("Microphone OFF - sending to Google STT...");
        StartCoroutine(SendAudioToGoogle(trimmed));
    }

    private AudioClip TrimClip(AudioClip clip, int position)
    {
        float[] samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        AudioClip trimmed = AudioClip.Create("trimmed", position, clip.channels, clip.frequency, false);
        trimmed.SetData(samples, 0);
        return trimmed;
    }

    private IEnumerator SendAudioToGoogle(AudioClip clip)
    {
        byte[] wavData = ConvertToWav(clip);
        string audioBase64 = Convert.ToBase64String(wavData);
        Destroy(clip);

        string json = "{" +
            "\"config\": {" +
                "\"encoding\": \"LINEAR16\"," +
                "\"sampleRateHertz\": 16000," +
                "\"languageCode\": \"es-419\"" +
            "}," +
            "\"audio\": {" +
                "\"content\": \"" + audioBase64 + "\"" +
            "}" +
        "}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        string url = googleSTTUrl + "?key=" + apiKey;

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
                Debug.Log("Google STT response: " + response);

                GoogleSTTResponse parsed = JsonUtility.FromJson<GoogleSTTResponse>(response);
                if (parsed != null && parsed.results != null && parsed.results.Length > 0)
                {
                    string text = parsed.results[0].alternatives[0].transcript;
                    Debug.Log("Transcribed: " + text);
                    agentController.ReceiveUserInput(text);
                }
                else
                {
                    Debug.LogWarning("Empty transcription.");
                }
            }
            else
            {
                Debug.LogError("Google STT error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    private byte[] ConvertToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            int byteCount = samples.Length * 2;

            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + byteCount);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((short)(clip.channels * 2));
            writer.Write((short)16);
            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(byteCount);

            foreach (float sample in samples)
                writer.Write((short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));

            return stream.ToArray();
        }
    }

    [Serializable]
    private class GoogleSTTResponse
    {
        public Result[] results;
    }

    [Serializable]
    private class Result
    {
        public Alternative[] alternatives;
    }

    [Serializable]
    private class Alternative
    {
        public string transcript;
        public float confidence;
    }
}
