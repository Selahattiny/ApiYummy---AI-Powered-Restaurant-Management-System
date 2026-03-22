using ApiProjeKampi.WebUI.Dtos.MessageDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static ApiProjeKampi.WebUI.Controllers.AIController;

namespace ApiProjeKampi.WebUI.Controllers
{
    public class MessageController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MessageController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> MessageList()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7156/api/Messages");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultMessageDto>>(jsonData);
                return View(values);
            }
            return View();
        }

        [HttpGet]
        public IActionResult CreateMessage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageDto createMessageDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createMessageDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync("https://localhost:7156/api/Messages",
                stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("MessageList");
            }
            return View();
        }

        public async Task<IActionResult> DeleteMessage(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync("https://localhost:7156/api/Messages?id=" + id);
            return RedirectToAction("MessageList");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateMessage(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync("https://localhost:7156/api/Messages/GetMessage?id=" + id);
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<GetMessageByIdDto>(jsonData);
            return View(value);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMessage(UpdateMessageDto updateMessageDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateMessageDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            await client.PutAsync("https://localhost:7156/api/Messages/", stringContent);
            return RedirectToAction("MessageList");
        }

        [HttpGet]
        public async Task<IActionResult> AnswerMessageWithGroqAI(int id)
        {
            var client = _httpClientFactory.CreateClient();

            // 1. API'den mesajı çek
            var responseMessage = await client.GetAsync("https://localhost:7156/api/Messages/GetMessage?id=" + id);

            if (!responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("MessageList"); // Mesaj bulunamazsa listeye geri dön
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<GetMessageByIdDto>(jsonData);

            // KRİTİK KONTROL: Eğer value null ise aşağıya geçme
            if (value == null || string.IsNullOrEmpty(value.MessageDetails))
            {
                ViewBag.answerAI = "Hata: Mesaj içeriği boş veya mesaj bulunamadı.";
                return View(value ?? new GetMessageByIdDto());
            }

            // 2. Groq Ayarları
            var apiKey = "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";
            var baseUrl = "https://api.groq.com/openai/v1/chat/completions";

            // Yetkilendirmeyi buraya ekle
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestData = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
            new { role = "system", content = "Sen Yummy restoranı için nazik bir asistanısın." },
            new { role = "user", content = value.MessageDetails } // Artık value null değil, eminiz!
        },
                temperature = 0.7
            };

            // 3. Groq'a Gönder
            var response = await client.PostAsJsonAsync(baseUrl, requestData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GroqResponse>();
                ViewBag.answerAI = result?.choices?[0]?.message?.content ?? "Cevap üretilemedi.";
            }
            else
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                ViewBag.answerAI = $"Hata: {response.StatusCode} - {errorDetail}";
            }

            return View(value);
        }

        public PartialViewResult SendMessage()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(CreateMessageDto createMessageDto)
        {
            var client=new HttpClient();
            var apiKey = "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";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer" , apiKey);
            try
            {
                var translateRequestBody = new
                {
                    inputs = createMessageDto
                };
                var translateJson=System.Text.Json.JsonSerializer.Serialize(translateRequestBody);
                var translateContent=new StringContent(translateJson, Encoding.UTF8, "application/json");

                var translateResponse = await client.PostAsync("https://api-inference.huggingface.com/models/Helsinki-NPL/opus-mt-tr-en", translateContent);
                var translateResponseString=await translateResponse.Content.ReadAsStringAsync();

                string englishText = createMessageDto.MessageDetails;
                if(translateResponseString.TrimStart().StartsWith("["))
                {
                    var translateDoc=JsonDocument.Parse(translateResponseString);
                    englishText = translateDoc.RootElement[0].GetProperty("translation_text").GetString();
                    ViewBag.v=englishText;
                }

                var toxicRequestBody = new
                {
                    inputs = englishText
                };

                var toxicJson = System.Text.Json.JsonSerializer.Serialize(toxicRequestBody);
                var toxicContent=new StringContent(toxicJson, Encoding.UTF8, "application/json");
                var toxicResponse = await client.PostAsync("https://api-inference.huggingface.co/models/unitary/toxic-bert", toxicContent); var toxicResponseString=await toxicResponse.Content.ReadAsStringAsync();

                ViewBag.DebugToxic = toxicResponseString;

                if (toxicResponseString.TrimStart().StartsWith("["))
                {
                    var toxicDoc = JsonDocument.Parse(toxicResponseString);
                    foreach(var item in toxicDoc.RootElement[0].EnumerateArray())
                    {
                        string label = item.GetProperty("label").GetString();
                        double score=item.GetProperty("score").GetDouble();

                        if(score > 0.5)
                        {
                            createMessageDto.Status = "Toksik Mesaj";
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(createMessageDto.Status))
                {
                    createMessageDto.Status = "Mesaj Alındı";
                }

            }
            catch
            {
                createMessageDto.Status = "OnayBekliyor";
            }

            var client2 = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createMessageDto);
            StringContent stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var responseMessage = await client2.PostAsync("https://localhost:7156/api/Messages",
                stringContent);
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("MessageList");
            }
            return View();
        }
    }
}

