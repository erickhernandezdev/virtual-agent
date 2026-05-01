using UnityEngine;
using System.Collections;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

public class TextToSpeech : MonoBehaviour
{
    [Header("TTS Settings")]
    public int rate = 1;    // -10 (slow) to 10 (fast)
    public int volume = 100; // 0 to 100

    public bool IsSpeaking { get; private set; }
    public event System.Action OnPlaybackComplete;

    private Process ttsProcess;
    private AgentController agentController;

    void Start()
    {
        agentController = GetComponent<AgentController>();
    }

    public void Speak(string text)
    {
        if (IsSpeaking)
        {
            StopSpeaking();
        }
        StartCoroutine(SpeakCoroutine(text));
    }

    private IEnumerator SpeakCoroutine(string text)
    {
        IsSpeaking = true;
        if (agentController != null)
            agentController.isAgentSpeaking = true;

        // Escape single quotes in text
        text = text.Replace("'", " ");

        // PowerShell command to speak
        string psCommand = $"Add-Type -AssemblyName System.Speech; " +
                           $"$s = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
                           $"$s.Rate = {rate}; " +
                           $"$s.Volume = {volume}; " +
                           $"$s.SelectVoiceByHints('Female', 'Adult', 0, 'es-ES'); " +
                           $"$s.Speak('{text}');";

        ttsProcess = new Process();
        ttsProcess.StartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -WindowStyle Hidden -Command \"{psCommand}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        ttsProcess.Start();
        Debug.Log("Speaking: " + text);

        // Wait for process to finish
        while (!ttsProcess.HasExited)
        {
            yield return null;
        }

        ttsProcess.Dispose();
        ttsProcess = null;

        IsSpeaking = false;
        if (agentController != null)
            agentController.isAgentSpeaking = false;

        OnPlaybackComplete?.Invoke();
        Debug.Log("Finished speaking");
    }

    public void StopSpeaking()
    {
        if (ttsProcess != null && !ttsProcess.HasExited)
        {
            ttsProcess.Kill();
            ttsProcess.Dispose();
            ttsProcess = null;
        }
        IsSpeaking = false;
        if (agentController != null)
            agentController.isAgentSpeaking = false;
    }

    void OnDestroy()
    {
        StopSpeaking();
    }
}