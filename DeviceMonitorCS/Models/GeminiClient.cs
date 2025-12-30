using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceMonitorCS.Models
{
    public class GeminiClient
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly string _apiKey;

        public GeminiClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> AskAsync(string question)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = question }
                        }
                    }
                }
            };

            var response = await _http.PostAsJsonAsync(url, requestBody);
            
            if (!response.IsSuccessStatusCode)
            {
                return $"Error: {response.ReasonPhrase} (Status: {response.StatusCode})";
            }

            var json = await response.Content.ReadAsStringAsync();

            try 
            {
                using var doc = JsonDocument.Parse(json);
                var answer = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return answer;
            }
            catch (Exception ex)
            {
                return $"Error parsing response: {ex.Message}";
            }
        }
    }
}
