using System.Text;
using System.Text.Json;

namespace AiHighlightsWinFormsUi
{
    public class AiChatClient
    {
        public HttpClient httpClient;
        public AiChatClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> SendMessageAsync(string endPoint, string message)
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
                    request = $"{endPoint}?prompt={Uri.EscapeDataString(message)}";
                    httpVerb = "GET";
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

            return result;
        }
    }
}
