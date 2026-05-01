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
    [Header("OpenRouter Settings")]
    public string apiKey;
    public string apiUrl = "https://openrouter.ai/api/v1/chat/completions";
    public string model = "meta-llama/llama-3.1-8b-instruct";
    public int maxTokens = 150;
    public int requestTimeout = 30;

    [TextArea(3, 6)]
    public string systemPrompt = "Eres un agente virtual amigable y natural. Responde siempre en español de forma conversacional, breve y clara.";

    private AgentController agentController;
    private Coroutine currentRequest;

    void Start()
    {
        agentController = GetComponent<AgentController>();

        if (string.IsNullOrEmpty(apiKey))
            Debug.LogError("API Key is empty! Paste your OpenRouter key in the Inspector.");
    }

    public void SendToLLM(string userText)
    {
        if (string.IsNullOrEmpty(userText)) return;

        if (currentRequest != null)
        {
            StopCoroutine(currentRequest);
            currentRequest = null;
            Debug.Log("Cancelled previous request");
        }
        currentRequest = StartCoroutine(PostRequest(userText));
    }

    private IEnumerator PostRequest(string userText)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key is empty!");
            currentRequest = null;
            yield break;
        }

        LLMRequest requestData = new LLMRequest
        {
            model = model,
            max_tokens = maxTokens,
            messages = new Message[]
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user",   content = userText }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log("JSON sent: " + jsonBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Accept", "application/json");
        request.timeout = requestTimeout;

        Debug.Log("Sending to OpenRouter: " + userText);
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
                    Debug.LogError("Failed to parse response (null)");
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
                    Debug.LogError("No choices in response");
                    currentRequest = null;
                    yield break;
                }

                string agentReply = parsed.choices[0].message.content;
                Debug.Log("Agent reply: " + agentReply);

                if (agentController != null)
                    agentController.ReceiveAgentReply(agentReply);
                else
                    Debug.LogError("AgentController not found!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Parse error: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("OpenRouter error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }

        currentRequest = null;
    }
}