using UnityEngine;

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

public class SpeechToText
{
    
}
