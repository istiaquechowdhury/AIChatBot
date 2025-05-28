
using System.Text.Json;
using System.Text;

namespace AIChatBot.Web.Service
{
    public class TavilyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TavilyService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Tavily:ApiKey"];
            Console.WriteLine($"Using API Key: {_apiKey}");
        }

        //public async Task<string> GetAnswerAsync(string question)
        //{
        //    var requestData = new
        //    {
        //        query = question
        //    };

        //    var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

        //    _httpClient.DefaultRequestHeaders.Clear();
        //    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        //    var response = await _httpClient.PostAsync("https://api.tavily.com/search", content);

        //    if (!response.IsSuccessStatusCode)
        //        return "Sorry, I couldn't respond right now.";

        //    var responseJson = await response.Content.ReadAsStringAsync();
        //    Console.WriteLine("Tavily API Response: " + responseJson);

        //    using var doc = JsonDocument.Parse(responseJson);
        //    if (doc.RootElement.TryGetProperty("answer", out var answerElement))
        //        return answerElement.GetString() ?? "No answer received.";

        //    return "No answer received.";
        //}
        public async Task<string> GetAnswerAsync(string question)
        {
            var requestData = new
            {
                api_key = _apiKey,
                query = question
            };

            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.tavily.com/search", content);

            if (!response.IsSuccessStatusCode)
                return "Sorry, I couldn't respond right now.";

            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var answer = root.GetProperty("answer").GetString();

            if (!string.IsNullOrEmpty(answer))
            {
                return answer;
            }
            else if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
            {
                var firstResult = results[0];
                var title = firstResult.GetProperty("title").GetString();
                var contentSnippet = firstResult.GetProperty("content").GetString();

                return $"{title}: {contentSnippet}";
            }

            return "No answer received.";
        }

    }
}
