using System.Text;
using System.Text.Json;
using AiHighlightsMcpServer.Prompt_Engineering;

namespace AiHighlightsWinFormsUi
{
    /// <summary>
    /// ChatClientApi is a class that provides methods to send HTTP requests to an AI chat server. 
    /// It supports various endpoints for retrieving system prompts, models, tools, and options, 
    /// as well as running prompts and structured prompts. The class uses an HttpClient instance 
    /// to perform GET, POST, PUT, and DELETE requests based on the specified endpoint and message. 
    /// It serializes messages to JSON when necessary and ensures successful responses from the server.
    /// </summary>
    public class ChatClientApi
    {
        public HttpClient httpClient;
        public ChatClientApi(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> SendMessageAsync(string endPoint, object message)
        {
            string request = null;
            string httpVerb = null;

            switch (endPoint)
            {
                case "getSystemPrompts":
                case "getModels":
                case "getTools":
                case "getOptions":
                    request = $"{endPoint}";
                    httpVerb = "GET";
                    break;
                case "runPrompt":
                    request = $"{endPoint}?prompt={Uri.EscapeDataString(message?.ToString() ?? "")}";
                    httpVerb = "GET";
                    break;
                case "runStructuredPrompt":
                case "runTypedPrompt":
                    request = endPoint;
                    httpVerb = "POST";
                    break;
                case "setModel":
                case "setSystemPrompt":
                    request = endPoint;
                    httpVerb = "PUT";
                    break;
                default:
                    break;
            }

            string result = null;

            if (httpVerb == "GET")
            {
                using var response = await httpClient.GetAsync(request);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            else if (httpVerb == "PUT")
            {
                var json = JsonSerializer.Serialize(new { Value = message });
                using var response = await httpClient.PutAsync(request, new StringContent(json, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            else if (httpVerb == "POST")
            {
                var json = JsonSerializer.Serialize(message);
                using var response = await httpClient.PostAsync(request, new StringContent(json, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            } else if (httpVerb == "DELETE") 
            {
                using var response = await httpClient.DeleteAsync(request);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}
