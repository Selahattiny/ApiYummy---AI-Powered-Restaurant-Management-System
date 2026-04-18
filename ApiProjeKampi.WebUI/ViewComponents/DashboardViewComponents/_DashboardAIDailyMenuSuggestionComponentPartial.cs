using ApiProjeKampi.WebUI.Dtos.AISuggestionDtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ApiProjeKampi.WebUI.ViewComponents.DashboardViewComponents
{
    public class _DashboardAIDailyMenuSuggestionComponentPartial : ViewComponent
    {
        // DÜZELTME: API Key direkt olarak değişkene atandı.
        private const string GROQ_API_KEY = "";

        private readonly IHttpClientFactory _httpClientFactory;
        public _DashboardAIDailyMenuSuggestionComponentPartial(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var groqClient = _httpClientFactory.CreateClient();

            // Groq API Base Address
            groqClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
            groqClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GROQ_API_KEY);

            string prompt = @"
4 farklı dünya mutfağından tamamen rastgele günlük menü oluştur. Ülke listesi: Türkiye, Fransa, Almanya, İtalya, İspanya, Portekiz, Bulgaristan, Gürcistan, Yunanistan, İran, Çin.

KURALLAR:
1. Her seferinde farklı 4 ülke seç.
2. Tüm içerik TÜRKÇE olsun.
3. CountryCode ISO formatında olsun (TR, IT vb.).
4. Sadece geçerli bir JSON array döndür. Başına veya sonuna açıklama ekleme.

JSON formatı:
[
  {
    ""Cuisine"": ""X Mutfağı"",
    ""CountryCode"": ""XX"",
    ""MenuTitle"": ""Günlük Menü"",
    ""Items"": [
      { ""Name"": ""Yemek 1"", ""Description"": ""Açıklama"", ""Price"": 100 },
      { ""Name"": ""Yemek 2"", ""Description"": ""Açıklama"", ""Price"": 120 },
      { ""Name"": ""Yemek 3"", ""Description"": ""Açıklama"", ""Price"": 90 },
      { ""Name"": ""Yemek 4"", ""Description"": ""Açıklama"", ""Price"": 70 }
    ]
  }
]";

            var body = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                    new { role = "system", content = "Sen sadece JSON array dönen bir asistansın. JSON dışında hiçbir metin yazma." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.8
            };

            var jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await groqClient.PostAsync("chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    // Loglama yapılabilir: response.StatusCode
                    return View(new List<MenuSuggestionDto>());
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(responseJson);
                string aiContent = obj.choices[0].message.content.ToString();

                // EKSTRA GÜVENLİK: Markdown bloklarını temizle ve sadece JSON kısmını al
                aiContent = aiContent.Replace("```json", "").Replace("```", "").Trim();

                int startIndex = aiContent.IndexOf('[');
                int lastIndex = aiContent.LastIndexOf(']');

                if (startIndex != -1 && lastIndex != -1)
                {
                    aiContent = aiContent.Substring(startIndex, lastIndex - startIndex + 1);
                }

                var menus = JsonConvert.DeserializeObject<List<MenuSuggestionDto>>(aiContent);
                return View(menus);
            }
            catch (Exception)
            {
                // Hata durumunda boş liste döner
                return View(new List<MenuSuggestionDto>());
            }
        }
    }
}