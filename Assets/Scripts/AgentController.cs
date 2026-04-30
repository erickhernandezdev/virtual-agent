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

public class AgentController
{
    
}
