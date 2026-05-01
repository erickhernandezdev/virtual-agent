using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class LLMRequest
{
    public string model;
    public Message[] messages;
    public int max_tokens;
}

[System.Serializable]
public class LLMResponse
{
    public Choice[] choices;
    public string error;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

public class LLMService : MonoBehaviour
{
    [Header("NVIDIA NIM Settings")]
    public string apiKeyEnvVarName = "OPEN_ROUTER_KEY";
    public string apiUrl = "https://openrouter.ai/api/v1/chat/completions";
    public string model = "meta-llama/llama-3.1-8b-instruct";
    public int requestTimeout = 120;

    [TextArea(3, 6)]
    public string systemPrompt = "Eres un agente virtual amigable y natural. Responde siempre en español de forma conversacional, breve y clara.";

    private AgentController agentController;
    private Coroutine currentRequest;

    void Start()
    {
        agentController = GetComponent<AgentController>();
    }

    public void SendToLLM(string userText)
    {
        if (currentRequest != null)
        {
            StopCoroutine(currentRequest);
            currentRequest = null;
            Debug.Log("Cancelled previous LLM request");
        }
        currentRequest = StartCoroutine(PostRequest(userText));
    }

    private string GetApiKey()
    {
        string key = System.Environment.GetEnvironmentVariable(apiKeyEnvVarName);
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"Environment variable '{apiKeyEnvVarName}' is not set. Please set it before running.");
        }
        
        return key;
    }

    private IEnumerator PostRequest(string userText)
    {
        string apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            currentRequest = null;
            yield break;
        }

        // Build request using proper serialization
        LLMRequest requestData = new LLMRequest
        {
            model = model,
            max_tokens = 512,
            messages = new Message[]
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user",   content = userText }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log("JSON enviado: " + jsonBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Accept", "application/json");
        request.timeout = requestTimeout;

        Debug.Log("Sending to NVIDIA NIM: " + userText);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("Raw response: " + response);

            try
            {
                LLMResponse parsed = JsonUtility.FromJson<LLMResponse>(response);

                if (parsed == null)
                {
                    Debug.LogError("Failed to parse LLM response (null)");
                    currentRequest = null;
                    yield break;
                }

                if (!string.IsNullOrEmpty(parsed.error))
                {
                    Debug.LogError("API Error: " + parsed.error);
                    currentRequest = null;
                    yield break;
                }

                if (parsed.choices == null || parsed.choices.Length == 0)
                {
                    Debug.LogError("Invalid response from LLM API: no choices returned");
                    currentRequest = null;
                    yield break;
                }

                string agentReply = parsed.choices[0].message.content;
                Debug.Log("Agent reply: " + agentReply);

                if (agentController != null)
                    agentController.ReceiveAgentReply(agentReply);
                else
                    Debug.LogError("AgentController not found on this GameObject");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error processing LLM response: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("NVIDIA NIM error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }

        currentRequest = null;
    }
}