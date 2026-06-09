using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AiHighlightsWinFormsUi
{
    public enum ChatMessageRole
    {
        User,
        Assistant,
        System
    }

    public class ChatInterface
    {
        public ChatMessageRole Role { get; set; }

        public string Text { get; set; } = "";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    // send a HTTP GET request to the AiChatController's runPrompt endpoint for each message, and stream the response back as it arrives
    public class AiChatClient
    {
        public HttpClient httpClient;
        public AiChatClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> SendMessageAsync(string endPoint, string message)
        {
            // send the message as a query parameter in a GET or PUT request to theselected AiChatController's endpoint

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
                var json = JsonSerializer.Serialize(new
                {
                    Value = message
                });

                using var response = await httpClient.PutAsync(request, new StringContent(json, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}




/*

        public async IAsyncEnumerable<string> SendMessageAsync(string endPoint, string message, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // send the message as a query parameter in a GET or PUT request to theselected AiChatController's endpoint

            string requestUri = null;
            string httpVerb = null;

            switch (endPoint)
            {
                case "getSystemPrompts":
                case "getModels":
                case "getTools":
                case "getOptions":
                    requestUri = $"/{endPoint}";
                    httpVerb = "GET";
                    break;
                case "runPrompt":
                    requestUri = $"/{endPoint}?prompt={Uri.EscapeDataString(message)}";
                    httpVerb = "GET";
                    break;
                case "setModel":
                case "setSystemPrompt":
                    requestUri = $"/{endPoint}";
                    httpVerb = "PUT";
                    break;
                default:
                    break;
            }

            if (httpVerb == "GET")
            {
                using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        yield return line;
                    }
                }
            }
            else if (httpVerb == "PUT")
            {
                using var response = await httpClient.PutAsync(requestUri, new StringContent(message, Encoding.UTF8, "application/text"), cancellationToken);
                response.EnsureSuccessStatusCode();
            }
        }

 */
