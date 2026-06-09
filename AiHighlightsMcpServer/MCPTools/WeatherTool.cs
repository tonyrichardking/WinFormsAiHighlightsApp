using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MCPServer.MCPTools
{
    [McpServerToolType]
    public static class WeatherTool
    {
        [McpServerTool(Name = "getWeatherForCity"), Description("Get the current weather in a city")]
        public static async Task<string> GetWeatherForCityAsync(string city)
        {
            Console.WriteLine($"\n**** WeatherTool.GetWeatherForCityAsync called.  Parameter city = {city} ****");

            var apiKey = "3a15290535384e82b01133504250810";
            var url = $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={Uri.EscapeDataString(city)}";

            try
            {
                var json = await new HttpClient().GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var tempC = root.GetProperty("current").GetProperty("temp_c").GetDouble();
                var condition = root.GetProperty("current").GetProperty("condition").GetProperty("text").GetString();

                return $"It's {tempC}°C and {condition?.ToLower()} in {city}.";
            }
            catch (Exception ex)
            {
                return $"Sorry, I couldn't fetch the weather for {city}. ({ex.Message})";
            }
        }
    }
}