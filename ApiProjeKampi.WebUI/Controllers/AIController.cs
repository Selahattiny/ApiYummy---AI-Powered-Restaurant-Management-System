using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ApiProjeKampi.WebUI.Controllers
{
    public class AIController : Controller
    {
        [HttpGet]
        public IActionResult CreateRecipeWithGroqAI()
        {
            return View("CreateRecipeWithGroqAI");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipeWithGroqAI(string prompt)
        {
            var apiKey = "";
            var baseUrl = "https://api.groq.com/openai/v1/chat/completions";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestData = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
    {
        new { role = "system", content = "Sen Yummy restoranı için profesyonel bir şef asistanısın." },
        new { role = "user", content = prompt }
    },
                temperature = 0.7
            };

            var response = await client.PostAsJsonAsync(baseUrl, requestData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GroqResponse>();
                ViewBag.recipe = result.choices[0].message.content;
            }
            else
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                ViewBag.recipe = $"Hata Detayı: {response.StatusCode} - {errorDetail}";
            }

            return View("CreateRecipeWithGroqAI");
        }

        public class GroqResponse
        {
            public List<Choice> choices { get; set; }
        }

        public class Choice
        {
            public Message message { get; set; }
        }

        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }
    }
}