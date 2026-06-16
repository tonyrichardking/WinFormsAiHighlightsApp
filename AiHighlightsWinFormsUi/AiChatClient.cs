using System.Text;
using System.Text.Json;
using AiHighlightsMcpServer.Prompt_Engineering;

namespace AiHighlightsWinFormsUi
{
    public class AiChatClient
    {
        public HttpClient httpClient;
        public AiChatClient(HttpClient httpClient)
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
                    // create a POST request with the structured prompt as JSON in the body
                    request = $"{endPoint}";
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
